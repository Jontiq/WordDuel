using WordDuel.BLL.Services;

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
            WordService ws = new WordService(words);            
            Assert.True(ws.IsValidWord(word));
        }

        [Theory]
        [InlineData("stark")]
        [InlineData("sLApt")]
        [InlineData("SkäPP")]
        public void WordServiceWrongTest(string word)
        {
            HashSet<string> words = new HashSet<string>() { "STORK", "STÄPP", "SLApp" };
            WordService ws = new WordService(words);
            Assert.False(ws.IsValidWord(word));
        }

    }
}
