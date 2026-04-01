using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.DAL.Data;
using WordDuel.DAL.Interfaces;
using WordDuel.DAL.Models;

namespace WordDuel.DAL.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly AppDbContext _context;

        public MatchRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MatchModel?> GetByIdAsync(int id) //Hämtar en match med spelare, rundor, nuvarande spelare, vinnare baserat på match-id
        {
            return await _context.Matches
                .Include(m => m.Players)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.Moves)
                .Include(m => m.CurrentPlayer)
                .Include(m => m.CurrentRound)
                .Include(m => m.WinnerPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<MatchModel> CreateAsync(MatchModel match) //Skapar en match i databasen
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }
        public async Task<MatchModel> UpdateAsync(MatchModel match) //Uppdaterar en match i databasen, används för att spara ändringar i matchen, t.ex. när en runda avslutas eller när en spelare gör ett drag
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();
            return match;
        }
        public async Task<bool> DeleteAsync(int id) //Tar bort en match från databasen baserat på match-id, används för att ta bort matcher som inte längre behövs, t.ex. avslutade matcher eller matcher som avbrutits
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
                return false;

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<MatchModel>> GetAllAsync() //Hämtar alla matcher med spelare, rundor, nuvarande spelare, vinnare från databasen, används för att visa en lista över alla matcher i adminpanelen eller för att visa statistik över matcher
        {
            return await _context.Matches
                .Include(m => m.Players)
                .Include(m => m.CurrentPlayer)
                .Include(m => m.Rounds)
                .ToListAsync();
        }

        public async Task<IEnumerable<MatchModel>> GetActiveMatchesAsync() //Hämtar alla aktiva matcher (matcher som inte är avslutade) med spelare, nuvarande spelare från databasen, används för att visa en lista över pågående matcher i lobby eller för att visa statistik över aktiva matcher
        {
            return await _context.Matches
                .Include(m => m.Players)
                .Include(m => m.CurrentPlayer)
                .Where(m => m.State == "InProgress" || m.State == "WaitingForPlayers")
                .ToListAsync();
        }

    }
}
