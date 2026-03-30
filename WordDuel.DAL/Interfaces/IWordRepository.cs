using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> ExistsAsync(string word);
        Task<IEnumerable<string>> GetAllWordsAsync();
    }
}
