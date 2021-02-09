using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Streamfox.Server
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Streamfox.Server.Controllers.Formatters;
    using Streamfox.Server.Persistence.Database;

    public class Startup
    {
        private const string SpaStaticFilesPath = "client";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<VideoDatabaseContext>(
                    options => options.UseNpgsql("Uid=postgres;Pwd=123456;Host=localhost;Port=5432;Database=streamfox"));
            services.AddSpaStaticFiles(
                    configuration => configuration.RootPath = SpaStaticFilesPath);
            services.AddControllers(
                    options =>
                    {
                        options.InputFormatters.Add(new RawByteStreamBodyFormatter());
                        options.InputFormatters.Add(new VideoIdParameterFormatter());
                    });
            services.AddVideoHosting();
            services.Configure<KestrelServerOptions>(
                    options => options.Limits.MaxRequestBodySize = int.MaxValue);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseMiddleware<HtmlOpenGraphMiddleware>();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    Console.WriteLine(context.Response.StatusCode);
                    await next.Invoke();
                    Console.WriteLine(context.Response.StatusCode);
                }
                else
                {
                    string rq = context.Request.Path.ToString();
                    string[] p = rq.Split('/');
                    
                    if (p[p.Length - 2] == "css")
                    {
                        await context.Response.WriteAsync(await File.ReadAllTextAsync($"/home/streamfox/client/dist/css/{p[p.Length - 1]}"));
                    }
                    else if (p[p.Length - 2] == "js")
                    {
                        await context.Response.WriteAsync(await File.ReadAllTextAsync($"/home/streamfox/client/dist/js/{p[p.Length - 1]}"));
                    }
                    else
                    {
                        await context.Response.WriteAsync(await File.ReadAllTextAsync("/home/streamfox/client/dist/index.html"));
                    }
                }
            });

            // app.UseSpa(
            //         spa =>
            //         {
            //             spa.Options.SourcePath = SpaStaticFilesPath;
            //
            //             if (env.IsDevelopment())
            //             {
            //                 spa.UseProxyToSpaDevelopmentServer("http://localhost:8080/");
            //             }
            //         });
        }
    }
}