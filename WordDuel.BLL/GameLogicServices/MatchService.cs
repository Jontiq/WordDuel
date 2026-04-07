using System.Text;
using System.Text.RegularExpressions;
using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.BLL.WordServices;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;

namespace WordDuel.BLL.GameLogicServices;

public class MatchService : IMatchService
{
    private readonly IWordService _wordService;
    private readonly Random _random;

    public MatchService(IWordService wordService, Random random)
    {
        _wordService = wordService;
        _random = random;
    }


    // Creates a new match and immediately adds the first player.
    // The match starts in WaitingForPlayers because player 2 has not joined yet.
    public MatchDto CreateMatch(int roundsToWin,  int secondsPerRound, string? firstPlayerName)
    {
        if (roundsToWin <= 0)
            throw new ArgumentException("RoundsToWin must be greater than 0");

        var match = new MatchDto
        {
            RoundsToWin = roundsToWin,
            TurnTimeSeconds = secondsPerRound,
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
    public async Task StartNewRoundAsync(MatchDto match, string startingWord)
    {
        if (match.State != MatchState.InProgress)
            throw new InvalidOperationException("Match must be in progress before starting a round.");

        if (match.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer must be set before starting a new round.");

        // ? VALIDERA ATT ORDET ÄR GILTIGT
        if (string.IsNullOrWhiteSpace(startingWord))
            throw new ArgumentException("Starting word is required.", nameof(startingWord));

        var normalizedWord = startingWord.Trim().ToLowerInvariant();

        // ? VALIDERA ATT ORDET FINNS I ORDLISTAN
        if (!await _wordService.IsValidWordAsync(normalizedWord))
            throw new InvalidOperationException("Starting word is not valid.");

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

        Console.WriteLine($"? Round {round.RoundNumber} started with word: {normalizedWord}");
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
        // The loser starts the next round according to the game rules.
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


    public async Task SubmitMoveAsync(MatchDto match, int playerId, string word)
    {
        //If the match is not in progress, the move should not be allowed.
        if (match.State != MatchState.InProgress)
            throw new InvalidOperationException("Match is not in progress.");
        //A player cannot make a move if no round has started.
        if (match.Rounds.Count == 0)
            throw new InvalidOperationException("No active round.");
        //The last round is the active one.
        var round = match.Rounds.Last();
        //If the round is already finished, reject the move.
        if (round.State != RoundState.InProgress)
            throw new InvalidOperationException("Round is not active.");
        //Only CurrentPlayer is allowed to submit a move.
        if (match.CurrentPlayer == null || match.CurrentPlayer.Id != playerId)
            throw new InvalidOperationException("It is not this player's turn.");
        //Word must be provided and cannot be just whitespace.
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Word is required.", nameof(word));

        var normalizedWord = word.Trim().ToLowerInvariant();

        if (normalizedWord.Length != round.CurrentWord.Length)
            throw new InvalidOperationException("Word must have the same length as the current word.");

        if (!await _wordService.IsValidWordAsync(normalizedWord))
            throw new InvalidOperationException("Word is not valid.");

        if (!await _wordService.OneLetterChangedAsync(normalizedWord, round.CurrentWord))
            throw new InvalidOperationException("Exactly one letter must be changed.");

        if (round.UsedWords.Contains(normalizedWord))
            throw new InvalidOperationException("Word has already been used in this round.");
        // If all checks pass, we create a new move and update the round state accordingly.
        var move = new MoveDto
        {
            MoveNumber = round.Moves.Count + 1,
            Word = normalizedWord,
            Player = match.CurrentPlayer
        };

        round.Moves.Add(move);
        round.UsedWords.Add(normalizedWord);
        round.CurrentWord = normalizedWord;//This becomes the new active word for the next player.

        SwitchTurn(match); //After a valid move, next player gets the turn.
    }

    //If a player gives up:
    //that player loses the round
    //the other player wins
    //call EndRound(match, winnerId)
    public void GiveUpRound(MatchDto match, int playerId)
    {
        var winner = match.Players.Single(p => p.Id != playerId);
        EndRound(match, winner.Id);
    }

    //f a player’s time runs out:
    //that player loses the round
    //the other player wins
    //call EndRound(match, winnerId)
    public void HandleTurnTimeout(MatchDto match, int playerId)
    {
        var winner = match.Players.Single(p => p.Id != playerId);
        EndRound(match, winner.Id);
    }

}
