using Newtonsoft.Json;
using System.Collections.Generic;

namespace LandmarksAPI.Models
{
	public class Landmark
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "images")]
		public List<Photo> Images { get; set; }
	}
}
