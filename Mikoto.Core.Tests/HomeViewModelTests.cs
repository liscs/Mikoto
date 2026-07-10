using CommunityToolkit.Mvvm.Messaging;
using Mikoto.Core.Interfaces;
using Mikoto.Core.Models;
using Mikoto.Core.ViewModels;
using Mikoto.Core.ViewModels.AddGame;
using Mikoto.DataAccess;
using Moq;
using Xunit;

namespace Mikoto.Core.Tests;

public class HomeViewModelTests : IDisposable // 实现 IDisposable 用于统一清理 Messenger
{
    private readonly Mock<IAppEnvironment> _mockEnv;
    private readonly Mock<IGameInfoService> _mockGameService;
    private readonly HomeViewModel _viewModel;

    public HomeViewModelTests()
    {
        _mockEnv = new Mock<IAppEnvironment>();
        _mockGameService = new Mock<IGameInfoService>();

        // 组装 Mock 对象
        _mockEnv.Setup(e => e.GameInfoService).Returns(_mockGameService.Object);

        _viewModel = new HomeViewModel(_mockEnv.Object);
    }

    [Fact]
    public async Task LoadGamesAsync_ShouldPopulateGames_OrderedByLastPlayAt()
    {
        // 1. Arrange
        var fakeData = new List<GameInfo>
        {
            new() { GameID = Guid.NewGuid(), FilePath = "A", LastPlayAt = DateTime.Now.AddDays(-1) },
            new() { GameID = Guid.NewGuid(), FilePath = "B", LastPlayAt = DateTime.Now }
        };

        _mockGameService.Setup(s => s.AllCompletedGamesIdDict)
            .Returns(fakeData.ToDictionary(g => g.GameID));

        Func<string, Task<object?>> mockGetIcon = path => Task.FromResult((object?)$"Icon_{path}");

        // 2. Act
        await _viewModel.LoadGamesCommand.ExecuteAsync(mockGetIcon);

        // 3. Assert
        Assert.Equal(2, _viewModel.Games.Count);
        Assert.Equal("B", _viewModel.Games[0].GameInfo.FilePath); // 验证排序（B最新，应在首位）
        Assert.Equal("Icon_B", _viewModel.Games[0].GameIcon);
        _mockGameService.Verify(s => s.GetAllCompletedGames(), Times.Once);
    }

    [Fact]
    public void AddGame_ShouldSendNavigationMessage()
    {
        // 1. Arrange
        NavigationMessage? receivedMessage = null;
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            receivedMessage = m;
        });

        // 2. Act
        _viewModel.AddGameCommand.Execute(null);

        // 3. Assert
        Assert.NotNull(receivedMessage);
        Assert.Equal(typeof(AddGameViewModel), receivedMessage!.ViewModelType);
    }

    [Fact]
    public void AutoAttachGame_WhenGameRunning_ShouldSendMessageWithGameInfo()
    {
        // Arrange
        var runningGame = new GameInfo { FilePath = "Running.exe" };
        _mockGameService.Setup(s => s.GetRunningGame()).Returns(runningGame);

        NavigationMessage? sentMsg = null;
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) => sentMsg = m);

        // Act
        _viewModel.AutoAttachGameCommand.Execute(null);

        // Assert
        Assert.NotNull(sentMsg);
        Assert.Same(runningGame, sentMsg!.Parameter); // 验证引用一致性
    }

    // 每个测试运行完后都会调用 Dispose，确保 Messenger 干净
    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    [Fact]
    public async Task LoadGamesAsync_WhenIconFunctionThrows_ShouldHandleException()
    {
        // Arrange
        var fakeData = new List<GameInfo> { new() { GameID = Guid.NewGuid(), FilePath = "Broken.exe" } };
        _mockGameService.Setup(s => s.AllCompletedGamesIdDict).Returns(fakeData.ToDictionary(g => g.GameID));

        // 模拟一个会抛出异常的委托
        Func<string, Task<object?>> brokenFunc = p => throw new Exception("Disk Error");

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _viewModel.LoadGamesCommand.ExecuteAsync(brokenFunc));
    }

    [Fact]
    public async Task LoadGamesAsync_WhenNoGamesFound_ShouldResultInEmptyCollection()
    {
        // Arrange
        _mockGameService.Setup(s => s.AllCompletedGamesIdDict).Returns(new Dictionary<Guid, GameInfo>());

        // Act
        await _viewModel.LoadGamesCommand.ExecuteAsync(p => Task.FromResult<object?>(null));

        // Assert
        Assert.Empty(_viewModel.Games);
    }

    [Fact]
    public void AutoAttachGame_WhenNoGameIsRunning_ShouldNotSendAnyMessage()
    {
        // Arrange
        _mockGameService.Setup(s => s.GetRunningGame()).Returns((GameInfo?)null);
        bool messageSent = false;
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) => messageSent = true);

        // Act
        _viewModel.AutoAttachGameCommand.Execute(null);

        // Assert
        Assert.False(messageSent);
    }

    [Fact]
    public async Task LoadGamesAsync_WhenCalledTwice_ShouldReplaceOldDataWithNewData()
    {
        // Arrange: 第一次的数据
        var firstBatch = new List<GameInfo> { new() { GameID = Guid.NewGuid(), FilePath = "Game1.exe" } };
        _mockGameService.SetupSequence(s => s.AllCompletedGamesIdDict)
            .Returns(firstBatch.ToDictionary(g => g.GameID))  // 第一次调用返回
            .Returns(new List<GameInfo> { new() { GameID = Guid.NewGuid(), FilePath = "Game2.exe" } }.ToDictionary(g => g.GameID)); // 第二次返回

        Func<string, Task<object?>> mockIcon = p => Task.FromResult<object?>(null);

        // Act
        await _viewModel.LoadGamesCommand.ExecuteAsync(mockIcon); // 第一次加载
        await _viewModel.LoadGamesCommand.ExecuteAsync(mockIcon); // 第二次加载

        // Assert
        Assert.Single(_viewModel.Games); // 应该是 1 个，而不是 2 个
        Assert.Equal("Game2.exe", _viewModel.Games[0].GameInfo.FilePath);
    }
}