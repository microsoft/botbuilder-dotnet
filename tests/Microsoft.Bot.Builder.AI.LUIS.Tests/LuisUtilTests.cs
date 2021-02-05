using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Xunit;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    public class LuisUtilTests
    {
        private const string _ENTITY = "testEntity";

        [Fact]
        public void GetEntityTextShouldReturnTheEntityItself()
        {
            //Arrange
            var mockEntity = new EntityModel { Entity = _ENTITY, StartIndex = 0, EndIndex = _ENTITY.Length };

            //Act
            var actualEntityText = LuisUtil.GetEntityText(mockEntity, _ENTITY, mockEntity.StartIndex, mockEntity.EndIndex + 1);

            //Assert
            Assert.Equal(_ENTITY, actualEntityText);
        }

        [Fact]
        public void GetEntityTextShouldReturnUtterance()
        {
            //Arrange
            const string UTTERANCE = "entity";
            var mockEntity = new EntityModel { Entity = _ENTITY, StartIndex = 0, EndIndex = _ENTITY.Length };

            //Act
            var actualEntityText = LuisUtil.GetEntityText(mockEntity, UTTERANCE, mockEntity.StartIndex, mockEntity.EndIndex + 1);

            //Assert
            Assert.Equal(UTTERANCE, actualEntityText);
        }

        [Theory]
        [InlineData("testEntity with utterance", 0, 9)]
        [InlineData("utterance with testEntity", 15, 24)]
        [InlineData("utterance, testEntity and other things", 12, 21)]
        public void GetEntityTextShouldReturnUtteranceSubstring(string utterance, int start, int end)
        {
            //Arrange
            var mockEntity = new EntityModel { Entity = _ENTITY, StartIndex = start, EndIndex = end };

            //Act
            var actualEntityText = LuisUtil.GetEntityText(mockEntity, utterance, mockEntity.StartIndex, mockEntity.EndIndex + 1);

            //Assert
            Assert.Equal(_ENTITY, actualEntityText);
        }

        [Theory]
        [InlineData("utterance with entity", 15, 20)]
        [InlineData("entity with utterance", 0, 5)]
        [InlineData("utterance, entity and other things", 11, 16)]
        public void GetEntityTextShouldReturnUtteranceSubstringWithEntityTextSmallerThanTheEntity(string utterance, int start, int end)
        {
            //Arrange
            const string SMALL_ENTITY = "entity";
            var mockEntity = new EntityModel { Entity = _ENTITY, StartIndex = start, EndIndex = end };

            //Act
            var actualEntityText = LuisUtil.GetEntityText(mockEntity, utterance, mockEntity.StartIndex, mockEntity.EndIndex + 1);

            //Assert
            Assert.Equal(SMALL_ENTITY, actualEntityText);
        }
    }
}
