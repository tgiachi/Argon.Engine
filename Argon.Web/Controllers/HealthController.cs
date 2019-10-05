using Microsoft.AspNetCore.Mvc;

namespace Argon.Web.Controllers
{

	[Route("api/health")]
	[ApiController]
	public class HealthController : ControllerBase
	{
		public ActionResult<string> Ping()
		{
			return Ok("pong");
		}
	}
}
