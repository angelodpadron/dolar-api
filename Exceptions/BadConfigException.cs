namespace DolarApi.Exceptions;

using System;

public class BadConfigException : Exception
{
    public BadConfigException(string message)
        : base(message) { }
}
