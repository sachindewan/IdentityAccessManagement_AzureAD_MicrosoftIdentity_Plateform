using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AccountTypesSingleOrg.Controllers;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AccountTypesSingleOrg
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication(config =>
            //{
            //    config.DefaultAuthenticateScheme = "IdentityServer.Cookie";
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
            //    op.ClientSecret = "Kq18Q~H0aoiWgrPM1FEtxDpnjRNn.s37awegZb1d";
            //    op.GetClaimsFromUserInfoEndpoint = true;
            //});
            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi(ProductCatalogAPI.SCOPES)
                .AddInMemoryTokenCaches();

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
