using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace MongoDBApp
{
    public partial class VisualizzaDocumentiWindow : Window
    {
        private bool _isShowingCategories = true; // Per tenere traccia se stiamo mostrando categorie o titoli
        private IMongoCollection<BsonDocument> _documentiCollection;


        public VisualizzaDocumentiWindow()
        {
            InitializeComponent();
            // Configurazione della connessione a MongoDB
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DocumentiDB");
            _documentiCollection = database.GetCollection<BsonDocument>("Documenti");

            // Caricamento categorie
            LoadCategories();
        }

        // Metodo per caricare le categorie nella ListBox
        private void LoadCategories()
        {
            // Recupera tutte le categorie uniche dai documenti
            var categorie = _documentiCollection
                .Distinct<string>("category", new BsonDocument())
                .ToList();

            TitoliListBox.ItemsSource = categorie;
            _isShowingCategories = true;
            BackButton.Visibility = Visibility.Collapsed;
        }


        // Metodo per caricare i titoli dei documenti di una categoria specifica
        private void LoadTitlesForCategory(string category)
        {

            // Filtra i documenti per categoria
            var filter = Builders<BsonDocument>.Filter.Eq("category", category);

            // Recupera tutti i documenti che corrispondono al filtro
            var documents = _documentiCollection
                .Find(filter)
                .ToList();  // Recupera i documenti

            // Estrai i titoli in modo sicuro
            var titoli = documents
                .Where(doc => doc.Contains("title") && doc["title"] != BsonNull.Value)  // Verifica se esiste il campo "title" e se non è null
                .Select(doc => doc["title"].AsString)  // Estrai il titolo come stringa
                .ToList();

            TitoliListBox.ItemsSource = titoli;
            _isShowingCategories = false;
            BackButton.Visibility = Visibility.Visible;
        }

        // Evento per gestire il doppio clic su una categoria o un titolo
        private void TitoliListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isShowingCategories && TitoliListBox.SelectedItem is string selectedCategory)
            {
                // Carica i titoli dei documenti per la categoria selezionata
                LoadTitlesForCategory(selectedCategory);
            }
            else
            {
                // Se stiamo mostrando i titoli, apri il documento selezionato
                string selectedTitle = TitoliListBox.SelectedItem.ToString();

                var client = new MongoClient("mongodb://localhost:27017");
                var database = client.GetDatabase("DocumentiDB");
                var collection = database.GetCollection<BsonDocument>("Documenti");

                // Cerca il documento per titolo
                var document = collection.Find(new BsonDocument { { "title", selectedTitle } }).FirstOrDefault();

                if (document != null)
                {
                    // Estrai il contenuto del documento e mostra la finestra personalizzata
                    string content = document["content"].ToString();
                    string title = document["title"].ToString();

                    // Crea e mostra la finestra personalizzata con il titolo e il contenuto del documento
                    CustomMessageBox messageBox = new CustomMessageBox(title, content);
                    messageBox.DocumentoEliminato += (s, args) =>
                    {
                        // Ricarichiamo direttamente nella mainwindow da qui (assurdo)

                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow?.LoadChart();  // Ricarica il grafico nella MainWindow
                    };
                    messageBox.ShowDialog();
                }
            }


        }

        // Evento per tornare alla lista delle categorie
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        
    }
}