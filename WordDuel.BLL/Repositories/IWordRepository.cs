using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.BLL.Repositories
{
    public interface IWordRepository
    {
        Task<bool> ExistsAsync(string word);
    }
}
