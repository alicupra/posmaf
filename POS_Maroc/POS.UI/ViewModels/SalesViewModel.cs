using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using POS.Core.Models;
using POS.Core.Services;

namespace POS.UI.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        
        private string _searchText;
        private Category _selectedCategory;
        private Customer _selectedCustomer;
        
        public SalesViewModel(
            ISaleService saleService,
            IProductService productService,
            ICustomerService customerService)
        {
            _saleService = saleService;
            _productService = productService;
            _customerService = customerService;
            
            LoadData();
            
            // Commandes
            AddProductCommand = new RelayCommand<Product>(AddProductToCart);
            RemoveItemCommand = new RelayCommand<CartItem>(RemoveCartItem);
            ClearCartCommand = new RelayCommand(ClearCart);
            ProcessCashPaymentCommand = new RelayCommand(ProcessCashPayment);
            ProcessCardPaymentCommand = new RelayCommand(ProcessCardPayment);
        }
        
        public string CashierName { get; set; }
        public string CurrentDateTime => DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product> FilteredProducts { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();
        public ObservableCollection<CartItem> CartItems { get; } = new ObservableCollection<CartItem>();
        
        public decimal Subtotal => CartItems.Sum(i => i.TotalPrice);
        public decimal TaxAmount => CartItems.Sum(i => i.TotalPrice * i.Product.Tax.Rate / 100);
        public decimal TotalAmount => Subtotal + TaxAmount;
        
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterProducts();
                }
            }
        }
        
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    FilterProducts();
                }
            }
        }
        
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer != value)
                     {
                    _selectedCustomer = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Commands
        public ICommand AddProductCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand ProcessCashPaymentCommand { get; }
        public ICommand ProcessCardPaymentCommand { get; }
        
        private async void LoadData()
        {
            // Load categories
            var categories = await _productService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            
            // Load products
            var products = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            
            FilterProducts();
            
            // Load customers
            var customers = await _customerService.GetAllCustomersAsync();
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
        
        private void FilterProducts()
        {
            var query = Products.AsQueryable();
            
            // Filter by category
            if (SelectedCategory != null)
            {
                query = query.Where(p => p.CategoryId == SelectedCategory.Id);
            }
            
            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchLower) || 
                    p.Barcode.ToLower().Contains(searchLower));
            }
            
            FilteredProducts.Clear();
            foreach (var product in query)
            {
                FilteredProducts.Add(product);
            }
        }
        
        private void AddProductToCart(Product product)
        {
            if (product == null) return;
            
            var existingItem = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
            }
            else
            {
                CartItems.Add(new CartItem
                {
                    Product = product,
                    UnitPrice = product.Price,
                    Quantity = 1,
                    TotalPrice = product.Price
                });
            }
            
            UpdateTotals();
        }
        
        private void RemoveCartItem(CartItem item)
        {
            if (item == null) return;
            
            CartItems.Remove(item);
            UpdateTotals();
        }
        
        private void ClearCart()
        {
            CartItems.Clear();
            UpdateTotals();
        }
        
        private async void ProcessCashPayment()
        {
            if (CartItems.Count == 0) return;
            
            try
            {
                var sale = CreateSaleFromCart(1); // 1 = Cash payment method ID
                await _saleService.CreateSaleAsync(sale);
                ClearCart();
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
        
        private async void ProcessCardPayment()
        {
            if (CartItems.Count == 0) return;
            
            try
            {
                var sale = CreateSaleFromCart(2); // 2 = Card payment method ID
                await _saleService.CreateSaleAsync(sale);
                ClearCart();
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
        
        private Sale CreateSaleFromCart(int paymentMethodId)
        {
            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                UserId = App.CurrentUser.Id,
                CustomerId = SelectedCustomer?.Id,
                Subtotal = Subtotal,
                TaxAmount = TaxAmount,
                DiscountAmount = 0,
                TotalAmount = TotalAmount,
                AmountTendered = TotalAmount,
                Change = 0,
                PaymentMethodId = paymentMethodId,
                Items = CartItems.Select(i => new SaleItem
                {
                    ProductId = i.Product.Id,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TaxRate = i.Product.Tax.Rate,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
            
            return sale;
        }
        
        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(TaxAmount));
            OnPropertyChanged(nameof(TotalAmount));
        }
        
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class CartItem
    {
        public Product Product { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
    
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }
        
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
    
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }
        
        public void Execute(object parameter)
        {
            _execute();
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}