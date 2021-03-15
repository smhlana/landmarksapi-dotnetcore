using Newtonsoft.Json;
using System.Collections.Generic;

namespace LandmarksAPI.Models
{
	public class Location
	{
		[JsonProperty(PropertyName = "userid")]
		public string UserId { get; set; }
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
		[JsonProperty(PropertyName = "displayname")]
		public string DisplayName { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name => DisplayName.ToLower();
		[JsonProperty(PropertyName = "latitude")]
		public string Latitude { get; set; }
		[JsonProperty(PropertyName = "longitude")]
		public string Longitude { get; set; }
		[JsonProperty(PropertyName = "landmarks")]
		public List<Landmark> Landmarks { get; set; }
	}
}
