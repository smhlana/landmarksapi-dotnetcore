using FlickrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services
{
	public interface IFlickrService
	{
		Task<PhotoCollection> PhotosSearchAsync(PhotoSearchOptions options);
	}
}
