using Microsoft.EntityFrameworkCore;
using l5.Data;
using Microsoft.AspNetCore.Identity;
using l5.Models;
using l5.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using l5.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MySqlConnection")));

builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Erkin",
        ValidAudience = "Bazar",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var cookieToken = context.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                context.Token = cookieToken;
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5000").AllowCredentials().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddScoped<JWTController>();


builder.Services.AddControllers();

builder.Services.AddRazorPages();

builder.Services.AddTransient<DataSeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await DataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding error:{ex.Message}");
    }
}

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/index.html");
        return;
    }
    await next();
});

app.UseStaticFiles();



app.UseRouting();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.MapRazorPages();

app.Run();




#region v1



//builder.Services.AddIdentity<User, IdentityRole>(options =>
//{
//    options.Password.RequireDigit = false;
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;  // Allow passwords without non-alphanumeric characters
//}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//public void ConfigureServices(IServiceCollection services)        where does this go
//{
//    services.AddIdentity<User, IdentityRole>()
//            .AddEntityFrameworkStores<AppDbContext>()
//            .AddDefaultTokenProviders();
//}

//public static IHostBuilder CreateHostBuilder(string[] args) =>
//    Host.CreateDefaultBuilder(args)
//        .ConfigureWebHostDefaults(webBuilder =>
//        {
//            webBuilder.UseStartup<Startup>();
//        })
//        .ConfigureServices((hostContext, services) =>
//        {
//            services.AddTransient<DataSeeder>();
//        })
//        .ConfigureAppConfiguration((context, config) =>
//        {
//            var serviceProvider = services.BuildServiceProvider();
//            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
//            DataSeeder.SeedAsync(serviceProvider, userManager).Wait();
//        });

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    //app.useswagger();
//    //app.useswaggerui();
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

#endregion