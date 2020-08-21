using System;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Umbraco.Core.Dictionary;
using Umbraco.DictionaryBuilder.Models;

namespace Umbraco.DictionaryBuilder.Tests.Models
{
    [TestFixture]
    public class DictionaryModelTests
    {
        [Test]
        public void ToString_ICultureDictionaryFactoryThrowsException_ReturnNull()
        {
            // Arrange
            Mock<ILogger> logger = new Mock<ILogger>();
            TestFactory.CultureDictionaryFactory
                .Setup(x => x.CreateDictionary())
                .Throws<Exception>();

            Log.Logger = logger.Object;

            DictionaryModel subject = new DictionaryModelWrapper(It.IsAny<Guid>(), It.IsAny<string>());
            
            // Act
            string result = subject.ToString(It.IsAny<CultureInfo>());

            // Assert
            Assert.That(result, Is.Null);
            logger.Verify(x => x.Write(
                It.Is<LogEventLevel>(level => level == LogEventLevel.Error),
                It.IsAny<Exception>(), 
                It.IsAny<string>()
            ));
        }

        [Test]
        public void ToString_ICultureDictionaryThrowsException_ReturnNull()
        {
            // Arrange
            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<ICultureDictionary> cultureDictionary = new Mock<ICultureDictionary>();

            cultureDictionary.SetupGet(x => x[It.IsAny<string>()]).Throws<Exception>();

            TestFactory.CultureDictionaryFactory
                .Setup(x => x.CreateDictionary())
                .Returns(cultureDictionary.Object);

            Log.Logger = logger.Object;

            DictionaryModel subject = new DictionaryModelWrapper(It.IsAny<Guid>(), It.IsAny<string>());
            
            // Act
            string result = subject.ToString(It.IsAny<CultureInfo>());

            // Assert
            Assert.That(result, Is.Null);
            logger.Verify(x => x.Write(
                It.Is<LogEventLevel>(level => level == LogEventLevel.Error),
                It.IsAny<Exception>(), 
                It.IsAny<string>()
            ));
        }
    }
}