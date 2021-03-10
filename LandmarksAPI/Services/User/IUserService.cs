using System.Collections.Generic;
using System.Threading.Tasks;
using LandmarksAPI.Entities;
using LandmarksAPI.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace LandmarksAPI.Services.User
{
	public interface IUserService
	{
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress);
        Task<string> LogoutAsync(string userId);
        IEnumerable<Entities.User> GetAll();
        Task<Account> GetById(string id);
        Task<IActionResult> RegisterAsync(RegisterRequest model, string origin);
    }
}
