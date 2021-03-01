using System.Collections.Generic;
using System.Threading.Tasks;
using LandmarksAPI.Entities;

namespace LandmarksAPI.Services.User
{
	public interface IUserService
	{
        Task<Entities.User> AuthenticateAsync(string username, string password);
        IEnumerable<Entities.User> GetAll();
        Entities.User GetById(int id);
        Task<Entities.User> CreateAsync(Entities.User user, string password);
    }
}
