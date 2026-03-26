using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class PlayerModel //spelaren
    {
        public int Id { get; set; } 
        public int MatchId { get; set; } // Id för matchen som spelaren är med i
        public string? Name { get; set; } // Spelarens namn? ska vi ha?
        public int Score { get; set; } // Spelarens poäng

        //Nav prop
        public MatchModel Match { get; set; } = null!; // Matchen som spelaren är med i

        public ICollection<MoveModel> Moves { get; set; } = new List<MoveModel>(); // Drag som spelaren har gjort

    }
}
