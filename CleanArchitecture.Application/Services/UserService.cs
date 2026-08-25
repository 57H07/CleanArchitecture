using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Domain.Entities;
using Mapster;
using System.Text.RegularExpressions;
using CleanArchitecture.Application.Interfaces.Services;
using CleanArchitecture.Application.Interfaces.Repositories;

namespace CleanArchitecture.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        return user?.Adapt<UserDto>();
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        return user?.Adapt<UserDto>();
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.Adapt<IEnumerable<UserDto>>();
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetActiveUsersAsync(cancellationToken);
        return users.Adapt<IEnumerable<UserDto>>();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        // Validate email format
        if (string.IsNullOrWhiteSpace(createUserDto.Email) || !IsValidEmail(createUserDto.Email))
        {
            throw new ArgumentException("Invalid email format", nameof(createUserDto.Email));
        }

        if (await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email, cancellationToken))
        {
            throw new DuplicateEntityException("User", "email", createUserDto.Email);
        }

        var user = createUserDto.Adapt<User>();
        user.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Adapt<UserDto>();
    }

    public async Task<UserDto> UpdateAsync(int id, CreateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        // Check if email is changing and if new email already exists
        if (user.Email != updateUserDto.Email &&
            await _unitOfWork.Users.EmailExistsAsync(updateUserDto.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        updateUserDto.Adapt(user);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Adapt<UserDto>();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Users.ExistsAsync(id, cancellationToken))
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        await _unitOfWork.Users.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        user.Activate();
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        user.Deactivate();
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Users.ExistsAsync(id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }
}
