using Dapper;
using DotNetEnv;
using Hermes.Configs.Authentication;
using Hermes.Configs.Cache;
using Hermes.Configs.Cloudinary;
using Hermes.Configs.Cors;
using Hermes.Configs.JsonSerializer;
using Hermes.Configs.Postgresql;
using Hermes.Configs.Swagger;
using Hermes.Core.Interfaces.Data;
using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Services;
using Hermes.Data.Repositories;
using Hermes.Data.TypeHandlers;

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

            string connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")!;

            // Auth
            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddJsonSerializerConfiguration();
            builder.Services.AddJwtConfiguration();
            builder.Services.AddCorsConfiguration();
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddCloudinaryConfiguration();
            builder.Services.AddCacheConfiguration();

            // Database Connection.
            builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
            new PostgresConnectionFactory(connectionString));
            SqlMapper.AddTypeHandler(new RoleTypeHandler());
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

            // Dependencies Injection.
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
            builder.Services.AddScoped<IBlogPostService, BlogPostService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}