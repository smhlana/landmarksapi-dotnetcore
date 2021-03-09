using LandmarksAPI.Helpers;
using LandmarksAPI.Models;
using LandmarksAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LandmarksAPI.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class LandmarksController : BaseController
	{
		private readonly Landmarks _landmarks;
		private readonly IDistributedCache _cache;
		public LandmarksController(ICosmosDbService cosmosDbService, IFourSquareService fourSquareService, IFlickrService flickrService, IDistributedCache cache)
		{
			 _landmarks = new Landmarks(fourSquareService, flickrService, cosmosDbService);
			_cache = cache;
		}

		// Get: api/landmarks
		[HttpGet]
		public async Task<IEnumerable<Location>> Index()
		{
			return await _landmarks.FetchAllItemsAsync(AccountContext.Id);
		}

		// Get: api/landmarks/searchbyname/{name}
		[HttpGet("searchbyname/{name}")]
		public async Task<IEnumerable<string>> SearchLandmarksAsync(string name)
		{
			string cachedUrls = _cache.GetString(AccountContext.Id + "_url_" + name.ToLower());
			List<string> urls;
			if (string.IsNullOrEmpty(cachedUrls))
			{
				urls = await _landmarks.SearchAsync(AccountContext.Id, name);
				if (urls.Count > 0)
				{
					DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
					options.SetAbsoluteExpiration(TimeSpan.FromSeconds(150));
					_cache.SetString(AccountContext.Id + "_url_" + name.ToLower(), JsonConvert.SerializeObject(urls), options);
				}
			}
			else
			{
				urls = JsonConvert.DeserializeObject<List<string>>(cachedUrls);
			}
			return urls;
		}

		// Get: api/landmarks/searchbylatlong/latitude/longitude
		[HttpGet("searchbylatlong/{latitude}/{longitude}")]
		public async Task<IEnumerable<string>> SearchLandmarksAsync(string latitude, string longitude)
		{
			Location locationDetails = await _landmarks.FetchLocationDetailsAsync(AccountContext.Id, latitude, longitude);
			string cachedUrls = _cache.GetString(AccountContext.Id + "_url_" + locationDetails.Name.ToLower());
			List<string> urls;
			if (string.IsNullOrEmpty(cachedUrls))
			{
				urls = await _landmarks.SearchAsync(locationDetails);
				if (urls.Count > 0)
				{
					DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
					options.SetAbsoluteExpiration(TimeSpan.FromSeconds(150));
					_cache.SetString(AccountContext.Id + "_url_" + locationDetails.Name.ToLower(), JsonConvert.SerializeObject(urls), options);
				}
			}
			else
			{
				urls = JsonConvert.DeserializeObject<List<string>>(cachedUrls);
			}
			
			return urls;
		}

		// Get: api/landmarks/locationimages/locationName
		[HttpGet("locationimages/{locationName}")]
		public async Task<IEnumerable<string>> GetLocationImagesAsync(string locationName)
		{
			return await _landmarks.GetImagesByLocation(AccountContext.Id,locationName);
		}

		// Get: api/landmarks/imagedetailsbyurl?url=<url>
		[HttpGet("imagedetailsbyurl")]
		public async Task<Photo> GetImageDetailsByUrlAsync(string url)
		{
			return await _landmarks.GetImageDetaisByUrlAsync(AccountContext.Id, url);
		}

		// Get: api/landmarks/imagedetailsbyid?id=<id>
		[HttpGet("imagedetailsbyid")]
		public async Task<Photo> GetImageDetailsByIdAsync(string id)
		{
			return await _landmarks.GetImageDetaisByIdAsync(AccountContext.Id, id);
		}
	}
}

// create users dotnet core api
// https://jasonwatmore.com/post/2019/10/14/aspnet-core-3-simple-api-for-authentication-registration-and-user-management
// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-5.0&tabs=visual-studio