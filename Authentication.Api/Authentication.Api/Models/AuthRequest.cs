namespace Authentication.Api.Models
{
    public class AuthRequest
    {
        public string? Code  { get; set; }
        public string? State { get; set; }
    }
}
