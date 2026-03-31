using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.DAL.Models;

namespace WordDuel.DAL.Interfaces
{
    public interface IMatchRepository
    {
        Task<MatchModel?> GetByIdAsync(int id); // hämtar en match baserat på id
        Task<MatchModel> CreateAsync(MatchModel match); // skapar en ny match
        Task<MatchModel> UpdateAsync(MatchModel match); // uppdaterar en match
        Task<bool> DeleteAsync(int id); // tar bort en match baserat på id
        Task<IEnumerable<MatchModel>> GetAllAsync(); // hämtar alla matcher
        Task<IEnumerable<MatchModel>> GetActiveMatchesAsync(); // hämtar alla aktiva matcher (t.ex. matcher som inte är avslutade)
    }
}
