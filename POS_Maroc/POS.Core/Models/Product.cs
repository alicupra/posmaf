using System.ComponentModel.DataAnnotations;

namespace POS.Core.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(100)]
        public string NameAr { get; set; }
        
        [MaxLength(50)]
        public string Barcode { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        public decimal CostPrice { get; set; }
        
        public int TaxId { get; set; }
        public Tax Tax { get; set; }
        
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        
        public int StockQuantity { get; set; }
        
        public int MinStockLevel { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        public byte[] Image { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
}