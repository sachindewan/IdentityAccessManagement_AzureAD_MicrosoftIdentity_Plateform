using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AccountTypesSingleOrg.Areas.MicrosoftIdentity
{
    public class AccountController : Controller
    {
        //[AllowAnonymous]
        public async Task SignOut()
        {
            await HttpContext.SignOutAsync("IdentityServer.Cookie");
            await HttpContext.SignOutAsync("IdentityServer.Cookie");
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
