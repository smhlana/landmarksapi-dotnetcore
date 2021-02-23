using FourSquare.SharpSquare.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandmarksAPI.Models;
using FlickrNet;

namespace LandmarksAPI.Services
{
	public class Landmarks
	{
		IFourSquareService _fourSquareService;
		IFlickrService _flickrService;
		ICosmosDbService _cosmosDbService;
		public Landmarks(IFourSquareService fourSquareService, IFlickrService flickrService, ICosmosDbService cosmosDbService)
		{
			_fourSquareService = fourSquareService;
			_flickrService = flickrService;
			_cosmosDbService = cosmosDbService;
		}

		public async Task<List<string>> SearchAsync(string name)
		{
			List<string> urls = new List<string>();
			var parameters = new Dictionary<string, string>
			{
				{"near", name},
				{"radius", "50000"},
				{"limit", "6" },
				{"categoryId", "4bf58dd8d48988d12d941735"}
			};

			return await Search(parameters);
		}

		public async Task<List<string>> SearchAsync(string latitude, string longitude)
		{
			List<string> urls = new List<string>();
			string latLong = $"{latitude},{longitude}";
			var parameters = new Dictionary<string, string>
			{
				{"ll", latLong},
				{"radius", "50000"},
				{"limit", "6" },
				{"categoryId", "4bf58dd8d48988d12d941735"}
			};

			return await Search(parameters);
		}

		private async Task<List<string>> Search(Dictionary<string, string> parameters)
		{
			List<string> urls = new List<string>();
			List<Venue> venues = _fourSquareService.SearchVenues(parameters);
			if (venues.Count == 0) return urls;

			Models.Location location = await CreateLocationObjectAsync(venues, "1");
			if (await DocumentExists(location, "1"))
			{
				await _cosmosDbService.UpdateItemAsync(location);
				return FetchAllUrlsForLocation(location);
			}

			_cosmosDbService.AddItemAsync(location);
			return FetchAllUrlsForLocation(location);
		}

		public async Task<IEnumerable<Models.Location>> FetchAllAsync()
		{
			return await _cosmosDbService.GetItemsAsync("SELECT * FROM c where c.userid='1'");
		}

		public async Task<Models.Photo> GetImageDetaisByUrlAsync(string url)
		{
			var items = await FetchAllAsync();

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
				if (photo.Url == url) return photo;
			}

			return null;
		}

		public async Task<Models.Photo> GetImageDetaisByIdAsync(string id)
		{
			var items = await FetchAllAsync();

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
				if (photo.PhotoId == id) return photo;
			}

			return null;
		}

		public async Task<List<string>> GetImagesByLocation(string locationName)
		{
			List<string> urls = new List<string>();

			string queryString = "SELECT * FROM c where c.city='" + locationName + "' and c.userid='" + "1" + "'";
			var items = await _cosmosDbService.GetItemsAsync(queryString);
			if (items.ToArray().Length == 0) return urls;

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

			return urls;
		}

		private async Task<bool> DocumentExists(Models.Location newLocation, string userId)
		{
			string queryString = "SELECT c.name FROM c where c.city='" + newLocation.Name + "' and c.userid='" + userId + "'";
			var items = await _cosmosDbService.GetItemsAsync(queryString);
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

		private async Task<Models.Location> CreateLocationObjectAsync(List<Venue> venues, string user)
		{
			Models.Location location = new Models.Location
			{
				UserId = user,
				Name = null,
				Id = Guid.NewGuid().ToString(),
				Latitude = null,
				Longitude = null,
				Landmarks = new List<Landmark>()
			};

			foreach (var venue in venues)
			{
				if (venue.location.city != null && location.Name == null) location.Name = venue.location.city;
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

			if (location.Name == null) location.Name = "Unknown";
			return location;
		}
	}
}
