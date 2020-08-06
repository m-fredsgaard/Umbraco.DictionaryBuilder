using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Building;
using Umbraco.DictionaryBuilder.Configuration;
using Umbraco.DictionaryBuilder.Services;
using Umbraco.Web.Dictionary;

namespace Umbraco.DictionaryBuilder.Compose
{
    public class DictionaryBuilderComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Configs.Add<IDictionaryBuilderConfiguration>(() => new DictionaryBuilderConfiguration());
            composition.RegisterAuto<ILocalizationService>();
            composition.Register<ICultureDictionary, DefaultCultureDictionary>();
            composition.Register<IUmbracoService, UmbracoService>();
            composition.RegisterUnique<ModelsGenerator>();
            composition.Components().Append<DictionaryComponent>();
        }
    }
}
