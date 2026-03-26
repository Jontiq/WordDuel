using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.BLL.Repositories
{
    public class LocalWordRepository : IWordRepository
    {
        private readonly HashSet<string> _words;

        public LocalWordRepository(IEnumerable<string> words)
        {
            _words = new HashSet<string>(words.Select(w => w.ToUpperInvariant()));
        }

        public Task<bool> ExistsAsync(string word)
        {
            return Task.FromResult(_words.Contains(word.ToUpperInvariant()));
        }
    }
}
