using LandmarksAPI.Services.UsersDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services.User
{
	public class UserService : IUserService
	{
		private IUsersDbService _userDbService;

		public UserService(IUsersDbService userDbService)
		{
			_userDbService = userDbService;
		}

		public async Task<Entities.User> AuthenticateAsync(string username, string password)
		{
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
				return null;

			Entities.User user = await _userDbService.GetUserAsync(username);

			// check if username exists
			if (user == null)
				return null;

			// check if password is correct
			if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
				return null;

			// authentication successful
			return user;
		}

		public async Task<Entities.User> CreateAsync(Entities.User user, string password)
		{
			// validation
			if (string.IsNullOrWhiteSpace(password))
				throw new Exception("Password is required");

			Entities.User dbUser = await _userDbService.GetUserAsync(user.Username);
			if (dbUser != null)
				throw new Exception("Username \"" + user.Username + "\" is already taken");

			byte[] passwordHash, passwordSalt;
			CreatePasswordHash(password, out passwordHash, out passwordSalt);

			user.PasswordHash = passwordHash;
			user.PasswordSalt = passwordSalt;

			_userDbService.AddUserAsync(user);

			return user;
		}

		public IEnumerable<Entities.User> GetAll()
		{
			throw new NotImplementedException();
		}

		public Entities.User GetById(int id)
		{
			throw new NotImplementedException();
		}

		// private helper methods
		private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
			if (password == null) throw new ArgumentNullException("password");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

			using (var hmac = new System.Security.Cryptography.HMACSHA512())
			{
				passwordSalt = hmac.Key;
				passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
			}
		}

		private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
		{
			if (password == null) throw new ArgumentNullException("password");
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
			if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
			if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

			using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
			{
				var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
				for (int i = 0; i < computedHash.Length; i++)
				{
					if (computedHash[i] != storedHash[i]) return false;
				}
			}

			return true;
		}

		
	}
}
