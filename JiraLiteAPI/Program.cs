
using JiraLiteAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using JiraLiteAPI.Enum;


namespace WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            //Database
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    "Server=localhost\\SQLEXPRESS;Database=JiraLite;Trusted_Connection=True;TrustServerCertificate=True;"
                )
            );

            //Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                        const string doubleBearerPrefix = "Bearer Bearer ";

                        if (authorizationHeader.StartsWith(doubleBearerPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authorizationHeader[doubleBearerPrefix.Length..].Trim();
                        }

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost:5009",
                    ValidAudience = "http://localhost:5009",


                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("sgdsgd648d9f*/w43U4354t69ts8e22365fh")
                    ),

                    RoleClaimType = ClaimTypes.Role

                };
            });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(static options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Student Management API",
                    Version = "v1"
                });

                // JWT Swagger Auth
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Paste the JWT token only. Swagger adds the Bearer prefix automatically.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
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
                        Array.Empty<string>()
                    }
                });
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            app.UseCors("AllowAll");


            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();



            //  app.UseHttpsRedirection();

            // Static Files
            app.UseStaticFiles();


            // Authentication
            app.UseAuthentication();

            app.UseAuthorization();
            app.MapControllers();
           /* using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await DbInitializer.SeedRoles(services);
            }*/
            app.Run();
        }
    }
}
