using LandmarksAPI.Models.Users;
using LandmarksAPI.Services.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LandmarksAPI.Controllers
{
	[Route("api/landmarks/[controller]")]
	[ApiController]
	public class UsersController : BaseController
	{
        private IUserService _userService;
        public UsersController(IUserService userService)
		{
            _userService = userService;
		}

		// Post: api/landmarks/users/login
		[HttpPost("login")]
		public async System.Threading.Tasks.Task<IActionResult> AuthenticateAsync([FromBody] AuthenticateRequest model)
		{
			AuthenticateResponse response = await _userService.AuthenticateAsync(model, IpAddress());

			setTokenCookie(response.RefreshToken);
			return Ok(response);
		}

		// Post: api/landmarks/users/logout
		[Helpers.Authorize]
		[HttpPost("logout")]
		public async System.Threading.Tasks.Task<IActionResult> LogoutAsync(RevokeTokenRequest model)
		{
			var token = model.Token ?? Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(token))
				return BadRequest(new { message = "Token is required" });

			string response = await _userService.LogoutAsync(AccountContext.Id, token, IpAddress());

			deleteTokenCookie();
			return Ok(response);
		}

		// Post: api/landmarks/users/register
		[HttpPost("register")]
		public async System.Threading.Tasks.Task<IActionResult> RegisterAsync([FromBody] RegisterRequest model)
		{
			return await _userService.RegisterAsync(model, Request.Headers["origin"]);
		}

		private string IpAddress()
		{
			if (Request.Headers.ContainsKey("X-Forwarded-For"))
				return Request.Headers["X-Forwarded-For"];
			else
				return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
		}

		private void setTokenCookie(string token)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Expires = DateTime.UtcNow.AddDays(7)
			};
			Response.Cookies.Append("refreshToken", token, cookieOptions);
		}

		private void deleteTokenCookie()
		{
			Response.Cookies.Delete("refreshToken");
		}

	}
}
