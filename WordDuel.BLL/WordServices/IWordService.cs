using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.BLL.WordServices
{
    public interface IWordService
    {
        Task<bool> IsValidWordAsync(string word);
        Task<string?> GetRandomWordAsync(int length);
        Task<bool> OneLetterChangedAsync(string word, string currentWord);
    }
}
