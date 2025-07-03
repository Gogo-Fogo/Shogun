using NUnit.Framework;

namespace Shogun.Tests.Characters
{
    public class CharacterTests
    {
        [Test]
        public void ExampleCharacterTest()
        {
            // Arrange
            string name = "Naruto";
            // Act
            int length = name.Length;
            // Assert
            Assert.AreEqual(6, length);
        }
    }
} 