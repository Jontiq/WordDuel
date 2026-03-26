using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class RoundModel // En runda i en match, varje match består av flera rundor. En runda består av flera moves. Alltså "set"
    {
        public int Id { get; set; }
        public int MatchId { get; set; } // Id för matchen som rundan tillhör
        public int RoundNumber { get; set; } // Nummer på rundan i matchen
        public string StartingWord { get; set; } = string.Empty; // Startordet för rundan
        public string CurrentWord { get; set; } = string.Empty; // Det nuvarande ordet 
        public string State { get; set; } = string.Empty; // Status 
        public int? WinnerPlayerId { get; set; } // Id för vinnaren av rundan
        public int StartingPlayerId { get; set; } // Id för spelaren som startade rundan

        public MatchModel Match { get; set; } = null!; // Matchen som rundan tillhör
        public PlayerModel? WinnerPlayer { get; set; } // Vinnaren av rundan
        public PlayerModel? StartingPlayer { get; set; } // Spelaren som startade rundan
        public ICollection<MoveModel> Moves { get; set; } = new List<MoveModel>(); // Drag i rundan

    }
}
