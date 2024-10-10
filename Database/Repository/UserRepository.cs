

using Domain.Users.Users.Entities;
using Domain.Users.Users.Interface;
using Shared.Infra;
    public class UserRepository : Repository<User>, IUserRepository
    {
        private IUserRepository _userRepositoryImplementation;

        public UserRepository(DbFactory dbFactory) : base(dbFactory)
        {
        }

        public Task GetAdminUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<User> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
    
