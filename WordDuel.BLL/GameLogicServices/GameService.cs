using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.GameLogicModels;

namespace WordDuel.BLL.GameLogicServices
{
    public class GameService
    {
        public Game CreateGame(int roundsToWin)
        {
            return new Game
            {
                RoundsToWin = roundsToWin,
                State = Enums.GameState.WaitingForPlayers
            };
        }

    }
}
