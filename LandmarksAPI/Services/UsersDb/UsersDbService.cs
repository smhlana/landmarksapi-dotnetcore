using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services.UsersDb
{
	public class UsersDbService : IUsersDbService
	{
		private Container _container;

		public UsersDbService(CosmosClient dbClient, string databaseName, string containerName)
		{
			_container = dbClient.GetContainer(databaseName, containerName);
		}

		public async void AddUserAsync(Entities.User user)
		{
			await _container.CreateItemAsync(user, new PartitionKey(user.Username));
		}

		public async Task<Entities.User> GetUserAsync(string username)
		{
			string queryString = "SELECT * FROM c where c.username='" + username + "'";
			var query = _container.GetItemQueryIterator<Entities.User>(new QueryDefinition(queryString));
			List<Entities.User> results = new List<Entities.User>();
			try
			{
				while (query.HasMoreResults)
				{
					var response = await query.ReadNextAsync();

					results.AddRange(response.ToList());
				}
			}
			catch (Exception)
			{
				throw;
			}

			if (results.Count > 0) return results[0];
			return null;
		}
	}
}
