using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class MatchModel
    {
        public int Id { get; set; }

        public string State { get; set; } = string.Empty;
        public int BestOf { get; set; }

        public int CurrentRoundId { get; set; }
        public int CurrentPlayerId { get; set; }
        public int? WinnerPlayerId { get; set; }
        public int TurnTimeSeconds { get; set; }

        public RoundModel CurrentRound { get; set; }
        public PlayerModel CurrentPlayer { get; set; }
        public PlayerModel WinnerPlayer { get; set; }

        public ICollection<PlayerModel> Players { get; set; } = new List<PlayerModel>();
        public ICollection<RoundModel> Rounds { get; set; } = new List<RoundModel>();
    }
}
