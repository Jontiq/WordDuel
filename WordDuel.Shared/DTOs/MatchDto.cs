using WordDuel.Shared.Enums;

namespace WordDuel.Shared.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public MatchState State { get; set; }

    public int RoundsToWin { get; set; }
    public int CurrentRoundNumber { get; set; }
    public int TurnTimeSeconds { get; set; }

    public PlayerDto? CurrentPlayer { get; set; }
    public PlayerDto? Winner { get; set; }

    public List<PlayerDto> Players { get; set; } = new();
    public List<RoundDto> Rounds { get; set; } = new();

    public List<string> UsedWords { get; set; } = new();
}
