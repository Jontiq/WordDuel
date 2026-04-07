using Microsoft.AspNetCore.Mvc;
using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.BLL.GameLogicServices;
using WordDuel.BLL.WordServices;
using WordDuel.Shared.DTOs;

namespace WordDuel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly IWordService _wordService;
    private readonly SessionStore _sessionStore;
    private readonly ILogger<GameController> _logger;

    public GameController(
        IMatchService matchService, 
        IWordService wordService, 
        SessionStore sessionStore,
        ILogger<GameController> logger)
    {
        _matchService = matchService;
        _wordService = wordService;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    // POST /api/game/host
    [HttpPost("host")]
    public IActionResult HostGame([FromBody] HostGameRequest request)
    {
        try
        {

        var match = _matchService.CreateMatch(request.RoundsToWin, request.SecondsPerRound, request.PlayerName);

        var roomCode = GenerateRoomCode();
        match.RoomCode = roomCode;

        _sessionStore.Add(roomCode, match);

            return Ok(new { roomCode, matchId = match.Id });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid host: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid host av spel");
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // POST /api/game/join/{roomCode}
    [HttpPost("join/{roomCode}")]
    public IActionResult JoinGame(string roomCode, [FromBody] JoinGameRequest request)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            if (!_matchService.CanJoinMatch(match))
                return BadRequest("Rummet är fullt.");

            _matchService.JoinMatch(match, request.PlayerName);

            return Ok(new { roomCode, matchId = match.Id });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid join: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid join till rum: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // GET /api/game/{roomCode}
    [HttpGet("{roomCode}")]
    public IActionResult GetMatch(string roomCode)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            return Ok(new
            {
                roomCode = match.RoomCode,
                state = match.State.ToString(),
                roundsToWin = match.RoundsToWin,
                currentRound = match.CurrentRoundNumber,
                currentPlayer = match.CurrentPlayer?.Name,
                players = match.Players.Select(p => new { p.Name, p.Score }),
                currentWord = match.Rounds.LastOrDefault()?.CurrentWord
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid hämtning av match: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // POST /api/game/{roomCode}/startmatch
    [HttpPost("{roomCode}/startmatch")]
    public IActionResult StartMatch(string roomCode)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            if (!_matchService.IsMatchReadyToStart(match))
                return BadRequest("Matchen är inte redo att starta.");

            _matchService.StartMatch(match);
            var starterIndex = match.Players.IndexOf(match.CurrentPlayer!);

            return Ok(new
            {
                starterIndex,
                starterName = match.CurrentPlayer!.Name
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid start: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid start av match: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // GET /api/game/{roomCode}/startwords
    [HttpGet("{roomCode}/startwords")]
    public async Task<IActionResult> GetStartWords(string roomCode)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            var words = new List<string>();
            while (words.Count < 3)
            {
                var word = await _wordService.GetRandomWordAsync(5);
                if (word != null && !words.Contains(word))
                    words.Add(word);
            }

            return Ok(new { words });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid hämtning av startord: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // POST /api/game/{roomCode}/selectword
    [HttpPost("{roomCode}/selectword")]
    public async Task<IActionResult> SelectWord(string roomCode, [FromBody] SelectWordRequest request)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            await _matchService.StartNewRoundAsync(match, request.Word);

            return Ok(new
            {
                word = request.Word,
                currentPlayer = match.CurrentPlayer?.Name
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid val av ord: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid val av ord: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // POST /api/game/{roomCode}/submitword
    [HttpPost("{roomCode}/submitword")]
    public async Task<IActionResult> SubmitWord(string roomCode, [FromBody] SubmitWordRequest request)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            await _matchService.SubmitMoveAsync(match, request.PlayerId, request.Word);

            return Ok(new
            {
                word = request.Word,
                nextPlayer = match.CurrentPlayer?.Name,
                matchFinished = _matchService.IsMatchFinished(match)
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid submit: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid submit av ord: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // POST /api/game/{roomCode}/giveup
    [HttpPost("{roomCode}/giveup")]
    public IActionResult GiveUp(string roomCode, [FromBody] PlayerActionRequest request)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            _matchService.GiveUpRound(match, request.PlayerId);

            return Ok(BuildRoundOrMatchResult(match, "Spelaren gav upp."));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid give up: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt spelläge vid give up: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid give up: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // POST /api/game/{roomCode}/timerexpired
    [HttpPost("{roomCode}/timerexpired")]
    public IActionResult TimerExpired(string roomCode, [FromBody] PlayerActionRequest request)
    {
        try
        {
            var match = _sessionStore.Get(roomCode);
            if (match == null)
                return NotFound("Rummet hittades inte.");

            _matchService.HandleTurnTimeout(match, request.PlayerId);

            return Ok(BuildRoundOrMatchResult(match, "Tiden rann ut."));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt argument vid timeout: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Ogiltigt spelläge vid timeout: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oväntat fel vid timeout: {RoomCode}", roomCode);
            return StatusCode(500, "Ett oväntat fel inträffade.");
        }
    }

    // ── HJÄLPMETODER ──
    private object BuildRoundOrMatchResult(MatchDto match, string reason)
    {
        if (_matchService.IsMatchFinished(match))
        {
            return new
            {
                matchFinished = true,
                winner = match.Winner?.Name,
                scores = match.Players.Select(p => new { p.Name, p.Score }),
                reason
            };
        }

        return new
        {
            matchFinished = false,
            roundWinner = match.Players.OrderByDescending(p => p.Score).First().Name,
            scores = match.Players.Select(p => new { p.Name, p.Score }),
            nextPlayer = match.CurrentPlayer?.Name,
            reason
        };
    }

    private static string GenerateRoomCode()
    {
        var number = Random.Shared.Next(1000, 9999);
        return $"WD-{number}";
    }
}

// ── REQUEST MODELS ──
public record HostGameRequest(int RoundsToWin, int SecondsPerRound, string PlayerName);
public record JoinGameRequest(string PlayerName);
public record SelectWordRequest(string Word);
public record SubmitWordRequest(int PlayerId, string Word);
public record PlayerActionRequest(int PlayerId);