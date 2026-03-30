using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> IsValidWordAsync (string word);

        Task<bool> ExistsAsync(string word);
        Task<IEnumerable<string>> GetAllWordsAsync();
    }
}
