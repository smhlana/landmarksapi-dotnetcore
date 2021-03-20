namespace LandmarksAPI.Helpers.AppSettings
{
	public class AppSettings
	{
		public CosmosDbSettings LandmarksDb { get; set;}
		public CosmosDbSettings UsersDb { get; set; }
		public FSquareSettings FourSquare { get; set; }
		public FlickrSettings Flickr { get; set; }
		public AuthSettings AuthSettings { get; set; }
		public RedisSettings Redis { get; set; }
	}
}
