using Microsoft.Playwright;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions;

namespace WordDuel.UI.Tests.Tests.Steps
{
    [Binding]
    public class LobbySteps
    {
        private IPage _page = null!;
        private IBrowser _browser = null!;
        private IPlaywright _playwright = null!;

        [BeforeScenario]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            _page = await _browser.NewPageAsync();
        }

        [AfterScenario]
        public async Task Teardown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Given("I am on the mainpage")]
        public async Task GivenIAmOnTheMainpage()
        {
            await _page.GotoAsync("http://localhost:5227");
        }

        [Then("{string} is selected")]
        public async Task ThenIsSelected(string label)
        {
            var chip = _page.Locator(".chip.selected", new() { HasTextString = label });
            await Assertions.Expect(chip).ToBeVisibleAsync();
        }

        [When("I click on {string}")]
        public async Task WhenIClickOn(string label)
        {
            var locator = _page.Locator(".chip, button", new() { HasTextString = label });
            await locator.ClickAsync();
        }

        [Then("{string} is not selected")]
        public async Task ThenIsNotSelected(string label)
        {
            var chip = _page.Locator(".chip.selected", new() { HasTextString = label });
            await Assertions.Expect(chip).Not.ToBeVisibleAsync();
        }

        [Then("The game-state is {string}")]
        public async Task ThenTheGameStateIs(string expectedState)
        {
            await Assertions.Expect(_page.Locator("#di-gamestate"))
                    .ToHaveTextAsync(expectedState);
        }

        [Then("The join modal is visible")]
        public async Task ThenTheJoinModalIsVisible()
        {
            await Assertions.Expect(_page.Locator("#join-modal"))
                            .ToHaveClassAsync(new Regex("open"));
        }

        [Given("The join modal is open")]
        public async Task GivenTheJoinModalIsOpen()
        {
            await _page.Locator("button", new() { HasTextString = "JOIN GAME" }).ClickAsync();
        }

        [When("I enter the code {string}")]
        public async Task WhenIEnterTheCode(string code)
        {
            await _page.Locator("#join-code-input").FillAsync(code);
        }

        [Then("The join modal is not visible")]
        public async Task ThenTheJoinModalIsNotVisible()
        {
            await Assertions.Expect(_page.Locator("#join-modal"))
                            .Not.ToHaveClassAsync(new Regex("open"));
        }

        [Then("The join modal is still visible")]
        public async Task ThenTheJoinModalIsStillVisible()
        {
            await Assertions.Expect(_page.Locator("#join-modal"))
                            .ToHaveClassAsync(new Regex("open"));
        }

        [Given("The coin flip result is forced to {string}")]
        public async Task GivenTheCoinFlipResultIsForcedTo(string winner)
        {
            await _page.EvaluateAsync($"setCoinFlipWinner({winner})");
        }

        [When("The coin flip completes")]
        public async Task WhenTheCoinFlipCompletes()
        {
            await _page.EvaluateAsync("showState('coin-flip')");
            // Vänta på att animationen + countdownen är klar (3.5s + 5s)
            await _page.WaitForTimeoutAsync(9000);
        }

        [When("I click the modal button {string}")]
        public async Task WhenIClickTheModalButton(string label)
        {
            await _page.Locator("#join-modal button", new() { HasTextString = label }).ClickAsync();
        }

        [Given("I navigate to player turn with word {string}")]
        public async Task GivenINavigateToPlayerTurnWithWord(string word)
        {
            await _page.EvaluateAsync($"selectedWord = '{word}'");
            await _page.EvaluateAsync("showState('player-turn')");
        }

        [Then("The submit button is disabled")]
        public async Task ThenTheSubmitButtonIsDisabled()
        {
            await Assertions.Expect(_page.Locator("#pt-submit-btn"))
                            .ToBeDisabledAsync();
        }

        [Then("The submit button is enabled")]
        public async Task ThenTheSubmitButtonIsEnabled()
        {
            await Assertions.Expect(_page.Locator("#pt-submit-btn"))
                            .ToBeEnabledAsync();
        }

        [When("I change letter at position {string} to {string}")]
        public async Task WhenIChangeLetterAtPositionTo(string position, string letter)
        {
            var index = int.Parse(position);
            var inputs = _page.Locator("#pt-tiles .tile input");
            await inputs.Nth(index).FillAsync(letter);
            await inputs.Nth(index).DispatchEventAsync("input");
        }

        [Then("The other tiles are locked")]
        public async Task ThenTheOtherTilesAreLocked()
        {
            var inputs = _page.Locator("#pt-tiles .tile input[disabled]");
            var count = await inputs.CountAsync();
            Assert.That(count, Is.EqualTo(4));
        }

        [Then("The tile at position {string} shows {string}")]
        public async Task ThenTheTileAtPositionShows(string position, string letter)
        {
            var index = int.Parse(position);
            var input = _page.Locator("#pt-tiles .tile input").Nth(index);
            await Assertions.Expect(input).ToHaveValueAsync(letter);
        }

        [When("The timer expires")]
        public async Task WhenTheTimerExpires()
        {
            await _page.EvaluateAsync("timerActive = false; showState('round-result')");
        }

        [Given("I navigate to spectating")]
        public async Task GivenINavigateToSpectating()
        {
            await _page.EvaluateAsync("showState('spectating')");
        }

        [Then("All tiles are disabled")]
        public async Task ThenAllTilesAreDisabled()
        {
            var tiles = _page.Locator("#sp-tiles .tile input");
            var count = await tiles.CountAsync();
            for (int i = 0; i < count; i++)
            {
                await Assertions.Expect(tiles.Nth(i)).ToBeDisabledAsync();
            }
        }

        [Then("The submit button is not visible")]
        public async Task ThenTheSubmitButtonIsNotVisible()
        {
            await Assertions.Expect(_page.Locator("#pt-submit-btn"))
                            .Not.ToBeVisibleAsync();
        }

        [Then("The undo button is not visible")]
        public async Task ThenTheUndoButtonIsNotVisible()
        {
            await Assertions.Expect(_page.Locator("#pt-undo-btn"))
                            .Not.ToBeVisibleAsync();
        }

        [Then("The give up button is not visible")]
        public async Task ThenTheGiveUpButtonIsNotVisible()
        {
            await Assertions.Expect(_page.Locator("button", new() { HasTextString = "Ge upp" }))
                            .Not.ToBeVisibleAsync();
        }

        [Then("The opponent turn badge is visible")]
        public async Task ThenTheOpponentTurnBadgeIsVisible()
        {
            await Assertions.Expect(_page.Locator(".badge-amber"))
                            .ToBeVisibleAsync();
        }

        [When("The opponent is selecting a start word")]
        public async Task WhenTheOpponentIsSelectingAStartWord()
        {
            await _page.EvaluateAsync("showOpponentOverlay('Motståndaren väljer ett startord...')");
        }

        [Then("The opponent overlay is visible")]
        public async Task ThenTheOpponentOverlayIsVisible()
        {
            await Assertions.Expect(_page.Locator("#opponent-overlay"))
                            .ToHaveClassAsync(new Regex("open"));
        }

        [Then("The opponent overlay shows {string}")]
        public async Task ThenTheOpponentOverlayShows(string text)
        {
            await Assertions.Expect(_page.Locator("#opponent-overlay-text"))
                            .ToHaveTextAsync(text);
        }

        [Given("I navigate to round result where I won")]
        public async Task GivenINavigateToRoundResultWhereIWon()
        {
            await _page.EvaluateAsync("scores = { you: 0, opponent: 0 }");
            await _page.EvaluateAsync("showState('round-result')");
            await _page.EvaluateAsync("initRoundResult(true, 'Motståndaren gick ut på tid.')");
        }

        [Given("I navigate to round result where I lost")]
        public async Task GivenINavigateToRoundResultWhereILost()
        {
            await _page.EvaluateAsync("scores = { you: 0, opponent: 0 }");
            await _page.EvaluateAsync("showState('round-result')");
            await _page.EvaluateAsync("initRoundResult(false, 'Du gick ut på tid.')");
        }

        [Given("The match is won by the player")]
        public async Task GivenTheMatchIsWonByThePlayer()
        {
            await _page.EvaluateAsync("scores = { you: 0, opponent: 0 }");
            await _page.EvaluateAsync("roundsToWin = 2");
            // Simulera att spelaren vunnit 2 set i rad
            await _page.EvaluateAsync("scores.you = 1");
            await _page.EvaluateAsync("showState('round-result')");
            await _page.EvaluateAsync("initRoundResult(true, '')");
        }

        [Then("The result text shows {string}")]
        public async Task ThenTheResultTextShows(string text)
        {
            await Assertions.Expect(_page.Locator("#rr-result-text"))
                            .ToHaveTextAsync(text);
        }

        [Then("The player score shows {string}")]
        public async Task ThenThePlayerScoreShows(string score)
        {
            await Assertions.Expect(_page.Locator("#rr-score-you"))
                            .ToHaveTextAsync(score);
        }

        [Then("The opponent score shows {string}")]
        public async Task ThenTheOpponentScoreShows(string score)
        {
            await Assertions.Expect(_page.Locator("#rr-score-opponent"))
                            .ToHaveTextAsync(score);
        }

        [Then("The next button shows {string}")]
        public async Task ThenTheNextButtonShows(string text)
        {
            await Assertions.Expect(_page.Locator("#rr-next-btn"))
                            .ToHaveTextAsync(text);
        }

        [When("I click the next round button")]
        public async Task WhenIClickTheNextRoundButton()
        {
            await _page.Locator("#rr-next-btn").ClickAsync();
        }

        [Then("The game-state eventually becomes {string}")]
        public async Task ThenTheGameStateEventuallyBecomes(string expectedState)
        {
            await Assertions.Expect(_page.Locator("#di-gamestate"))
                            .ToHaveTextAsync(expectedState, new() { Timeout = 15000 });
        }
    }
}
