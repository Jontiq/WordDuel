Feature: RoundResult

Vierw for presenting the current scores between "rounds". 

Scenario: Winner text is shown when player wins a round
    Given I am on the mainpage
    And I navigate to round result where I won
    Then The result text shows "Du vann setet!"

  Scenario: Loser text is shown when player loses a round
    Given I am on the mainpage
    And I navigate to round result where I lost
    Then The result text shows "Motståndaren vann setet!"

  Scenario: Score is updated when player wins
    Given I am on the mainpage
    And I navigate to round result where I won
    Then The player score shows "1"
    And The opponent score shows "0"

  Scenario: Score is updated when player loses
    Given I am on the mainpage
    And I navigate to round result where I lost
    Then The player score shows "0"
    And The opponent score shows "1"

  Scenario: Automatically navigates to coin flip when round ends and match is not over
    Given I am on the mainpage
    And I navigate to round result where I won
    Then The game-state eventually becomes "coin-flip"

  Scenario: Match result button shown when match is over
    Given I am on the mainpage
    And The match is won by the player
    Then The next button shows "Se resultat →"

  Scenario: Next button navigates to match result when match is over
    Given I am on the mainpage
    And The match is won by the player
    When I click the next round button
    Then The game-state is "match-result"