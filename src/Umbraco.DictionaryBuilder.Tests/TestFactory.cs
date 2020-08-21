using Moq;
using NUnit.Framework;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Services;

namespace Umbraco.DictionaryBuilder.Tests
{
    [SetUpFixture]
    public class TestFactory
    {
        internal static Mock<ILocalizationService> LocalizationService { get; private set; }
        internal static Mock<ICultureDictionaryFactory> CultureDictionaryFactory { get; private set; }

        [OneTimeSetUp]
        public void Setup()
        {
            Mock<Core.Composing.IFactory> factory = new Mock<Core.Composing.IFactory>();
            LocalizationService = new Mock<ILocalizationService>();
            CultureDictionaryFactory = new Mock<ICultureDictionaryFactory>();

            factory
                .Setup(x => x.GetInstance(typeof(ServiceContext)))
                .Returns(ServiceContext.CreatePartial(localizationService: LocalizationService.Object));

            factory
                .Setup(x => x.GetInstance(typeof(ICultureDictionaryFactory)))
                .Returns(CultureDictionaryFactory.Object);

            Core.Composing.Current.Factory = factory.Object;
        }
    }
}