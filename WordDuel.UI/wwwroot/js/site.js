// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ── DEV PANEL ──
function togglePanel() {
    document.getElementById('dev-panel').classList.toggle('open');
}

// ── STATE SWITCHER ──
function showState(name) {
    clearInterval(nextRoundInterval);
    clearInterval(timerInterval);
    clearInterval(coinFlipInterval);
    clearInterval(spectatingTimerInterval);
    hideOpponentOverlay();
    coinFlipActive = false;
    timerActive = false;
    spectatingTimerActive = false;
    

    console.log('showState called with: ' + name);

    // Hantera synlighet för sidor och knappar
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.dev-btn').forEach(b => b.classList.remove('active'));

    document.getElementById('state-' + name).classList.add('active');

    document.querySelectorAll('.dev-btn').forEach(b => {
        if (b.textContent.toLowerCase().replace(/\s/g, '') === name.replace('-', ''))
            b.classList.add('active');
    });

    document.getElementById('di-gamestate').textContent = name;

    // Specifik initiering per state
    if (name === 'coin-flip') startCoinFlip();
    if (name === 'player-turn') {
        wordHistory = [];
        initPlayerTurn(selectedWord || 'LUNKA');
    }
    if (name === 'spectating') initSpectating();

    // NYTT: Nu byter vi bara till vyn, men anropar inte initRoundResult härifrån
    if (name === 'round-result') {
        // initRoundResult anropas separat av spelflödet eller tester
        console.log('Switched to round-result view. Waiting for external data init...');
    }
}

// ── CHIP SELECTOR ──
function selectChip(groupId, el) {
    document.querySelectorAll('#' + groupId + ' .chip')
        .forEach(c => c.classList.remove('selected'));
    el.classList.add('selected');
}

// ── JOIN MODAL ──
function openJoinModal() {
    document.getElementById('join-modal').classList.add('open');
    document.getElementById('join-code-input').focus();
}

function closeJoinModal(event) {
    if (event && event.target !== document.getElementById('join-modal')) return;
    document.getElementById('join-modal').classList.remove('open');
}

function submitJoinCode() {
    const code = document.getElementById('join-code-input').value;
    if (code.length < 7) return;
    closeJoinModal();
    showState('waiting');
}

// ── COIN FLIP ──

// Används av tester för att tvinga ett utfall
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

    // Återställ UI
    coinFlipActive = true;
    clearInterval(coinFlipInterval);
    result.style.display = 'none';
    countdown.style.display = 'none';
    document.getElementById('cf-player1').classList.remove('winner');
    document.getElementById('cf-player2').classList.remove('winner');

    // Slumpa vinnare – kommer från BLL via SignalR senare (Kan anropas av tester just nu)
    const winner = _coinFlipOverride !== null ? _coinFlipOverride : Math.random() < 0.5 ? 0 : 1;


    // 270° = vänster = spelare 1, 90° = höger = spelare 2
    const finalAngle = winner === 0 ? 270 : 90;
    const totalRotation = (8 * 360) + finalAngle;
    const duration = 3500;
    let startTime = null;

    function spin(timestamp) {
        if (!coinFlipActive) return; // ← avbryt om vi navigerat bort
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
    // Highlighta vinnaren
    document.getElementById(winner === 0 ? 'cf-player1' : 'cf-player2').classList.add('winner');

    resultText.textContent = winner === 0 ? 'Du börjar!' : 'Motståndaren börjar!';
    result.style.display = 'block';
    countdown.style.display = 'block';

    // Countdown 5 → 0
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
                showState('word-select');
            } else {
                showState('spectating');
                showOpponentOverlay('Motståndaren väljer ett startord...');
            }
        }
    }, 1000);
}

// ── WORD SELECT ──
let selectedWord = null;

function selectWord(cardEl, word) {
    document.querySelectorAll('.word-card').forEach(c => c.classList.remove('selected'));
    cardEl.classList.add('selected');
    selectedWord = word;

    // Kort fördröjning sen gå vidare till spelarens tur
    setTimeout(() => {
        showState('player-turn');
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

    renderTiles();
    updateButtons();
    addWordToHistory(word, true);
    startTimer(30);
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

        // Lås alla andra rutor om en redan är ändrad
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
    const val = e.target.value.toUpperCase().slice(-1);
    e.target.value = val;

    if (val === originalWord[index]) {
        // Återställd till original
        currentWord[index] = val;
        changedIndex = null;
    } else {
        currentWord[index] = val;
        changedIndex = index;
    }

    renderTiles();
    updateButtons();

    // Fokusera rätt input efter re-render
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
    const word = currentWord.join('');
    addWordToHistory(word, true);
    showState('spectating');
}

// ── ORDHISTORIK ──
let wordHistory = [];

function addWordToHistory(word, isLatest = false) {
    // Ta bort latest-markering från tidigare
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

function startTimer(seconds) {
    clearInterval(timerInterval);
    timerActive = true;
    let remaining = seconds;
    const arc = document.getElementById('timer-arc');
    const label = document.getElementById('timer-label');
    const circumference = 2 * Math.PI * 35;

    function update() {
        if (!timerActive) return; // ← avbryt om vi navigerat bort

        const ratio = remaining / seconds;
        arc.style.strokeDashoffset = circumference * (1 - ratio);
        arc.style.stroke = remaining <= 10 ? '#A32D2D' : '#1D9E75';
        label.style.color = remaining <= 10 ? '#A32D2D' : 'var(--text)';
        label.textContent = remaining;

        if (remaining <= 0) {
            clearInterval(timerInterval);
            timerActive = false;
            showState('round-result');
        }
        remaining--;
    }

    update();
    timerInterval = setInterval(update, 1000);
}

// ── SPECTATING ──
function initSpectating() {
    renderSpectatingTiles();
    renderSpectatingHistory();
    startSpectatingTimer(30);
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

function startSpectatingTimer(seconds) {
    clearInterval(spectatingTimerInterval);
    spectatingTimerActive = true;
    let remaining = seconds;
    const arc = document.getElementById('sp-timer-arc');
    const label = document.getElementById('sp-timer-label');
    const circumference = 2 * Math.PI * 35;

    function update() {
        if (!spectatingTimerActive) return;
        const ratio = remaining / seconds;
        arc.style.strokeDashoffset = circumference * (1 - ratio);
        arc.style.stroke = remaining <= 10 ? '#A32D2D' : '#1D9E75';
        label.style.color = remaining <= 10 ? '#A32D2D' : 'var(--text)';
        label.textContent = remaining;

        if (remaining <= 0) {
            clearInterval(spectatingTimerInterval);
            spectatingTimerActive = false;
            // Motståndaren gick ut på tid – vi vinner
            showState('round-result');
        }
        remaining--;
    }

    update();
    spectatingTimerInterval = setInterval(update, 1000);
}

// ── ROUND RESULT ──
let scores = { you: 0, opponent: 0 };
let roundsToWin = 2; // bäst av 3 = 2, bäst av 5 = 3 osv – kommer från lobby-valet senare

function initRoundResult(youWon, reason) {
    if (youWon) {
        scores.you++;
        document.getElementById('rr-result-text').textContent = 'Du vann setet!';
        document.getElementById('rr-icon').textContent = '🎯';
        document.getElementById('rr-score-you-card').classList.add('active-player');
        document.getElementById('rr-score-opponent-card').classList.remove('active-player');
    } else {
        scores.opponent++;
        document.getElementById('rr-result-text').textContent = 'Motståndaren vann setet!';
        document.getElementById('rr-icon').textContent = '😔';
        document.getElementById('rr-score-you-card').classList.remove('active-player');
        document.getElementById('rr-score-opponent-card').classList.add('active-player');
    }

    document.getElementById('rr-reason').textContent = reason || '';
    document.getElementById('rr-score-you').textContent = scores.you;
    document.getElementById('rr-score-opponent').textContent = scores.opponent;

    renderPips();

    // Kolla om matchen är slut
    const matchOver = scores.you >= roundsToWin || scores.opponent >= roundsToWin;
    const btn = document.getElementById('rr-next-btn');

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
            showState('coin-flip');
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
        showState('coin-flip');
    }
}