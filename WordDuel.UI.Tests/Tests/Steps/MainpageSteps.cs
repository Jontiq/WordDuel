using Microsoft.Playwright;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
