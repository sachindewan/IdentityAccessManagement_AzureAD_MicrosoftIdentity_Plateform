using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountTypesSingleOrg.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AccountTypesSingleOrg.Models;
using AccountTypesSingleOrg.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
namespace AccountTypesSingleOrg.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ITokenAcquisition tokenAcquisition;
        //public HomeController(ILogger<HomeController> logger, ITokenAcquisition tokenAcquisition)
        //{
        //    _logger = logger;
        //    this.tokenAcquisition = tokenAcquisition;
        //}
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger,
            IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var b2cObjectId = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = _userService.GetById(b2cObjectId);
                if (user == null || string.IsNullOrWhiteSpace(user.B2CObjectId))
                {
                    var role = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("extension_UserRoles").Value;

                    user = new()
                    {
                        B2CObjectId = b2cObjectId,
                        Email = ((ClaimsIdentity)this.HttpContext.User.Identity).FindFirst("emails").Value,
                        UserRole = role
                    };

                    _userService.Create(user);
                }
            }
            return View();
        }
        //public async Task<IActionResult> Index()
        //    {
        //       //var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(ProductCatalogAPI.SCOPES);
        //       var idToken = HttpContext.GetTokenAsync("id_token").GetAwaiter().GetResult();
        //        var accessToken = HttpContext.GetTokenAsync("access_token").GetAwaiter().GetResult();
        //        return View();
        //    }

        public IActionResult EditProfile()
        {
            return  Challenge(new AuthenticationProperties()
            {
                RedirectUri = "/"
            }, "B2C_1_Edit_Profile");
        }
        
        public IActionResult Privacy()
        {
            return View();
        }
        [Permission("Profile Picture Approver")]
        public IActionResult ProfileApprover()
        {
            return View();
        }
        [Permission("Contractor")]
        public IActionResult Contractor()
        {
            return View();
        }
        public async Task<IActionResult> APICall()
        {
            var acccessToken = await HttpContext.GetTokenAsync("access_token");

            
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:44381/WeatherForecast");
            request.Headers.Authorization =
                new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, acccessToken);

            var response = await client.SendAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                //issue
            }
            return Content(await response.Content.ReadAsStringAsync());

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
