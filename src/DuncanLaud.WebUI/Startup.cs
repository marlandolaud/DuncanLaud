using System.Collections.Generic;
using System.IO;
using AspNetCoreRateLimit;
using DuncanLaud.Infrastructure.Data;
using DuncanLaud.Infrastructure.Interfaces;
using DuncanLaud.Infrastructure.Repositories;
using DuncanLaud.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DuncanLaud.WebUI
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
            // MVC Controllers
            services.AddControllers();

            // Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();

            // Domain/Application Services
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IPersonService, PersonService>();

            // Rate Limiting (OWASP A07)
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddInMemoryRateLimiting();

            services.AddRouting();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "DuncanLaud API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Auto-apply EF Core migrations on startup (skip for InMemory provider used in tests).
            // Wrapped in try-catch so a DB failure never blocks middleware registration —
            // without this, a missing connection string causes Configure() to abort before
            // UseRouting / MapControllers, making every request fall to the SPA fallback.
            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    if (db.Database.IsRelational())
                    {
                        db.Database.Migrate();
                    }
                    else
                    {
                        db.Database.EnsureCreated();
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<Startup>();
                logger.LogCritical(ex, "Database migration failed — API routes will still be registered but DB operations will return errors.");
            }

            // Trust X-Forwarded-Proto from IIS (SSL termination) so UseHttpsRedirection
            // doesn't redirect requests that are already HTTPS at the client.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DuncanLaud API v1");
                });
            }
            else
            {
                app.UseHsts();
            }

            // Security headers (OWASP A05)
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
                ctx.Response.Headers["X-Frame-Options"] = "DENY";
                ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                ctx.Response.Headers["X-XSS-Protection"] = "0";
                ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                // Remove server identification header
                ctx.Response.Headers.Remove("Server");
                await next();
            });

            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Rate Limiting
            app.UseIpRateLimiting();

            // Development: React files live in ../duncanlaud-react/dist after running
            //   "npm run build" in the react project.
            // Production (dotnet publish): the MSBuild target copies dist/ into wwwroot/,
            //   so the dist path won't exist and we fall back to wwwroot automatically.
            // In both cases we compose with wwwroot so /img/* assets are always served.
            var reactDistPath = Path.GetFullPath(
                Path.Combine(env.ContentRootPath, "..", "duncanlaud-react", "dist"));

            if (Directory.Exists(reactDistPath))
            {
                var fileProviders = new List<IFileProvider>
                {
                    new PhysicalFileProvider(reactDistPath)
                };

                if (!string.IsNullOrEmpty(env.WebRootPath) && Directory.Exists(env.WebRootPath))
                {
                    fileProviders.Add(new PhysicalFileProvider(env.WebRootPath));
                }

                env.WebRootPath = reactDistPath;
                env.WebRootFileProvider = new CompositeFileProvider(fileProviders);
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Diagnostic: inline endpoint to verify routing pipeline works
                endpoints.MapGet("/api/ping", async context =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        $"{{\"status\":\"ok\",\"env\":\"{env.EnvironmentName}\",\"routes\":{endpoints.DataSources.Sum(ds => ds.Endpoints.Count)}}}");
                });

                // API controllers must be mapped BEFORE the SPA fallback
                endpoints.MapControllers();

                // Fall back to index.html for any route React Router handles
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
