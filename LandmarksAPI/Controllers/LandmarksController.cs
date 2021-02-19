using LandmarksAPI.Models;
using LandmarksAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LandmarksController : ControllerBase
	{
		private readonly ICosmosDbService _cosmosDbService;
		private readonly ILandmarksService _landmarksService;
		public LandmarksController(ICosmosDbService cosmosDbService, ILandmarksService landmarksService)
		{
			_cosmosDbService = cosmosDbService;
			_landmarksService = landmarksService;
		}

		// Get: api/landmarks
		[HttpGet]
		public async Task<IEnumerable<Location>> Index()
		{
			return await _cosmosDbService.GetItemsAsync("SELECT * FROM c where c.userid='1'");
		}

		// Get: api/locations/searchbyname/{name}
		[HttpGet("searchbyname/{name}")]
		public async Task<IEnumerable<string>> SearchLocationsAsync(string name)
		{
			return await _landmarksService.SearchByName(name);
		}
	}
}
