using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

[ApiController]
[Route("api/test-auth")]
public class TestAuthController : ControllerBase
{
    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new
        {
            message = "You are authenticated."
        });
    }

    [Authorize(Roles = "Merchant")]
    [HttpGet("merchant")]
    public IActionResult MerchantOnly()
    {
        return Ok(new
        {
            message = "You are authenticated as a Merchant."
        });
    }
}
