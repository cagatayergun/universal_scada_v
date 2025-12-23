namespace TekstilScada.WebAPI.Models
{
    public class CentralFactory
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string FactoryName { get; set; }
        public string HardwareKey { get; set; }
    }

    public class CentralUser
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string AllowedFactoryIds { get; set; } // "1,2" veya "ALL"
    }
}