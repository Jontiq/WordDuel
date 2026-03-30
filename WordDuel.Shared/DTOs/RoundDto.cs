namespace WordDuel.Shared.DTOs;

public class RoundDto
{
    public int Id { get; set; }
    public int RoundNumber { get; set; }

    public string StartingWord { get; set; } = string.Empty;
    public string CurrentWord { get; set; } = string.Empty;

    public RoundState State { get; set; }

    public PlayerDto? StartingPlayer { get; set; }
    public PlayerDto? Winner { get; set; }

    public List<MoveDto> Moves { get; set; } = new();
}
