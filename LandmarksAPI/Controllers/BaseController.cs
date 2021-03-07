using LandmarksAPI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LandmarksAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public abstract class BaseController : ControllerBase
	{
		// returns the current authenticated account (null if not logged in)
		public Account AccountContext => (Account)HttpContext.Items["Account"];
	}
}
