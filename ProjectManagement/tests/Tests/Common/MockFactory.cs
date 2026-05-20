using Application.Interfaces;
using Domain.Interfaces;
using Moq;

namespace Tests.Common;

public static class MockFactory
{
    public static Mock<IUserRepository> Users()
        => new();

    public static Mock<IProjectRepository> Projects()
        => new();

    public static Mock<ITaskRepository> Tasks()
        => new();

    public static Mock<IPasswordHasher> PasswordHasher()
        => new();

    public static Mock<IJwtProvider> JwtProvider()
        => new();
}