using POS.Core.Interfaces;
using POS.Core.Models;

namespace POS.Core.Services
{
    public class SaleService : ISaleService
    {
        private readonly IRepository<Sale> _saleRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IReceiptService _receiptService;

        public SaleService(
            IRepository<Sale> saleRepository,
            IRepository<Product> productRepository,
            IReceiptService receiptService)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _receiptService = receiptService;
        }

        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            // Générer un numéro de ticket unique
            sale.ReceiptNumber = GenerateReceiptNumber();
            
            // Calculer les totaux
            CalculateTotals(sale);
            
            // Mettre à jour le stock
            await UpdateInventoryAsync(sale);
            
            // Enregistrer la vente
            await _saleRepository.AddAsync(sale);
            
            // Imprimer le reçu
            await _receiptService.PrintReceiptAsync(sale);
            
            return sale;
        }

        private string GenerateReceiptNumber()
        {
            // Format marocain: MAG-AAMMJJ-XXXX
            string prefix = "AL"; // Préfixe du magasin
            string date = DateTime.Now.ToString("yyMMdd");
            string random = new Random().Next(1000, 9999).ToString();
            
            return $"{prefix}-{date}-{random}";
        }

        private void CalculateTotals(Sale sale)
        {
            sale.Subtotal = sale.Items.Sum(item => item.Quantity * item.UnitPrice);
            sale.TaxAmount = sale.Items.Sum(item => item.Quantity * item.UnitPrice * item.TaxRate / 100);
            sale.TotalAmount = sale.Subtotal + sale.TaxAmount - sale.DiscountAmount;
            sale.Change = sale.AmountTendered - sale.TotalAmount;
        }

        private async Task UpdateInventoryAsync(Sale sale)
        {
            foreach (var item in sale.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }
        }

        // Autres méthodes du service de vente...
    }
}