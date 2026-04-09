using Moq;
using WordDuel.BLL.GameLogicServices;
using WordDuel.DAL.Interfaces;
using WordDuel.DAL.Models;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;

namespace WordDuel.UnitTests;

public class MatchPersistenceTests
{
    private readonly Mock<IMatchRepository> _repoMock;
    private readonly MatchPersistence _persistence;

    public MatchPersistenceTests()
    {
        _repoMock = new Mock<IMatchRepository>();
        _persistence = new MatchPersistence(_repoMock.Object);
    }

    //SaveMatchAsync — ny match (Id == 0)

    [Fact]
    public async Task SaveMatchAsync_NewMatch_ShouldCallCreateAsync()
    {
        var dto = CreateNewMatchDto();

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<MatchModel>()))
            .ReturnsAsync((MatchModel m) => SimulateDbCreate(m));

        await _persistence.SaveMatchAsync(dto);

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<MatchModel>()), Times.Once);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<MatchModel>()), Times.Never);
    }

    [Fact]
    public async Task SaveMatchAsync_NewMatch_ShouldZeroOutAllIds()
    {
        var dto = CreateNewMatchDto();
        dto.Players.Add(new PlayerDto { Id = 1, Name = "A", Score = 0 });

        int capturedId = -1;
        int? capturedCurrentPlayerId = -1;
        List<int> capturedPlayerIds = [];

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<MatchModel>()))
            .Callback<MatchModel>(m =>
            {
                capturedId = m.Id;
                capturedCurrentPlayerId = m.CurrentPlayerId;
                capturedPlayerIds = m.Players.Select(p => p.Id).ToList();
            })
            .ReturnsAsync((MatchModel m) => SimulateDbCreate(m));

        await _persistence.SaveMatchAsync(dto);

        Assert.Equal(0, capturedId);
        Assert.Null(capturedCurrentPlayerId);
        Assert.All(capturedPlayerIds, id => Assert.Equal(0, id));
    }

    [Fact]
    public async Task SaveMatchAsync_NewMatch_ShouldReturnDtoWithDbGeneratedIds()
    {
        var dto = CreateNewMatchDto();
        dto.Players.Add(new PlayerDto { Id = 1, Name = "A", Score = 0 });

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<MatchModel>()))
            .ReturnsAsync((MatchModel m) => SimulateDbCreate(m));

        var result = await _persistence.SaveMatchAsync(dto);

        Assert.NotEqual(0, result.Id);
        Assert.All(result.Players, p => Assert.NotEqual(0, p.Id));
    }

    //SaveMatchAsync — befintlig match med ny spelare

    [Fact]
    public async Task SaveMatchAsync_ExistingMatch_NewPlayer_ShouldResetNewPlayerIdToZero()
    {
        // Befintlig match i DB med en spelare (Id=10)
        var existingModel = CreateExistingModel(playerIds: [10]);

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingModel);

        List<int> capturedPlayerIds = [];
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<MatchModel>()))
            .Callback<MatchModel>(m =>
            {
                capturedPlayerIds = m.Players.Select(p => p.Id).ToList();
            })
            .ReturnsAsync((MatchModel m) => SimulateDbUpdate(m));

        var dto = new MatchDto
        {
            Id = 1,
            RoomCode = "WD-1234",
            State = MatchState.InProgress,
            RoundsToWin = 2,
            TurnTimeSeconds = 30,
            CurrentPlayer = new PlayerDto { Id = 10, Name = "A", Score = 0 },
            Players =
            [
                new PlayerDto { Id = 10, Name = "A", Score = 0 },
                new PlayerDto { Id = 2, Name = "B", Score = 0 }  // Ny spelare!
            ]
        };

        await _persistence.SaveMatchAsync(dto);

        Assert.Contains(10, capturedPlayerIds);  // Befintlig behåller ID
        Assert.Contains(0, capturedPlayerIds);   // Ny nollställd
    }

    //SaveMatchAsync — befintlig match med ny runda

    [Fact]
    public async Task SaveMatchAsync_ExistingMatch_NewRound_ShouldResetNewRoundIdToZero()
    {
        var existingModel = CreateExistingModel(playerIds: [10, 20], roundIds: [100]);

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingModel);

        List<int> capturedRoundIds = [];
        bool firstCall = true;
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<MatchModel>()))
            .Callback<MatchModel>(m =>
            {
                if (firstCall)
                {
                    capturedRoundIds = m.Rounds.Select(r => r.Id).ToList();
                    firstCall = false;
                }
            })
            .ReturnsAsync((MatchModel m) => SimulateDbUpdate(m));

        var dto = new MatchDto
        {
            Id = 1,
            RoomCode = "WD-1234",
            State = MatchState.InProgress,
            RoundsToWin = 2,
            TurnTimeSeconds = 30,
            CurrentRoundNumber = 2,
            CurrentPlayer = new PlayerDto { Id = 10, Name = "A", Score = 0 },
            Players =
            [
                new PlayerDto { Id = 10, Name = "A", Score = 0 },
                new PlayerDto { Id = 20, Name = "B", Score = 0 }
            ],
            Rounds =
            [
                new RoundDto
                {
                    Id = 100, RoundNumber = 1, StartingWord = "stark",
                    CurrentWord = "stark", State = RoundState.Finished
                },
                new RoundDto
                {
                    Id = 0, RoundNumber = 2, StartingWord = "klang",
                    CurrentWord = "klang", State = RoundState.InProgress
                }
            ]
        };

        await _persistence.SaveMatchAsync(dto);

        Assert.Contains(100, capturedRoundIds); // Befintlig
        Assert.Contains(0, capturedRoundIds);   // Ny
    }

    //SaveMatchAsync — CurrentRoundId fixup

    [Fact]
    public async Task SaveMatchAsync_ExistingMatch_NewRound_ShouldFixCurrentRoundIdAfterSave()
    {
        var existingModel = CreateExistingModel(playerIds: [10, 20]);

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingModel);

        // UpdateAsync simulerar att DB genererar Id=500 för nya rundan
        _repoMock
            .Setup(r => r.UpdateAsync(It.IsAny<MatchModel>()))
            .ReturnsAsync((MatchModel m) =>
            {
                foreach (var r in m.Rounds.Where(r => r.Id == 0))
                    r.Id = 500;
                return m;
            });

        var dto = new MatchDto
        {
            Id = 1,
            RoomCode = "WD-1234",
            State = MatchState.InProgress,
            RoundsToWin = 2,
            TurnTimeSeconds = 30,
            CurrentRoundNumber = 1,
            CurrentPlayer = new PlayerDto { Id = 10, Name = "A", Score = 0 },
            Players =
            [
                new PlayerDto { Id = 10, Name = "A", Score = 0 },
                new PlayerDto { Id = 20, Name = "B", Score = 0 }
            ],
            Rounds =
            [
                new RoundDto
                {
                    Id = 0, RoundNumber = 1, StartingWord = "stark",
                    CurrentWord = "stark", State = RoundState.InProgress
                }
            ]
        };

        await _persistence.SaveMatchAsync(dto);

        // UpdateAsync anropas 2 gånger: en gång för själva uppdateringen,
        // en gång för att fixa CurrentRoundId
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<MatchModel>()), Times.Exactly(2));
    }

    //LoadMatchAsync

    [Fact]
    public async Task LoadMatchAsync_ShouldReturnNull_WhenRoomCodeNotFound()
    {
        _repoMock
            .Setup(r => r.GetByRoomCodeAsync("WD-9999"))
            .ReturnsAsync((MatchModel?)null);

        var result = await _persistence.LoadMatchAsync("WD-9999");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoadMatchAsync_ShouldReturnDto_WhenFound()
    {
        var model = CreateExistingModel(playerIds: [10]);
        _repoMock
            .Setup(r => r.GetByRoomCodeAsync("WD-1234"))
            .ReturnsAsync(model);

        var result = await _persistence.LoadMatchAsync("WD-1234");

        Assert.NotNull(result);
        Assert.Equal("WD-1234", result.RoomCode);
    }

    //DeleteMatchAsync

    [Fact]
    public async Task DeleteMatchAsync_ShouldCallDeleteAsync_WhenMatchExists()
    {
        var model = CreateExistingModel(playerIds: [10]);
        _repoMock
            .Setup(r => r.GetByRoomCodeAsync("WD-1234"))
            .ReturnsAsync(model);

        await _persistence.DeleteMatchAsync("WD-1234");

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteMatchAsync_ShouldNotCallDeleteAsync_WhenMatchNotFound()
    {
        _repoMock
            .Setup(r => r.GetByRoomCodeAsync("WD-9999"))
            .ReturnsAsync((MatchModel?)null);

        await _persistence.DeleteMatchAsync("WD-9999");

        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    // ── Hjälpmetoder ──

    private static MatchDto CreateNewMatchDto() => new()
    {
        Id = 0,
        RoomCode = "WD-1234",
        State = MatchState.WaitingForPlayers,
        RoundsToWin = 2,
        TurnTimeSeconds = 30
    };

    private static MatchModel CreateExistingModel(
        int[] playerIds,
        int[]? roundIds = null)
    {
        return new MatchModel
        {
            Id = 1,
            RoomCode = "WD-1234",
            State = "InProgress",
            BestOf = 3,
            TurnTimeSeconds = 30,
            Players = playerIds.Select(id => new PlayerModel
            {
                Id = id,
                Name = $"Player{id}",
                Score = 0,
                MatchId = 1
            }).ToList(),
            Rounds = (roundIds ?? []).Select(id => new RoundModel
            {
                Id = id,
                RoundNumber = id / 100,
                StartingWord = "stark",
                CurrentWord = "stark",
                State = "Finished",
                MatchId = 1,
                Moves = []
            }).ToList()
        };
    }

    // Simulerar att DB genererar ID:n vid Create.
    private static MatchModel SimulateDbCreate(MatchModel m)
    {
        m.Id = 42;
        var playerId = 100;
        foreach (var p in m.Players)
            p.Id = playerId++;
        var roundId = 200;
        foreach (var r in m.Rounds)
            r.Id = roundId++;
        return m;
    }

    // Simulerar att DB genererar ID:n för nya entiteter vid Update.
    private static MatchModel SimulateDbUpdate(MatchModel m)
    {
        var nextId = 500;
        foreach (var p in m.Players.Where(p => p.Id == 0))
            p.Id = nextId++;
        foreach (var r in m.Rounds.Where(r => r.Id == 0))
            r.Id = nextId++;
        return m;
    }
}