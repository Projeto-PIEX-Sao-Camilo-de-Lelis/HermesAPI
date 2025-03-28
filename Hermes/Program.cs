
using Hermes.Configs.Cors;
using Hermes.Configs.Swagger;
using DotNetEnv;

namespace Hermes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                Env.Load();
            }

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            // Swagger.
            builder.Services.AddSwaggerConfiguration();
            // Cors.
            builder.Services.AddCorsConfiguration();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            app.UseCors("AllowAllOrigins");
            // app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
