namespace Pulse.DAL.Queries.Users;

public interface IUserQueries
{
    Task<bool> EmailExistsAsync(string email);
}
