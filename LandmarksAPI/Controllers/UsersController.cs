using AutoMapper;
using LandmarksAPI.Helpers;
using LandmarksAPI.Models.Users;
using LandmarksAPI.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace LandmarksAPI.Controllers
{
	//[Helpers.Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : BaseController
	{
        private IUserService _userService;
		private IMapper _mapper;
		private readonly AppSettings _appSettings;
        public UsersController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
		{
            _userService = userService;
			_mapper = mapper;
			_appSettings = appSettings.Value;
		}

		//[AllowAnonymous]
		[HttpPost("authenticate")]
		public async System.Threading.Tasks.Task<IActionResult> AuthenticateAsync([FromBody] AuthenticateRequest model)
		{
			AuthenticateResponse response = await _userService.AuthenticateAsync(model, IpAddress());

			setTokenCookie(response.RefreshToken);
			return Ok(response);
		}

		//[AllowAnonymous]
		[HttpPost("register")]
		public async System.Threading.Tasks.Task<IActionResult> RegisterAsync([FromBody] RegisterRequest model)
		{
			await _userService.RegisterAsync(model, Request.Headers["origin"]);
			return Ok(new { message = "Registration successful." });
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
	}
}
