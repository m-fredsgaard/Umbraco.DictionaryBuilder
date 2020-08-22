using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Serilog;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.DictionaryBuilder.Building;
using Umbraco.DictionaryBuilder.Configuration;
using Umbraco.DictionaryBuilder.Extensions;
using Umbraco.DictionaryBuilder.Factories;
using Umbraco.DictionaryBuilder.Models;
using File = System.IO.File;

namespace Umbraco.DictionaryBuilder.Compose
{
    public class DictionaryComponent : IComponent
    {
        private readonly IDictionaryBuilderConfiguration _configuration;
        private readonly ModelsGenerator _modelsGenerator;

        public DictionaryComponent(IDictionaryBuilderConfiguration configuration, ModelsGenerator modelsGenerator)
        {
            _configuration = configuration;
            _modelsGenerator = modelsGenerator;
        }

        public void Initialize()
        {
            EnsureDictionaries();
            GenerateModels();
            LocalizationService.SavingDictionaryItem += LocalizationServiceOnSavingDictionaryItem;
            LocalizationService.SavedDictionaryItem += LocalizationServiceOnSavedDictionaryItem;
            LocalizationService.DeletedDictionaryItem += LocalizationServiceOnDeletedDictionaryItem;
        }

        private void LocalizationServiceOnDeletedDictionaryItem(ILocalizationService sender, DeleteEventArgs<IDictionaryItem> e)
        {
            GenerateModels();
            RemoveOutOfDateMark();
        }

        private void LocalizationServiceOnSavedDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            if (!HasOutOfDateMark()) 
                return;

            GenerateModels();
            RemoveOutOfDateMark();
        }

        private void LocalizationServiceOnSavingDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            // If not enabled or if we do use nested structure or if e can't cancel, then return. Nothing to validate or cancel.
            if(!_configuration.Enable || !_configuration.UseNestedStructure || !e.CanCancel)
                return;

            // See if there are any newly created items
            bool containsNewItems = e.SavedEntities.Any(x => !x.HasIdentity);
            // An indicator of whether any item key has changed
            bool anyItemKeyChanged = false;
            
            foreach (IDictionaryItem dictionaryItem in e.SavedEntities)
            {
                IDictionaryItem parentDictionaryItem = dictionaryItem.ParentId.HasValue
                    ? sender.GetDictionaryItemById(dictionaryItem.ParentId.Value)
                    : null;
                // Create the parent model
                DictionaryModel parentModel = parentDictionaryItem != null ? new DictionaryModelWrapper(parentDictionaryItem) : null;
                // Create the model
                DictionaryModel model = new DictionaryModelWrapper(dictionaryItem, parentModel);

                if (dictionaryItem.HasIdentity && !anyItemKeyChanged)
                {
                    // Load the current dictionary item, to see if the ItemKey has changed.
                    IDictionaryItem currentDictionaryItem = sender.GetDictionaryItemById(dictionaryItem.Id);
                    anyItemKeyChanged = currentDictionaryItem.ItemKey != dictionaryItem.ItemKey;
                }

                if (model.IsValidForNestedStructure()) 
                    continue;

                e.CancelOperation(new EventMessage("DictionaryItem", "Invalid key. The key must start with the full key of the parent + .ItemKey", EventMessageType.Error));
            }
            
            // If the operation isn't cancelled and there are new items, or any existing dictionary has a new item key
            // then mark the models to be out-of-date.
            if(!e.Cancel && (containsNewItems || anyItemKeyChanged))
                SetOutOfDateMark();
        }

        private bool HasOutOfDateMark()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(_configuration.DictionaryDirectory))
                Directory.CreateDirectory(_configuration.DictionaryDirectory);

            // Save the file
            string filename = Path.Combine(_configuration.DictionaryDirectory, "ood");
            return File.Exists(filename);
        }

        private void RemoveOutOfDateMark()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(_configuration.DictionaryDirectory))
                Directory.CreateDirectory(_configuration.DictionaryDirectory);

            // Save the file
            string filename = Path.Combine(_configuration.DictionaryDirectory, "ood");
            File.Delete(filename);
        }
        
        private void SetOutOfDateMark()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(_configuration.DictionaryDirectory))
                Directory.CreateDirectory(_configuration.DictionaryDirectory);

            // Save the file
            string filename = Path.Combine(_configuration.DictionaryDirectory, "ood");
            File.WriteAllText(filename, string.Empty);
        }

        private void EnsureDictionaries()
        {
            try
            {
                Type[] dictionaryFactoryTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
                {
                    try
                    {
                        return x.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return new Type[0];
                    }
                }).Where(type => type.IsSubclassOf(typeof(DictionaryFactory))).Distinct().ToArray();

                foreach (Type dictionaryFactoryType in dictionaryFactoryTypes)
                {
                    DictionaryFactory dictionaryFactory =
                        Activator.CreateInstance(dictionaryFactoryType) as DictionaryFactory;
                    dictionaryFactory?.EnsureDictionaries();
                }
            }
            catch(Exception exception)
            {
                Log.Error(exception, "Not able to ensure dictionaries");
            }
        }

        private void GenerateModels()
        {
            try
            {
                if (!_configuration.Enable)
                    return;

                _modelsGenerator.GenerateModels();
            }
            catch(Exception exception)
            {
                Log.Error(exception, "Not able to generate models");
            }
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}