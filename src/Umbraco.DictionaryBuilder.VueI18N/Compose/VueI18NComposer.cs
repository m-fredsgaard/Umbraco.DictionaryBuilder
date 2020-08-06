using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Compose;
using Umbraco.DictionaryBuilder.Configuration;

namespace Umbraco.DictionaryBuilder.VueI18N.Compose
{
    public class VueI18NComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Configs.Add<IDictionaryBuilderConfiguration>(() => new DictionaryBuilderConfiguration());
            composition.RegisterAuto<ILocalizationService>();
            composition.Register<IDictionaryHttpHandler, DictionaryHttpHandler>();

            composition.Components().Append<DictionaryComponent>();
        }
    }
}
