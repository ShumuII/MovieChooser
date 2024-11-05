using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using Avalonia.Animation;
using Color = System.Drawing.Color;
using Avalonia.Collections;
using MovieChooser.Resources;

namespace MovieChooser;

public partial class MainWindow : Window
{
    private List<string> movies = new();
    private List<SolidColorBrush> colorsList = new();

    private string _id;
    private static string _url;
    private string iRating;
    private string mRating;
    public MainWindow()
    {
        InitializeComponent();
        
        
        var moviesTextBox = this.FindControl<TextBox>("MovieTextBox");
        var addButton = this.FindControl<Button>("AddButton");
        var chooseButton = this.FindControl<Button>("ChooseButton");
        var listPanel = this.FindControl<Panel>("ListPanel");
        var chosen = this.FindControl<ListBox>("ChosenListBox");
        var title = this.FindControl<Label>("Title");
        var ImdbButton = this.FindControl<Button>("ImDbButton");
        var YandexButton = this.FindControl<Button>("YandexButton");
        var metascoreButton = this.FindControl<Button>("Metascore");
        
        addButton.Click += addButton_Click;
        chooseButton.Click += chooseButton_Click;
        moviesTextBox.KeyDown += moviesTextBox_KeyDown;
        ImdbButton.Click += ImdbButton_Click;
        YandexButton.Click += YandexButton_Click;
        Metascore.Click += metascoreButton_Click;

    }

    public void addToColors()
    {
        colorsList.Add(new SolidColorBrush { Color = Colors.Gray });
        colorsList.Add(new SolidColorBrush { Color = Colors.DarkGray});
        colorsList.Add(new SolidColorBrush { Color = Colors.LightSlateGray});
        colorsList.Add(new SolidColorBrush { Color = Colors.SlateGray});
        colorsList.Add(new SolidColorBrush { Color = Colors.DimGray});
    }

    public void metascoreButton_Click(object sender, EventArgs e)
    {
        var metascoreButton = this.FindControl<Button>("Metascore");
        var rating = this.FindControl<Label>("Rating");
        if (metascoreButton.Content == "Metascore")
        {
            metascoreButton.Content = "ImDb";
            rating.Content = mRating;
        }
        else if (metascoreButton.Content == "ImDb")
        {
            metascoreButton.Content = "Metascore";
            rating.Content = iRating;
        }
    }

    public void ImdbButton_Click(object sender, RoutedEventArgs e)
    {
        if (!(_id == String.Empty))
        {
            var psi2 = new ProcessStartInfo
            {
                FileName = "https://www.imdb.com/title/" + _id,
                UseShellExecute = true
            };
            Process.Start(psi2);
        }
    }

    public void YandexButton_Click(object sender, RoutedEventArgs e)
    {
        
        string searchUrl = "https://yandex.com.tr/search/?text=" + _url + "+izle";
        var psi = new ProcessStartInfo
        {
            FileName = searchUrl,
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    //Adds entered movie to movies list and creates a new label to show it to user
    private void addButton_Click(object sender, RoutedEventArgs e)
    {
        var moviesTextBox = this.FindControl<TextBox>("MovieTextBox");
        if (!string.IsNullOrWhiteSpace(moviesTextBox.Text))
        {
            movies.Add(moviesTextBox.Text);
            Label label = new Label()
            {
                Content = moviesTextBox.Text
            };
            ListPanel.Children.Add(label);
            moviesTextBox.Text = "";
        }
    }

    //runs addButton_Click method by pressing enter key
    private void moviesTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            addButton_Click(null, null);
        }
    }

    //Takes random movies and shows 'em to give a feel like computer selecting one of them. At the end selects one.
    private async void chooseButton_Click(object sender, RoutedEventArgs e)
    {
        addToColors();
        var listPanel = this.FindControl<StackPanel>("ListPanel");
        listPanel.IsVisible = false;
        var title = this.FindControl<Label>("Title");
        title.IsVisible = true;
        if (!(movies.Count == 0))
        {
            int rand, colorRand;
            for (var i = 0; i < 20; i++)
            {
                Random random = new();
                Random randomColor = new();
                rand = random.Next(movies.Count);
                colorRand = randomColor.Next(0,4);
                await Task.Delay(200);
                title.Background = colorsList[colorRand];
                title.Content = movies[rand];
            }

            string titleN, yearN, idN, posterN;
            
            string createdUrl = apiOp.urlMaker(title.Content.ToString());
            (titleN,yearN,idN,posterN) = await apiOp.taskname(createdUrl);
            
            var MovieTitle = this.FindControl<Label>("MovieTitle");
            MovieTitle.Content = titleN;
            var ReleaseDate = this.FindControl<Label>("ReleaseDate");
            ReleaseDate.Content += yearN;
            var posterAv = this.FindControl<Image>("PosterAv");
            
            var bitmap = await CreateBitmapFromUrlAsync(posterN);
            posterAv.Source = bitmap;

            _id = idN;

            string directorN, ratingN, metascoreN;

            string task2url = "https://www.omdbapi.com/?i=" + _id + "&apikey=" + api_key.apikey();
            
            (ratingN,directorN, metascoreN) = await taskname_2(task2url);
            var rating = this.FindControl<Label>("Rating");
            var director = this.FindControl<Label>("Director");
            director.Content = directorN;
            rating.Content += ratingN;
            iRating = ratingN;
            mRating = metascoreN;
            var ratingMargin = this.FindControl<Label>("RatingText");
            ratingMargin.Margin = new Thickness(75, ratingMargin.Margin.Top, ratingMargin.Margin.Right, ratingMargin.Margin.Bottom);
        }
    }
    
    public async Task<Bitmap> CreateBitmapFromUrlAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            // URL'den veriyi indiriyoruz
            var imageData = await client.GetByteArrayAsync(url);

            // Byte array'den bir MemoryStream oluşturuyoruz
            using (var stream = new MemoryStream(imageData))
            {
                // Bitmap nesnesini stream'den oluşturuyoruz
                return new Bitmap(stream);
            }
        }
    }
    
    public class apiOp
    {
        public static string urlMaker(string chosen) {
        string url1 = "https://www.omdbapi.com/?s=";
        chosen = chosen.Replace(" ", "+");
        _url = chosen;
        url1 += chosen;
        url1 += "&type=movie&apikey=";
        url1 += api_key.apikey();
        return url1;
    }



    public static async Task<(string title, string year, string id, string poster)> taskname(string url){
            HttpClient client = new();
            string jsonString = await client.GetStringAsync(url);
            
            Movies response = JsonConvert.DeserializeObject<Movies>(jsonString);

            return (
                response.Search.First()?.Title,
                response.Search.First()?.Year,
                response.Search.First()?.imdbId,
                response.Search.First()?.Poster
            );
    }
        
    }

    public class Movies
    {
        public List<insideMovies> Search { get; set; }
    }

    public class insideMovies
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string imdbId { get; set; }
        public string Poster { get; set; }
    }

    public static async Task<(string rating, string director, string metascore)> taskname_2(string url) 
    {
        HttpClient client = new();
        string jsonstring = await client.GetStringAsync(url);
        extraInfo response = JsonConvert.DeserializeObject<extraInfo>(jsonstring);

        return (
            response.imdbRating,
            response.Director,
            response.Metascore
        );

    }

    public class extraInfo
    {
        public string imdbRating { get; set; }
        public string Director { get; set; }
        
        public string Metascore { get; set; }
    }
    
    
    
    
    
}

