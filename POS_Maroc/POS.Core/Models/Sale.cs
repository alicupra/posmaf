namespace POS.Core.Models
{
    public class Sale
    {
        public int Id { get; set; }
        
        public string ReceiptNumber { get; set; }
        
        public DateTime SaleDate { get; set; } = DateTime.Now;
        
        public int UserId { get; set; }
        public User Cashier { get; set; }
        
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }
        
        public decimal Subtotal { get; set; }
        
        public decimal TaxAmount { get; set; }
        
        public decimal DiscountAmount { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public decimal AmountTendered { get; set; }
        
        public decimal Change { get; set; }
        
        public int PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        
        public string Notes { get; set; }
        
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}