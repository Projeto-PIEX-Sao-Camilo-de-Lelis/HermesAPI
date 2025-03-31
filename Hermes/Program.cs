
using Hermes.Configs.Cors;
using Hermes.Configs.Swagger;
using DotNetEnv;
using Hermes.Configs.JsonSerializer;
using Hermes.Core.Interfaces.Data;
using Hermes.Configs.Postgresql;
using Hermes.Core.Interfaces.Repositories;
using Hermes.Data.Repositories;
using Hermes.Core.Interfaces.Services;
using Hermes.Core.Services;
using Hermes.Core.Profiles;
using Hermes.Configs.Authentication;

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
            builder.Services.ConfigureJwtAuthentication();
            builder.Services.AddCorsConfiguration();
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
            new PostgresConnectionFactory(connectionString));

            // Dependencies Injection.
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthenticationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            // app.UseHttpsRedirection();

            app.UseCors("AllowAllOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
