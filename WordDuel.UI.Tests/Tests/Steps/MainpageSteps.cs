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
    }
}
