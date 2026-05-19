using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}