using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountTypesSingleOrg.Controllers;
using AccountTypesSingleOrg.Data;
using AccountTypesSingleOrg.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AccountTypesSingleOrg
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        //public static string Tenant = "azureADB2CDotNetMastery.onmicrosoft.com";
        //public static string AzureADB2CHostname = "azureadb2cdotnetmastery.b2clogin.com";
        public static string ClientID = "207e762c-a9e8-40c6-b53c-db43bdcc4a9f";
        //public static string PolicySignUpSignIn = "B2C_1_SignIn_Up";
       // public static string PolicyEditProfile = "B2C_1_Edit";
        public static string Scope = "https://SachinRADB2C.onmicrosoft.com/379fdec8-dbe1-482c-97fc-c8d5e7db9abd/FullAccess";
       // public static string ClientSecret = "qPZJSSK0.Zw3V.534v~gCcXr.4.lqGl_BX";

        //public static string AuthorityBase = $"https://{AzureADB2CHostname}/{Tenant}/";
        //public static string AuthoritySignInUp = $"{AuthorityBase}{PolicySignUpSignIn}/v2.0";
        //public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}/v2.0";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(
                Configuration.GetConnectionString("DefaultConnection")));
            services.AddHttpClient();
            services.AddScoped<IUserService, UserService>();
            //Azure AdB2C Flow single tinant
            services.AddAuthentication(config =>
            {
                config.DefaultScheme = "IdentityServer.Cookie";
                config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            }).AddCookie("IdentityServer.Cookie", config =>
            {
                config.LogoutPath = "/Account/SignOut";
            }).AddOpenIdConnect(op =>
            {
                op.SignInScheme = "IdentityServer.Cookie";
                op.Authority = "https://sachinradb2c.b2clogin.com/SachinRADB2C.onmicrosoft.com/B2C_1_SignIn_Up_Policy/v2.0/";
                op.ClientId = Startup.ClientID;
                op.SaveTokens = true;
                op.ResponseType = OpenIdConnectResponseType.Code;
                //delegate access to the api
                op.Scope.Add(Startup.Scope);
                op.ClientSecret = "l9a8Q~L1ayb1lW6WDmMEL4~12ip3GNHsMXN4VbgH";
                op.Prompt ="consent";
                //required when ever used implicit flow
                //op.GetClaimsFromUserInfoEndpoint = true;
                op.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "name"
                };
                op.Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = async opt =>
                    {
                        string role = opt.Principal.FindFirstValue("extension_UserRoles");

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, role)
                        };
                        var appIdentity = new ClaimsIdentity(claims);
                        opt.Principal.AddIdentity(appIdentity);
                    }
                };
            }).AddOpenIdConnect("B2C_1_Edit_Profile",getOpenIdConnectOptions("B2C_1_Edit_Profile"));
            //Azure Ad Flow single tinant
            //services.AddAuthentication(config =>
            //{
            //    config.DefaultScheme = "IdentityServer.Cookie";
            //    config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            //}).AddCookie("IdentityServer.Cookie", config =>
            //{
            //    config.LogoutPath = "/Account/SignOut";
            //}).AddOpenIdConnect(op =>
            //{
            //    op.SignInScheme = "IdentityServer.Cookie";
            //    op.Authority = "https://login.microsoftonline.com/1bacb7b6-5bdb-4fd0-8d14-667ea869df26/v2.0";
            //    op.ClientId = "4d2bac1c-dd8e-4b05-9f59-7d32ec93eb87";
            //    op.SaveTokens = true;
            //    op.ResponseType = OpenIdConnectResponseType.Code;
            //    op.SaveTokens = true;
            //    //delegate access to the api
            //    op.Scope.Add("api://96ae1c8d-0ff6-49b6-a493-daa31d0ecfaf/AdminAccess");
            //    op.ClientSecret = "Kq18Q~H0aoiWgrPM1FEtxDpnjRNn.s37awegZb1d";
            //    //if you want to prompt every time.
            //    op.Prompt = "consent";
            //    //required when ever used implicit flow
            //    //op.GetClaimsFromUserInfoEndpoint = true;
            //    op.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        NameClaimType = "name"
            //    };
            //});
            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));
            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
            //    .EnableTokenAcquisitionToCallDownstreamApi(ProductCatalogAPI.SCOPES)
            //    .AddInMemoryTokenCaches();

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddRazorPages()
                .AddMicrosoftIdentityUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private Action<OpenIdConnectOptions> getOpenIdConnectOptions(string policy) => (op) =>
        {
            op.SignInScheme = "IdentityServer.Cookie";
            op.Authority = $"https://sachinradb2c.b2clogin.com/SachinRADB2C.onmicrosoft.com/{policy}/v2.0/";
            op.ClientId = Startup.ClientID;
            op.SaveTokens = true;
            op.ResponseType = OpenIdConnectResponseType.Code;
            //delegate access to the api
            op.CallbackPath = "/signin-oidc-" + policy;
            op.Scope.Add(Startup.Scope);
            op.ClientSecret = "l9a8Q~L1ayb1lW6WDmMEL4~12ip3GNHsMXN4VbgH";
            //required when ever used implicit flow
            //op.GetClaimsFromUserInfoEndpoint = true;
            op.TokenValidationParameters = new TokenValidationParameters()
            {
                NameClaimType = "name"
            };
            op.Events = new OpenIdConnectEvents
            {
                OnMessageReceived = context =>
                {
                    if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) &&
                        !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
                    {
                        if (context.ProtocolMessage.Error.Contains("access_denied"))
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
                        }
                    }
                    return Task.FromResult(0);
                }
            };
        };
    }
}
