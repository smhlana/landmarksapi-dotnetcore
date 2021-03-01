using System.Threading.Tasks;

namespace LandmarksAPI.Services.UsersDb
{
	public interface IUsersDbService
	{
		Task<Entities.User> GetUserAsync(string username);
		void AddUserAsync(Entities.User user);
	}
}
