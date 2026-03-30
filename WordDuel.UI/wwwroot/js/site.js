// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ── DEV PANEL ──
function togglePanel() {
    document.getElementById('dev-panel').classList.toggle('open');
}

// ── STATE SWITCHER ──
function showState(name) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.dev-btn').forEach(b => b.classList.remove('active'));
    document.getElementById('state-' + name).classList.add('active');
    document.querySelectorAll('.dev-btn').forEach(b => {
        if (b.textContent.toLowerCase().replace(/\s/g, '') === name.replace('-', ''))
            b.classList.add('active');
    });
    document.getElementById('di-gamestate').textContent = name;

    if (name === 'coin-flip') startCoinFlip();
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
function startCoinFlip() {
    const arrow = document.getElementById('cf-arrow');
    const result = document.getElementById('cf-result');
    const resultText = document.getElementById('cf-result-text');
    const countdown = document.getElementById('cf-countdown');
    const countEl = document.getElementById('cf-count');

    // Återställ UI
    result.style.display = 'none';
    countdown.style.display = 'none';
    document.getElementById('cf-player1').classList.remove('winner');
    document.getElementById('cf-player2').classList.remove('winner');

    // Slumpa vinnare – kommer från BLL via SignalR senare
    const winner = Math.random() < 0.5 ? 0 : 1;

    // 270° = vänster = spelare 1, 90° = höger = spelare 2
    const finalAngle = winner === 0 ? 270 : 90;
    const totalRotation = (8 * 360) + finalAngle;
    const duration = 3500;
    let startTime = null;

    function spin(timestamp) {
        if (!startTime) startTime = timestamp;
        const elapsed = timestamp - startTime;
        const progress = Math.min(elapsed / duration, 1);

        // Cubic ease out: kraftig start, mjuk inbromsning
        const easeOut = 1 - Math.pow(1 - progress, 3);
        arrow.style.transform = `rotate(${totalRotation * easeOut}deg)`;

        if (progress < 1) {
            requestAnimationFrame(spin);
        } else {
            // Animationen klar – visa resultat
            showCoinFlipResult(winner, result, resultText, countdown, countEl);
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

    const interval = setInterval(() => {
        count--;
        countEl.textContent = count;
        if (count <= 0) {
            clearInterval(interval);
            // Navigera till rätt state beroende på vem som vann
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