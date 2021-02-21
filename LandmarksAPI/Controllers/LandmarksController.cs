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
		private readonly IFourSquareService _fourSquareService;
		private readonly IFlickrService _flickrService;
		public LandmarksController(ICosmosDbService cosmosDbService, IFourSquareService fourSquareService, IFlickrService flickrService)
		{
			_cosmosDbService = cosmosDbService;
			_fourSquareService = fourSquareService;
			_flickrService = flickrService;
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
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.SearchAsync(name);
		}

		// Get: api/locations/searchbylatlong/latitude/longitude
		[HttpGet("searchbylatlong/{latitude}/{longitude}")]
		public async Task<IEnumerable<string>> SearchLocationsAsync(string latitude, string longitude)
		{
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.SearchAsync(latitude, longitude);
		}
	}
}
