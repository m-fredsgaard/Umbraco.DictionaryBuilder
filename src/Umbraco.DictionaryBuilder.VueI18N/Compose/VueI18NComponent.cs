using System;
using System.Web.Routing;
using Umbraco.DictionaryBuilder.VueI18N.Configuration;
using IComponent = Umbraco.Core.Composing.IComponent;

namespace Umbraco.DictionaryBuilder.VueI18N.Compose
{
    public class VueI18NComponent : IComponent
    {
        private readonly IVueI18NConfiguration _configuration;
        private readonly IDictionaryHttpHandler _dictionaryHttpHandler;

        public VueI18NComponent(IVueI18NConfiguration configuration, IDictionaryHttpHandler dictionaryHttpHandler)
        {
            _configuration = configuration;
            _dictionaryHttpHandler = dictionaryHttpHandler;
        }

        public void Initialize()
        {
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

        public void Terminate()
        { }
    }
}