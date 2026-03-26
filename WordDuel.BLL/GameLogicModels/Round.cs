using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.Enums;

namespace WordDuel.BLL.GameLogicModels
{
    public class Round
    {
        public int RoundNumber { get; set; }
        public string CurrentWord { get; set; } = string.Empty;
        public List<string> UsedWords { get; set; } = new();
        public Player? StartingPlayer { get; set; }
        public Player? Winner {  get; set; }
        public RoundState State { get; set; }
    }
}
