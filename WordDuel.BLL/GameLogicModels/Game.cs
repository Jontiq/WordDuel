using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.Enums;

namespace WordDuel.BLL.GameLogicModels
{
    public class Game
    {
        public List<Player> Players { get; set; } = new();
        public List<Round> Rounds { get; set; } =new();

        public int CurrentRoundNumber { get; set; }
        public GameState State { get; set; }
        
        public Player? CurrentPlayer { get; set; }
        public Player? Winner { get; set; }
        public int RoundsToWin { get; set; } //how many rounds a player must win to win the game
                                             //ex. for best of 5 RoundsToWin=3
        
    }
}
