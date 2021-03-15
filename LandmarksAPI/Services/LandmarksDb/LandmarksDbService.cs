using LandmarksAPI.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services
{
    public class LandmarksDbService : ILandmarksDbService
    {
        private Container _container;

        public LandmarksDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            _container = dbClient.GetContainer(databaseName, containerName);
        }

		public async void AddItemAsync(Location item)
		{
            await _container.CreateItemAsync(item, new PartitionKey(item.UserId));
        }

		public async Task<IEnumerable<Location>> GetItemsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<Location>(new QueryDefinition(queryString));
            List<Location> results = new List<Location>();
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

            return results;
        }

		public async Task<IEnumerable<LocationName>> GetLocationNameAsync(string queryString)
		{
			var query = _container.GetItemQueryIterator<LocationName>(new QueryDefinition(queryString));
			List<LocationName> results = new List<LocationName>();
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

			return results;
		}

		public async Task UpdateItemAsync(Location item)
		{
            await _container.UpsertItemAsync(item, new PartitionKey(item.UserId));
        }
	}
}
