using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService AuthenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        AuthenticationService = authenticationService;
    }

    [HttpGet("token")]
    public async Task<IActionResult> GenerateToken([FromHeader(Name = "Authorization")] string secret)
    {
        var token = await AuthenticationService.Authenticate(secret);

        if (token == null)
        {
            return Unauthorized();
        }

        return Ok(token);
    }
}
