// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function togglePanel() {
    document.getElementById('dev-panel').classList.toggle('open');
}

function showState(name) {
    // ... befintlig kod ...

    // Uppdatera dev-panelen
    document.getElementById('di-gamestate').textContent = name;
}

//Löser så att man kan selecta och presentera vad som är selected bättre.
function selectChip(groupId, el) {
    document.querySelectorAll('#' + groupId + ' .chip')
        .forEach(c => c.classList.remove('selected'));
    el.classList.add('selected');
}