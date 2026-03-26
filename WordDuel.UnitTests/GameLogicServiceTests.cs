
using WordDuel.BLL.Enums;
using WordDuel.BLL.GameLogicServices;

namespace WordDuel.UnitTests
{
    public class GameLogicServiceTests
    {
        //======================CreateGame===========================================
        [Fact]
        public void CreateGame_ShouldReturnGameWithCorrectRoundsToWinAndWaitingForPlayersState()
        {
            // Arrange
            var service = new GameService();

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
            // Arrange
            var service = new GameService();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => service.CreateGame(0));
        }

        //============================AddPlayers=======================================

        [Fact]
        public void AddPlayers_ShouldAddPlayerToGame ()
        {
            // Arrange 
            var service = new GameService();
            var game = service.CreateGame(3);
            // Act
            service.AddPlayers(game, "Bob");
            // Assert
            Assert.Single(game.Players);
            Assert.Equal("Bob", game.Players[0].Name);
        }

        [Fact]
        public void AddPlayers_ShouldAddTwoPlayersToGame()
        {
            // Arrange 
            var service = new GameService();
            var game = service.CreateGame(3);
            // Act
            service.AddPlayers(game, "Bob");
            service.AddPlayers(game, "Anna");
            // Assert
            Assert.Equal(2, game.Players.Count);
            Assert.Equal("Bob", game.Players[0].Name);
            Assert.Equal("Anna", game.Players[1].Name);
        }

        [Fact]
        public void AddPlayers_ShouldThrowException_WhenAddingThreePlayers()
        {
            // Arrange 
            var service = new GameService();
            var game = service.CreateGame(3);      
            service.AddPlayers(game, "Bob");
            service.AddPlayers(game, "Anna");

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => service.AddPlayers(game, "Lisa"));
        }

        [Fact]
        public void AddPlayer_ShouldAssignDefaultName_WhenNameIsEmpty()
        {
            //Arrange
            var service = new GameService();
            var game = service.CreateGame(3);
            //Act
            service.AddPlayers(game, "");
            service.AddPlayers(game, null);
            //Assert
            Assert.Equal("Player 1", game.Players[0].Name);
            Assert.Equal("Player 2", game.Players[1].Name);
        }
    }
}
