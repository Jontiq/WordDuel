Feature: Spectating

Through the spectation the player will be able to view the opponents actions live, but not make any changes to the game

Scenario: Tiles are not interactable during spectating
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    And I navigate to spectating
    Then All tiles are disabled

  Scenario: No action buttons are visible during spectating
    Given I am on the mainpage
    And I navigate to player turn with word "LUNKA"
    And I navigate to spectating
    Then The submit button is not visible
    And The undo button is not visible
    And The give up button is not visible

  Scenario: Opponent turn badge is visible during spectating
    Given I am on the mainpage
    And I navigate to spectating
    Then The opponent turn badge is visible

  Scenario: Opponent overlay shows when opponent is selecting start word
    Given I am on the mainpage
    And I navigate to spectating
    When The opponent is selecting a start word
    Then The opponent overlay is visible
    And The opponent overlay shows "Motståndaren väljer ett startord..."
