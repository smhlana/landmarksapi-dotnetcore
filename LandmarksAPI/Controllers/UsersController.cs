using AutoMapper;
using LandmarksAPI.Helpers;
using LandmarksAPI.Models.Users;
using LandmarksAPI.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LandmarksAPI.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
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

		[AllowAnonymous]
		[HttpPost("authenticate")]
		public async System.Threading.Tasks.Task<IActionResult> AuthenticateAsync([FromBody] Authenticate model)
		{
			var user = await _userService.AuthenticateAsync(model.Username, model.Password);

			if (user == null)
				return BadRequest(new { message = "Username or password is incorrect" });

			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, user.Id.ToString())
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);

			// return basic user info and authentication token
			return Ok(new
			{
				Id = user.Id,
				Username = user.Username,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Token = tokenString
			});
		}

		[AllowAnonymous]
		[HttpPost("register")]
		public async System.Threading.Tasks.Task<IActionResult> RegisterAsync([FromBody] Register model)
		{
			// map model to entity
			var user = _mapper.Map<Entities.User>(model);

			try
			{
				// create user
				await _userService.CreateAsync(user, model.Password);
				return Ok();
			}
			catch (Exception ex)
			{
				// return error message if there was an exception
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}
