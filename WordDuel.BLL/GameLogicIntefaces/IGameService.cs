using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.GameLogicModels;

namespace WordDuel.BLL.GameLogicIntefaces
{
    public interface IGameService
    {
        Game CreateGame(int roundsToWin);
        void AddPlayer(Game game, string? name);
        void StartGame(Game game);
        void StartNewRound(Game game);
    }
}
