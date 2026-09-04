using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Tests.Helpers;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();

        _mockUnitOfWork.Setup(u => u.Customers).Returns(_mockCustomerRepository.Object);

        _customerService = new CustomerService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCustomers()
    {
        var customers = TestDataBuilder.CreateCustomerList(3);
        _mockCustomerRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var result = await _customerService.GetAllAsync();

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<CustomerDto>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        var customer = TestDataBuilder.CreateValidCustomer(1);
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _customerService.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Email.Should().Be("customer.1@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _customerService.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateCustomer()
    {
        var dto = TestDataBuilder.CreateValidCustomerDto();
        _mockCustomerRepository.Setup(r => r.ExistsByEmailAsync(dto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _customerService.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        result.Email.Should().Be(dto.Email);
        _mockCustomerRepository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrowDuplicateEntityException()
    {
        var dto = TestDataBuilder.CreateValidCustomerDto(email: "existing@example.com");
        _mockCustomerRepository.Setup(r => r.ExistsByEmailAsync(dto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Func<Task> act = () => _customerService.CreateAsync(dto);

        await act.Should().ThrowAsync<DuplicateEntityException>();
        _mockCustomerRepository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        var dto = TestDataBuilder.CreateValidCustomerDto();
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        Func<Task> act = () => _customerService.UpdateAsync(999, dto);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateCustomer()
    {
        var customer = TestDataBuilder.CreateValidCustomer(1);
        var dto = TestDataBuilder.CreateValidCustomerDto(name: "Updated Name", email: customer.Email);

        _mockCustomerRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _mockCustomerRepository.Setup(r => r.ExistsByEmailAsync(dto.Email, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _customerService.UpdateAsync(1, dto);

        result.Name.Should().Be("Updated Name");
        _mockCustomerRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_ShouldDeleteCustomer()
    {
        _mockCustomerRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _customerService.DeleteAsync(1);

        _mockCustomerRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        _mockCustomerRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Func<Task> act = () => _customerService.DeleteAsync(999);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
