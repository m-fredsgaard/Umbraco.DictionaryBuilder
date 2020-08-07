using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
            if(!_configuration.Enable || !_configuration.UseParentItemKeyPrefix || !e.CanCancel)
                return;

            bool containsNewItems = e.SavedEntities.Any(x => !x.HasIdentity);
            bool anyItemKeyChanged = false;
            
            foreach (IDictionaryItem dictionaryItem in e.SavedEntities)
            {
                IDictionaryItem parentDictionaryItem = dictionaryItem.ParentId.HasValue
                    ? sender.GetDictionaryItemById(dictionaryItem.ParentId.Value)
                    : null;
                DictionaryModel parentModel = parentDictionaryItem != null ? new DictionaryModelWrapper(parentDictionaryItem) : null;
                DictionaryModel model = new DictionaryModelWrapper(dictionaryItem, parentModel);

                if (dictionaryItem.Id > 0 && !anyItemKeyChanged)
                {
                    IDictionaryItem currentDictionaryItem = sender.GetDictionaryItemById(dictionaryItem.Id);
                    anyItemKeyChanged = currentDictionaryItem.ItemKey != dictionaryItem.ItemKey;
                }

                if (model.IsValidGenerateCodeItemKey()) 
                    continue;

                e.CancelOperation(new EventMessage("DictionaryItem", "Message", EventMessageType.Error));
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
                DictionaryFactory dictionaryFactory = Activator.CreateInstance(dictionaryFactoryType) as DictionaryFactory;
                dictionaryFactory?.EnsureDictionaries();
            }
        }

        private void GenerateModels()
        {
            if (!_configuration.Enable)
                return;

            _modelsGenerator.GenerateModels();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}