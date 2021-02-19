using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandmarksAPI.Services
{
	public interface ILandmarksService
	{
		Task<IEnumerable<string>> SearchByName(string name);
	}
}
