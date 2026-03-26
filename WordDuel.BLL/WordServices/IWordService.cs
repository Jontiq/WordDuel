using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.BLL.Services
{
    public interface IWordService
    {
        public bool IsValidWordAsync(string word);

    }
}
