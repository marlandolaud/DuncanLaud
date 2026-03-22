using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

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

                // Set the primary web root to the React dist folder so MapFallbackToFile
                // can find index.html, then composite-in wwwroot for images etc.
                env.WebRootPath = reactDistPath;
                env.WebRootFileProvider = new CompositeFileProvider(fileProviders);
            }

            // Serve index.html for root requests
            app.UseDefaultFiles();

            // Serve static files (React bundle + wwwroot/img)
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Fall back to index.html for any route React Router handles (/about, /book/*)
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
