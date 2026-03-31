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

    // Creates a new match and immediately adds the first player.
    // The match starts in WaitingForPlayers because player 2 has not joined yet.
    public MatchDto CreateMatch(int roundsToWin, string? firstPlayerName)
    {
        if (roundsToWin <= 0)
            throw new ArgumentException("RoundsToWin must be greater than 0");

        var match = new MatchDto
        {
            RoundsToWin = roundsToWin,
            State = MatchState.WaitingForPlayers
        };

        match.Players.Add(CreatePlayer(1, firstPlayerName));
        return match;
    }

    // Adds the second player to an existing match.
    // We only allow this while the match is still waiting for players
    // and has fewer than two players.
    public MatchDto JoinMatch(MatchDto match, string? secondPlayerName)
    {
        if (!CanJoinMatch(match))
            throw new InvalidOperationException("Match cannot accept more players.");

        match.Players.Add(CreatePlayer(2, secondPlayerName));
        return match;
    }

    // Starts the match when both players have joined.
    // A random player gets the first turn.
    public void StartMatch(MatchDto match)
    {
        if (!IsMatchReadyToStart(match))
            throw new InvalidOperationException("Match is not ready to start.");

        match.State = MatchState.InProgress;
        match.CurrentPlayer = match.Players[_random.Next(match.Players.Count)];
    }

    // Creates a new round inside the match.
    // The current player becomes the starting player for the round.
    // StartingWord is the first word of the round and CurrentWord begins with the same value.
    public void StartNewRound(MatchDto match, string startingWord)
    {
        if (match.State != MatchState.InProgress)
            throw new InvalidOperationException("Match must be in progress before starting a round.");

        if (match.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer must be set before starting a new round.");

        if (string.IsNullOrWhiteSpace(startingWord))
            throw new ArgumentException("Starting word is required.", nameof(startingWord));

        var normalizedWord = startingWord.Trim().ToLowerInvariant();

        var round = new RoundDto
        {
            RoundNumber = match.Rounds.Count + 1,
            StartingPlayer = match.CurrentPlayer,
            StartingWord = normalizedWord,
            CurrentWord = normalizedWord,
            State = RoundState.InProgress
        };

        round.UsedWords.Add(normalizedWord);

        match.Rounds.Add(round);
        match.CurrentRoundNumber = round.RoundNumber;
    }

    // Switches turn to the other player.
    public void SwitchTurn(MatchDto match)
    {
        if (match.Players.Count != 2)
            throw new InvalidOperationException("Match must have exactly 2 players.");

        if (match.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer is not set.");

        match.CurrentPlayer = match.Players.Single(player => player.Id != match.CurrentPlayer.Id);
    }

    // Ends the current round and awards one point to the winner.
    // If the winner reaches the required number of round wins,
    // the whole match is finished.
    // Otherwise the losing player becomes the next starting player.
    public void EndRound(MatchDto match, int winnerPlayerId)
    {
        if (match.Rounds.Count == 0)
            throw new InvalidOperationException("There is no active round to end.");

        var round = match.Rounds.Last();
        if (round.State == RoundState.Finished)
            throw new InvalidOperationException("The current round is already finished.");

        var winner = match.Players.SingleOrDefault(player => player.Id == winnerPlayerId);
        if (winner == null)
            throw new InvalidOperationException("Winner must be one of the players in the match.");

        var loser = match.Players.Single(player => player.Id != winnerPlayerId);

        round.Winner = winner;
        round.State = RoundState.Finished;

        winner.Score++;

        if (IsMatchFinished(match))
        {
            match.Winner = winner;
            match.State = MatchState.Finished;
            match.CurrentPlayer = null;
            return;
        }
        // The loser starts the next round according to your game rules.
        match.CurrentPlayer = loser;
    }

    // Returns true if the match can accept another player.
    public bool CanJoinMatch(MatchDto match)
    {
        return match.State == MatchState.WaitingForPlayers && match.Players.Count < 2;
    }

    // Returns true when the match has exactly two players and is still waiting to begin.
    public bool IsMatchReadyToStart(MatchDto match)
    {
        return match.State == MatchState.WaitingForPlayers && match.Players.Count == 2;
    }

    // Checks if any player has reached the required number of round wins.
    public bool IsMatchFinished(MatchDto match)
    {
        return match.Players.Any(player => player.Score >= match.RoundsToWin);
    }

    // Helper method for creating players consistently.
    // We assign ids manually here because this logic is currently in-memory
    // and not yet handled by the database.
    private static PlayerDto CreatePlayer(int id, string? name)
    {
        name = name?.Trim();

        return new PlayerDto
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(name) ? $"Player {id}" : name,
            Score = 0
        };
    }
}
