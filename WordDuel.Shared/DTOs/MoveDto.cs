namespace WordDuel.Shared.DTOs;

public class MoveDto
{
    public int Id { get; set; }
    public int MoveNumber { get; set; }
    public string Word { get; set; } = string.Empty;

    public PlayerDto? Player { get; set; }
}
