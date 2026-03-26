
using WordDuel.BLL.Enums;
using WordDuel.BLL.GameLogicServices;

namespace WordDuel.UnitTests
{
    public class GameLogicServiceTests
    {
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
    }
}
