using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class RoundModel
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int RoundNumber { get; set; }
        public string StartingWord { get; set; } = string.Empty;
        public string CurrentWord { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int? WinnerPlayerId { get; set; }
        public int StartingPlayerId { get; set; }

        public MatchModel Match { get; set; } = null!;
        public PlayerModel? WinnerPlayer { get; set; }
        public PlayerModel? StartingPlayer { get; set; }

        public ICollection<MoveModel> Moves { get; set; } = new List<MoveModel>();

    }
}
