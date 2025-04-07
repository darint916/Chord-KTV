using ChordKTV.Models.UserData;

namespace ChordKTV.Data.Api.UserData;

public interface IUserRepo
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> SaveChangesAsync();
}
