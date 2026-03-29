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