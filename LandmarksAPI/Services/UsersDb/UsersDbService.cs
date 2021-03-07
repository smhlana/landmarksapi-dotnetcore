using LandmarksAPI.Entities;
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

		public async Task AddUserAsync(Account user)
		{
			await _container.CreateItemAsync(user, new PartitionKey(user.Username));
		}

		public async Task<Account> GetUserAsync(string username)
		{
			string queryString = "SELECT * FROM c where c.username='" + username + "'";
			var query = _container.GetItemQueryIterator<Account>(new QueryDefinition(queryString));
			List<Account> results = new List<Account>();
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

		public async Task<Account> GetUserByIdAsync(string id)
		{
			string queryString = "SELECT * FROM c where c.id='" + id + "'";
			var query = _container.GetItemQueryIterator<Account>(new QueryDefinition(queryString));
			List<Account> results = new List<Account>();
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

		public async Task UpdateItemAsync(Account user)
		{
			await _container.UpsertItemAsync<Account>(user, new PartitionKey(user.Username));
		}
	}
}
