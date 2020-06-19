using System;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.DictionaryBuilder.Configuration;

namespace Umbraco.DictionaryBuilder.Compose
{
    public class DictionaryComponent : IComponent
    {
        private readonly ILocalizationService _localizationService;
        private readonly IConfiguration _configuration;
        private readonly IDictionaryHttpHandler _dictionaryHttpHandler;

        public DictionaryComponent(ILocalizationService localizationService, IConfiguration configuration, IDictionaryHttpHandler dictionaryHttpHandler)
        {
            _localizationService = localizationService;
            _configuration = configuration;
            _dictionaryHttpHandler = dictionaryHttpHandler;
        }

        public void Initialize()
        {
            LocalizationService.SavedDictionaryItem += LocalizationServiceOnSavedDictionaryItem;
            LocalizationService.DeletedDictionaryItem += LocalizationServiceOnDeletedDictionaryItem;

            EnableDictionaryHttpHandler();
        }

        protected virtual void EnableDictionaryHttpHandler()
        {
            RouteTable.Routes.Add(new Route
            (
                _configuration.DictionaryHttpHandlerUrl,
                _dictionaryHttpHandler
            ));
        }

        private void LocalizationServiceOnDeletedDictionaryItem(ILocalizationService sender, DeleteEventArgs<IDictionaryItem> e)
        {
            throw new NotImplementedException();
        }

        private void LocalizationServiceOnSavedDictionaryItem(ILocalizationService sender, SaveEventArgs<IDictionaryItem> e)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}