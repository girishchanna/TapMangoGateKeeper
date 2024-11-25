using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TapMangoGatekeeper.Configurations;
using TapMangoGatekeeper.Services;
using Microsoft.OpenApi.Models;

namespace TapMangoGateKeeper
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var rateLimitingOptions = new RateLimitingOptions();
            _configuration.Bind("RateLimitingOptions", rateLimitingOptions);

            services.AddSingleton(rateLimitingOptions);
            services.AddSingleton<IRateLimitService>(sp =>
                new RateLimitService(
                    rateLimitingOptions.MaxMessagesPerPhoneNumber,
                    rateLimitingOptions.MaxMessagesPerAccount
                ));

            services.AddControllers();

            // Add CORS services
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost",
                    builder => builder
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo
                {
                    Title = "TapMangoGateKeeper API",
                    Version = "v3"
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            // Use CORS policy
            app.UseCors("AllowLocalhost");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "TapMangoGateKeeper API V1");
            });
        }
    }
}
