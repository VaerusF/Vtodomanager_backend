using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using VtodoManager.NewsService.Controllers;
using VtodoManager.NewsService.DataAccess.Postgres;
using VtodoManager.NewsService.DomainServices.Interfaces;
using VtodoManager.NewsService.Infrastructure.Implementation.Options;
using VtodoManager.NewsService.Infrastructure.Implementation.Services;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;
using VtodoManager.NewsService.UseCases.Handlers.Errors.Commands;
using VtodoManager.NewsService.Web.Utils;

namespace VtodoManager.NewsService.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(SendErrorToClientRequest).Assembly);
            });

            services.AddOptions();
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.Configure<ProjectFilesOptions>(Configuration.GetSection("ProjectFilesOptions"));
            services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
            
            services.AddDbContext<IDbContext, AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PgSqlConnection"),
                    x => x.MigrationsAssembly("VtodoManager.NewsService.DataAccess.Postgres.Migrations")));
            
            services.AddStackExchangeRedisCache(options => {
                options.Configuration = Configuration.GetConnectionString("RedisVtodoManagerNewsService");
                options.InstanceName = Configuration.GetConnectionString("Hostname");
            });
            
            services.AddAuthentication(
                op =>
                {
                    op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    op.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }
            ).AddJwtBearer(
                op =>
                {
                    op.SaveToken = true;
                    op.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["JwtOptions:Issuer"],
                        ValidAudience = Configuration["IpListOptions:FrontClientAddress"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtOptions:Key"]!)),
                        ClockSkew = TimeSpan.Zero,
                    };
                }
            );

            services.AddHealthChecks();
            

            services.AddScoped<IConfigService, ConfigService>();
            services.AddScoped<ILogProducerService, LogProducerService>();
            services.AddScoped<INewsService, DomainServices.Implementation.NewsService>();
            services.AddScoped<IRedisKeysUtilsService, RedisKeysUtilsService>();
            
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
                Configuration.GetConnectionString("RedisVtodoManagerNewsService")!));
            
            services.AddHttpContextAccessor();
            
            services.AddCors();

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
            
            services.AddControllers()
                .AddApplicationPart(typeof(NewsController).Assembly);

            services.AddSwaggerGen(config =>
            {
                const string xmlFileProjectControllersDocs = "VtodoManager.NewsService.Controllers.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileProjectControllersDocs);
                config.IncludeXmlComments(xmlPath);
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                config.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[]{}
                    }
                });
            });
            
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(config =>
                {
                    config.RoutePrefix = string.Empty;
                    config.SwaggerEndpoint("swagger/v1/swagger.json", "VTodoNewsService private api");
                });
            }
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseRouting();
            
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHealthChecks("/health_check");
            
            //app.UseHttpsRedirection();
            
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}