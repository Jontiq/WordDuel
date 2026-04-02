using WordDuel.DAL.Models;
using WordDuel.Shared.DTOs;
using WordDuel.Shared.Enums;

namespace WordDuel.BLL.Mappers;

public static class MatchMapper
{
    public static MatchDto ToDto(MatchModel model)
    {
        return new MatchDto
        {
            Id = model.Id,
            State = Enum.TryParse<MatchState>(model.State, out var matchState)
                ? matchState
                : MatchState.WaitingForPlayers,
            RoundsToWin = (model.BestOf / 2) + 1,
            TurnTimeSeconds = model.TurnTimeSeconds,
            CurrentRoundNumber = model.CurrentRound?.RoundNumber ?? 0,

            CurrentPlayer = model.CurrentPlayer is null ? null : ToDto(model.CurrentPlayer),
            Winner = model.WinnerPlayer is null ? null : ToDto(model.WinnerPlayer),

            Players = model.Players.Select(ToDto).ToList(),
            Rounds = model.Rounds.Select(ToDto).ToList()
        };
    }

    public static MatchModel ToModel(MatchDto dto)
    {
        var model = new MatchModel
        {
            Id = dto.Id,
            State = dto.State.ToString(),
            BestOf = (dto.RoundsToWin * 2) - 1,
            TurnTimeSeconds = dto.TurnTimeSeconds,

            CurrentPlayerId = dto.CurrentPlayer?.Id,
            WinnerPlayerId = dto.Winner?.Id,

            Players = dto.Players.Select(ToModel).ToList(),
            Rounds = dto.Rounds.Select(ToModel).ToList()
        };

        foreach (var player in model.Players)
        {
            player.MatchId = model.Id;
        }

        foreach (var round in model.Rounds)
        {
            round.MatchId = model.Id;

            foreach (var move in round.Moves)
            {
                move.RoundId = round.Id;
            }
        }

        model.CurrentRoundId = model.Rounds
            .FirstOrDefault(r => r.RoundNumber == dto.CurrentRoundNumber)?.Id;

        return model;
    }

    private static PlayerDto ToDto(PlayerModel model)
    {
        return new PlayerDto
        {
            Id = model.Id,
            Name = model.Name ?? string.Empty,
            Score = model.Score
        };
    }

    private static PlayerModel ToModel(PlayerDto dto)
    {
        return new PlayerModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Score = dto.Score
        };
    }

    private static RoundDto ToDto(RoundModel model)
    {
        return new RoundDto
        {
            Id = model.Id,
            RoundNumber = model.RoundNumber,
            StartingWord = model.StartingWord,
            CurrentWord = model.CurrentWord,
            State = Enum.TryParse<RoundState>(model.State, out var roundState)
                ? roundState
                : RoundState.NotStarted,

            StartingPlayer = model.StartingPlayer is null ? null : ToDto(model.StartingPlayer),
            Winner = model.WinnerPlayer is null ? null : ToDto(model.WinnerPlayer),

            Moves = model.Moves.Select(ToDto).ToList(),
            UsedWords = model.Moves
                .Select(m => m.Word)
                .Prepend(model.StartingWord)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static RoundModel ToModel(RoundDto dto)
    {
        return new RoundModel
        {
            Id = dto.Id,
            RoundNumber = dto.RoundNumber,
            StartingWord = dto.StartingWord,
            CurrentWord = dto.CurrentWord,
            State = dto.State.ToString(),

            StartingPlayerId = dto.StartingPlayer?.Id ?? 0,
            WinnerPlayerId = dto.Winner?.Id,

            Moves = dto.Moves.Select(ToModel).ToList()
        };
    }

    private static MoveDto ToDto(MoveModel model)
    {
        return new MoveDto
        {
            Id = model.Id,
            MoveNumber = model.MoveNumber,
            Word = model.Word,
            Player = model.Player is null ? null : ToDto(model.Player)
        };
    }

    private static MoveModel ToModel(MoveDto dto)
    {
        return new MoveModel
        {
            Id = dto.Id,
            MoveNumber = dto.MoveNumber,
            Word = dto.Word,
            PlayerId = dto.Player?.Id ?? 0
        };
    }
}
