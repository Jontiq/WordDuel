using WordDuel.BLL.GameLogicServices;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;

namespace WordDuel.UnitTests;

public class MatchServiceTests
{
    private readonly MatchService service;

    public MatchServiceTests()
    {
        service = new MatchService(new Random());
    }

    private MatchDto CreateMatchWithTwoPlayers()
    {
        var match = service.CreateMatch(3);
        service.AddPlayer(match, "A");
        service.AddPlayer(match, "B");
        return match;
    }

    [Fact]
    public void CreateMatch_ShouldReturnMatchWithCorrectRoundsToWinAndWaitingForPlayersState()
    {
        var result = service.CreateMatch(3);

        Assert.NotNull(result);
        Assert.Equal(3, result.RoundsToWin);
        Assert.Equal(MatchState.WaitingForPlayers, result.State);
    }

    [Fact]
    public void CreateMatch_ShouldThrowException_WhenRoundsToWinIsLessThanOrEqualToZero()
    {
        Assert.Throws<ArgumentException>(() => service.CreateMatch(0));
    }

    [Fact]
    public void AddPlayer_ShouldAddPlayerToMatch()
    {
        var match = service.CreateMatch(3);

        service.AddPlayer(match, "A");

        Assert.Single(match.Players);
        Assert.Equal("A", match.Players[0].Name);
    }

    [Fact]
    public void AddPlayer_ShouldAddTwoPlayersToMatch()
    {
        var match = CreateMatchWithTwoPlayers();

        Assert.Equal(2, match.Players.Count);
        Assert.Equal("A", match.Players[0].Name);
        Assert.Equal("B", match.Players[1].Name);
    }

    [Fact]
    public void AddPlayer_ShouldThrowException_WhenAddingThreePlayers()
    {
        var match = CreateMatchWithTwoPlayers();

        Assert.Throws<InvalidOperationException>(() => service.AddPlayer(match, "Lisa"));
    }

    [Fact]
    public void AddPlayer_ShouldAssignDefaultName_WhenNameIsEmpty()
    {
        var match = service.CreateMatch(3);

        service.AddPlayer(match, "");
        service.AddPlayer(match, null);

        Assert.Equal("Player 1", match.Players[0].Name);
        Assert.Equal("Player 2", match.Players[1].Name);
    }

    [Fact]
    public void AddPlayer_ShouldTrimName_WhenNameContainsSpaces()
    {
        var match = service.CreateMatch(3);

        service.AddPlayer(match, "   Anna   ");

        Assert.Single(match.Players);
        Assert.Equal("Anna", match.Players[0].Name);
    }

    [Fact]
    public void StartMatch_ShouldThrowException_WhenLessThanTwoPlayers()
    {
        var match = service.CreateMatch(3);
        service.AddPlayer(match, "Anna");

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
    public void StartMatch_ShouldCreateFirstRound()
    {
        var match = CreateMatchWithTwoPlayers();

        service.StartMatch(match);

        Assert.Single(match.Rounds);
        Assert.Equal(1, match.CurrentRoundNumber);
    }

    [Fact]
    public void StartNewRound_ShouldSetCorrectRoundNumber()
    {
        var match = CreateMatchWithTwoPlayers();
        match.CurrentPlayer = match.Players[0];

        service.StartNewRound(match);

        Assert.Equal(1, match.Rounds[0].RoundNumber);
    }

    [Fact]
    public void StartNewRound_ShouldSetRoundStateToInProgress()
    {
        var match = CreateMatchWithTwoPlayers();
        match.CurrentPlayer = match.Players[0];

        service.StartNewRound(match);

        Assert.Equal(RoundState.InProgress, match.Rounds[0].State);
    }

    [Fact]
    public void StartNewRound_ShouldSetStartingPlayer()
    {
        var match = CreateMatchWithTwoPlayers();
        var currentPlayer = match.Players[1];
        match.CurrentPlayer = currentPlayer;

        service.StartNewRound(match);

        Assert.Equal(currentPlayer, match.Rounds[0].StartingPlayer);
    }

    [Fact]
    public void StartNewRound_ShouldUpdateCurrentRoundNumber()
    {
        var match = CreateMatchWithTwoPlayers();
        match.CurrentPlayer = match.Players[0];

        service.StartNewRound(match);

        Assert.Equal(1, match.CurrentRoundNumber);
    }

    [Fact]
    public void StartNewRound_ShouldIncrementRoundNumber_WhenMultipleRounds()
    {
        var match = CreateMatchWithTwoPlayers();
        match.CurrentPlayer = match.Players[0];

        service.StartNewRound(match);
        service.StartNewRound(match);

        Assert.Equal(2, match.Rounds.Count);
        Assert.Equal(2, match.Rounds[1].RoundNumber);
    }

    [Fact]
    public void StartNewRound_ShouldThrowException_WhenCurrentPlayerIsNull()
    {
        var match = CreateMatchWithTwoPlayers();

        Assert.Throws<InvalidOperationException>(() => service.StartNewRound(match));
    }
}
