using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.BLL.Mappers;
using WordDuel.DAL.Interfaces;
using WordDuel.Shared.DTOs;

namespace WordDuel.BLL.GameLogicServices;

public class MatchPersistence : IMatchPersistence
{
    private readonly IMatchRepository _repo;

    public MatchPersistence(IMatchRepository repo)
    {
        _repo = repo;
    }

    public async Task<MatchDto> SaveMatchAsync(MatchDto match)
    {
        if (match.Id == 0)
        {
            var model = MatchMapper.ToModel(match);

            // Ny match – låt DB generera alla ID:n
            model.Id = 0;
            model.CurrentPlayerId = null;
            model.CurrentRoundId = null;
            model.WinnerPlayerId = null;

            foreach (var p in model.Players)
                p.Id = 0;

            foreach (var r in model.Rounds)
            {
                r.Id = 0;
                foreach (var m in r.Moves)
                    m.Id = 0;
            }

            var created = await _repo.CreateAsync(model);
            return MatchMapper.ToDto(created);
        }
        else
        {
            var model = MatchMapper.ToModel(match);

            // Ladda befintliga ID:n från DB för att identifiera NYA entiteter
            var existing = await _repo.GetByIdAsync(model.Id);
            var existingPlayerIds = existing?.Players.Select(p => p.Id).ToHashSet() ?? [];
            var existingRoundIds = existing?.Rounds.Select(r => r.Id).ToHashSet() ?? [];
            var existingMoveIds = existing?.Rounds
                .SelectMany(r => r.Moves)
                .Select(m => m.Id).ToHashSet() ?? [];

            // Nollställ ID för spelare som inte finns i DB (t.ex. från CreatePlayer(2, ...))
            foreach (var p in model.Players)
            {
                if (!existingPlayerIds.Contains(p.Id))
                {
                    p.Id = 0;
                    p.MatchId = model.Id;
                }
            }

            // Nollställ ID för rundor som inte finns i DB
            foreach (var r in model.Rounds)
            {
                if (!existingRoundIds.Contains(r.Id))
                {
                    r.Id = 0;
                    r.MatchId = model.Id;
                }

                foreach (var m in r.Moves)
                {
                    if (!existingMoveIds.Contains(m.Id))
                    {
                        m.Id = 0;
                        m.RoundId = r.Id;
                    }
                }
            }

            // Fixa CurrentRoundId om den pekar på en ny (osparad) runda
            bool hasNewCurrentRound = model.CurrentRoundId.HasValue
                && !existingRoundIds.Contains(model.CurrentRoundId.Value);
            if (hasNewCurrentRound)
                model.CurrentRoundId = null;

            // Fixa CurrentPlayerId om den pekar på en ny (osparad) spelare
            if (model.CurrentPlayerId.HasValue
                && !existingPlayerIds.Contains(model.CurrentPlayerId.Value))
                model.CurrentPlayerId = null;

            var updated = await _repo.UpdateAsync(model);

            // Sätt CurrentRoundId till den nya rundans riktiga ID
            if (hasNewCurrentRound)
            {
                var latestRound = updated.Rounds
                    .OrderByDescending(r => r.RoundNumber)
                    .FirstOrDefault();

                if (latestRound != null && latestRound.Id != 0)
                {
                    updated.CurrentRoundId = latestRound.Id;
                    await _repo.UpdateAsync(updated);
                }
            }

            // Sätt CurrentPlayerId om den pekade på en ny spelare
            if (model.CurrentPlayerId == null && match.CurrentPlayer != null)
            {
                // Hitta den nya spelarens riktiga DB-ID baserat på namn
                var newPlayer = updated.Players
                    .FirstOrDefault(p => !existingPlayerIds.Contains(p.Id));

                if (newPlayer != null)
                {
                    updated.CurrentPlayerId = newPlayer.Id;
                    await _repo.UpdateAsync(updated);
                }
            }

            return MatchMapper.ToDto(updated);
        }
    }

    public async Task<MatchDto?> LoadMatchAsync(string roomCode)
    {
        var model = await _repo.GetByRoomCodeAsync(roomCode);
        return model is null ? null : MatchMapper.ToDto(model);
    }

    public async Task DeleteMatchAsync(string roomCode)
    {
        var model = await _repo.GetByRoomCodeAsync(roomCode);
        if (model is not null)
        {
            await _repo.DeleteAsync(model.Id);
        }
    }
}