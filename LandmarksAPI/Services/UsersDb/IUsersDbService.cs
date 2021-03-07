using LandmarksAPI.Entities;
using System.Threading.Tasks;

namespace LandmarksAPI.Services.UsersDb
{
	public interface IUsersDbService
	{
		Task<Account> GetUserAsync(string username);
		Task<Account> GetUserByIdAsync(string id);
		Task AddUserAsync(Account user);
		Task UpdateItemAsync(Account user);
	}
}
