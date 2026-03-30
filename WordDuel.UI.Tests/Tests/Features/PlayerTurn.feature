Feature: PlayerTurn

Player turn is where the player gets to change the word, give up, reset their change or lose a point.

Scenario: Submit button is disabled when word is unchanged
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    Then The submit button is disabled

  Scenario: Submit button is enabled after changing a letter
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    When I change letter at position "0" to "B"
    Then The submit button is enabled

  Scenario: Player can only change one letter
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    When I change letter at position "0" to "B"
    Then The other tiles are locked

  Scenario: Undo restores original word
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    When I change letter at position "0" to "B"
    And I click on "Ångra"
    Then The submit button is disabled
    And The tile at position "0" shows "L"

  Scenario: Give up navigates to round result
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    When I click on "Ge upp"
    Then The game-state is "round-result"

  Scenario: Timer expiry navigates to round result
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    When The timer expires
    Then The game-state is "round-result"
