using FourSquare.SharpSquare.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services
{
	public class LandmarksService : ILandmarksService
	{
		IFourSquareService _fourSquareService;
		public LandmarksService(IFourSquareService fourSquareService)
		{
			_fourSquareService = fourSquareService;
		}

		public Task<IEnumerable<string>> SearchByName(string name)
		{
			var parameters = new Dictionary<string, string>
			{
				{"near", name},
				{"radius", "50000"},
				{"limit", "6" },
				{"categoryId", "4bf58dd8d48988d12d941735"}
			};

			List<Venue> venues = _fourSquareService.SearchVenues(parameters);
			if (venues.Count == 0) return null;

			return null;
		}
	}
}
