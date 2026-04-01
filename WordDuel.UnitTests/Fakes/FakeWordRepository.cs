using WordDuel.DAL.Interfaces;

namespace WordDuel.UnitTests.Fakes;

public class FakeWordRepository : IWordRepository
{
    private readonly HashSet<string> _words = new(StringComparer.OrdinalIgnoreCase)
    {
        "stark",
        "start",
        "stort",
        "sport",
        "spark",
        "stork"
    };

    public Task<bool> IsValidWordAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult(false);

        return Task.FromResult(_words.Contains(word.Trim()));
    }

    public Task<bool> ExistsAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return Task.FromResult(false);

        return Task.FromResult(_words.Contains(word.Trim()));
    }

    public Task<IEnumerable<string>> GetAllWordsAsync()
    {
        return Task.FromResult<IEnumerable<string>>(_words);
    }
}
