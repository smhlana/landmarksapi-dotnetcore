﻿using LandmarksAPI.Helpers;
using LandmarksAPI.Services.UsersDb;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarksAPI.Middleware
{
	public class JwtMiddleware
	{
        private readonly RequestDelegate _next;
		private readonly AuthSettings _authSettings;
        private IUsersDbService _userDbService;

        public JwtMiddleware(RequestDelegate next, AuthSettings authSettings, IUsersDbService userDbService)
		{
			_next = next;
			_authSettings = authSettings;
            _userDbService = userDbService;
		}

		public async Task Invoke(HttpContext context)
		{
			var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

			if (token != null)
				await attachAccountToContext(context, token);

			await _next(context);
		}

        private async Task attachAccountToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_authSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = jwtToken.Claims.First(x => x.Type == "id").Value;

                // attach account to context on successful jwt validation
                var account = await _userDbService.GetUserByIdAsync(accountId);
				if (account.RefreshTokens != null)
				{
					var validToken = account.RefreshTokens.FirstOrDefault(t => t.Token == token);
					if (validToken != null && !validToken.IsExpired) context.Items["Account"] = await _userDbService.GetUserByIdAsync(accountId); 
				}
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

