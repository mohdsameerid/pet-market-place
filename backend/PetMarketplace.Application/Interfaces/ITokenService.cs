using PetMarketplace.Core.Entities;

namespace PetMarketplace.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
