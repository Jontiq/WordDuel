
using WordDuel.BLL.Enums;
using WordDuel.BLL.GameLogicModels;
using WordDuel.BLL.GameLogicServices;

namespace WordDuel.UnitTests
{
    public class GameServiceTests
    {
        //Helpers
        private readonly GameService service;
        public GameServiceTests()
        {
            service = new GameService(new Random());
            
        }

        private Game CreateGameWithTwoPlayers()
        {
            var game = service.CreateGame(3);
            service.AddPlayer(game, "A");
            service.AddPlayer(game, "B");
            return game;
        }

        //======================CreateGame===========================================
        [Fact]
        public void CreateGame_ShouldReturnGameWithCorrectRoundsToWinAndWaitingForPlayersState()
        {

            // Act
            var result = service.CreateGame(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.RoundsToWin);
            Assert.Equal(GameState.WaitingForPlayers, result.State);
        }
        
        [Fact]
        public void CreateGame_ShouldThrowException_WhenRoundsToWinIsLessThanOrEqualToZero()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => service.CreateGame(0));
        }

        //============================AddPlayers=======================================

        [Fact]
        public void AddPlayers_ShouldAddPlayerToGame ()
        {
            // Arrange 
            var game = service.CreateGame(3);
            // Act
            service.AddPlayer(game, "A");
            // Assert
            Assert.Single(game.Players);
            Assert.Equal("A", game.Players[0].Name);
        }

        [Fact]
        public void AddPlayers_ShouldAddTwoPlayersToGame()
        {
           var game = CreateGameWithTwoPlayers();
            // Assert
            Assert.Equal(2, game.Players.Count);
            Assert.Equal("A", game.Players[0].Name);
            Assert.Equal("B", game.Players[1].Name);
        }

        [Fact]
        public void AddPlayers_ShouldThrowException_WhenAddingThreePlayers()
        {
            // Arrange 
            var game = CreateGameWithTwoPlayers();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => service.AddPlayer(game, "Lisa"));
        }

        [Fact]
        public void AddPlayer_ShouldAssignDefaultName_WhenNameIsEmpty()
        {
            //Arrange
            var game = service.CreateGame(3);
            //Act
            service.AddPlayer(game, "");
            service.AddPlayer(game, null);
            //Assert
            Assert.Equal("Player 1", game.Players[0].Name);
            Assert.Equal("Player 2", game.Players[1].Name);
        }

        [Fact]
        public void AddPlayer_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var game = service.CreateGame(3);

            // Act
            service.AddPlayer(game, "   Anna   ");

            // Assert
            Assert.Single(game.Players);
            Assert.Equal("Anna", game.Players[0].Name);
        }

        //====================StartGame=================================

        [Fact]
        public void StartGame_ShouldThrowException_WhenLessThanTwoPlayers()
        {
            // Arrange
            var game = service.CreateGame(3);
            service.AddPlayer(game, "Anna");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.StartGame(game));
        }

        [Fact]
        public void StartGame_ShouldSetStateToInProgress()
        {
            // Arrange
            var game = CreateGameWithTwoPlayers();

            // Act
            service.StartGame(game);

            // Assert
            Assert.Equal(GameState.InProgress, game.State);
        }
        [Fact]
        public void StartGame_ShouldSetCurrentPlayer()
        {
            // Arrange
            var game = CreateGameWithTwoPlayers();

            // Act
            service.StartGame(game);

            // Assert
            Assert.NotNull(game.CurrentPlayer);
            Assert.Contains(game.CurrentPlayer, game.Players);
        }

        [Fact]
        public void StartGame_ShouldCreateFirstRound()
        {
            // Arrange
            var game = CreateGameWithTwoPlayers();

            // Act
            service.StartGame(game);

            // Assert
            Assert.Single(game.Rounds);
            Assert.Equal(1, game.CurrentRoundNumber);
        }
    }
}
