using WordDuel.BLL.Repositories;
using WordDuel.BLL.WordServices;

namespace WordDuel.UnitTests
{
    public class WordTests
    {
        [Theory]
        [InlineData("stork")]
        [InlineData("sLApp")]
        [InlineData("StäPP")]
        public void WordServiceTest(string word)
        {
            HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            IWordRepository wordRepository = new LocalWordRepository(words);
            WordService ws = new WordService(wordRepository);            
            Assert.True(ws.IsValidWordAsync(word)?.Result);
        }

        [Theory]
        [InlineData("stark")]
        [InlineData("sLApt")]
        [InlineData("SkäPP")]
        public void WordServiceWrongTest(string word)
        {
            HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            IWordRepository wordRepository = new LocalWordRepository(words);
            WordService ws = new WordService(wordRepository);
            Assert.False(ws.IsValidWordAsync(word)?.Result);
        }

    }
}
