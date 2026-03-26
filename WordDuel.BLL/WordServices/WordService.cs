using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.Repositories;

namespace WordDuel.BLL.WordServices
{
    //En service som använder ett repository, vilket möjliggör en enklare implementation av db, eller API lösning senare.
    public class WordService
    {
        private readonly IWordRepository _repo;

        public WordService(IWordRepository repo)
        {
            _repo = repo;
        }

        public Task<bool> IsValidWordAsync(string word)
        {
            return _repo.ExistsAsync(word);
        }
    }
}
