using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

 

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllersWithViews();



// Configure authentication

builder.Services.AddAuthentication(options =>

{

    options.DefaultAuthenticateScheme = "Cookies";

    options.DefaultChallengeScheme = "Microsoft";

})

.AddCookie("Cookies")

.AddOAuth("Microsoft", options =>

{

    IConfiguration configuration = builder.Configuration;

    options.ClientId = configuration["Authentication:Microsoft:ClientId"];

    options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];

    options.CallbackPath = "/signin-microsoft"; // Customize this as needed

    options.AuthorizationEndpoint = "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/authorize"; // Replace {tenant-id} with the appropriate value

    options.TokenEndpoint = "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token"; // Replace {tenant-id} with the appropriate value



    options.ClaimActions.MapJsonKey("urn:microsoft:identity:username", "upn");



    options.SaveTokens = true; // Save the access tokens for future use



    options.Events = new OAuthEvents

    {

        OnCreatingTicket = context =>

        {

            // Handle additional logic when creating the ticket

            // For example, you can map user claims here

            return Task.CompletedTask;

        }

    };

});



var app = builder.Build();



// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Home/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

    app.UseHsts();

}



app.UseHttpsRedirection();

app.UseStaticFiles();



app.UseRouting();



// Use authentication

app.UseAuthentication();

app.UseAuthorization();



app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
