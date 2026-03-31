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

        public async Task<MatchModel?> GetByIdAsync(int id)
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
    }
}
