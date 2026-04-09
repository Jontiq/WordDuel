using WordDuel.BLL.Mappers;
using WordDuel.DAL.Models;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;

namespace WordDuel.UnitTests;

public class MatchMapperTests
{
    // ── ToDto ──

    [Theory]
    [InlineData(1, 1)]  // BestOf 1 → RoundsToWin 1
    [InlineData(3, 2)]  // BestOf 3 → RoundsToWin 2
    [InlineData(5, 3)]  // BestOf 5 → RoundsToWin 3
    [InlineData(7, 4)]  // BestOf 7 → RoundsToWin 4

    public void ToDto_ShouldConvertBestOfToRoundsToWin(int bestOf, int expectedRoundsToWin)
    {
        var model = CreateMinimalModel(bestOf: bestOf);

        var dto = MatchMapper.ToDto(model);

        Assert.Equal(expectedRoundsToWin, dto.RoundsToWin);
    }

    [Fact]
    public void ToDto_ShouldMapBasicProperties()
    {
        var model = CreateMinimalModel();
        model.RoomCode = "WD-1234";
        model.State = "InProgress";
        model.TurnTimeSeconds = 30;

        var dto = MatchMapper.ToDto(model);

        Assert.Equal("WD-1234", dto.RoomCode);
        Assert.Equal(MatchState.InProgress, dto.State);
        Assert.Equal(30, dto.TurnTimeSeconds);
    }

    [Fact]
    //Korrupt data i DB ska inte krascha appen
    public void ToDto_ShouldDefaultToWaitingForPlayers_WhenStateIsInvalid()
    {
        var model = CreateMinimalModel();
        model.State = "Nonsense";

        var dto = MatchMapper.ToDto(model);

        Assert.Equal(MatchState.WaitingForPlayers, dto.State);
    }

    [Fact]
    public void ToDto_ShouldMapPlayers_OrderedById()
    {
        var model = CreateMinimalModel();
        model.Players =
        [
            new PlayerModel { Id = 20, Name = "B", Score = 0 },
            new PlayerModel { Id = 10, Name = "A", Score = 1 }
        ];

        var dto = MatchMapper.ToDto(model);

        Assert.Equal(2, dto.Players.Count);
        Assert.Equal("A", dto.Players[0].Name);
        Assert.Equal("B", dto.Players[1].Name);
    }

    [Fact]
    public void ToDto_ShouldMapCurrentPlayer_WhenSet()
    {
        var player = new PlayerModel { Id = 5, Name = "Anna", Score = 0 };
        var model = CreateMinimalModel();
        model.CurrentPlayer = player;
        model.CurrentPlayerId = 5;
        model.Players = [player];

        var dto = MatchMapper.ToDto(model);

        Assert.NotNull(dto.CurrentPlayer);
        Assert.Equal("Anna", dto.CurrentPlayer.Name);
    }

    [Fact]
    public void ToDto_ShouldLeaveCurrentPlayerNull_WhenNotSet()
    {
        var model = CreateMinimalModel();

        var dto = MatchMapper.ToDto(model);

        Assert.Null(dto.CurrentPlayer);
    }

    [Fact]
    public void ToDto_ShouldRebuildUsedWordsFromMoves()
    {
        var model = CreateMinimalModel();
        model.Rounds =
        [
            new RoundModel
            {
                Id = 1, RoundNumber = 1, StartingWord = "stark",
                CurrentWord = "start", State = "InProgress",
                Moves =
                [
                    new MoveModel { Id = 1, MoveNumber = 1, Word = "start", PlayerId = 1 }
                ]
            }
        ];

        var dto = MatchMapper.ToDto(model);

        Assert.Contains("stark", dto.Rounds[0].UsedWords);
        Assert.Contains("start", dto.Rounds[0].UsedWords);
    }

    // ── ToModel ──

    [Theory]
    [InlineData(1, 1)]  // RoundsToWin 1 → BestOf 1
    [InlineData(2, 3)]  // RoundsToWin 2 → BestOf 3
    [InlineData(3, 5)]  // RoundsToWin 3 → BestOf 5
    [InlineData(4, 7)]  // RoundsToWin 4 → BestOf 7
    public void ToModel_ShouldConvertRoundsToWinToBestOf(int roundsToWin, int expectedBestOf)
    {
        var dto = CreateMinimalDto(roundsToWin: roundsToWin);

        var model = MatchMapper.ToModel(dto);

        Assert.Equal(expectedBestOf, model.BestOf);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void RoundTrip_ShouldPreserveRoundsToWin(int roundsToWin)
    {
        var dto = CreateMinimalDto(roundsToWin: roundsToWin);

        var model = MatchMapper.ToModel(dto);
        var result = MatchMapper.ToDto(model);

        Assert.Equal(roundsToWin, result.RoundsToWin);
    }

    [Fact]
    public void ToModel_ShouldSetMatchIdOnPlayers()
    {
        var dto = CreateMinimalDto();
        dto.Id = 42;
        dto.Players = [new PlayerDto { Id = 1, Name = "A", Score = 0 }];

        var model = MatchMapper.ToModel(dto);

        Assert.All(model.Players, p => Assert.Equal(42, p.MatchId));
    }

    [Fact]
    public void ToModel_ShouldSetCurrentPlayerId()
    {
        var player = new PlayerDto { Id = 5, Name = "Anna", Score = 0 };
        var dto = CreateMinimalDto();
        dto.CurrentPlayer = player;
        dto.Players = [player];

        var model = MatchMapper.ToModel(dto);

        Assert.Equal(5, model.CurrentPlayerId);
    }

    // Hjälpmetoder

    private static MatchModel CreateMinimalModel(int bestOf = 3)
    {
        return new MatchModel
        {
            Id = 1,
            RoomCode = "WD-0000",
            State = "WaitingForPlayers",
            BestOf = bestOf,
            TurnTimeSeconds = 30,
            Players = [],
            Rounds = []
        };
    }

    private static MatchDto CreateMinimalDto(int roundsToWin = 2)
    {
        return new MatchDto
        {
            Id = 1,
            RoomCode = "WD-0000",
            State = MatchState.WaitingForPlayers,
            RoundsToWin = roundsToWin,
            TurnTimeSeconds = 30
        };
    }
}