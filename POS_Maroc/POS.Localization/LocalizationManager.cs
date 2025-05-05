using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace POS.Localization
{
    public static class LocalizationManager
    {
        public static event EventHandler LanguageChanged;
        
        public static CultureInfo CurrentCulture { get; private set; }
        
        static LocalizationManager()
        {
            // Par défaut, utilisation du français marocain
            CurrentCulture = new CultureInfo("fr-MA");
        }
        
        public static void SetLanguage(string languageCode)
        {
            try
            {
                CultureInfo culture = new CultureInfo(languageCode);
                
                // Définition de la culture pour le thread actuel
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                
                // Pour les nouveaux threads
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                
                // Mise à jour des ressources WPF
                var resources = Application.Current.Resources;
                var dictionaries = resources.MergedDictionaries;
                
                // Charge le dictionnaire de ressources appropriate
                string resourcePath = $"/POS.Localization;component/Resources.{languageCode}.xaml";
                var resourceDict = new ResourceDictionary { Source = new Uri(resourcePath, UriKind.Relative) };
                
                // Remplace l'ancien dictionnaire
                dictionaries.Clear();
                dictionaries.Add(resourceDict);
                
                // Mise à jour du format de date et des nombres pour le Maroc
                ApplyMoroccanFormats();
                
                CurrentCulture = culture;
                
                // Notifier les observateurs du changement de langue
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Gérer l'exception si la culture n'est pas disponible
                Console.WriteLine($"Erreur lors du changement de langue: {ex.Message}");
            }
        }
        
        private static void ApplyMoroccanFormats()
        {
            // Configuration spécifique pour le Maroc
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            
            // Format monétaire (Dirham marocain)
            customCulture.NumberFormat.CurrencySymbol = "MAD";
            customCulture.NumberFormat.CurrencyDecimalSeparator = ",";
            customCulture.NumberFormat.CurrencyGroupSeparator = " ";
            
            // Format de date (jour/mois/année)
            customCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            customCulture.DateTimeFormat.LongDatePattern = "dddd d MMMM yyyy";
            
            // Application de la culture personnalisée
            Thread.CurrentThread.CurrentCulture = customCulture;
        }
    }
}