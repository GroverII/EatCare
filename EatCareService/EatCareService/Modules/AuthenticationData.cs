namespace EatCareService.Modules
{
    public class AuthenticationData
    {
        public int UserId { get; set; }
        public required string ApiKey { get; set; }
        public required string Password { get; set; }
    }
}
