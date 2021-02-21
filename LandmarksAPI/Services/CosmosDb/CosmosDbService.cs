using LandmarksAPI.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

		public async void AddItemAsync(Location item)
		{
            await this._container.CreateItemAsync(item, new PartitionKey(item.Name));
        }

		public async Task<IEnumerable<Location>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Location>(new QueryDefinition(queryString));
            List<Location> results = new List<Location>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

		public async Task UpdateItemAsync(Location item)
		{
            await this._container.UpsertItemAsync(item, new PartitionKey(item.Name));
        }
	}
}
