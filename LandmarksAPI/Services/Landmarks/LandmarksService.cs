﻿using FourSquare.SharpSquare.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandmarksAPI.Models;
using FlickrNet;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace LandmarksAPI.Services
{
	public class LandmarksService
	{
		private readonly IDistributedCache _cache;
		private readonly IFourSquareService _fourSquareService;
		private readonly IFlickrService _flickrService;
		private readonly ILandmarksDbService _cosmosDbService;
		private readonly TimeSpan CACHE_RETENTION_TIME = TimeSpan.FromSeconds(86400);

		public LandmarksService(IFourSquareService fourSquareService, IFlickrService flickrService, ILandmarksDbService cosmosDbService, IDistributedCache cache)
		{
			_fourSquareService = fourSquareService;
			_flickrService = flickrService;
			_cosmosDbService = cosmosDbService;
			_cache = cache;
		}

		public async Task<List<string>> SearchAsync(string userId, string name)
		{
			string cacheKey = userId + "_url_" + name.ToLower();
			string cachedUrls = GetCachedItem(cacheKey);
			List<string> urls;

			if (!string.IsNullOrEmpty(cachedUrls))
			{
				return JsonConvert.DeserializeObject<List<string>>(cachedUrls);
			}

			var parameters = new Dictionary<string, string>
			{
				{"near", name},
				{"radius", "50000"},
				{"limit", "6" },
				{"categoryId", "4bf58dd8d48988d12d941735"}
			};

			Models.Location location = await GetLocationObject(userId, parameters, name);

			await SaveSearchResults(location);
			urls = FetchLocationUrls(location);
			CacheResults(cacheKey, urls);
			return urls;
		}

		public async Task<List<string>> SearchAsync(string userId, string latitude, string longitude)
		{
			Models.Location location = await FetchLocationDetailsAsync(userId, latitude, longitude);
			string cacheKey = userId + "_url_" + location.Name;
			string cachedUrls = GetCachedItem(cacheKey);
			List<string> urls;

			if (!string.IsNullOrEmpty(cachedUrls))
			{
				return JsonConvert.DeserializeObject<List<string>>(cachedUrls);
			}

			await SaveSearchResults(location);
			urls = FetchLocationUrls(location);
			CacheResults(cacheKey, urls);
			return urls;
		}

		public async Task<Models.Location> FetchLocationDetailsAsync(string userId, string latitude, string longitude)
		{
			string latLong = $"{latitude},{longitude}";
			var parameters = new Dictionary<string, string>
			{
				{"ll", latLong},
				{"radius", "50000"},
				{"limit", "6" },
				{"categoryId", "4bf58dd8d48988d12d941735"}
			};

			return await GetLocationObject(userId, parameters);
		}

		public async Task<IEnumerable<Models.Location>> FetchAllItemsAsync(string userId)
		{
			string queryString = "SELECT * FROM c where c.userid='" + userId + "'";
			return await _cosmosDbService.GetItemsAsync(queryString);
		}

		public async Task<Models.Photo> GetImageDetaisByUrlAsync(string userId, string url)
		{
			string cacheKey = userId + "_image";
			string cachedImagesSerialized = GetCachedItem(cacheKey);
			List<Models.Photo> cachedImages = new List<Models.Photo>();

			if (cachedImagesSerialized != null)
			{
				cachedImages = JsonConvert.DeserializeObject<List<Models.Photo>>(cachedImagesSerialized);

				if (cachedImages.Count > 0)
				{
					Models.Photo image = cachedImages.FirstOrDefault(img => img.Url == url);
					if (image != null) return image;
				}
			}

			var items = await FetchAllItemsAsync(userId);

			List<Landmark> landmarks = new List<Landmark>();
			List<Models.Photo> photos = new List<Models.Photo>();
			foreach (Models.Location location in items)
			{
				landmarks.AddRange(location.Landmarks);
			}
			foreach (Landmark landmark in landmarks)
			{
				photos.AddRange(landmark.Images);
			}
			foreach (Models.Photo photo in photos)
			{
				if (photo.Url == url) 
				{
					cachedImages.Add(photo);
					CacheResults(cacheKey, cachedImages);
					return photo;
				} 
			}

			return null;
		}

		public async Task<Models.Photo> GetImageDetaisByIdAsync(string userId, string imageId)
		{
			string cacheKey = userId + "_image";
			string cachedImagesSerialized = GetCachedItem(cacheKey);
			List<Models.Photo> cachedImages = new List<Models.Photo>();

			if (cachedImagesSerialized != null)
			{
				cachedImages = JsonConvert.DeserializeObject<List<Models.Photo>>(cachedImagesSerialized);

				if (cachedImages.Count > 0)
				{
					Models.Photo image = cachedImages.FirstOrDefault(img => img.PhotoId == imageId);
					if (image != null) return image;
				}
			}

			var items = await FetchAllItemsAsync(userId);

			List<Landmark> landmarks = new List<Landmark>();
			List<Models.Photo> photos = new List<Models.Photo>();
			foreach (Models.Location location in items)
			{
				landmarks.AddRange(location.Landmarks);
			}
			foreach (Landmark landmark in landmarks)
			{
				photos.AddRange(landmark.Images);
			}
			foreach (Models.Photo photo in photos)
			{
				if (photo.PhotoId == imageId)
				{
					cachedImages.Add(photo);
					CacheResults(cacheKey, cachedImages);
					return photo;
				}
			}

			return null;
		}

		public async Task<List<string>> GetImagesByLocation(string userId, string locationName)
		{
			string cacheKey = userId + "_url_" + locationName.ToLower();
			string cachedUrls = GetCachedItem(cacheKey);
			List<string> urls = new List<string>();

			if (!string.IsNullOrEmpty(cachedUrls))
			{
				return JsonConvert.DeserializeObject<List<string>>(cachedUrls);
			}

			string queryString = "SELECT * FROM c where c.name='" + locationName + "' and c.userid='" + userId + "'";
			var items = await _cosmosDbService.GetItemsAsync(queryString);
			if (items.ToArray().Length == 0) return null;

			List<Landmark> landmarks = new List<Landmark>();
			List<Models.Photo> photos = new List<Models.Photo>();
			foreach (Models.Location location in items)
			{
				landmarks.AddRange(location.Landmarks);
			}
			foreach (Landmark landmark in landmarks)
			{
				photos.AddRange(landmark.Images);
			}
			foreach (Models.Photo photo in photos)
			{
				urls.Add(photo.Url);
			}

			CacheResults(cacheKey, urls);
			return urls;
		}

		private void CacheResults(string key, List<string> value)
		{
			if (value.Count > 0)
			{
				DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
				options.SetAbsoluteExpiration(CACHE_RETENTION_TIME);
				_cache.SetString(key, JsonConvert.SerializeObject(value), options);
			}
		}

		private void CacheResults(string key, List<Models.Photo> value)
		{
			if (value.Count > 0)
			{
				DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
				options.SetAbsoluteExpiration(CACHE_RETENTION_TIME);
				_cache.SetString(key, JsonConvert.SerializeObject(value), options);
			}
		}

		private string GetCachedItem(string key)
		{
			string item;
			try
			{
				item = _cache.GetString(key);
			}
			catch
			{
				item = null;
			}

			return item;
		}

		private async Task SaveSearchResults(Models.Location location)
		{
			if (!await DocumentExists(location))
			{
				_cosmosDbService.AddItemAsync(location);
			}
		}

		private List<string> FetchLocationUrls(Models.Location location)
		{
			return FetchAllUrlsForLocation(location);
		}

		private async Task<Models.Location> GetLocationObject(string userId, Dictionary<string, string> parameters, string locationName = null)
		{
			List<Venue> venues = _fourSquareService.SearchVenues(parameters);
			if (venues.Count == 0) return null;

			return await CreateLocationObjectAsync(venues, userId, locationName);
		}

		private async Task<bool> DocumentExists(Models.Location newLocation)
		{
			string queryString = "SELECT c.name FROM c where c.name='" + newLocation.Name + "' and c.userid='" + newLocation.UserId + "'";
			var items = await _cosmosDbService.GetLocationNameAsync(queryString);
			if (items.ToArray().Length > 0) return true;
			else return false;
		}

		private List<string> FetchAllUrlsForLocation(Models.Location location)
		{
			List<string> urls = new List<string>();
			List<Models.Photo> photos = new List<Models.Photo>();
			foreach (Landmark landmark in location.Landmarks)
			{
				photos.AddRange(landmark.Images);
			}
			foreach (Models.Photo photo in photos)
			{
				urls.Add(photo.Url);
			}

			return urls;
		}

		private async Task<Models.Location> CreateLocationObjectAsync(List<Venue> venues, string userId, string locationName)
		{
			Models.Location location = new Models.Location
			{
				UserId = userId,
				DisplayName = locationName,
				Id = Guid.NewGuid().ToString(),
				Latitude = null,
				Longitude = null,
				Landmarks = new List<Landmark>()
			};

			foreach (var venue in venues)
			{
				if (venue.location.city != null && location.DisplayName == null) location.DisplayName = venue.location.city;
				if (location.Latitude == null) location.Latitude = venue.location.lat.ToString();
				if (location.Longitude == null) location.Longitude = venue.location.lng.ToString();

				var options = new PhotoSearchOptions
				{
					Latitude = venue.location.lat,
					Longitude = venue.location.lng,
					PerPage = 20,
					Page = 1,
					Extras = PhotoSearchExtras.LargeUrl | PhotoSearchExtras.Tags
				};

				PhotoCollection photoCollection = await _flickrService.PhotosSearchAsync(options);
				List<Models.Photo> photos = new List<Models.Photo>();
				foreach (FlickrNet.Photo flickrPhoto in photoCollection)
				{
					Models.Photo photo = new Models.Photo()
					{
						PhotoId = flickrPhoto.PhotoId,
						UserId = flickrPhoto.UserId,
						Title = flickrPhoto.Title,
						Url = flickrPhoto.Small320Url,
						LicenseType = flickrPhoto.License.ToString(),
						Tags = string.Join(", ", flickrPhoto.Tags),
						Landmark = venue.name
					};
					photos.Add(photo);
				}

				Landmark landmark = new Landmark()
				{
					Name = venue.name,
					Id = venue.id,
					Images = photos
				};

				location.Landmarks.Add(landmark);
			}

			if (location.DisplayName == null) location.DisplayName = "Unknown";
			return location;
		}
	}
}
