using Newtonsoft.Json;

namespace LandmarksAPI.Entities
{
	public class User
	{
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
        [JsonProperty(PropertyName = "passwordhash")]
        public byte[] PasswordHash { get; set; }
        [JsonProperty(PropertyName = "passwordsalt")]
        public byte[] PasswordSalt { get; set; }
    }
}
