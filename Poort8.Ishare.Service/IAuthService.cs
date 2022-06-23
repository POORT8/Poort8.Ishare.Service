using Microsoft.AspNetCore.Mvc;

namespace Poort8.Ishare.Service;

public interface IAuthService
{
    IActionResult? HandleAuthenticationAndAuthorization(string authorization, string delegationEvidence);
}
