using LandmarksAPI.Models;
using LandmarksAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LandmarksAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class LandmarksController : ControllerBase
	{
		private readonly Landmarks _landmarks;
		public LandmarksController(ICosmosDbService cosmosDbService, IFourSquareService fourSquareService, IFlickrService flickrService)
		{
			 _landmarks = new Landmarks(fourSquareService, flickrService, cosmosDbService);
		}

		// Get: api/landmarks
		[HttpGet]
		public async Task<IEnumerable<Location>> Index()
		{
			return await _landmarks.FetchAllItemsAsync();
		}

		// Get: api/landmarks/searchbyname/{name}
		[HttpGet("searchbyname/{name}")]
		public async Task<IEnumerable<string>> SearchLandmarksAsync(string name)
		{
			return await _landmarks.SearchAsync(name);
		}

		// Get: api/landmarks/searchbylatlong/latitude/longitude
		[HttpGet("searchbylatlong/{latitude}/{longitude}")]
		public async Task<IEnumerable<string>> SearchLandmarksAsync(string latitude, string longitude)
		{
			return await _landmarks.SearchAsync(latitude, longitude);
		}

		// Get: api/landmarks/locationimages/locationName
		[HttpGet("locationimages/{locationName}")]
		public async Task<IEnumerable<string>> GetLocationImagesAsync(string locationName)
		{
			return await _landmarks.GetImagesByLocation(locationName);
		}

		// Get: api/landmarks/imagedetailsbyurl?url=<url>
		[HttpGet("imagedetailsbyurl")]
		public async Task<Photo> GetImageDetailsByUrlAsync(string url)
		{
			return await _landmarks.GetImageDetaisByUrlAsync(url);
		}

		// Get: api/landmarks/imagedetailsbyid?id=<id>
		[HttpGet("imagedetailsbyid")]
		public async Task<Photo> GetImageDetailsByIdAsync(string id)
		{
			return await _landmarks.GetImageDetaisByIdAsync(id);
		}
	}
}

// create users dotnet core api
// https://jasonwatmore.com/post/2019/10/14/aspnet-core-3-simple-api-for-authentication-registration-and-user-management
// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-5.0&tabs=visual-studio