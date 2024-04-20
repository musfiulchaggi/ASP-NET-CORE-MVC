using System.ComponentModel.DataAnnotations;

namespace project_ilcs.Models
{
    public class Products
    {
        [Key]
        public int ProductID { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public int? Tax { get; set; }

    }
}
