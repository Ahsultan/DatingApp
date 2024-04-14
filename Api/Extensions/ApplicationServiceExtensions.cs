using Microsoft.EntityFrameworkCore;


namespace Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDataContext>(option =>
        {
            option.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<ITokenService, TokenService>();

        services.AddCors();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}