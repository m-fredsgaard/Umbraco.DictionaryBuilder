using System;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Models;
using Umbraco.DictionaryBuilder.Services;

namespace Umbraco.DictionaryBuilder.Tests.Services
{
    [TestFixture]
    public class UmbracoServiceTests
    {
        [Test]
        public void GetDictionaryModels_LocalizationServiceThrowsException_ErrorLoggedReturnNull()
        {
            // Arrange
            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<ILocalizationService> localizationService = new Mock<ILocalizationService>();
            
            localizationService
                .Setup(x => x.GetDictionaryItemDescendants(It.IsAny<Guid?>()))
                .Throws<Exception>();

            Log.Logger = logger.Object;

            UmbracoService subject = new UmbracoService(localizationService.Object);

            // Act
            DictionaryModel[] result = subject.GetDictionaryModels();

            // Assert
            Assert.That(result, Is.Null);
            logger.Verify(x => x.Write(
                It.Is<LogEventLevel>(level => level == LogEventLevel.Warning),
                It.IsAny<Exception>(), 
                It.IsAny<string>()
            ));
        }
    }
}