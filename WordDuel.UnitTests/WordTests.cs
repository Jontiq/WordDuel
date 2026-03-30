using WordDuel.DAL.Interfaces;
using WordDuel.BLL.WordServices;
using WordDuel.DAL.Repositories;

namespace WordDuel.UnitTests
{
    public class WordTests
    {
        private readonly IWordRepository _wordRepository;
        private readonly WordService _ws;

        public WordTests()
        {
            _wordRepository = new WordRepository();
            _ws = new WordService(_wordRepository);
        }

        [Theory]
        [InlineData("stork")]
        [InlineData("sLApp")]
        [InlineData("StäPP")]
        public void IsValidWordAsyncTest(string word)
        {
            Assert.True(_ws.IsValidWordAsync(word)?.Result);
        }

        [Theory]
        [InlineData("storken")]
        [InlineData("sLApptt")]
        [InlineData("SkärPP")]
        public void IsValidWordAsyncWrongTest(string word)
        {
            Assert.False(_ws.IsValidWordAsync(word)?.Result);
        }

        [Theory]
        [InlineData(5)]

        public void GetRandomWordAsyncTest(int length)
        {
            var result = _ws.GetRandomWordAsync(length)?.Result;

            Assert.NotNull(result);
            Assert.Equal(length, result.Length);
            //Assert.Contains(result, words,StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(3)]

        public void GetRandomWordAsyncFailTest(int length)
        {
            var result = _ws.GetRandomWordAsync(length)?.Result;

            Assert.Null(result);
        }

        [Theory]
        [InlineData("hasta","haspa")]
        [InlineData("halta", "halka")]

        public void OneLetterChangedAsyncTest(string word, string currentWord)
        {
            var result = _ws.OneLetterChangedAsync(word, currentWord)?.Result;
            Assert.True(result);
        }

        [Theory]
        [InlineData("hasta", "hasta")]
        [InlineData("halta", "salva")]
        [InlineData("målar", "snåla")]
        [InlineData("mulan", "mulna")]



        public void OneLetterChangedAsyncFailTest(string word, string currentWord)
        {
            var result = _ws.OneLetterChangedAsync(word, currentWord)?.Result;
            Assert.False(result);
        }

    }
}
