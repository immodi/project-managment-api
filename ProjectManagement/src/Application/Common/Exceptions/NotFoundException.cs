namespace Application.Common.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException For(string entity, object key)
        => new($"{entity} with id '{key}' was not found.");
}