using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using AutoFixture;

namespace CleanArchitecture.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserService _userService;
    private readonly Fixture _fixture;

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _fixture = new Fixture();

        // Configure AutoFixture to handle circular references
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Setup UnitOfWork to return mocked repositories
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        _userService = new UserService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = _fixture.CreateMany<User>(3).ToList();
        _mockUserRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<UserDto>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var user = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .With(u => u.FirstName, "John")
            .With(u => u.LastName, "Doe")
            .With(u => u.Email, "john.doe@example.com")
            .With(u => u.Role, UserRole.User)
            .Create();
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var createDto = _fixture.Build<CreateUserDto>()
            .With(u => u.FirstName, "Jane")
            .With(u => u.LastName, "Smith")
            .With(u => u.Email, "jane.smith@example.com")
            .With(u => u.Role, UserRole.User)
            .Create();

        var createdUser = _fixture.Build<User>()
            .With(u => u.Id, 1)
            .With(u => u.FirstName, createDto.FirstName)
            .With(u => u.LastName, createDto.LastName)
            .With(u => u.Email, createDto.Email)
            .Create();

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(createDto.FirstName);
        result.LastName.Should().Be(createDto.LastName);
        result.Email.Should().Be(createDto.Email);
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    public async Task CreateAsync_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange
        var createDto = _fixture.Build<CreateUserDto>()
            .With(u => u.Email, invalidEmail)
            .Create();

        // Act & Assert
        var act = async () => await _userService.CreateAsync(createDto);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_ShouldUpdateUser()
    {
        // Arrange
        var userId = 1;
        var updateDto = _fixture.Build<CreateUserDto>()
            .With(u => u.FirstName, "Updated")
            .With(u => u.LastName, "User")
            .With(u => u.Email, "updated@example.com")
            .Create();

        var existingUser = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .With(u => u.FirstName, "Old")
            .With(u => u.LastName, "User")
            .Create();

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("User");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        var userId = 1;
        
        _mockUserRepository.Setup(r => r.ExistsAsync(userId))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _userService.DeleteAsync(userId);

        // Assert
        _mockUserRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var userId = 1;
        _mockUserRepository.Setup(r => r.ExistsAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.ExistsAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(r => r.ExistsAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.ExistsAsync(userId);

        // Assert
        result.Should().BeFalse();
    }
}
