using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Cache;
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
using Umbraco.Web.Cache;

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
            DictionaryCacheRefresher.CacheUpdated += DictionaryCacheRefresherOnCacheUpdated;
            LocalizationService.SavingDictionaryItem += LocalizationServiceOnSavingDictionaryItem;

        }

        private void LocalizationServiceOnSavingDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            if(!_configuration.UseParentItemKeyPrefix || !e.CanCancel)
                return;

            foreach (IDictionaryItem dictionaryItem in e.SavedEntities)
            {
                IDictionaryItem parentDictionaryItem = dictionaryItem.ParentId.HasValue
                    ? sender.GetDictionaryItemById(dictionaryItem.ParentId.Value)
                    : null;
                DictionaryModel parentModel = parentDictionaryItem != null ? new DictionaryModelWrapper(parentDictionaryItem.Key, parentDictionaryItem.ItemKey) : null;
                DictionaryModel model = new DictionaryModelWrapper(dictionaryItem.Key, dictionaryItem.ItemKey, parentModel);
                if (model.IsValidGenerateCodeItemKey()) 
                    continue;

                e.CancelOperation(new EventMessage("DictionaryItem", "Message", EventMessageType.Error));
            }
            
        }

        private void DictionaryCacheRefresherOnCacheUpdated(DictionaryCacheRefresher sender, CacheRefresherEventArgs e)
        {
            GenerateModels();
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