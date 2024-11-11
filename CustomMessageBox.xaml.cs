using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MongoDB.Bson;
using MongoDB.Driver;



namespace MongoDBApp
{
    public partial class CustomMessageBox : Window
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<BsonDocument> collection;

        private string originalTitle;

        public event EventHandler DocumentoEliminato;



        public CustomMessageBox(string intestazione, string message)
        {
           
            client = new MongoClient("mongodb://localhost:27017");
            database = client.GetDatabase("DocumentiDB");
            collection = database.GetCollection<BsonDocument>("Documenti");

            InitializeComponent();

            originalTitle = intestazione;
            MessageTextBox.Text = message;
            Intestazione.Text = intestazione;
        }


        // Evento per il pulsante "Modifica"
        private void Modifica_Click(object sender, RoutedEventArgs e)
        {
            // Abilita la modifica del testo
            MessageTextBox.IsReadOnly = false;
            Intestazione.IsReadOnly = false;
            

            // Modifica il pulsante OK per salvare le modifiche
            Button okButton = (Button)this.FindName("okButton");
            okButton.Content = "Salva";
            okButton.Click -= Button_Click;
            okButton.Click += Salva_Click;
        }

        // Evento per il pulsante "Salva" per salvare la modifica
        private void Salva_Click(object sender, RoutedEventArgs e)
        {
            // Disabilita la modifica e salva il contenuto
            MessageTextBox.IsReadOnly = true;
            Intestazione.IsReadOnly = true;

            // Cambia il pulsante Salva di nuovo a OK
            Button okButton = (Button)this.FindName("okButton");
            okButton.Content = "OK";
            okButton.Click -= Salva_Click;
            okButton.Click += Button_Click;

            // Aggiorna il documento in MongoDB
            var filter = Builders<BsonDocument>.Filter.Eq("title", originalTitle);
            var update = Builders<BsonDocument>.Update
                .Set("title", Intestazione.Text)      // Nuovo titolo
                .Set("content", MessageTextBox.Text);  // Nuovo contenuto

            var result = collection.UpdateOne(filter, update);

            if (result.ModifiedCount > 0)
            {
                MessageBox.Show("Modifica salvata in MongoDB.");
                originalTitle = Intestazione.Text;
                
            }
            else
            {
                MessageBox.Show("Errore nel salvataggio.");
            }
        }

        // Evento per il pulsante "Elimina"
        private void Elimina_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Eliminare elemento?", "Conferma Eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {

                var filter = Builders<BsonDocument>.Filter.Eq("title", originalTitle);
                var deleteResult = collection.DeleteOne(filter);

                if (deleteResult.DeletedCount > 0)
                {
                    MessageBox.Show("Elemento eliminato.");
                    MessageTextBox.Text = string.Empty;
                    DocumentoEliminato?.Invoke(this, EventArgs.Empty); // Solleva l'evento
                    this.Close(); // Chiude la finestra dopo l'eliminazione
                }
                else {

                    MessageBox.Show("Errore nell'eliminazione");

                      }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
