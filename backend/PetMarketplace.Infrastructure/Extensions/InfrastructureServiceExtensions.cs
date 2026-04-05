using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Infrastructure.Persistence;
using PetMarketplace.Infrastructure.Services;

namespace PetMarketplace.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IListingImageService, ListingImageService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IInquiryService, InquiryService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
