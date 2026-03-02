namespace MovieRentalAPI.Exceptions
{
    public class UnAuthorizedException : IOException
    {
        public UnAuthorizedException() : base("Unauthorized access.")
        {
        }
        public UnAuthorizedException(string message) : base(message)
        {
        }
        public UnAuthorizedException(string message, IOException innerException) : base(message, innerException)
        {
        }
    }
}