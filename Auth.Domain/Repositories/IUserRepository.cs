using Auth.Domain.Entities;

namespace Auth.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User newUser);
    }
}
