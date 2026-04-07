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
                .AsNoTracking()
                .AsSplitQuery()
                .Include(m => m.Players)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.Moves)
                        .ThenInclude(mv => mv.Player)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.StartingPlayer)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.WinnerPlayer)
                .Include(m => m.CurrentPlayer)
                .Include(m => m.CurrentRound)
                .Include(m => m.WinnerPlayer)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MatchModel?> GetByRoomCodeAsync(string roomCode) // Hämtar rummet i vilket matchen utspelar sig
        {
            return await _context.Matches
                .AsNoTracking()
                .AsSplitQuery()
                .Include(m => m.Players)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.Moves)
                        .ThenInclude(mv => mv.Player)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.StartingPlayer)
                .Include(m => m.Rounds)
                    .ThenInclude(r => r.WinnerPlayer)
                .Include(m => m.CurrentPlayer)
                .Include(m => m.CurrentRound)
                .Include(m => m.WinnerPlayer)
                .FirstOrDefaultAsync(m => m.RoomCode == roomCode);
        }
        public async Task<MatchModel> CreateAsync(MatchModel match) //Skapar en match i databasen
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }
        public async Task<MatchModel> UpdateAsync(MatchModel match) //Uppdaterar en match i databasen, används för att spara ändringar i matchen, t.ex. när en runda avslutas eller när en spelare gör ett drag
        {
            // Rensa alla trackade entiteter för att undvika konflikter
            // (AsNoTracking vid Load skapar separata instanser för nav-properties)
            _context.ChangeTracker.Clear();

            // Markera nya spelare som Added, befintliga som Modified
            foreach (var player in match.Players)
            {
                _context.Entry(player).State = player.Id == 0
                    ? EntityState.Added
                    : EntityState.Modified;
            }

            // Markera nya rundor och moves som Added
            foreach (var round in match.Rounds)
            {
                _context.Entry(round).State = round.Id == 0
                    ? EntityState.Added
                    : EntityState.Modified;

                foreach (var move in round.Moves)
                {
                    _context.Entry(move).State = move.Id == 0
                        ? EntityState.Added
                        : EntityState.Modified;
                }
            }

            _context.Entry(match).State = EntityState.Modified;
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
