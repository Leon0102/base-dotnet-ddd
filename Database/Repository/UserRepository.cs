using Domain.Users.Users.Entities;
using Domain.Users.Users.Interface;
using Microsoft.EntityFrameworkCore;
using Shared.Infra;

namespace Database.Repository;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly DbFactory _dbFactory;
    private readonly AppDbContext _dbContext;
    public UserRepository(DbFactory dbFactory, AppDbContext dbContext) : base(dbFactory, dbContext.Users)
    {
        _dbFactory = dbFactory;
        _dbContext = dbContext;
    }

    public Task GetAdminUserAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetByIdAsync(string id)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Id.ToString() == id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(user);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    Task<User> IUserRepository.FindByEmailAsync(string email)
    {
        try 
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(user);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public Task<User> AddAsync(User user)
    {
        try
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return Task.FromResult(user);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);       
        }
    }

    public Task<User> GetById(string userId)
    {
        try
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Id.ToString() == userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return Task.FromResult(user);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public IEnumerable<User> GetAll()
    {
        return _dbContext.Users.ToList();
    }

    public Task<User> GetByRefreshToken(string token)
    {
        throw new NotImplementedException();
    }

    public async Task FindByEmailAsync(string email)
    {   
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        await Task.FromResult(user);
    }
}