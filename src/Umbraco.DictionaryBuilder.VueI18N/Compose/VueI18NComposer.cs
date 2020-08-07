using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Services;
using Umbraco.DictionaryBuilder.VueI18N.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N.Compose
{
    public class VueI18NComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Configs.Add<IVueI18NConfiguration>(() => new VueI18NConfiguration());
            composition.RegisterAuto<ILocalizationService>();
            composition.RegisterAuto<ICultureDictionary>();
            composition.Register<IUmbracoService, UmbracoService>();
            composition.Register<IDictionaryHttpHandler, DictionaryHttpHandler>();

            composition.Components().Append<VueI18NComponent>();
        }
    }
}
