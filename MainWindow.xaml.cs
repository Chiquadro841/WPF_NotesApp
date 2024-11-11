using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MongoDB.Bson;
using MongoDB.Driver;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;



namespace MongoDBApp
{
  
    public partial class MainWindow : Window
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<BsonDocument> collection;


        public MainWindow()
        {
            client = new MongoClient("mongodb://localhost:27017");
            database = client.GetDatabase("DocumentiDB");
            collection = database.GetCollection<BsonDocument>("Documenti");

            InitializeComponent();
            LoadChart();


        }

        // Apri la finestra per inserire un nuovo documento
        private void Nuovo_Click(object sender, RoutedEventArgs e)
        {
            var nuovoWindow = new NuovoDocumentoWindow();

            nuovoWindow.DocumentoSalvato += (s, args) => LoadChart();

            nuovoWindow.Show();
        }

        // Apri la finestra per visualizzare i titoli dei documenti
        private void Visualizza_Click(object sender, RoutedEventArgs e)
        {
            var visualizzaWindow = new VisualizzaDocumentiWindow();

            
            visualizzaWindow.Show();
        }

        public void LoadChart()
        {
            // Crea un modello di grafico
            var plotModel = new PlotModel { Title = "Documenti per Giorno" };



            // Crea un asse X temporale con il formato di data desiderato
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yyyy",  // Formato desiderato per le date
                Title = "Giorno",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
            };
            plotModel.Axes.Add(dateAxis);

            // Crea un asse Y per il conteggio dei documenti
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Numero di Documenti",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
            };
            plotModel.Axes.Add(valueAxis);



            // Ottieni i documenti e raggruppali per giorno
            var documents = collection.Find(new BsonDocument()).ToList();
            var groupedByDate = documents
                .GroupBy(d => d["timestamp"].ToUniversalTime().Date)  // Raggruppa per giorno
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToList();

            // Crea una serie di dati per il grafico a linee
            var lineSeries = new LineSeries
            {
                Title = "Documenti",
                Color = OxyColors.Blue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3
            };

            // Aggiungi i dati alla serie
            foreach (var item in groupedByDate)
            {
                // Aggiungi i dati X (giorno) e Y (numero di documenti)
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(item.Date), item.Count));
            }

            // Aggiungi la serie al modello di grafico
            plotModel.Series.Add(lineSeries);

            // Imposta il grafico nel controllo PlotView
            plotView.Model = plotModel;
        }



    }
}
