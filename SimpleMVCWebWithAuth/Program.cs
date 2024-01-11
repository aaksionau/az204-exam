using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleMVCWebWithAuth.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//database:
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//TODO: (chapter 7 - Microsoft Identity)
//       Make sure to put appropriate settings into user.secrets
/*
builder.Services.AddAuthentication().AddMicrosoftAccount(options => {
    options.ClientId = builder.Configuration["MicrosoftSignIn:ClientId"];
    options.ClientSecret = builder.Configuration["MicrosoftSignIn:ClientSecret"];
});
*/

//Identity:
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

//Application Insights
//TODO (chapter 4): Make sure to set a valid application insights key in user.secrets to run locally:
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

//TODO (chapter 7): Inject the User service for managing users and roles
//user service
builder.Services.AddScoped<IUserRolesService, UserRolesService>();

//***********************************************************************
// TODO (chapter 4 - optional): enable automatic migrations [ensures migrations at Azure]
// *************
// NOTE: IF you do this, you will never be able to roll-back a migration with this code in place.  Use at your own discretion:
// **************
// automatically apply database migrations (breaks solution if database not wired up correctly, forces roll-forward approach
/*
var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options;
using (var context = new ApplicationDbContext(contextOptions))
{
    context.Database.Migrate();
}
*/
//***********************************************************************


//***************************************** REDIS CACHE **************************************************
// TODO: Chapter 9:
//Redis Cache Integration
//Uncomment the following section to integrate with Redis Cache
//additional information: https://github.com/blgorman/UsingRedisWithAzure/blob/main/RedisNotes.md#set-up-application
//********************************************************************************************************
/* 
var redisCNSTR = builder.Configuration["Redis:ConnectionString"]; //redisSection.GetValue<string>("ConnectionString").ToString();
var redisInstanceName = builder.Configuration["Redis:InstanceName"]; // redisSection.GetValue<string>("InstanceName");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString(redisCNSTR);
    options.InstanceName = redisInstanceName;
});

//Direct access to the cache
builder.Services.AddSingleton(async x => await RedisConnection.InitializeAsync(connectionString: redisCNSTR));
*/
//***************************************** END REDIS CACHE **************************************************

//TODO (chapter 8 - secure cloud solutions)
//********************************************* SECURE CLOUD SOLUTIONS ***************************************
/*
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    var settings = config.Build();

    var env = settings["Application:Environment"];

    if (env == null || !env.Trim().Equals("develop", StringComparison.OrdinalIgnoreCase))
    {
        //requires managed identity on both app service and app config
        var cred = new ManagedIdentityCredential();

        config.AddAzureAppConfiguration(options =>
                options.Connect(new Uri(settings["AzureAppConfigConnection"]), cred));

        //enable this and disable the two lines above to utilize Key vault in combination with the Azure App Configuration:
        //note: This requires an access policy for Get Secret on key vault for the app service and app config using their managed identities

        //config.AddAzureAppConfiguration(options =>
        //    options.Connect(new Uri(settings["AzureAppConfigConnection"]), cred)
        //                    .ConfigureKeyVault(kv => { kv.SetCredential(cred); }));
    }
    else
    {
        var cred = new DefaultAzureCredential();
        config.AddAzureAppConfiguration(options =>
            options.Connect(settings["AzureAppConfigConnection"]));

        //enable this and disable the two lines above to utilize Key vault in combination with the Azure App Configuration:
        //note: This required an access policy for key vault for the developer

        //config.AddAzureAppConfiguration(options =>
        //    options.Connect(settings["AzureAppConfigConnection"])
        //                    .ConfigureKeyVault(kv => { kv.SetCredential(cred); }));
    }
});
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
