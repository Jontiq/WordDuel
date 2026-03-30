using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.DAL.Interfaces;

namespace WordDuel.BLL.WordServices
{
    //En service som använder ett repository, vilket möjliggör en enklare implementation av db, eller API lösning senare.
    public class WordService : IWordService
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

        public async Task<string?> GetRandomWordAsync(int length)
        {
            // hämta alla ord med rätt längd (bra om vi introducerar möjligheten att välja längden på orden)
            var allWords = await _repo.GetAllWordsAsync();
            var candidates = allWords
                .Where(w => w.Length == length)
                .ToList();

            if (candidates.Count == 0)
                return null;

            //Välj ett slumpvis ord från listan
            var random = new Random();
            return candidates[random.Next(candidates.Count)];

        }
    }
}