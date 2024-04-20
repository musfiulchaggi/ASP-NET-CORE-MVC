namespace project_ilcs.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string? Country { get; set; }
        public string? Harbor { get; set; }
        public int? ProductID { get; set; }
        public int? Price { get; set; }
        public int? TotalTaxPrice { get; set; }
    }
}
