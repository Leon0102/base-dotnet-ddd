using Domain.Users.Users.Entities;
using Domain.Users.Users.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Interfaces;

namespace API.Services;

public class UserService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
    )
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<User> RegisterAsync(string userName, string email)
    {
        try
        {
            var user = new User(userName, email);
            _userRepository.Add(user);
            await _unitOfWork.CommitAsync();
        
            return user;
        }
        catch (Exception e)
        {
            throw;
        }
        
    }

    public IActionResult UpdateAsync(string fullname, string email, int id)
    {
        throw new NotImplementedException();
    }
}