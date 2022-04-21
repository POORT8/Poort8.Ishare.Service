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
    private readonly IPolicyEnforcementPoint _policyEnforcementPoint;

    public ServiceController(ILogger<ServiceController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IAuthenticationService authenticationService,
        IPolicyEnforcementPoint policyEnforcementPoint)
    {
        _logger = logger;
        _configuration = configuration;

        _httpClient = httpClientFactory.CreateClient(nameof(ServiceController));

        _authenticationService = authenticationService;
        _policyEnforcementPoint = policyEnforcementPoint;
    }

    //TODO: Swagger
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromHeader(Name = "delegation_evidence")] string delegationEvidence)
    {
        var authorization = Request.Headers.Authorization;

        _logger.LogInformation("Received service GET request with authorization header: {authorization}", authorization);

        if (string.IsNullOrEmpty(authorization)) { return new UnauthorizedResult(); }

        try
        {
            _authenticationService.ValidateAuthorizationHeader(_configuration["ClientId"], authorization);
        }
        catch (Exception e)
        {
            _logger.LogWarning("Returning bad request: invalid authorization header. {msg}", e.Message);
            return new UnauthorizedObjectResult("Invalid authorization header.");
        }

        _logger.LogInformation("Received service GET request with delegation_evidence header: {delegationEvidence}", delegationEvidence);
        try
        {
            var isPermitted = _policyEnforcementPoint.VerifyDelegationTokenPermit(_configuration["AuthorizationRegistryIdentifier"], delegationEvidence);
            if (!isPermitted) { throw new Exception("VerifyDelegationTokenPermit returned false."); }
        }
        catch (Exception e)
        {
            _logger.LogInformation("Returning forbidden, invalid delegation evidence: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        try
        {
            var data = await _httpClient.GetStringAsync(_configuration["BackendUrl"]);

            _logger.LogInformation("Returning data.");
            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            _logger.LogError("Returning internal server error, could not get data at backend url: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
