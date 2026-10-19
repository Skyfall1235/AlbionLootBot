namespace AlbionLootBot.Models;

/// <summary>
/// Exception for when a user is not inside of the database and would therefor break some internal logic
/// </summary>
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message) : base(message) { }
}

/// <summary>
/// custom exception for when a user attempts to perform an action of an item that is no longer allowed to be modified
/// </summary>
public class InaccessableActionException : Exception
{
    public InaccessableActionException(string message) : base(message) { }
}

/// <summary>
/// could use just exception, this is just for knowing more specifically where in the code as it appears rarely
/// </summary>
public class UnknownErrorException : Exception
{
    public UnknownErrorException(string message) : base(message) { }
}
