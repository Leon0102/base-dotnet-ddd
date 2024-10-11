using Domain.Users.Users.Entities;
using Shared.Domain.Interfaces;

namespace Domain.Users.Users.Interface
{
    public interface IUserRepository : IRepository<Entities.User>
    {
        Task GetAdminUserAsync();
        Task<User> GetByIdAsync(string id);
        Task<User> FindByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task<User> GetById(string userId);
        IEnumerable<User> GetAll();
        Task<User> GetByRefreshToken(string token);
    }
}
