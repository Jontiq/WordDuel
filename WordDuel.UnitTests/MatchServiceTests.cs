using WordDuel.BLL.GameLogicServices;
using WordDuel.BLL.WordServices;
using WordDuel.DAL.Repositories;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;
using WordDuel.UnitTests.Fakes;

namespace WordDuel.UnitTests;

public class MatchServiceTests
{
    private readonly MatchService service;

    public MatchServiceTests()
    {
        var fakeRepo = new FakeWordRepository();// Using fakeRepo to avoid dependency on actual data source,
                                                // making tests faster and more reliable.
        var wordService = new WordService(fakeRepo);
        service = new MatchService(wordService, new Random());
    }

    private MatchDto CreateMatchWithTwoPlayers()
    {
        var match = service.CreateMatch(3, "A");
        service.JoinMatch(match, "B");
        return match;
    }

    [Fact]
    public void CreateMatch_ShouldReturnMatchWithFirstPlayerAndWaitingForPlayersState()
    {
        var result = service.CreateMatch(3, "Anna");

        Assert.NotNull(result);
        Assert.Equal(3, result.RoundsToWin);
        Assert.Equal(MatchState.WaitingForPlayers, result.State);
        Assert.Single(result.Players);
        Assert.Equal("Anna", result.Players[0].Name);
        Assert.Equal(1, result.Players[0].Id);
    }

    [Fact]
    public void CreateMatch_ShouldThrowException_WhenRoundsToWinIsLessThanOrEqualToZero()
    {
        Assert.Throws<ArgumentException>(() => service.CreateMatch(0, "Anna"));
    }

    [Fact]
    public void CreateMatch_ShouldAssignDefaultName_WhenNameIsEmpty()
    {
        var match = service.CreateMatch(3, "");

        Assert.Single(match.Players);
        Assert.Equal("Player 1", match.Players[0].Name);
    }

    [Fact]
    public void JoinMatch_ShouldAddSecondPlayer()
    {
        var match = service.CreateMatch(3, "A");

        service.JoinMatch(match, "B");

        Assert.Equal(2, match.Players.Count);
        Assert.Equal("B", match.Players[1].Name);
        Assert.Equal(2, match.Players[1].Id);
    }

    [Fact]
    public void JoinMatch_ShouldThrowException_WhenMatchAlreadyHasTwoPlayers()
    {
        var match = CreateMatchWithTwoPlayers();

        Assert.Throws<InvalidOperationException>(() => service.JoinMatch(match, "Lisa"));
    }

    [Fact]
    public void CanJoinMatch_ShouldReturnTrue_WhenThereIsRoomForOneMorePlayer()
    {
        var match = service.CreateMatch(3, "A");

        Assert.True(service.CanJoinMatch(match));
    }

    [Fact]
    public void IsMatchReadyToStart_ShouldReturnTrue_WhenTwoPlayersHaveJoined()
    {
        var match = CreateMatchWithTwoPlayers();

        Assert.True(service.IsMatchReadyToStart(match));
    }

    [Fact]
    public void StartMatch_ShouldThrowException_WhenLessThanTwoPlayers()
    {
        var match = service.CreateMatch(3, "Anna");

        Assert.Throws<InvalidOperationException>(() => service.StartMatch(match));
    }

    [Fact]
    public void StartMatch_ShouldSetStateToInProgress()
    {
        var match = CreateMatchWithTwoPlayers();

        service.StartMatch(match);

        Assert.Equal(MatchState.InProgress, match.State);
    }

    [Fact]
    public void StartMatch_ShouldSetCurrentPlayer()
    {
        var match = CreateMatchWithTwoPlayers();

        service.StartMatch(match);

        Assert.NotNull(match.CurrentPlayer);
        Assert.Contains(match.CurrentPlayer, match.Players);
    }

    [Fact]
    public void StartNewRound_ShouldSetCorrectRoundData()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);

        service.StartNewRound(match, "stark");

        Assert.Single(match.Rounds);
        Assert.Equal(1, match.Rounds[0].RoundNumber);
        Assert.Equal("stark", match.Rounds[0].StartingWord);
        Assert.Equal("stark", match.Rounds[0].CurrentWord);
        Assert.Equal(RoundState.InProgress, match.Rounds[0].State);
        Assert.Equal("stark", match.Rounds[0].UsedWords[0]);
    }

    [Fact]
    public void StartNewRound_ShouldThrowException_WhenCurrentPlayerIsNull()
    {
        var match = CreateMatchWithTwoPlayers();

        Assert.Throws<InvalidOperationException>(() => service.StartNewRound(match, "stark"));
    }

    [Fact]
    public void SwitchTurn_ShouldChangeCurrentPlayerToTheOtherPlayer()
    {
        var match = CreateMatchWithTwoPlayers();
        match.CurrentPlayer = match.Players[0];

        service.SwitchTurn(match);

        Assert.Equal(match.Players[1].Id, match.CurrentPlayer?.Id);
    }

    [Fact]
    public void EndRound_ShouldIncreaseWinnerScoreAndFinishRound()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.EndRound(match, match.Players[0].Id);

        Assert.Equal(1, match.Players[0].Score);
        Assert.Equal(RoundState.Finished, match.Rounds[0].State);
        Assert.Equal(match.Players[0].Id, match.Rounds[0].Winner?.Id);
    }

    [Fact]
    public void EndRound_ShouldFinishMatch_WhenWinnerReachesRoundsToWin()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");
        match.Players[0].Score = 2;

        service.EndRound(match, match.Players[0].Id);

        Assert.Equal(MatchState.Finished, match.State);
        Assert.Equal(match.Players[0].Id, match.Winner?.Id);
        Assert.Null(match.CurrentPlayer);
    }

    [Fact]
    public void EndRound_ShouldSetLoserAsNextCurrentPlayer_WhenMatchContinues()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.EndRound(match, match.Players[0].Id);

        Assert.Equal(match.Players[1].Id, match.CurrentPlayer?.Id);
    }

    [Fact]
    public void GiveUpRound_ShouldMakeOtherPlayerWinRound()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.GiveUpRound(match, match.Players[0].Id);

        Assert.Equal(RoundState.Finished, match.Rounds[0].State);
        Assert.Equal(match.Players[1].Id, match.Rounds[0].Winner?.Id);
    }

    [Fact]
    public void GiveUpRound_ShouldIncreaseWinnersScore()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.GiveUpRound(match, match.Players[0].Id);

        Assert.Equal(1, match.Players[1].Score);
    }

    [Fact]
    public void GiveUpRound_ShouldFinishMatch_WhenWinnerReachesRoundsToWin()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        match.Players[1].Score = 2;

        service.GiveUpRound(match, match.Players[0].Id);

        Assert.Equal(MatchState.Finished, match.State);
        Assert.Equal(match.Players[1].Id, match.Winner?.Id);
    }

    [Fact]
    public void GiveUpRound_ShouldSetLoserNextCurrentPlayer_WhenMatchContinues()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.GiveUpRound(match, match.Players[0].Id);

        Assert.Equal(match.Players[0].Id, match.CurrentPlayer?.Id);
    }

    [Fact]
    public void HandleTurnTimeout_ShouldMakeOtherPlayerWinRound()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.HandleTurnTimeout(match, match.Players[0].Id);

        Assert.Equal(RoundState.Finished, match.Rounds[0].State);
        Assert.Equal(match.Players[1].Id, match.Rounds[0].Winner?.Id);
    }

    [Fact]
    public void HandleTurnTimeout_ShouldIncreaseWinnersScore()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.HandleTurnTimeout(match, match.Players[0].Id);

        Assert.Equal(1, match.Players[1].Score);
    }

    [Fact]
    public void HandleTurnTimeout_ShouldFinishMatch_WhenWinnerReachesRoundsToWin()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        match.Players[1].Score = 2;

        service.HandleTurnTimeout(match, match.Players[0].Id);

        Assert.Equal(MatchState.Finished, match.State);
        Assert.Equal(match.Players[1].Id, match.Winner?.Id);
    }

    [Fact]
    public void HandleTurnTimeout_ShouldSetLoserAsNextCurrentPlayer_WhenMatchContinues()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        service.StartNewRound(match, "stark");

        service.HandleTurnTimeout(match, match.Players[0].Id);

        Assert.Equal(match.Players[0].Id, match.CurrentPlayer?.Id);
    }


    [Fact]
    public async Task SubmitMoveAsync_ShouldUpdateCurrentWord_WhenMoveIsValid()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await service.SubmitMoveAsync(match, match.Players[0].Id, "start");

        Assert.Equal("start", match.Rounds[0].CurrentWord);
    }

    [Fact]
    public async Task SubmitMoveAsync_ShouldAddMove_WhenMoveIsValid()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await service.SubmitMoveAsync(match, match.Players[0].Id, "start");

        Assert.Single(match.Rounds[0].Moves);
        Assert.Equal("start", match.Rounds[0].Moves[0].Word);
        Assert.Equal(match.Players[0].Id, match.Rounds[0].Moves[0].Player?.Id);
    }

    [Fact]
    public async Task SubmitMoveAsync_ShouldSwitchTurn_WhenMoveIsValid()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await service.SubmitMoveAsync(match, match.Players[0].Id, "start");

        Assert.Equal(match.Players[1].Id, match.CurrentPlayer?.Id);
    }

    [Fact]
    public async Task SubmitMoveAsync_ShouldThrow_WhenWrongPlayerTriesToPlay()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitMoveAsync(match, match.Players[1].Id, "start"));
    }

    [Fact]
    public async Task SubmitMoveAsync_ShouldThrow_WhenWordIsInvalid()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitMoveAsync(match, match.Players[0].Id, "zzzzz"));
    }

    [Fact]
    public async Task SubmitMoveAsync_ShouldThrow_WhenMoreThanOneLetterIsChanged()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitMoveAsync(match, match.Players[0].Id, "sport"));
    }

    [Fact]
    public async Task SubmitMoveAsync_ShouldThrow_WhenWordHasAlreadyBeenUsed()
    {
        var match = CreateMatchWithTwoPlayers();
        service.StartMatch(match);
        match.CurrentPlayer = match.Players[0];
        service.StartNewRound(match, "stark");

        await service.SubmitMoveAsync(match, match.Players[0].Id, "start");

        match.CurrentPlayer = match.Players[0];

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitMoveAsync(match, match.Players[0].Id, "start"));
    }


}
