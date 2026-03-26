using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.BLL.WordServices
{
    public interface IWordService
    {
        public bool IsValidWordAsync(string word);

    }
}
