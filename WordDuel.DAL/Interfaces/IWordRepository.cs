using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> IsValidWordAsync (string word);
    }
}
