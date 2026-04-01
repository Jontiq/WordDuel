Feature: MatchResult

The final view, will present the final results and who won / lost and scores.

Scenario: Winner text is shown when player wins the match
    Given I am on the mainpage
    And The player has won the match
    Then The match result text shows "Du vann matchen!"

  Scenario: Loser text is shown when player loses the match
    Given I am on the mainpage
    And The opponent has won the match
    Then The match result text shows "Motståndaren vann matchen!"

  Scenario: Final scores are shown correctly
    Given I am on the mainpage
    And The player has won the match
    Then The final player score shows "2"
    And The final opponent score shows "1"

  Scenario: Till lobbyn button resets and navigates to lobby
    Given I am on the mainpage
    And The player has won the match
    When I click on "Till lobbyn"
    Then The game-state is "lobby"
