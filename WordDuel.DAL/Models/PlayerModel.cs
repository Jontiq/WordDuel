using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public string? Name { get; set; }
        public int Score { get; set; }

        public MatchModel Match { get; set; } = null!;
        public ICollection<MoveModel> Moves { get; set; } = new List<MoveModel>();

    }
}
