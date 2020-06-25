namespace Streamfox.Server
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Streamfox.Server.Controllers.Formatters;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.InputFormatters.Add(new RawByteStreamBodyFormatter());
                options.InputFormatters.Add(new VideoIdParameterFormatter());
            });
            services.AddVideoHosting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseSpa(
                    spa =>
                    {
                        if (env.IsDevelopment())
                        {
                            spa.UseProxyToSpaDevelopmentServer("http://localhost:8080/");
                        }
                    });
        }
    }
}