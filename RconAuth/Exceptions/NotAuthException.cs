using System;

namespace RconAuth.Exceptions
{
    public class NotAuthException : Exception
    {
        public NotAuthException() : base ("Not authenticated.") { }
    }
}
