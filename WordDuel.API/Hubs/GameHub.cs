using Microsoft.AspNetCore.SignalR;
using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.BLL.WordServices;
using WordDuel.Shared.DTOs;

namespace WordDuel.API.Hubs;

public class GameHub : Hub
{
    private readonly IMatchService _matchService;
    private readonly IWordService _wordService;
    private readonly IMatchPersistence _persistence;

    public GameHub(IMatchService matchService, IWordService wordService, IMatchPersistence persistence)
    {
        _matchService = matchService;
        _wordService = wordService;
        _persistence = persistence;
    }

    // ── HOST GAME ──
    public async Task HostGame(int roundsToWin, int secondsPerRound, string playerName)
    {
        var match = _matchService.CreateMatch(roundsToWin, secondsPerRound, playerName);

        var roomCode = GenerateRoomCode();
        match.RoomCode = roomCode;

        match = await _persistence.SaveMatchAsync(match); //Spara till DB

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        await Clients.Caller.SendAsync("OnGameHosted", new
        {
            roomCode,
            matchId = match.Id,
            playerId = match.Players[0].Id, //Skicka DB-genererat ID
            playerIndex = 0
        });
    }

    // ── JOIN GAME ──
    public async Task JoinGame(string roomCode, string playerName)
    {
        var match = await _persistence.LoadMatchAsync(roomCode); //Ladda från DB
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
        _matchService.StartMatch(match);

        match = await _persistence.SaveMatchAsync(match); //Spara ändringarna

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        // Skicka spelinställningar till den som joinar
        await Clients.Caller.SendAsync("OnGameSettings", new
        {
            roundsToWin = match.RoundsToWin,
            secondsPerRound = match.TurnTimeSeconds,
            playerId = match.Players[1].Id, //Skicka DB-genererat ID
            playerIndex = 1

        });

        // Notifiera båda spelare att någon anslutit
        await Clients.Group(roomCode).SendAsync("OnPlayerJoined");

        var starterIndex = match.Players.FindIndex(p => p.Id == match.CurrentPlayer!.Id);
        await Clients.Group(roomCode).SendAsync("OnCoinFlipResult", starterIndex);
    }

    // ── START MATCH (coin flip) ──
    public async Task StartMatch(string roomCode)
    {
        var match = await _persistence.LoadMatchAsync(roomCode);
        if (match == null) return;

        if (!_matchService.IsMatchReadyToStart(match)) return;

        _matchService.StartMatch(match);

        match = await _persistence.SaveMatchAsync(match);

        var starterIndex = match.Players.FindIndex(p => p.Id == match.CurrentPlayer!.Id);

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
        var match = await _persistence.LoadMatchAsync(roomCode);
        if (match == null) return;

        await _matchService.StartNewRoundAsync(match, word);
        match = await _persistence.SaveMatchAsync(match); //Spara ny runda

        // Efter StartNewRound är CurrentPlayer den som börjar – skicka index
        var nextPlayerIndex = match.Players.FindIndex(p => p.Id == match.CurrentPlayer!.Id);

        await Clients.Group(roomCode).SendAsync("OnStartWordSelected", new
        {
            word,
            nextPlayerIndex
        });
    }

    // ── BEGIN NEXT ROUND ──
    public async Task BeginNextRound(string roomCode)
    {
        var match = await _persistence.LoadMatchAsync(roomCode);
        if (match == null) return;
        if (_matchService.IsMatchFinished(match)) return;
        if (match.CurrentPlayer == null) return;

        var starterIndex = match.Players.IndexOf(match.CurrentPlayer);
        if (starterIndex < 0) return;

        await Clients.Group(roomCode).SendAsync("OnNextRoundStarter", new
        {
            starterIndex
        });
    }

    // ── SUBMIT WORD ──
    // OBS: SubmitMoveAsync anropar SwitchTurn internt – vi gör det inte igen här
    public async Task SubmitWord(string roomCode, string newWord)
    {
        var match = await _persistence.LoadMatchAsync(roomCode);
        if (match == null) return;

        try
        {
            await _matchService.SubmitMoveAsync(match, match.CurrentPlayer!.Id, newWord);

            match = await _persistence.SaveMatchAsync(match); //Spara draget

            // SubmitMoveAsync anropar SwitchTurn internt
            // match.CurrentPlayer är nu nästa spelare
            var nextPlayerIndex = match.Players.FindIndex(p => p.Id == match.CurrentPlayer!.Id);

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
        var match = await _persistence.LoadMatchAsync(roomCode);
        if (match == null) return;

        // Skydda mot dubbla anrop (runda redan avslutad)
        var currentRound = match.Rounds.LastOrDefault();
        if (currentRound == null || currentRound.State != Shared.Enums.RoundState.InProgress)
            return;

        var playerWhoGaveUp = match.CurrentPlayer!; //Spara innan GiveUpRound ändrar CurrentPlayer

        _matchService.GiveUpRound(match, playerWhoGaveUp.Id);

        // Efter GiveUpRound: match.CurrentPlayer = förloraren, winner = den andra
        var winner = match.Players.First(p => p.Id != playerWhoGaveUp.Id);


        if (_matchService.IsMatchFinished(match))
        {
            await _persistence.DeleteMatchAsync(roomCode); //Radera avslutad match
            // Match över - skicka till alla
            await Clients.Group(roomCode).SendAsync("OnMatchResult", new
            {
                winnerId = match.Winner!.Id,
                winnerName = match.Winner!.Name,
                scores = match.Players.Select(p => new { p.Id, p.Name, p.Score }).ToList()
            });
        }
        else
        {
            match = await _persistence.SaveMatchAsync(match); //Spara rund-resultat

            //Skicka OnRoundResult med playerWhoGaveUpId
            await Clients.Group(roomCode).SendAsync("OnRoundResult", new
            {
                winnerId = winner.Id,
                winnerName = winner.Name,
                scores = match.Players.Select(p => new { p.Id, p.Name, p.Score }).ToList(),
                reason = "gaveUp",                        //Flagga för att identifiera give-up
                playerWhoGaveUpId = playerWhoGaveUp.Id,   //Vem som gav upp
                nextStarterId = match.CurrentPlayer!.Id
            });
        }
    }

    // ── TIMER EXPIRED ──
    public async Task TimerExpired(string roomCode)
    {
        var match = await _persistence.LoadMatchAsync(roomCode);
        if (match == null) return;

        // Skydda mot dubbla anrop (runda redan avslutad)
        var currentRound = match.Rounds.LastOrDefault();
        if (currentRound == null || currentRound.State != Shared.Enums.RoundState.InProgress)
            return;

        var playerWhoTimedOut = match.CurrentPlayer!;//Spara innan CurrentPlayer ändras

        // match.CurrentPlayer är spelaren vars tid gick ut – korrekt
        _matchService.HandleTurnTimeout(match, playerWhoTimedOut.Id);

        var winner = match.Players.First(p => p.Id != playerWhoTimedOut.Id);

        if (_matchService.IsMatchFinished(match))
        {
            await _persistence.DeleteMatchAsync(roomCode); //Radera avslutad match

            await Clients.Group(roomCode).SendAsync("OnMatchResult", new
            {
                winnerId = match.Winner!.Id,
                winnerName = match.Winner!.Name,
                scores = match.Players.Select(p => new { p.Id, p.Name, p.Score }).ToList()
            });
        }
        else
        {
            match = await _persistence.SaveMatchAsync(match); //Spara rund-resultat

            await Clients.Group(roomCode).SendAsync("OnRoundResult", new
            {
                winnerId = winner.Id,
                winnerName = winner.Name,
                scores = match.Players.Select(p => new { p.Id, p.Name, p.Score }).ToList(),
                reason = "timeout",
                playerWhoTimedOutId = playerWhoTimedOut.Id,
                nextStarterId = match.CurrentPlayer!.Id
            });
        }
    }

    // ── HJÄLPMETODER ──
    private async Task NotifyRoundOrMatchResult(MatchDto match, string roomCode, string reason)
    {
        if (_matchService.IsMatchFinished(match))
        {
            await Clients.Group(roomCode).SendAsync("OnMatchResult", new
            {
                winnerId = match.Winner!.Id,
                winnerName = match.Winner!.Name,
                scores = match.Players.Select(p => new { p.Id, p.Name, p.Score }).ToList()
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
                winnerId = winner.Id,                     
                winnerName = winner.Name,             
                scores = match.Players.Select(p => new { p.Id, p.Name, p.Score }).ToList(),
                reason,
                nextStarterId = match.CurrentPlayer!.Id
            });
        }
    }

    private static string GenerateRoomCode()
    {
        var number = Random.Shared.Next(1000, 9999);
        return $"WD-{number}";
    }
}
