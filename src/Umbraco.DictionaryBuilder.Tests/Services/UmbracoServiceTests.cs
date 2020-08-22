using System;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Umbraco.Core.Services;
using Umbraco.DictionaryBuilder.Services;

namespace Umbraco.DictionaryBuilder.Tests.Services
{
    [TestFixture]
    public class UmbracoServiceTests
    {
        [Test]
        public void GetDictionaryModels_LocalizationServiceThrowsException_ErrorLoggedThrowException()
        {
            // Arrange
            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<ILocalizationService> localizationService = new Mock<ILocalizationService>();
            
            localizationService
                .Setup(x => x.GetDictionaryItemDescendants(It.IsAny<Guid?>()))
                .Throws<Exception>();

            Log.Logger = logger.Object;

            UmbracoService subject = new UmbracoService(localizationService.Object);

            // Assert
            Assert.Throws<Exception>(() =>
            {
                // Act
                subject.GetDictionaryModels();
            });
            
            logger.Verify(x => x.Write(
                It.Is<LogEventLevel>(level => level == LogEventLevel.Error),
                It.IsAny<Exception>(), 
                It.IsAny<string>()
            ));
        }
    }
}