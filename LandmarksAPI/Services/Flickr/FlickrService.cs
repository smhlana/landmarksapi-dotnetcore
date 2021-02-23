using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlickrNet;

namespace LandmarksAPI.Services
{
	public class FlickrService : IFlickrService
	{
		private Flickr _flickrClient;

		public FlickrService(string key)
		{
			_flickrClient = new Flickr(key);
		}

		public async Task<PhotoCollection> PhotosSearchAsync(PhotoSearchOptions options)
		{
			return await _flickrClient.PhotosSearchAsync(options);
		}
	}
}
