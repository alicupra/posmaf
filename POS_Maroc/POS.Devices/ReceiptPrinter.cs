using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using POS.Core.Interfaces;
using POS.Core.Models;

namespace POS.Devices
{
    public class ReceiptPrinter : IReceiptPrinter
    {
        private const int MaxCharPerLine = 42;
        private readonly string _printerName;
        
        public ReceiptPrinter(string printerName)
        {
            _printerName = printerName;
        }
        
        public async Task PrintReceiptAsync(Sale sale)
        {
            try
            {
                // Générer le contenu du reçu
                string receiptContent = GenerateReceiptContent(sale);
                
                // Envoi à l'imprimante ESC/POS
                using (var printer = new PrinterConnection(_printerName))
                {
                    await printer.OpenAsync();
                    
                    // Initialisation de l'imprimante
                    await printer.WriteAsync(EscPosCommands.Initialize);
                    
                    // Formatage et impression du contenu
                    byte[] contentBytes = Encoding.GetEncoding(860).GetBytes(receiptContent);
                    await printer.WriteAsync(contentBytes);
                    
                    // Coupe du papier
                    await printer.WriteAsync(EscPosCommands.CutPaper);
                }
            }
            catch (Exception ex)
            {
                // Gérer l'exception et éventuellement utiliser une imprimante de secours
                Console.WriteLine($"Erreur d'impression: {ex.Message}");
                await FallbackToPdfAsync(sale);
            }
        }
        
        private string GenerateReceiptContent(Sale sale)
        {
            var sb = new StringBuilder();
            
            // En-tête du magasin
            sb.AppendLine(CenterText("AL MANAR MARKET"));
            sb.AppendLine(CenterText("123 Avenue Hassan II"));
            sb.AppendLine(CenterText("Casablanca, Maroc"));
            sb.AppendLine(CenterText("Tel: 05-22-12-34-56"));
            sb.AppendLine(CenterText("ICE: 123456789012345"));
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            
            // Informations sur la vente
            sb.AppendLine($"Date: {sale.SaleDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Ticket #: {sale.ReceiptNumber}");
            sb.AppendLine($"Caissier: {sale.Cashier.Name}");
            if (sale.Customer != null)
            {
                sb.AppendLine($"Client: {sale.Customer.Name}");
            }
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            
            // Articles
            sb.AppendLine("Désignation      Qté   P.U.     Total");
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            
            foreach (var item in sale.Items)
            {
                // Format: Produit (max 15 chars) | Qté (3) | Prix (8) | Total (8)
                string productName = item.Product.Name.Length > 15 
                    ? item.Product.Name.Substring(0, 12) + "..." 
                    : item.Product.Name.PadRight(15);
                
                sb.AppendLine($"{productName} {item.Quantity,3} {item.UnitPrice,8:N2} {item.TotalPrice,8:N2}");
            }
            
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            
            // Totaux
            sb.AppendLine($"{"Sous-total:",MaxCharPerLine-13}{sale.Subtotal,13:N2}");
            sb.AppendLine($"{"TVA:",MaxCharPerLine-13}{sale.TaxAmount,13:N2}");
            if (sale.DiscountAmount > 0)
            {
                sb.AppendLine($"{"Remise:",MaxCharPerLine-13}{sale.DiscountAmount,13:N2}");
            }
            sb.AppendLine($"{"TOTAL:",MaxCharPerLine-13}{sale.TotalAmount,13:N2}");
            
            // Méthode de paiement
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            sb.AppendLine($"Paiement: {sale.PaymentMethod.Name}");
            sb.AppendLine($"{"Montant reçu:",MaxCharPerLine-13}{sale.AmountTendered,13:N2}");
            sb.AppendLine($"{"Monnaie:",MaxCharPerLine-13}{sale.Change,13:N2}");
            
            // Pied de page
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            sb.AppendLine(CenterText("Merci de votre visite!"));
            sb.AppendLine(CenterText("À bientôt!"));
            
            // Mentions légales (spécifiques au Maroc)
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            sb.AppendLine("TVA acquittée selon les dispositions de");
            sb.AppendLine("la loi de finances N° 115-12");
            
            // Codes barres ou QR pour contrôle fiscal (simulation)
            sb.AppendLine(RepeatChar('-', MaxCharPerLine));
            sb.AppendLine(CenterText("[CODE FISCAL]"));
            
            // Saut de ligne final pour la coupe
            sb.AppendLine("\n\n\n");
            
            return sb.ToString();
        }
        
        private string CenterText(string text)
        {
            if (text.Length >= MaxCharPerLine)
                return text;
                
            int spaces = (MaxCharPerLine - text.Length) / 2;
            return new string(' ', spaces) + text;
        }
        
        private string RepeatChar(char c, int count)
        {
            return new string(c, count);
        }
        
        private async Task FallbackToPdfAsync(Sale sale)
        {
            // Génération d'un PDF comme solution de secours
            string pdfPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "POS_Receipts",
                $"Receipt_{sale.ReceiptNumber}.pdf");
                
            Directory.CreateDirectory(Path.GetDirectoryName(pdfPath));
            
            // Utilisation d'une bibliothèque PDF simple
            using (var pdfGenerator = new SimplePdfGenerator())
            {
                pdfGenerator.AddText(GenerateReceiptContent(sale));
                await pdfGenerator.SaveToFileAsync(pdfPath);
            }
        }
    }
    
    public static class EscPosCommands
    {
        // Commandes ESC/POS standard
        public static readonly byte[] Initialize = { 0x1B, 0x40 }; // ESC @
        public static readonly byte[] CutPaper = { 0x1D, 0x56, 0x41 }; // GS V A
        public static readonly byte[] BoldOn = { 0x1B, 0x45, 0x01 }; // ESC E 1
        public static readonly byte[] BoldOff = { 0x1B, 0x45, 0x00 }; // ESC E 0
        public static readonly byte[] DoubleWidthOn = { 0x1B, 0x21, 0x10 }; // ESC ! 16
        public static readonly byte[] DoubleWidthOff = { 0x1B, 0x21, 0x00 }; // ESC ! 0
    }
    
    public class PrinterConnection : IDisposable
    {
        private readonly string _printerName;
        private Stream _printerStream;
        
        public PrinterConnection(string printerName)
        {
            _printerName = printerName;
        }
        
        public async Task OpenAsync()
        {
            // Ouverture du port d'imprimante (simulation)
            _printerStream = new FileStream(
                $"\\\\localhost\\{_printerName}", 
                FileMode.OpenOrCreate, 
                FileAccess.Write);
        }
        
        public async Task WriteAsync(byte[] data)
        {
            if (_printerStream == null)
                throw new InvalidOperationException("Printer connection not open");
                
            await _printerStream.WriteAsync(data, 0, data.Length);
        }
        
        public void Dispose()
        {
            _printerStream?.Dispose();
        }
    }
    
    public class SimplePdfGenerator : IDisposable
    {
        private readonly StringBuilder _content = new StringBuilder();
        
        public void AddText(string text)
        {
            _content.AppendLine(text);
        }
        
        public async Task SaveToFileAsync(string filePath)
        {
            // Simulation simple de génération PDF
            await File.WriteAllTextAsync(filePath, _content.ToString());
        }
        
        public void Dispose()
        {
            // Nettoyage des ressources
        }
    }
}