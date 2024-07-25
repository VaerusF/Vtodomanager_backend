using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VtodoManager.Logger.DataAccess.Postgres;
using VtodoManager.Logger.Infrastructure.Implementation.Options;
using VtodoManager.Logger.Infrastructure.Implementation.Service;
using VtodoManager.Logger.Infrastructure.Interfaces.DataAccess;
using VtodoManager.Logger.Infrastructure.Interfaces.Service;

namespace VtodoManager.Logger.Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public IConfiguration Configuration { get; }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();
        services.Configure<ConnectionStringsOptions>(Configuration.GetSection("ConnectionStrings"));
        
        services.AddDbContext<IDbContext, AppDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("PgSqlConnection"),
                x => x.MigrationsAssembly("VtodoManager.Logger.DataAccess.Postgres.Migrations")));
        
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<ILogSaverService, LogSaverService>();
        
        services.AddHostedService<LogConsumerBackgroundService>();
        
        services.AddHttpContextAccessor();
        
        services.AddCors();

        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });
    }

    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseRouting();
        app.UseHttpsRedirection();
        
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = CookieSecurePolicy.Always
        });
    }
}

