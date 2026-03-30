using WordDuel.DAL.Interfaces;
using WordDuel.BLL.WordServices;
using WordDuel.DAL.Repositories;

namespace WordDuel.UnitTests
{
    public class WordTests
    {
        [Theory]
        [InlineData("stork")]
        [InlineData("sLApp")]
        [InlineData("StäPP")]
        public void IsValidWordAsyncTest(string word)
        {
            HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            IWordRepository wordRepository = new WordRepository();
            WordService ws = new WordService(wordRepository);            
            Assert.True(ws.IsValidWordAsync(word)?.Result);
        }

        [Theory]
        [InlineData("storken")]
        [InlineData("sLApptt")]
        [InlineData("SkärPP")]
        public void IsValidWordAsyncWrongTest(string word)
        {
            HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            IWordRepository wordRepository = new WordRepository();
            WordService ws = new WordService(wordRepository);
            Assert.False(ws.IsValidWordAsync(word)?.Result);
        }

        [Theory]
        [InlineData(5)]

        public void GetRandomWordAsyncTest(int length)
        {
            HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            IWordRepository wordRepository = new WordRepository();
            WordService ws = new WordService(wordRepository);
            var result = ws.GetRandomWordAsync(length)?.Result;

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
            //HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            IWordRepository wordRepository = new WordRepository();
            WordService ws = new WordService(wordRepository);
            var result = ws.GetRandomWordAsync(length)?.Result;

            Assert.Null(result);
        }

        [Theory]
        [InlineData("hasta","haspa")]
        [InlineData("halta", "halka")]

        public void OneLetterChangedAsyncTest(string word, string currentWord)
        {
            IWordRepository wordRepository = new WordRepository();
            WordService ws = new WordService(wordRepository);
            var result = ws.OneLetterChangedAsync(word, currentWord)?.Result;
            Assert.True(result);
        }

        [Theory]
        [InlineData("hasta", "hasta")]
        [InlineData("halta", "salva")]
        [InlineData("målar", "snåla")]


        public void OneLetterChangedAsyncFailTest(string word, string currentWord)
        {
            IWordRepository wordRepository = new WordRepository();
            WordService ws = new WordService(wordRepository);
            var result = ws.OneLetterChangedAsync(word, currentWord)?.Result;
            Assert.False(result);
        }

    }
}
