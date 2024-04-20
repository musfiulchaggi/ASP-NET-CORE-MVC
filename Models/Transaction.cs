namespace project_ilcs.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string? Country { get; set; }
        public string? Harbor { get; set; }
        public string? ProductName { get; set; }
        public int? Price { get; set; }
        public int? Tax { get; set; }
    }
}
