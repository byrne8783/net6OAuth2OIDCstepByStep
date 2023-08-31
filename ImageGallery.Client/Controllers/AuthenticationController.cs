using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        [Authorize]
        public async Task Logout()
        {
            // Clears the  local cookie;  I.E logout of the Application.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirects to the IDP linked to scheme
            // "OpenIdConnectDefaults.AuthenticationScheme" (oidc)
            // so it can clear its own session/cookie; I.E. signout of the IDP
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
