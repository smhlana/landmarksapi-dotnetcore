using LandmarksAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services
{
	public interface ICosmosDbService
	{
		Task<IEnumerable<Location>> GetItemsAsync(string queryString);
		//Task<Location> GetItemAsync(string name);
		void AddItemAsync(Location item);
		Task UpdateItemAsync(Location item);
	}
}
