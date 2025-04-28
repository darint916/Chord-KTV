using ChordKTV.Models.UserData;

namespace ChordKTV.Data.Api.UserData;

public interface IUserRepo
{
    public Task<User?> GetUserByEmailAsync(string email);
    public Task<User?> GetUserByIdAsync(Guid id);
    public Task CreateUserAsync(User user);
    public Task UpdateUserAsync(User user);
    public Task<bool> SaveChangesAsync();
}
