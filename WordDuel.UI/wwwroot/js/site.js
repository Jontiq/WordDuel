// ── SIGNALR SETUP ──
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7222/gameHub")  // ← byt till API-projektets port
    .withAutomaticReconnect()
    .build();

let roomCode = null;
let myPlayerIndex = null; // 0 = spelare 1, 1 = spelare 2
let myPlayerId = null;
let myPlayerName = null;
let nextRoundStarterId = null;

function resetLocalGameState() {
    scores = { you: 0, opponent: 0 };
    wordHistory = [];
    selectedWord = null;
    changedIndex = null;
    currentWord = [];
    originalWord = [];
    nextRoundStarterId = null;
}

function updateRoomCodeUi(code) {
    document.getElementById('room-code').textContent = code ?? '';
    document.getElementById('di-session').textContent = code ?? '';
}

// Starta anslutningen
connection.start()
    .then(() => console.log("SignalR connected"))
    .catch(err => console.error("SignalR connection error:", err));

// ── SIGNALR EVENTS ──

// Värden har skapat spelet
connection.on("OnGameHosted", (data) => {
    roomCode = data.roomCode;
    myPlayerIndex = 0;
    resetLocalGameState();
    updateRoomCodeUi(data.roomCode);
    showState('waiting');
});

// Motståndaren har anslutit – bara visa waiting, StartMatch sker i hubben
connection.on("OnPlayerJoined", () => {
    console.log("Opponent joined!");
});

// Spelinställningar mottagna (för den som joinar)
connection.on("OnGameSettings", (data) => {
    roundsToWin = data.roundsToWin;
    currentTimerSeconds = data.secondsPerRound;
    console.log(`Settings synced: roundsToWin=${roundsToWin}, timer=${currentTimerSeconds}s`);
});

// Coin flip-resultat från BLL
connection.on("OnCoinFlipResult", (starterIndex) => {
    const iStart = starterIndex === myPlayerIndex ? 0 : 1;
    setCoinFlipWinner(iStart);
    showState('coin-flip');
});

// Startord mottagna från servern
connection.on("OnStartWordsReceived", (words) => {
    renderWordCards(words);
});

// Startord valt – båda spelare uppdateras
connection.on("OnStartWordSelected", (data) => {
    selectedWord = data.word.toUpperCase();
    hideOpponentOverlay();
    wordHistory = []; // ← återställ historik vid ny runda
    addWordToHistory(selectedWord, true); // ← lägg till startordet en gång

    //Uppdatera currentWord och originalWord för båda spelarna
    currentWord = selectedWord.split('');
    originalWord = [...currentWord];
    changedIndex = null;

    const isNowMyTurn = data.nextPlayerIndex === myPlayerIndex;
    if (isNowMyTurn) {
        showState('player-turn');
    } else {
        showState('spectating');
    }
});

// Servern avgör vem som börjar nästa set.
connection.on("OnNextRoundStarter", (data) => {
    wordHistory = [];
    selectedWord = null;
    currentWord = [];
    originalWord = [];
    changedIndex = null;

    const iStart = data.starterIndex === myPlayerIndex ? 0 : 1;
    setCoinFlipWinner(iStart);

    if (data.starterIndex === myPlayerIndex) {
        connection.invoke("GetStartWords", roomCode)
            .catch(err => console.error("GetStartWords error:", err));
        showState('word-select');
    } else {
        showState('spectating', false);
        showOpponentOverlay('Motståndaren väljer ett startord...');
    }
});

// Ord accepterat – uppdatera båda spelares vy
connection.on("OnWordAccepted", (data) => {
    clearInterval(timerInterval); // ← stoppa timern här
    timerActive = false;
    const word = data.word.toUpperCase();
    addWordToHistory(word, true);
    originalWord = word.split('');
    currentWord = [...originalWord];
    changedIndex = null;
    selectedWord = word; // ← lägg till denna

    const isNowMyTurn = data.nextPlayerIndex === myPlayerIndex;

    if (isNowMyTurn) {
        hideOpponentOverlay();
        showState('player-turn');
    } else {
        showState('spectating');
    }
});

// Ord avvisat
connection.on("OnWordRejected", (reason) => {
    const feedback = document.getElementById('pt-feedback');
    feedback.textContent = `⚠ ${reason}`;
    feedback.style.color = 'var(--red)';
    undoTileChange();
});

// Omgången är slut
connection.on("OnRoundResult", (data) => {
    const youWon = data.winnerId === myPlayerId;
    nextRoundStarterId = data.nextStarterId ?? null;

    const myScore = data.scores.find(s => s.id === myPlayerId);
    const opponentScore = data.scores.find(s => s.id !== myPlayerId);

    if (myScore && opponentScore) {
        scores.you = myScore.score;
        scores.opponent = opponentScore.score;
    }

    //Bestäm rätt meddelande baserat på reason
    let displayReason = data.reason;
    if (data.reason === "gaveUp") {
        displayReason = data.playerWhoGaveUpId === myPlayerId
            ? "Du gav upp."
            : "Motståndaren gav upp.";
    } else if (data.reason === "timeout") {
        displayReason = data.playerWhoTimedOutId === myPlayerId
            ? "Din tid rann ut."
            : "Motståndarens tid rann ut.";
    } else {
        displayReason = data.reason; // Fallback
    }
    showState('round-result');
    initRoundResult(youWon, displayReason);
    document.getElementById('di-roundstate').textContent = 'finished';
});

// Matchen är slut
connection.on("OnMatchResult", (data) => {
    const youWon = data.winnerId === myPlayerId;

    const myScore = data.scores.find(s => s.id === myPlayerId);
    const opponentScore = data.scores.find(s => s.id !== myPlayerId);

    if (myScore && opponentScore) {
        scores.you = myScore.score;
        scores.opponent = opponentScore.score;
    }

    showState('match-result');
});

// Felmeddelande från servern
connection.on("OnError", (message) => {
    console.error("Server error:", message);
    alert(message);
});

// ── SPELSTATE ──
let isMyTurn = false;
let currentTimerSeconds = 30;

// ── DEV PANEL ──
function togglePanel() {
    document.getElementById('dev-panel').classList.toggle('open');
}

// ── STATE SWITCHER ──
function showState(name, withTimer = true) {
    clearInterval(nextRoundInterval);
    clearInterval(timerInterval);
    clearInterval(coinFlipInterval);
    clearInterval(spectatingTimerInterval);
    hideOpponentOverlay();
    coinFlipActive = false;
    timerActive = false;
    spectatingTimerActive = false;

    console.log('showState called with: ' + name);

    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.dev-btn').forEach(b => b.classList.remove('active'));

    document.getElementById('state-' + name).classList.add('active');

    document.querySelectorAll('.dev-btn').forEach(b => {
        if (b.textContent.toLowerCase().replace(/\s/g, '') === name.replace('-', ''))
            b.classList.add('active');
    });

    document.getElementById('di-gamestate').textContent = name;

    if (name === 'coin-flip') startCoinFlip();
    if (name === 'player-turn') {
        initPlayerTurn(selectedWord || 'LUNKA');
    }
    if (name === 'spectating') initSpectating(withTimer);
    if (name === 'round-result') {
        console.log('Switched to round-result view. Waiting for external data init...');
    }
    if (name === 'match-result') initMatchResult();
}

// ── CHIP SELECTOR ──
function selectChip(groupId, el) {
    document.querySelectorAll('#' + groupId + ' .chip')
        .forEach(c => c.classList.remove('selected'));
    el.classList.add('selected');
}

// ── LOBBY – HOST GAME ──
function hostGame() {
    const setsChip = document.querySelector('#sets-group .chip.selected');
    const timeChip = document.querySelector('#time-group .chip.selected');

    const setsText = setsChip ? parseInt(setsChip.textContent) : 3;
    roundsToWin = Math.ceil(setsText / 2);
    currentTimerSeconds = timeChip ? parseInt(timeChip.textContent) : 30;

    resetLocalGameState();
    myPlayerName = "Player 1";
    myPlayerId = 1;       

    connection.invoke("HostGame", roundsToWin, currentTimerSeconds, myPlayerName)
        .catch(err => console.error("HostGame error:", err));
}

// ── LOBBY – JOIN GAME ──
function openJoinModal() {
    document.getElementById('join-modal').classList.add('open');
    document.getElementById('join-code-input').focus();
}

function closeJoinModal(event) {
    if (event && event.target !== document.getElementById('join-modal')) return;
    document.getElementById('join-modal').classList.remove('open');
}

function submitJoinCode() {
    const code = document.getElementById('join-code-input').value.trim().toUpperCase();
    if (code.length < 7) return;

    resetLocalGameState();
    myPlayerIndex = 1;
    myPlayerName = "Player 2";
    myPlayerId = 2;
    roomCode = code;
    updateRoomCodeUi(code);

    connection.invoke("JoinGame", code, myPlayerName)
        .catch(err => console.error("JoinGame error:", err));

    document.getElementById('join-code-input').value = '';
    closeJoinModal();
}

// ── COIN FLIP ──
let _coinFlipOverride = null;

function setCoinFlipWinner(winner) {
    _coinFlipOverride = winner;
}

function startCoinFlip() {
    const arrow = document.getElementById('cf-arrow');
    const result = document.getElementById('cf-result');
    const resultText = document.getElementById('cf-result-text');
    const countdown = document.getElementById('cf-countdown');
    const countEl = document.getElementById('cf-count');

    coinFlipActive = true;
    clearInterval(coinFlipInterval);
    result.style.display = 'none';
    countdown.style.display = 'none';
    document.getElementById('cf-player1').classList.remove('winner');
    document.getElementById('cf-player2').classList.remove('winner');

    const winner = _coinFlipOverride !== null ? _coinFlipOverride : Math.random() < 0.5 ? 0 : 1;

    const finalAngle = winner === 0 ? 270 : 90;
    const totalRotation = (8 * 360) + finalAngle;
    const duration = 3500;
    let startTime = null;

    function spin(timestamp) {
        if (!coinFlipActive) return;
        if (!startTime) startTime = timestamp;
        const elapsed = timestamp - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const easeOut = 1 - Math.pow(1 - progress, 3);
        arrow.style.transform = `rotate(${totalRotation * easeOut}deg)`;

        if (progress < 1) {
            requestAnimationFrame(spin);
        } else {
            if (coinFlipActive) showCoinFlipResult(winner, result, resultText, countdown, countEl);
        }
    }

    requestAnimationFrame(spin);
}

function showCoinFlipResult(winner, result, resultText, countdown, countEl) {
    document.getElementById(winner === 0 ? 'cf-player1' : 'cf-player2').classList.add('winner');

    resultText.textContent = winner === 0 ? 'Du börjar!' : 'Motståndaren börjar!';
    result.style.display = 'block';
    countdown.style.display = 'block';

    let count = 5;
    countEl.textContent = count;

    coinFlipInterval = setInterval(() => {
        if (!coinFlipActive) {
            clearInterval(coinFlipInterval);
            return;
        }
        count--;
        countEl.textContent = count;
        if (count <= 0) {
            clearInterval(coinFlipInterval);
            coinFlipActive = false;

            if (winner === 0) {
                connection.invoke("GetStartWords", roomCode)
                    .catch(err => console.error("GetStartWords error:", err));
                showState('word-select');
            } else {
                showState('spectating', false);// ← Starta INTE timern vid ordval
                showOpponentOverlay('Motståndaren väljer ett startord...');
            }
        }
    }, 1000);
}

// ── WORD SELECT ──
let selectedWord = null;

function renderWordCards(words) {
    const container = document.getElementById('word-cards');
    if (!container) return;
    container.innerHTML = '';

    words.forEach((word, i) => {
        const card = document.createElement('div');
        card.className = 'word-card';
        card.onclick = () => selectWord(card, word);
        card.innerHTML = `
            <div class="word-card-num">${i + 1}</div>
            <div class="word-card-letters">${word.toUpperCase()}</div>
            <button class="btn btn-secondary btn-sm">Välj</button>
        `;
        container.appendChild(card);
    });
}

function selectWord(cardEl, word) {
    document.querySelectorAll('.word-card').forEach(c => c.classList.remove('selected'));
    cardEl.classList.add('selected');
    selectedWord = word.toUpperCase();

    setTimeout(() => {
        connection.invoke("SelectStartWord", roomCode, word)
            .catch(err => console.error("SelectStartWord error:", err));
    }, 600);
}

// ── OPPONENT OVERLAY ──
function showOpponentOverlay(text) {
    document.getElementById('opponent-overlay-text').textContent = text;
    document.getElementById('opponent-overlay').classList.add('open');
}

function hideOpponentOverlay() {
    document.getElementById('opponent-overlay').classList.remove('open');
}

// ── PLAYER TURN ──
let currentWord = [];
let originalWord = [];
let changedIndex = null;

function initPlayerTurn(word) {
    currentWord = word.toUpperCase().split('');
    originalWord = [...currentWord];
    changedIndex = null;
    isMyTurn = true;

    renderTiles();
    updateButtons();
    startTimer(currentTimerSeconds);
    document.getElementById('di-player').textContent = 'Du';
}

function renderTiles() {
    const container = document.getElementById('pt-tiles');
    container.innerHTML = '';

    currentWord.forEach((letter, index) => {
        const tile = document.createElement('div');
        tile.className = 'tile';
        if (index === changedIndex) tile.classList.add('changed');

        const input = document.createElement('input');
        input.maxLength = 1;
        input.value = letter;

        if (changedIndex !== null && changedIndex !== index) {
            input.disabled = true;
            tile.style.opacity = '0.4';
        }

        input.addEventListener('focus', () => selectTile(index));
        input.addEventListener('input', (e) => handleTileInput(e, index));

        tile.appendChild(input);
        container.appendChild(tile);
    });
}

function selectTile(index) {
    if (changedIndex !== null && changedIndex !== index) return;
}

function handleTileInput(e, index) {
    document.getElementById('pt-feedback').textContent = '';
    const val = e.target.value.toUpperCase().slice(-1);
    e.target.value = val;

    if (val === originalWord[index]) {
        currentWord[index] = val;
        changedIndex = null;
    } else {
        currentWord[index] = val;
        changedIndex = index;
    }

    renderTiles();
    updateButtons();

    const inputs = document.querySelectorAll('#pt-tiles .tile input');
    if (inputs[index]) inputs[index].focus();
}

function undoTileChange() {
    currentWord = [...originalWord];
    changedIndex = null;
    renderTiles();
    updateButtons();
    document.getElementById('pt-feedback').textContent = '';
}

function updateButtons() {
    const hasChange = changedIndex !== null;
    document.getElementById('pt-submit-btn').disabled = !hasChange;
    document.getElementById('pt-undo-btn').disabled = !hasChange;
}

function submitWord() {
    const word = currentWord.join('').toLowerCase();
    // Ta INTE bort timern här – låt den fortsätta tills servern svarar

    connection.invoke("SubmitWord", roomCode, word)
        .catch(err => console.error("SubmitWord error:", err));
}

function giveUp() {
    clearInterval(timerInterval);
    timerActive = false;

    connection.invoke("GiveUp", roomCode)
        .catch(err => console.error("GiveUp error:", err));
}

// ── ORDHISTORIK ──
let wordHistory = [];

function addWordToHistory(word, isLatest = false) {
    wordHistory = wordHistory.map(w => ({ ...w, latest: false }));
    wordHistory.unshift({ word, latest: isLatest });
    renderWordHistory();
}

function renderWordHistory() {
    const container = document.getElementById('pt-word-history');
    if (!container) return;
    container.innerHTML = '';

    wordHistory.forEach((entry, i) => {
        const item = document.createElement('div');
        item.className = 'word-history-item' + (entry.latest ? ' latest' : '');

        const wordSpan = document.createElement('span');
        wordSpan.textContent = entry.word;

        const meta = document.createElement('span');
        meta.className = 'word-meta';
        meta.textContent = i === 0 ? 'senaste' : `#${wordHistory.length - i}`;

        item.appendChild(wordSpan);
        item.appendChild(meta);
        container.appendChild(item);
    });
}

// ── TIMER ──
let timerInterval = null;
let timerActive = false;
let coinFlipInterval = null;
let coinFlipActive = false;

function startTimer(seconds, arcId = 'timer-arc', labelId = 'timer-label') {
    clearInterval(timerInterval);
    timerActive = true;
    let remaining = seconds;
    const arc = document.getElementById(arcId);
    const label = document.getElementById(labelId);
    const circumference = 2 * Math.PI * 35;

    function update() {
        if (!timerActive) return;

        const ratio = remaining / seconds;
        arc.style.strokeDashoffset = circumference * (1 - ratio);
        arc.style.stroke = remaining <= 10 ? '#A32D2D' : '#1D9E75';
        label.style.color = remaining <= 10 ? '#A32D2D' : 'var(--text)';
        label.textContent = remaining;

        if (remaining <= 0) {
            clearInterval(timerInterval);
            timerActive = false;
            connection.invoke("TimerExpired", roomCode)
                .catch(err => console.error("TimerExpired error:", err));
        }
        remaining--;
    }

    update();
    timerInterval = setInterval(update, 1000);
}

// ── SPECTATING ──
function initSpectating(shouldStartTimer = true) {
    renderSpectatingTiles();
    renderSpectatingHistory();
    isMyTurn = false;

    // Alltid nollställ timer-displayen
    const arc = document.getElementById('sp-timer-arc');
    const label = document.getElementById('sp-timer-label');
    const circumference = 2 * Math.PI * 35;
    arc.style.strokeDashoffset = 0;
    arc.style.stroke = '#1D9E75';
    label.style.color = 'var(--text)';
    label.textContent = currentTimerSeconds;


    if (shouldStartTimer) {
        startTimer(currentTimerSeconds, 'sp-timer-arc', 'sp-timer-label');
    }

    document.getElementById('di-player').textContent = 'Motståndare';
}

function renderSpectatingTiles() {
    const container = document.getElementById('sp-tiles');
    container.innerHTML = '';

    currentWord.forEach((letter) => {
        const tile = document.createElement('div');
        tile.className = 'tile';
        tile.style.cursor = 'default';

        const input = document.createElement('input');
        input.maxLength = 1;
        input.value = letter;
        input.disabled = true;

        tile.appendChild(input);
        container.appendChild(tile);
    });
}

function renderSpectatingHistory() {
    const container = document.getElementById('sp-word-history');
    if (!container) return;
    container.innerHTML = '';

    wordHistory.forEach((entry, i) => {
        const item = document.createElement('div');
        item.className = 'word-history-item' + (entry.latest ? ' latest' : '');

        const wordSpan = document.createElement('span');
        wordSpan.textContent = entry.word;

        const meta = document.createElement('span');
        meta.className = 'word-meta';
        meta.textContent = i === 0 ? 'senaste' : `#${wordHistory.length - i}`;

        item.appendChild(wordSpan);
        item.appendChild(meta);
        container.appendChild(item);
    });
}

let spectatingTimerInterval = null;
let spectatingTimerActive = false;

// ── ROUND RESULT ──
let scores = { you: 0, opponent: 0 };
let roundsToWin = 2;

function initRoundResult(youWon, reason) {
    const btn = document.getElementById('rr-next-btn');
    const countdownText = document.getElementById('rr-countdown-text');

    if (youWon) {
        document.getElementById('rr-result-text').textContent = 'Du vann setet!';
        document.getElementById('rr-icon').textContent = '🎯';
        document.getElementById('rr-score-you-card').classList.add('active-player');
        document.getElementById('rr-score-opponent-card').classList.remove('active-player');
    } else {
        document.getElementById('rr-result-text').textContent = 'Motståndaren vann setet!';
        document.getElementById('rr-icon').textContent = '😔';
        document.getElementById('rr-score-you-card').classList.remove('active-player');
        document.getElementById('rr-score-opponent-card').classList.add('active-player');
    }

    document.getElementById('rr-reason').textContent = reason || '';
    document.getElementById('rr-score-you').textContent = scores.you;
    document.getElementById('rr-score-opponent').textContent = scores.opponent;

    renderPips();

    const matchOver = scores.you >= roundsToWin || scores.opponent >= roundsToWin;

    if (matchOver) {
        btn.textContent = 'Se resultat →';
        btn.style.display = 'block';
        btn.disabled = false;
        countdownText.style.display = 'none';
    } else {
        btn.style.display = 'none';
        startNextRoundCountdown();
    }
}

let nextRoundInterval = null;

function startNextRoundCountdown() {
    let count = 10;
    const btn = document.getElementById('rr-next-btn');
    const countdownText = document.getElementById('rr-countdown-text');

    btn.style.display = 'none';
    countdownText.style.display = 'block';
    countdownText.textContent = `Nästa set startar om ${count} sekunder...`;

    nextRoundInterval = setInterval(() => {
        count--;
        countdownText.textContent = `Nästa set startar om ${count} sekunder...`;
        if (count <= 0) {
            clearInterval(nextRoundInterval);
            if (nextRoundStarterId === myPlayerId) {
                connection.invoke("BeginNextRound", roomCode)
                    .catch(err => console.error("BeginNextRound error:", err));
            }
        }
    }, 1000);
}

function renderPips() {
    const container = document.getElementById('rr-pips');
    const label = document.getElementById('rr-pip-label');
    container.innerHTML = '';
    label.textContent = `Poäng (bäst av ${roundsToWin * 2 - 1})`;

    for (let i = 0; i < roundsToWin * 2 - 1; i++) {
        const pip = document.createElement('div');
        pip.className = 'pip' + (i < scores.you ? ' won' : '');
        container.appendChild(pip);
    }
}

function onRoundResultNext() {
    const matchOver = scores.you >= roundsToWin || scores.opponent >= roundsToWin;
    if (matchOver) {
        showState('match-result');
    } else {
        if (nextRoundStarterId === myPlayerId) {
            connection.invoke("BeginNextRound", roomCode)
                .catch(err => console.error("BeginNextRound error:", err));
        }
    }
}

// ── MATCH RESULT ──
function initMatchResult() {
    const youWon = scores.you >= roundsToWin;

    document.getElementById('mr-icon').textContent = youWon ? '🏆' : '😔';
    document.getElementById('mr-result-text').textContent = youWon ? 'Du vann matchen!' : 'Motståndaren vann matchen!';
    document.getElementById('mr-score-you').textContent = scores.you;
    document.getElementById('mr-score-opponent').textContent = scores.opponent;
    document.getElementById('mr-pip-label').textContent = `Poäng (bäst av ${roundsToWin * 2 - 1})`;

    const youCard = document.getElementById('mr-score-you-card');
    const opponentCard = document.getElementById('mr-score-opponent-card');
    youCard.classList.toggle('active-player', youWon);
    opponentCard.classList.toggle('active-player', !youWon);

    renderMatchPips();
}

function renderMatchPips() {
    const container = document.getElementById('mr-pips');
    container.innerHTML = '';

    for (let i = 0; i < roundsToWin * 2 - 1; i++) {
        const pip = document.createElement('div');
        pip.className = 'pip' + (i < scores.you ? ' won' : '');
        container.appendChild(pip);
    }
}

function resetGame() {
    resetLocalGameState();
    roomCode = null;
    myPlayerIndex = null;
    myPlayerId = null;
    myPlayerName = null;
    nextRoundStarterId = null;
    isMyTurn = false;
    updateRoomCodeUi('WD-4829');
    showState('lobby');
}
