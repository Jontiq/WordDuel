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

        // ── IsValidWordAsync ──

        [Theory]
        [InlineData("stork")]
        [InlineData("sLApp")]
        [InlineData("StäPP")]
        public async Task IsValidWordAsync_ValidWords_ReturnsTrue(string word)
        {
            Assert.True(await _ws.IsValidWordAsync(word));
        }

        [Theory]
        [InlineData("storken")]
        [InlineData("sLApptt")]
        [InlineData("SkärPP")]
        public async Task IsValidWordAsync_InvalidWords_ReturnsFalse(string word)
        {
            Assert.False(await _ws.IsValidWordAsync(word));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task IsValidWordAsync_EmptyOrWhitespace_ReturnsFalse(string word)
        {
            Assert.False(await _ws.IsValidWordAsync(word));
        }

        // ── GetRandomWordAsync ──

        [Fact]
        public async Task GetRandomWordAsync_Length5_ReturnsWordOfCorrectLength()
        {
            var result = await _ws.GetRandomWordAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result.Length);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(6)]
        public async Task GetRandomWordAsync_UnsupportedLength_ReturnsNull(int length)
        {
            var result = await _ws.GetRandomWordAsync(length);

            Assert.Null(result);
        }

        // ── OneLetterChangedAsync ──

        [Theory]
        [InlineData("hasta", "haspa")]
        [InlineData("halta", "halka")]
        public async Task OneLetterChangedAsync_ExactlyOneDifference_ReturnsTrue(string word, string currentWord)
        {
            Assert.True(await _ws.OneLetterChangedAsync(word, currentWord));
        }

        [Theory]
        [InlineData("hasta", "hasta")]   // Identiska ord → 0 skillnader
        [InlineData("halta", "salva")]   // Flera skillnader
        [InlineData("målar", "snåla")]   // Flera skillnader
        [InlineData("mulan", "mulna")]   // 2 skillnader (pos 3 & 4 swapped)
        public async Task OneLetterChangedAsync_NotExactlyOne_ReturnsFalse(string word, string currentWord)
        {
            Assert.False(await _ws.OneLetterChangedAsync(word, currentWord));
        }

        [Theory]
        [InlineData("halta", "halt")]    // Olika längd
        [InlineData("ab", "abc")]        // Olika längd
        public async Task OneLetterChangedAsync_DifferentLengths_ReturnsFalse(string word, string currentWord)
        {
            Assert.False(await _ws.OneLetterChangedAsync(word, currentWord));
        }

        [Fact]
        public async Task OneLetterChangedAsync_CaseDifference_CountsAsChange()
        {
            // "Halta" vs "halta" — versalen räknas som en skillnad i nuvarande implementation
            var result = await _ws.OneLetterChangedAsync("Halta", "halta");
            Assert.True(result);
        }
    }
}
