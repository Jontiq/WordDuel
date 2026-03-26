using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.GameLogicModels;

namespace WordDuel.BLL.GameLogicServices
{
    public class GameService
    {
        // <summary>
        // Creates a new game instance with the required number of wins.
        // We use "RoundsToWin" instead of total rounds because the game follows a "best of" format.
        // This means the match ends as soon as a player reaches the required number of wins,
        // so the total number of rounds is dynamic and does not need to be stored.
        // ex. maxRounds = (RoundsToWin * 2) - 1;
        // </summary>
        public Game CreateGame(int roundsToWin)
        {
            Game game = new Game
            {
                RoundsToWin = roundsToWin,
                State = Enums.GameState.WaitingForPlayers
            };
            if (roundsToWin <= 0)
            {
                throw new ArgumentException("RoundsToWin must be greater than 0");
            }

            return game;
        }

    }
}
