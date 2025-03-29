
using Hermes.Configs.Cors;
using Hermes.Configs.Swagger;
using DotNetEnv;
using Hermes.Configs.JsonSerializer;
using Hermes.Core.Interfaces.Data;
using Hermes.Configs.Postgresql;
using Hermes.Core.Interfaces.Repositories;
using Hermes.Data.Repositories;

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

            string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")!;

            // Auth
            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.ConfigureJsonSerializer();
            builder.Services.AddCorsConfiguration();
            builder.Services.AddSwaggerConfiguration();

            builder.Services.AddSingleton<IDbConnectionFactory>(provider => 
            new PostgresConnectionFactory(connectionString));

            // Dependencies Injection.
            builder.Services.AddScoped<IUserRepository, UserRepository>();

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
