Feature: CoinFlip

Presentation of the "random" play order between the 2 players
Will also decide which playstate to be set after the coinflip.

Scenario: Player who wins coin flip gets to select word
    Given I am on the mainpage
    And The coin flip result is forced to "0"
    When The coin flip completes
    Then The game-state is "word-select"

Scenario: Player who loses coin flip does not get to select word
    Given I am on the mainpage
    And The coin flip result is forced to "1"
    When The coin flip completes
    Then The game-state is "spectating"
