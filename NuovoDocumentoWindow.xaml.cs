using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Linq;

namespace MongoDBApp
{
    public partial class NuovoDocumentoWindow : Window
    {
        private IMongoCollection<BsonDocument> _documentiCollection;
        private List<string> _suggestedCategories;

        // Evento personalizzato
        public event EventHandler DocumentoSalvato;

        public NuovoDocumentoWindow()
        {
            InitializeComponent();

            // Configurazione della connessione a MongoDB
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DocumentiDB"); // Nome del database
            _documentiCollection = database.GetCollection<BsonDocument>("Documenti"); // Nome della collezione

            _suggestedCategories = new List<string>();
        }

        // Salva il nuovo documento nel database MongoDB
        private void Salva_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string content = ContentTextBox.Text;
            string category = CategoryTextBox.Text;

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Titolo e contenuto sono obbligatori.");
                return;
            }

            var document = new BsonDocument
            {
                { "title", title },
                { "category", category },
                { "content", content },
                { "timestamp", DateTime.Now }
            };

            _documentiCollection.InsertOne(document);
            MessageBox.Show("Documento salvato!");


            // Attiva l'evento per notificare che un documento è stato salvato
            DocumentoSalvato?.Invoke(this, EventArgs.Empty);

            // Chiudi la finestra dopo aver salvato
            this.Close();
        }

        // Evento per aggiornare i suggerimenti quando il testo cambia
        private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentText = CategoryTextBox.Text;

            if (!string.IsNullOrWhiteSpace(currentText))
            {
                UpdateSuggestions(currentText);
            }
            else
            {
                SuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        // Evento per selezionare un suggerimento dalla ListBox
        private void SuggestionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is string selectedCategory)
            {
                CategoryTextBox.Text = selectedCategory;
                SuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        // Metodo per aggiornare i suggerimenti basati sull'input
        private void UpdateSuggestions(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                SuggestionsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            // Query MongoDB per trovare categorie corrispondenti
            var filter = Builders<BsonDocument>.Filter.Regex("category", new BsonRegularExpression(text, "i"));
            var categorie = _documentiCollection
                .Find(filter)
                .Project(new BsonDocument { { "category", 1 } })
                .ToList();

            // Estrai solo i valori del campo "category" e rimuovi duplicati
            _suggestedCategories = categorie
                .Select(doc => doc["category"].AsString) // Estrai la categoria come stringa
                .Distinct()
                .ToList();

            // Aggiorna la ListBox dei suggerimenti
            SuggestionsListBox.ItemsSource = _suggestedCategories;
            SuggestionsListBox.Visibility = _suggestedCategories.Any() ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
