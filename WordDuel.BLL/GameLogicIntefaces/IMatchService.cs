using WordDuel.Shared.DTOs;

namespace WordDuel.BLL.GameLogicIntefaces;

public interface IMatchService
{
    MatchDto CreateMatch(int roundsToWin, string? firstPlayerName);
    MatchDto JoinMatch(MatchDto match, string? secondPlayerName);

    void StartMatch(MatchDto match);
    Task StartNewRoundAsync(MatchDto match, int wordLength);

    void SwitchTurn(MatchDto match);
    void EndRound(MatchDto match, int winnerPlayerId);

    bool CanJoinMatch(MatchDto match);
    bool IsMatchReadyToStart(MatchDto match);
    bool IsMatchFinished(MatchDto match);

    Task SubmitMoveAsync(MatchDto match, int playerId, string word);
    void GiveUpRound(MatchDto match, int playerId);
    void HandleTurnTimeout(MatchDto match, int playerId);
}
