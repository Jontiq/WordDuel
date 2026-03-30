using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.DAL.Interfaces;

namespace WordDuel.DAL.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly HashSet<string> _words;

        public WordRepository()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "swe_wordlist_5.txt");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Ordlistan hittades inte: {path}");
            }

            _words = File.ReadAllLines(path)
                .Select(w => w.Trim().ToLower())
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToHashSet();
        }
        public Task<bool> IsValidWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return Task.FromResult(false);

            var exists = _words.Contains(word.Trim().ToLower());
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return Task.FromResult(false);

            var exists = _words.Contains(word.Trim().ToLower());
            return Task.FromResult(exists);
        }

        public Task<IEnumerable<string>> GetAllWordsAsync()
        {
            return Task.FromResult<IEnumerable<string>>(_words);
        }
        
    }
    
}
