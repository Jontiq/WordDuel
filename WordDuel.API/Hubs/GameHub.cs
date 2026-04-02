using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.SignalR;
using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.BLL.GameLogicServices;
using WordDuel.BLL.WordServices;
using WordDuel.Shared.DTOs;

namespace WordDuel.API.Hubs;

public class GameHub : Hub
{
    private readonly IMatchService _matchService;
    private readonly IWordService _wordService;
    private readonly SessionStore _sessionStore;

    public GameHub(IMatchService matchService, IWordService wordService, SessionStore sessionStore)
    {
        _matchService = matchService;
        _wordService = wordService;
        _sessionStore = sessionStore;
    }

    // ── HOST GAME ──
    public async Task HostGame(int roundsToWin, string playerName)
    {
        var match = _matchService.CreateMatch(roundsToWin, playerName);

        var roomCode = GenerateRoomCode();
        match.RoomCode = roomCode;

        _sessionStore.Add(roomCode, match);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        await Clients.Caller.SendAsync("OnGameHosted", new
        {
            roomCode,
            matchId = match.Id
        });
    }

    // ── JOIN GAME ──
    public async Task JoinGame(string roomCode, string playerName)
    {
        var match = _sessionStore.Get(roomCode);
        if (match == null)
        {
            await Clients.Caller.SendAsync("OnError", "Rummet hittades inte.");
            return;
        }

        if (!_matchService.CanJoinMatch(match))
        {
            await Clients.Caller.SendAsync("OnError", "Rummet är fullt.");
            return;
        }

        _matchService.JoinMatch(match, playerName);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // Notifiera båda spelare att någon anslutit
        await Clients.Group(roomCode).SendAsync("OnPlayerJoined");

        // Starta matchen direkt här istället – bara en gång
        _matchService.StartMatch(match);
        var starterIndex = match.Players.IndexOf(match.CurrentPlayer!);
        await Clients.Group(roomCode).SendAsync("OnCoinFlipResult", starterIndex);
    }

    // ── START MATCH (coin flip) ──
    public async Task StartMatch(string roomCode)
    {
        var match = _sessionStore.Get(roomCode);
        if (match == null) return;

        if (!_matchService.IsMatchReadyToStart(match)) return;

        _matchService.StartMatch(match);

        var starterIndex = match.Players.IndexOf(match.CurrentPlayer!);

        await Clients.Group(roomCode).SendAsync("OnCoinFlipResult", starterIndex);
    }

    // ── GET START WORDS ──
    public async Task GetStartWords(string roomCode)
    {
        var words = new List<string>();
        while (words.Count < 3)
        {
            var word = await _wordService.GetRandomWordAsync(5);
            if (word != null && !words.Contains(word))
                words.Add(word);
        }

        await Clients.Caller.SendAsync("OnStartWordsReceived", words);
    }

    // ── SELECT START WORD ──
    public async Task SelectStartWord(string roomCode, string word)
    {
        var match = _sessionStore.Get(roomCode);
        if (match == null) return;

        await _matchService.StartNewRoundAsync(match, word.Length);

        // Efter StartNewRound är CurrentPlayer den som börjar – skicka index
        var nextPlayerIndex = match.Players.IndexOf(match.CurrentPlayer!);

        await Clients.Group(roomCode).SendAsync("OnStartWordSelected", new
        {
            word,
            nextPlayerIndex
        });
    }

    // ── SUBMIT WORD ──
    // OBS: SubmitMoveAsync anropar SwitchTurn internt – vi gör det inte igen här
    public async Task SubmitWord(string roomCode, string newWord)
    {
        var match = _sessionStore.Get(roomCode);
        if (match == null) return;

        try
        {
            await _matchService.SubmitMoveAsync(match, match.CurrentPlayer!.Id, newWord);

            // SubmitMoveAsync anropar SwitchTurn internt
            // match.CurrentPlayer är nu nästa spelare
            var nextPlayerIndex = match.Players.IndexOf(match.CurrentPlayer!);

            await Clients.Group(roomCode).SendAsync("OnWordAccepted", new
            {
                word = newWord,
                nextPlayerIndex
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("OnWordRejected", ex.Message);
        }
    }

    // ── GIVE UP ──
    public async Task GiveUp(string roomCode)
    {
        var match = _sessionStore.Get(roomCode);
        if (match == null) return;

        _matchService.GiveUpRound(match, match.CurrentPlayer!.Id);

        await NotifyRoundOrMatchResult(match, roomCode, "Motståndaren gav upp.");
    }

    // ── TIMER EXPIRED ──
    public async Task TimerExpired(string roomCode)
    {
        var match = _sessionStore.Get(roomCode);
        if (match == null) return;

        // match.CurrentPlayer är spelaren vars tid gick ut – korrekt
        _matchService.HandleTurnTimeout(match, match.CurrentPlayer!.Id);

        await NotifyRoundOrMatchResult(match, roomCode, "Tiden rann ut.");
    }

    // ── HJÄLPMETODER ──
    private async Task NotifyRoundOrMatchResult(MatchDto match, string roomCode, string reason)
    {
        if (_matchService.IsMatchFinished(match))
        {
            await Clients.Group(roomCode).SendAsync("OnMatchResult", new
            {
                winnerIndex = match.Players.IndexOf(match.Winner!),
                scores = match.Players.Select(p => p.Score).ToList()
            });
        }
        else
        {
            // Efter EndRound är CurrentPlayer förloraren (börjar nästa runda)
            // Vinnaren är den andra spelaren
            var winner = match.Players.First(p => p != match.CurrentPlayer);
            var winnerIndex = match.Players.IndexOf(winner);

            await Clients.Group(roomCode).SendAsync("OnRoundResult", new
            {
                winnerIndex,
                scores = match.Players.Select(p => p.Score).ToList(),
                reason
            });
        }
    }

    private static string GenerateRoomCode()
    {
        var number = Random.Shared.Next(1000, 9999);
        return $"WD-{number}";
    }
}