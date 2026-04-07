using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class MatchModel // En match består av flera rundor och spelare, alltså ett helt spel
    {
        public int Id { get; set; }

        public string RoomCode { get; set; } = string.Empty; // I vilket spelet sker
        public string State { get; set; } = string.Empty; // status
        public int BestOf { get; set; } // Bäst av 3 tex

        public int? CurrentRoundId { get; set; } // Id för den pågående rundan
        public int? CurrentPlayerId { get; set; } // Id för spelaren som har sin tur
        public int? WinnerPlayerId { get; set; } // Id för vinnaren av matchen
        public int TurnTimeSeconds { get; set; } // Tid i sekunder för varje spelares tur

        //Nav props
        public RoundModel? CurrentRound { get; set; } // Den pågående rundan
        public PlayerModel? CurrentPlayer { get; set; } // Spelaren som har sin tur
        public PlayerModel? WinnerPlayer { get; set; } // Vinnaren av matchen

        public ICollection<PlayerModel> Players { get; set; } = new List<PlayerModel>(); // Players i matchen
        public ICollection<RoundModel> Rounds { get; set; } = new List<RoundModel>(); // Rundor i matchen
    }
}
