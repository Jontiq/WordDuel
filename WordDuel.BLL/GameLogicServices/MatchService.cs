using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;

namespace WordDuel.BLL.GameLogicServices;

public class MatchService : IMatchService
{
    private readonly Random _random;

    public MatchService(Random random)
    {
        _random = random;
    }

    public MatchDto CreateMatch(int roundsToWin)
    {
        if (roundsToWin <= 0)
            throw new ArgumentException("RoundsToWin must be greater than 0");

        return new MatchDto
        {
            RoundsToWin = roundsToWin,
            State = MatchState.WaitingForPlayers
        };
    }

    public void AddPlayer(MatchDto match, string? name)
    {
        name = name?.Trim();

        if (match.Players.Count >= 2)
            throw new InvalidOperationException("Max 2 players");

        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"Player {match.Players.Count + 1}";
        }

        match.Players.Add(new PlayerDto
        {
            Name = name,
            Score = 0
        });
    }

    public void StartMatch(MatchDto match)
    {
        if (match.Players.Count != 2)
            throw new InvalidOperationException("Need 2 players");

        match.State = MatchState.InProgress;
        match.CurrentPlayer = match.Players[_random.Next(2)];

        StartNewRound(match);
    }

    public void StartNewRound(MatchDto match)
    {
        if (match.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer must be set before starting a new round.");

        var round = new RoundDto
        {
            RoundNumber = match.Rounds.Count + 1,
            State = RoundState.InProgress,
            StartingPlayer = match.CurrentPlayer
        };

        match.Rounds.Add(round);
        match.CurrentRoundNumber = round.RoundNumber;
    }
}
