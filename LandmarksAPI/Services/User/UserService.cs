using AutoMapper;
using LandmarksAPI.Entities;
using LandmarksAPI.Helpers;
using LandmarksAPI.Models.Users;
using LandmarksAPI.Services.UsersDb;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace LandmarksAPI.Services.User
{
	public class UserService : IUserService
	{
		private IUsersDbService _userDbService;
		private readonly IMapper _mapper;
		private readonly AppSettings _appSettings;

		public UserService(IUsersDbService userDbService, IMapper mapper, IOptions<AppSettings> appSettings)
		{
			_userDbService = userDbService;
			_mapper = mapper;
			_appSettings = appSettings.Value;
		}

		public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
		{
			if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
				throw new Exception("Username or password is incorrect");

			Account account = await _userDbService.GetUserAsync(model.Username);

			if (account == null || !BC.BCrypt.Verify(model.Password, account.PasswordHash))
				return null;

			// authentication successful so generate jwt and refresh tokens
			var jwtToken = GenerateJwtToken(account);
			var refreshToken = GenerateRefreshToken(account, ipAddress);
			account.RefreshTokens.Add(refreshToken);

			// remove old refresh tokens from account
			removeOldRefreshTokens(account);

			// update DB
			await _userDbService.UpdateItemAsync(account);

			AuthenticateResponse response = _mapper.Map<AuthenticateResponse>(account);
			response.JwtToken = jwtToken;
			response.RefreshToken = refreshToken.Token;
			return response;
		}

		public async Task<IActionResult> RegisterAsync(RegisterRequest model, string origin)
		{
			// validate
			var user = await _userDbService.GetUserAsync(model.Username);
			if (user != null)
			{
				return new JsonResult(new { message = "A user with this username has already been registered." }) { StatusCode = StatusCodes.Status200OK };
			}

			// map model to new account object
			Account account = _mapper.Map<Account>(model);

			account.Id = Guid.NewGuid().ToString();
			account.Created = DateTime.UtcNow;
			account.VerificationToken = GenerateJwtToken(account);
			account.RefreshTokens = new List<RefreshToken>();

			// hash password
			account.PasswordHash = BC.BCrypt.HashPassword(model.Password);

			// save account
			await _userDbService.AddUserAsync(account);

			return new JsonResult(new { message = "Registration successful." }) { StatusCode = StatusCodes.Status201Created };
		}

		public IEnumerable<Entities.User> GetAll()
		{
			throw new NotImplementedException();
		}

		public async Task<Account> GetById(string id)
		{
			return await _userDbService.GetUserByIdAsync(id);
		}

		// private helper methods
		private string GenerateJwtToken(Account account)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
				Expires = DateTime.UtcNow.AddMinutes(1440),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		private void removeOldRefreshTokens(Account account)
		{
			account.RefreshTokens.RemoveAll(x =>
				!x.IsActive &&
				x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
		}

		private RefreshToken GenerateRefreshToken(Account account, string ipAddress)
		{
			return new RefreshToken
			{
				Token = GenerateJwtToken(account),
				Expires = DateTime.UtcNow.AddDays(7),
				Created = DateTime.UtcNow,
				CreatedByIp = ipAddress
			};
		}
	}
}
