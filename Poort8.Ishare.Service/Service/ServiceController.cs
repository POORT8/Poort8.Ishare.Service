using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Poort8.Ishare.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

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

    public ServiceController(
        ILogger<ServiceController> logger,
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
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "delegation_evidence")] string? delegationEvidence,
        [FromBody] dynamic requestBody)
    {
        var authorization = Request.Headers.Authorization;

        _logger.LogDebug("Received service POST request with authorization header: {authorization}", authorization.IsNullOrEmpty() ? "null" : authorization);

        var errorResponse = HandleAuthentication(authorization!, out string accessTokenAud);
        if (errorResponse is not null) { return errorResponse; }

        try
        {
            _logger.LogInformation("Sending POST request to backend service with body: {body}", (string)JsonSerializer.Serialize(requestBody));

            var url = $"{_configuration["BackendUrl"]}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("service_consumer_id", accessTokenAud);
            request.Headers.TryAddWithoutValidation("delegation_evidence", delegationEvidence);

            if (_configuration.GetValue<bool>("VerifyDelegationEvidence"))
            {
                errorResponse = HandleAuthorization(delegationEvidence!, accessTokenAud);
                if (errorResponse is not null) { return errorResponse; }
            }

            //NOTE: Add Link header (for FIWARE Context-LD Broker)
            if (Request.Headers.ContainsKey("Link"))
            {
                Request.Headers.TryGetValue("Link", out StringValues linkHeader);
                request.Headers.TryAddWithoutValidation("Link", linkHeader[0]);
            }

            request.Content = JsonContent.Create(requestBody); //NOTE: For now only json bodies

            var response = await _httpClient.SendAsync(request);

            _logger.LogInformation("Returning status code: {statusCode}", (int)response.StatusCode);
            return new StatusCodeResult((int)response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError("Returning internal server error, could not POST data to backend url: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id?}")]
    public async Task<IActionResult> Read(
        string? id,
        [FromHeader(Name = "delegation_evidence")] string? delegationEvidence)
    {
        var authorization = Request.Headers.Authorization;

        _logger.LogDebug("Received service GET request with authorization header: {authorization}", authorization.IsNullOrEmpty() ? "null" : authorization);

        var errorResponse = HandleAuthentication(authorization!, out string accessTokenAud);
        if (errorResponse is not null) { return errorResponse; }

        try
        {
            var url = $"{_configuration["BackendUrl"]}/{id}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("service_consumer_id", accessTokenAud);
            request.Headers.TryAddWithoutValidation("delegation_evidence", delegationEvidence);

            if (_configuration.GetValue<bool>("VerifyDelegationEvidence"))
            {
                errorResponse = HandleAuthorization(delegationEvidence!, accessTokenAud);
                if (errorResponse is not null) { return errorResponse; }
            }

            //NOTE: Add Link and Accept header (for FIWARE Context-LD Broker)
            if (Request.Headers.ContainsKey("Link"))
            {
                Request.Headers.TryGetValue("Link", out StringValues linkHeader);
                request.Headers.TryAddWithoutValidation("Link", linkHeader[0]);
                request.Headers.TryAddWithoutValidation("Accept", "application/ld+json;masked=false");
            }

            var data = await _httpClient.SendAsync(request);
            var jsonData = JsonDocument.Parse(await data.Content.ReadAsStringAsync());

            _logger.LogInformation("Returning data: {data}", JsonSerializer.Serialize(jsonData));
            return new OkObjectResult(jsonData);
        }
        catch (Exception e)
        {
            _logger.LogError("Returning internal server error, could not GET data at backend url: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromHeader(Name = "delegation_evidence")] string? delegationEvidence,
        [FromBody] dynamic requestBody)
    {
        var authorization = Request.Headers.Authorization;

        _logger.LogDebug("Received service PUT request with authorization header: {authorization}", authorization.IsNullOrEmpty() ? "null" : authorization);

        var errorResponse = HandleAuthentication(authorization!, out string accessTokenAud);
        if (errorResponse is not null) { return errorResponse; }

        try
        {
            //TODO: Use backend PUT (quick fix for FIWARE Context-LD Broker)
            var url = $"{_configuration["BackendUrl"]}/{id}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.TryAddWithoutValidation("service_consumer_id", accessTokenAud);
            request.Headers.TryAddWithoutValidation("delegation_evidence", delegationEvidence);

            if (_configuration.GetValue<bool>("VerifyDelegationEvidence"))
            {
                errorResponse = HandleAuthorization(delegationEvidence!, accessTokenAud);
                if (errorResponse is not null) { return errorResponse; }
            }

            var response = await _httpClient.SendAsync(request);

            _logger.LogInformation("Received status code {statusCode} on DELETE.", (int)response.StatusCode);
            _logger.LogInformation("Sending POST request to backend service with body: {body}", (string)JsonSerializer.Serialize(requestBody));

            url = $"{_configuration["BackendUrl"]}";
            request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("service_consumer_id", accessTokenAud);
            request.Headers.TryAddWithoutValidation("delegation_evidence", delegationEvidence);

            //NOTE: Add Link header (for FIWARE Context-LD Broker)
            if (Request.Headers.ContainsKey("Link"))
            {
                Request.Headers.TryGetValue("Link", out StringValues linkHeader);
                request.Headers.TryAddWithoutValidation("Link", linkHeader[0]);
            }

            request.Content = JsonContent.Create(requestBody); //NOTE: For now only json bodies

            response = await _httpClient.SendAsync(request);

            _logger.LogInformation("Returning status code: {statusCode}", (int)response.StatusCode);
            return new StatusCodeResult((int)response.StatusCode);
        }
        catch (Exception e)
        {
            _logger.LogError("Returning internal server error, could not PUT data at backend url: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private IActionResult? HandleAuthentication(string authorization, out string accessTokenAud)
    {
        if (string.IsNullOrEmpty(authorization))
        {
            accessTokenAud = string.Empty;
            return new UnauthorizedResult(); 
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.ReadJwtToken(authorization.Replace("Bearer ", ""));
            accessTokenAud = accessToken.Audiences.First();

            _logger.LogInformation("Service called by accessTokenAud: {accessTokenAud}", accessTokenAud);

            _authenticationService.ValidateAuthorizationHeader(_configuration["ClientId"]!, authorization);
        }
        catch (Exception e)
        {
            accessTokenAud = string.Empty;
            _logger.LogWarning("Returning bad request: invalid authorization header. {msg}", e.Message);
            return new UnauthorizedObjectResult("Invalid authorization header.");
        }

        _logger.LogInformation("Valid authentication.");
        return null;
    }

    private IActionResult? HandleAuthorization(string delegationEvidence, string accessTokenAud)
    {
        try
        {
            _logger.LogDebug("Received delegation_evidence header: {delegationEvidence}", delegationEvidence);

            //TODO: Design generic way to verify the resource
            var isPermitted = _policyEnforcementPoint.VerifyDelegationTokenPermit(
                _configuration["AuthorizationRegistryIdentifier"]!,
                delegationEvidence,
                accessTokenAud);
            if (!isPermitted) { throw new Exception("VerifyDelegationTokenPermit returned false."); }
        }
        catch (Exception e)
        {
            _logger.LogInformation("Returning forbidden, invalid delegation evidence: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        _logger.LogInformation("Valid authorization.");
        return null;
    }
}
