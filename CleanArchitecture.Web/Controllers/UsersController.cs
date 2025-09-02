using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users");
            TempData["Error"] = "An error occurred while loading users.";
            return View(new List<UserDto>());
        }
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user {UserId}", id);
            TempData["Error"] = "An error occurred while loading user details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto createUserDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _userService.CreateAsync(createUserDto);
                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                ModelState.AddModelError("", "An error occurred while creating the user.");
            }
        }
        return View(createUserDto);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var editDto = new CreateUserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth
            };

            return View(editDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user {UserId} for editing", id);
            TempData["Error"] = "An error occurred while loading user for editing.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateUserDto updateUserDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _userService.UpdateAsync(id, updateUserDto);
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Email", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user {UserId}", id);
                ModelState.AddModelError("", "An error occurred while updating the user.");
            }
        }
        return View(updateUserDto);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user {UserId} for deletion", id);
            TempData["Error"] = "An error occurred while loading user for deletion.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user {UserId}", id);
            TempData["Error"] = "An error occurred while deleting the user.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Users/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            await _userService.ActivateAsync(id);
            TempData["Success"] = "User activated successfully!";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "User not found.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating user {UserId}", id);
            TempData["Error"] = "An error occurred while activating the user.";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Users/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _userService.DeactivateAsync(id);
            TempData["Success"] = "User deactivated successfully!";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "User not found.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating user {UserId}", id);
            TempData["Error"] = "An error occurred while deactivating the user.";
        }
        return RedirectToAction(nameof(Index));
    }
}
