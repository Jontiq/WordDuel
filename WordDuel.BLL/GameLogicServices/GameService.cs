using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.BLL.Enums;
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

        public void AddPlayer (Game game, string? name)
        {
            name = name?.Trim();
            if (game.Players.Count >= 2) //check if we allready have 2 playears
                throw new InvalidOperationException("Max 2 players");
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Player {game.Players.Count + 1}";
            }

            game.Players.Add(new Player { Name = name, Score = 0 });
        }

        public void StartNewRound(Game game)
        {
            var round = new Round
            {
                RoundNumber = game.Rounds.Count + 1,
                State = RoundState.InProgress,
                StartingPlayer = game.CurrentPlayer
            };

            game.Rounds.Add(round);
            game.CurrentRoundNumber = round.RoundNumber;
        }

        public void StartGame(Game game)
        {
            if (game.Players.Count != 2)
                throw new InvalidOperationException("Need 2 players");

            game.State = GameState.InProgress;

            game.CurrentPlayer = game.Players[new Random().Next(2)];

            StartNewRound(game);
        }
    }
}
