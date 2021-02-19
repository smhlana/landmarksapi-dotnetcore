using Newtonsoft.Json;

namespace LandmarksAPI.Models
{
	public class Photo
	{
		[JsonProperty(PropertyName = "photoid")]
		public string PhotoId { get; set; }
		[JsonProperty(PropertyName = "userid")]
		public string UserId { get; set; }
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
		[JsonProperty(PropertyName = "licensetype")]
		public string LicenseType { get; set; }
		[JsonProperty(PropertyName = "tags")]
		public string Tags { get; set; }
		[JsonProperty(PropertyName = "landmark")]
		public string Landmark { get; set; }
	}
}
