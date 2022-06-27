using Microsoft.AspNetCore.Mvc;
using Poort8.Ishare.Core;
using System.IdentityModel.Tokens.Jwt;

namespace Poort8.Ishare.Service;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuthenticationService _authenticationService;
    private readonly IPolicyEnforcementPoint _policyEnforcementPoint;

    public AuthService(
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IAuthenticationService authenticationService,
        IPolicyEnforcementPoint policyEnforcementPoint)
    {
        _logger = logger;
        _configuration = configuration;
        _authenticationService = authenticationService;
        _policyEnforcementPoint = policyEnforcementPoint;
    }

    public IActionResult? HandleAuthenticationAndAuthorization(string authorization, string delegationEvidence)
    {
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

        _logger.LogDebug("Received delegation_evidence header: {delegationEvidence}", delegationEvidence);

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.ReadJwtToken(authorization);

            var isPermitted = _policyEnforcementPoint.VerifyDelegationTokenPermit(
                _configuration["AuthorizationRegistryIdentifier"],
                delegationEvidence,
                _configuration["Playbook"],
                _configuration["MinimalPlaybookVersion"],
                accessToken.Audiences.First()); //TODO: Design generic way to verify the resource
            if (!isPermitted) { throw new Exception("VerifyDelegationTokenPermit returned false."); }
        }
        catch (Exception e)
        {
            _logger.LogInformation("Returning forbidden, invalid delegation evidence: {msg}", e.Message);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        _logger.LogInformation("Valid authentication and authorization.");
        return null;
    }
}
