using Microsoft.AspNetCore.Mvc;
using Poort8.Ishare.Core;

namespace Poort8.Ishare.Service.Service;

[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly ILogger<ServiceController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationService;

    public ServiceController(ILogger<ServiceController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IAuthenticationService authenticationService)
    {
        _logger = logger;
        _configuration = configuration;

        _httpClient = httpClientFactory.CreateClient(nameof(ServiceController));
        _httpClient.BaseAddress = new Uri("http://orion:1026");

        _authenticationService = authenticationService;
    }

    //TODO: Swagger
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromHeader(Name = "Authorization")] string authorization,
        [FromHeader(Name = "delegation_evidence")] string delegationEvidence,
        [FromBody] dynamic entity)
    {
        _logger.LogInformation("Received service request with authorization header: {authorization}", authorization);

        if (string.IsNullOrEmpty(authorization)) { return new UnauthorizedResult(); }

        try
        {
            _authenticationService.ValidateAccessToken(_configuration["ClientId"], authorization);
        }
        catch (Exception e)
        {
            _logger.LogWarning("Returning bad request: invalid authorization header. {msg}", e.Message);
            return new UnauthorizedObjectResult("Invalid authorization header.");
        }

        //TODO: Handle delegationEvidence

        //TODO: WIP post to context broker
        var content = JsonContent.Create(entity);
        try
        {
            var test = await _httpClient.PostAsync("/ngsi-ld/v1/entities/", content);
            return new OkObjectResult(test);
        }
        catch (Exception e)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
