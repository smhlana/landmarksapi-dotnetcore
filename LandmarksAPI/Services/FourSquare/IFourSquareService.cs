using System.Collections.Generic;
using FourSquare.SharpSquare.Entities;

namespace LandmarksAPI.Services
{
	public interface IFourSquareService
	{
		List<Venue> SearchVenues(Dictionary<string, string> parameters);
	}
}
