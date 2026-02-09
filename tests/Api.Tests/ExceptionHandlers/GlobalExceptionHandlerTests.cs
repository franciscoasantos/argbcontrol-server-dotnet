using ArgbControl.Api.ExceptionHandlers;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArgbControl.Api.Tests.ExceptionHandlers;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> loggerMock;
    private readonly GlobalExceptionHandler sut;

    public GlobalExceptionHandlerTests()
    {
        loggerMock = new Mock<ILogger<GlobalExceptionHandler>>(MockBehavior.Strict);
        SetupDefaultMockBehavior();
        sut = new GlobalExceptionHandler(loggerMock.Object);
    }

    private void SetupDefaultMockBehavior()
    {
        loggerMock.Setup(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // Arrange
        var loggerMockLocal = new Mock<ILogger<GlobalExceptionHandler>>(MockBehavior.Strict);
        loggerMockLocal.Setup(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        var handler = new GlobalExceptionHandler(loggerMockLocal.Object);

        // Assert
        handler.Should().NotBeNull();
        handler.Should().BeAssignableTo<IExceptionHandler>();
    }

    [Fact]
    public void GlobalExceptionHandler_ImplementsIExceptionHandler()
    {
        // Act & Assert
        sut.Should().BeAssignableTo<IExceptionHandler>();
    }

    [Fact]
    public void GlobalExceptionHandler_CanBeCreated()
    {
        // Arrange & Act
        var localSut = sut;

        // Assert
        localSut.Should().NotBeNull();
    }

    [Fact]
    public void GlobalExceptionHandler_HasCorrectInterfaceSignature()
    {
        // Act & Assert
        var method = typeof(GlobalExceptionHandler).GetMethod("TryHandleAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void GlobalExceptionHandler_TryHandleAsyncReturnsValueTask()
    {
        // Arrange
        var method = typeof(GlobalExceptionHandler).GetMethod("TryHandleAsync");

        // Act & Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(ValueTask<bool>));
    }

    [Fact]
    public void GlobalExceptionHandler_TryHandleAsyncHasThreeParameters()
    {
        // Arrange
        var method = typeof(GlobalExceptionHandler).GetMethod("TryHandleAsync");
        var parameters = method!.GetParameters();

        // Act & Assert
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Name.Should().Contain("HttpContext");
        parameters[1].ParameterType.Name.Should().Contain("Exception");
        parameters[2].ParameterType.Name.Should().Contain("CancellationToken");
    }

    [Fact]
    public void GlobalExceptionHandler_LoggerParameterIsRequired()
    {
        // Arrange
        var constructorInfo = typeof(GlobalExceptionHandler).GetConstructors();

        // Act & Assert
        constructorInfo.Should().HaveCount(1);
        var parameters = constructorInfo[0].GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Name.Should().Contain("ILogger");
    }

    [Fact]
    public void GlobalExceptionHandler_ImplementsExceptionHandlerInterface()
    {
        // Arrange
        var type = typeof(GlobalExceptionHandler);
        var interfaces = type.GetInterfaces();

        // Act & Assert
        interfaces.Should().Contain(typeof(IExceptionHandler));
    }

    [Fact]
    public void GlobalExceptionHandler_IsPublic()
    {
        // Arrange
        var type = typeof(GlobalExceptionHandler);

        // Act & Assert
        type.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void GlobalExceptionHandler_IsNotAbstract()
    {
        // Arrange
        var type = typeof(GlobalExceptionHandler);

        // Act & Assert
        type.IsAbstract.Should().BeFalse();
    }

    [Fact]
    public void GlobalExceptionHandler_CanBeInstantiatedWithMockedLogger()
    {
        // Arrange
        var loggerMockLocal = new Mock<ILogger<GlobalExceptionHandler>>(MockBehavior.Strict);
        loggerMockLocal.Setup(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        var handler = new GlobalExceptionHandler(loggerMockLocal.Object);

        // Assert
        handler.Should().NotBeNull();
        typeof(GlobalExceptionHandler).IsAssignableFrom(handler.GetType()).Should().BeTrue();
    }

    [Fact]
    public void GlobalExceptionHandler_ConstructorAcceptsLoggerInterface()
    {
        // Arrange & Act
        var handler = sut;

        // Assert
        handler.Should().NotBeNull();
    }
}
