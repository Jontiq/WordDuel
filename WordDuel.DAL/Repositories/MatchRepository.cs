using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.DAL.Interfaces;
using WordDuel.DAL.Models;

namespace WordDuel.DAL.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        public Task<MatchModel> CreateAsync(MatchModel match)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MatchModel>> GetActiveMatchesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MatchModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MatchModel?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<MatchModel> UpdateAsync(MatchModel match)
        {
            throw new NotImplementedException();
        }
    }
}
