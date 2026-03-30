using WordDuel.Shared.DTOs;

namespace WordDuel.BLL.GameLogicIntefaces;

public interface IMatchService
{
    MatchDto CreateMatch(int roundsToWin);
    void AddPlayer(MatchDto match, string? name);
    void StartMatch(MatchDto match);
    void StartNewRound(MatchDto match);
}
