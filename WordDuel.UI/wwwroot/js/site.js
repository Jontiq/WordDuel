// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function togglePanel() {
    document.getElementById('dev-panel').classList.toggle('open');
}

function showState(name) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.dev-btn').forEach(b => b.classList.remove('active'));
    document.getElementById('state-' + name).classList.add('active');
    document.querySelectorAll('.dev-btn').forEach(b => {
        if (b.textContent.toLowerCase().replace(/\s/g, '') === name.replace('-', ''))
            b.classList.add('active');
    });
    document.getElementById('di-gamestate').textContent = name;

    // Starta animation automatiskt
    if (name === 'coin-flip') startCoinFlip();
}

//Löser så att man kan selecta och presentera vad som är selected bättre.
function selectChip(groupId, el) {
    document.querySelectorAll('#' + groupId + ' .chip')
        .forEach(c => c.classList.remove('selected'));
    el.classList.add('selected');
}

function openJoinModal() {
    document.getElementById('join-modal').classList.add('open');
    document.getElementById('join-code-input').focus();
}

function closeJoinModal(event) {
    // Stäng bara om man klickar på overlayen, inte på modal-boxen
    if (event && event.target !== document.getElementById('join-modal')) return;
    document.getElementById('join-modal').classList.remove('open');
}

function submitJoinCode() {
    const code = document.getElementById('join-code-input').value;
    if (code.length < 7) return;
    closeJoinModal();
    showState('waiting');
}

/*COIN-FLIP*/
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

    const winner = Math.random() < 0.5 ? 0 : 1;

    // Inställningar för animationen
    const finalAngle = winner === 0 ? 270 : 90;
    const extraSpins = 8; // Hur många extra varv den ska snurra (3 * 360)
    const totalRotation = (extraSpins * 360) + finalAngle;

    const duration = 3500; // Animationen tar 3.5 sekunder
    let startTime = null;

    function spin(timestamp) {
        if (!startTime) startTime = timestamp;
        const elapsed = timestamp - startTime;
        const progress = Math.min(elapsed / duration, 1);

        // Easing-funktion: "Cubic Ease Out"
        // Formel: 1 - Math.pow(1 - progress, 3)
        // Ger en kraftig start och en mycket mjuk inbromsning mot slutet
        const easeOut = 1 - Math.pow(1 - progress, 3);

        const currentRotation = totalRotation * easeOut;
        arrow.style.transform = `rotate(${currentRotation}deg)`;

        if (progress < 1) {
            requestAnimationFrame(spin);
        } else {
            showCoinFlipResult(winner, result, resultText, countdown, countEl);
        }
    }

    requestAnimationFrame(spin);
}

function showCoinFlipResult(winner, result, resultText, countdown, countEl) {
    const winnerEl = document.getElementById(winner === 0 ? 'cf-player1' : 'cf-player2');
    winnerEl.classList.add('winner');

    resultText.textContent = winner === 0 ? 'Du börjar!' : 'Motståndaren börjar!';
    result.style.display = 'block';
    countdown.style.display = 'block';

    let count = 5;
    countEl.textContent = count;

    const interval = setInterval(() => {
        count--;
        countEl.textContent = count;
        if (count <= 0) {
            clearInterval(interval);
            // Om du börjar -> word-select, annars -> spectating
            showState(winner === 0 ? 'word-select' : 'spectating');
        }
    }, 1000);
}