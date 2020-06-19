using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Configuration;

namespace Umbraco.DictionaryBuilder.Compose
{
    public class DictionaryBuilderComposer : ICoreComposer
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
