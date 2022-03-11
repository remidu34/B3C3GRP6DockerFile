using B3C3GRP6.Data;
using B3C3GRP6.Data.Providers;
using B3C3GRP6.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
   options =>
   {
       Console.WriteLine("Enable authorization using Swagger JWT");
       options.SwaggerDoc("v1", new OpenApiInfo { Title = "B3C3GRP6", Version = "v1" });

       options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
       {
           Type = SecuritySchemeType.Http,
           BearerFormat = "JWT",
           In = ParameterLocation.Header,
           Scheme = "bearer"
       });

       options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}

            }
        });
   }

);
// Add DB
string connString = builder.Configuration.GetConnectionString("B3C3GRP6Context");
builder.Services.AddDbContext<B3C3GRP6Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("B3C3GRP6Context"));

});
Console.WriteLine(" connexion à la base : " + connString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Issuer"]))
            };
        }
    );

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserLoggedInOnly", policy =>
        policy.RequireClaim("IsLogged", "True"));

    options.AddPolicy("RequireOtpOnly", policy =>
        policy.RequireClaim("canEnterOtp", "True"));

    options.AddPolicy("RequireMailOnly", policy =>
        policy.RequireClaim("canEnterMail", "True"));
});

// Add browser detection service
builder.Services.AddBrowserDetection();

// JWT Token Generation from Server Side
builder.Services.AddMvc();

// Allow cross domain
builder.Services.AddCors();

// Configuration Smtp
builder.Services.AddSingleton<ISmtpServices, SmtpServices>();

builder.Services.AddTransient<CompteProvider>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

/*builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 2;
    options.Lockout.AllowedForNewUsers = true;
});*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "B3C3GRP6 v1"));
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
