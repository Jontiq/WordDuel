using WordDuel.Shared.DTOs;

namespace WordDuel.BLL.GameLogicServices;

public class SessionStore
{
    private readonly Dictionary<string, MatchDto> _sessions = new();

    public void Add(string roomCode, MatchDto match)
    {
        _sessions[roomCode] = match;
    }

    public MatchDto? Get(string roomCode)
    {
        return _sessions.GetValueOrDefault(roomCode);
    }

    public void Remove(string roomCode)
    {
        _sessions.Remove(roomCode);
    }

    public bool Exists(string roomCode)
    {
        return _sessions.ContainsKey(roomCode);
    }
}