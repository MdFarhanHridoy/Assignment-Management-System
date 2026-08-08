using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Infrastructure.Data;
using AssignmentManagement.Infrastructure.Identity;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace AssignmentManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
