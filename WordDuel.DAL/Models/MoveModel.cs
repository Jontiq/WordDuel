using System;
using System.Collections.Generic;
using System.Text;

namespace WordDuel.DAL.Models
{
    public class MoveModel
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public int PlayerId { get; set; }
        public string Word { get; set; } = string.Empty;
        public int MoveNumber { get; set; }

        public RoundModel Round { get; set; } = null!;
        public PlayerModel Player { get; set; } = null!;
    }
}
