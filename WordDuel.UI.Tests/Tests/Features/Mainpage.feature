Feature: Mainpage

Will be the landing page for the user where they will be able to set up the game with rules and host a game, or join an already configured game.

Scenario: Page is loaded with standard game-settings
	Given I am on the mainpage
	Then "3 set" is selected
	And "30 sek" is selected

Scenario: Change sets
	Given I am on the mainpage
	When I click on "5 set"
	Then "5 set" is selected
	And "3 set" is not selected

Scenario: Change roundtime
	Given I am on the mainpage
	When I click on "15 sek"
	Then "15 sek" is selected
	And "30 sek" is not selected

Scenario: Click "Host game"
    Given I am on the mainpage
    When I click on "HOST GAME"
    Then The game-state is "waiting"

Scenario: Click "JOIN GAME" opens modal
    Given I am on the mainpage
    When I click on "JOIN GAME"
    Then The join modal is visible

Scenario: Close join modal with cancel
    Given I am on the mainpage
    And The join modal is open
    When I click the modal button "Avbryt"
    Then The join modal is not visible

Scenario: Submit join code navigates to waiting
    Given I am on the mainpage
    And The join modal is open
    When I enter the code "WD-1234"
    And I click on "Gå med"
    Then The game-state is "waiting"

Scenario: Submit button disabled with incomplete code
    Given I am on the mainpage
    And The join modal is open
    When I enter the code "WD-12"
    And I click on "Gå med"
    Then The join modal is still visible