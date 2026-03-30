using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace WordDuel.Shared.DTOs
{
    internal class MatchDto
    {
        public int Id { get; set; }
        public MatchState State { get; set; }
        public int RoundsToWin { get; set; } //how many rounds a player must win to win the game
                                            //ex. for best of 5 RoundsToWin=3
        public int CurrentRoundNumber { get; set; }

        public PlayerDto? CurrentPlayer { get; set; }
        public PlayerDto? Winner { get; set; }

        public List<PlayerDto> Players { get; set; } = new();
        public List<RoundDto> Rounds { get; set; } = new(); 
    }
}
