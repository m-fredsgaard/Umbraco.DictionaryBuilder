using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.DictionaryBuilder.Compose
{
    public class DictionaryBuilderComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<DictionaryComponent>();
        }
    }
}
