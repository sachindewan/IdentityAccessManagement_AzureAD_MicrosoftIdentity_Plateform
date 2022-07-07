using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AccountTypesSingleOrg.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace AccountTypesSingleOrg.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ITokenAcquisition tokenAcquisition;
        public HomeController(ILogger<HomeController> logger, ITokenAcquisition tokenAcquisition)
        {
            _logger = logger;
            this.tokenAcquisition = tokenAcquisition;
        }
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        [AuthorizeForScopes]
        public async Task<IActionResult> Index()
        {
           var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(ProductCatalogAPI.SCOPES);
           //var idToken = HttpContext.GetTokenAsync("id_token").GetAwaiter().GetResult();
            //var accessToken1 = HttpContext.GetTokenAsync("access_token").GetAwaiter().GetResult();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    public static class ProductCatalogAPI
    {
        public const string CategoryUrl = "https://localhost:5050/api/Categories";
        public const string ProductUrl = "https://localhost:5050/api/Products";
        public const string ReadScope = "api://4d2bac1c-dd8e-4b05-9f59-7d32ec93eb87/application.read";

        public static List<string> SCOPES = new List<string>()
        {
            ReadScope
        };
    }

    public static class ClaimIds
    {
        public const string UserObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
    }
}
