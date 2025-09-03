using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user?.Adapt<UserDto>();
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        return user?.Adapt<UserDto>();
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return users.Adapt<IEnumerable<UserDto>>();
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
    {
        var users = await _unitOfWork.Users.GetActiveUsersAsync();
        return users.Adapt<IEnumerable<UserDto>>();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email))
        {
            throw new DuplicateEntityException("User", "email", createUserDto.Email);
        }

        var user = createUserDto.Adapt<User>();
        user.CreatedAt = DateTime.UtcNow;

        var createdUser = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return createdUser.Adapt<UserDto>();
    }

    public async Task<UserDto> UpdateAsync(int id, CreateUserDto updateUserDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        // Check if email is changing and if new email already exists
        if (user.Email != updateUserDto.Email && 
            await _unitOfWork.Users.EmailExistsAsync(updateUserDto.Email))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        updateUserDto.Adapt(user);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user.Adapt<UserDto>();
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _unitOfWork.Users.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        user.Activate();
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        user.Deactivate();
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Users.ExistsAsync(id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _unitOfWork.Users.EmailExistsAsync(email);
    }
}
