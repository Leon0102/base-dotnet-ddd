
using Shared.Domain.Interfaces;

namespace Domain.Users.Users.Interface
{
    public interface IUserRepository : IRepository<Entities.User>
    {
        Task GetAdminUserAsync();
        Task<Entities.User> GetByIdAsync(int id);
    }
}
