using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [Authorize]
        public async Task Logout()
        {
            var client = _httpClientFactory.CreateClient("IDPClient");
            var discoveryDocumentResponse = await client
                .GetDiscoveryDocumentAsync();
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var revocateToken = async (string name) =>
            {
                var request = new TokenRevocationRequest
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imagegalleryclient",
                    ClientSecret = "secret",
                    Token = await HttpContext.GetTokenAsync(name)
                };
                var result = await client.RevokeTokenAsync(request);
                if (result.IsError)
                {
                    throw new Exception(result.Error);
                }
                return result;
            };
            var acccessTokenRevocationResponse = await revocateToken(OpenIdConnectParameterNames.AccessToken);
            var refreshTokenRevocationResponse = await revocateToken(OpenIdConnectParameterNames.RefreshToken);

            // Clears the  local cookie;  I.E logout of the Application.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirects to the IDP linked to scheme "OpenIdConnectDefaults.AuthenticationScheme" (oidc)
            // so it can clear its own session/cookie; I.E. signout of the IDP
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public AuthenticationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
        }

    }
}
