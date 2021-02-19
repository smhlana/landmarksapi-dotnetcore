using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FourSquare.SharpSquare.Entities;

namespace LandmarksAPI.Services
{
	public interface IFourSquareService
	{
		List<Venue> SearchVenues(Dictionary<string, string> parameters);
	}
}
