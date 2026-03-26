using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class MoveModel // Ett drag i en runda, alltså "game"?
    {
        public int Id { get; set; } 
        public int RoundId { get; set; } // Id för rundan som draget tillhör
        public int PlayerId { get; set; } // Id för spelaren som gjorde draget
        public string Word { get; set; } = string.Empty; // Ordet som spelaren spelade
        public int MoveNumber { get; set; } // Nummer på draget i rundan

        public RoundModel Round { get; set; } = null!; // Rundan som draget tillhör
        public PlayerModel Player { get; set; } = null!; // Spelaren som gjorde draget
    }
}
