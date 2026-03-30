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

        public Task<bool> OneLetterChangedAsync(string word, string currentWord)
        {
            // Kontrollera att orden har samma längd
            if (word.Length != currentWord.Length)
                return Task.FromResult(false);

            // Räkna antal skillnader
            int differences = 0;
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] != currentWord[i])
                {
                    differences++;
                    if (differences > 1) // Om mer än 1 skillnad, returnera direkt
                        return Task.FromResult(false);
                }
            }

            // Exakt 1 skillnad måste finnas
            return Task.FromResult(differences == 1);
        }

    }
}