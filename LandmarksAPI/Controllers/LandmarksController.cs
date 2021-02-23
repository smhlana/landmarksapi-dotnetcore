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
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.FetchAllAsync();
		}

		// Get: api/landmarks/searchbyname/{name}
		[HttpGet("searchbyname/{name}")]
		public async Task<IEnumerable<string>> SearchLandmarksAsync(string name)
		{
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.SearchAsync(name);
		}

		// Get: api/landmarks/searchbylatlong/latitude/longitude
		[HttpGet("searchbylatlong/{latitude}/{longitude}")]
		public async Task<IEnumerable<string>> SearchLandmarksAsync(string latitude, string longitude)
		{
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.SearchAsync(latitude, longitude);
		}

		// Get: api/landmarks/locationimages/locationName
		[HttpGet("locationimages/{locationName}")]
		public async Task<IEnumerable<string>> GetLocationImagesAsync(string locationName)
		{
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.GetImagesByLocation(locationName);
		}

		// Get: api/landmarks/imagedetailsbyurl?url=<url>
		[HttpGet("imagedetailsbyurl")]
		public async Task<Photo> GetImageDetailsByUrlAsync(string url)
		{
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.GetImageDetaisByUrlAsync(url);
		}

		// Get: api/landmarks/imagedetailsbyid?id=<id>
		[HttpGet("imagedetailsbyid")]
		public async Task<Photo> GetImageDetailsByIdAsync(string id)
		{
			Landmarks landmarks = new Landmarks(_fourSquareService, _flickrService, _cosmosDbService);
			return await landmarks.GetImageDetaisByIdAsync(id);
		}
	}
}
