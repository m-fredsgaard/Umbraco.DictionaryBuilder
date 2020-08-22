using System;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Umbraco.Core.Models;
using Umbraco.DictionaryBuilder.Factories;

namespace Umbraco.DictionaryBuilder.Tests.Factories
{
    [TestFixture]
    public class DictionaryFactoryTests
    {
        [Test]
        public void Languages_LocalizationServiceThrowsException_ErrorLoggedReturnEmpty()
        {
            // Arrange
            Mock<ILogger> logger = new Mock<ILogger>();

            TestFactory.LocalizationService
                .Setup(x => x.GetAllLanguages())
                .Throws<Exception>();

            Log.Logger = logger.Object;

            DictionaryFactoryImpl subject = new DictionaryFactoryImpl();

            // Act
            ILanguage[] result = subject.Languages;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(0));
            logger.Verify(x => x.Write(
                It.Is<LogEventLevel>(level => level == LogEventLevel.Error),
                It.IsAny<Exception>(), 
                It.IsAny<string>()
                ));
        }

        [Test]
        public void Create_LocalizationServiceThrowsException_ThrowException()
        {
            // Arrange
            Mock<ILogger> logger = new Mock<ILogger>();

            TestFactory.LocalizationService
                .Setup(x => x.GetDictionaryItemByKey(It.IsAny<string>()))
                .Throws<Exception>();
            TestFactory.LocalizationService
                .Setup(x => x.CreateDictionaryItemWithIdentity(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
                .Throws<Exception>();
            TestFactory.LocalizationService
                .Setup(x => x.AddOrUpdateDictionaryValue(It.IsAny<IDictionaryItem>(), It.IsAny<Language>(), It.IsAny<string>()))
                .Throws<Exception>();
            TestFactory.LocalizationService
                .Setup(x => x.Save(It.IsAny<IDictionaryItem>(), It.IsAny<int>()))
                .Throws<Exception>();
            
            Log.Logger = logger.Object;

            DictionaryFactoryImpl subject = new DictionaryFactoryImpl();

            // Assert
            Assert.Throws<Exception>(() =>
            {
                // Act
                subject.Create(It.IsAny<string>());
            });
        }

        private class DictionaryFactoryImpl : DictionaryFactory
        {
            public override void EnsureDictionaries()
            {
                throw new NotImplementedException();
            }

            public new void Create(string itemKey)
            {
                base.Create(itemKey);
            }
        }
    }
}