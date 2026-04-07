using WordDuel.Shared.DTOs;

namespace WordDuel.BLL.GameLogicIntefaces;

public interface IMatchPersistence
{
    Task<MatchDto> SaveMatchAsync(MatchDto match);
    Task<MatchDto?> LoadMatchAsync(string roomCode);
    Task DeleteMatchAsync(string roomCode);
}