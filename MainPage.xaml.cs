using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace translator;

public partial class MainPage : ContentPage
{
    private const string ApiKey = "AIzaSyCIO1vPQzuUpeuFaCl0CYSQqYFhRS2C1G4"; // Замени на свой API-ключ Google

    public MainPage()
    {
        InitializeComponent();
        LanguagePicker.ItemsSource = new List<string> { "en", "es", "de", "fr" };
    }

    private async void OnTranslateClicked(object sender, EventArgs e)
    {
        string text = InputText.Text;
        string targetLanguage = LanguagePicker.SelectedItem?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(targetLanguage))
        {
            await DisplayAlert("Ошибка", "Введите текст и выберите язык", "OK");
            return;
        }

        try
        {
            string translatedText = await TranslateText(text, targetLanguage);
            TranslatedText.Text = translatedText;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    private async Task<string> TranslateText(string text, string targetLanguage)
    {
        using var client = new HttpClient();

        string url = $"https://translation.googleapis.com/language/translate/v2?key={ApiKey}";

        var requestBody = new
        {
            q = text,
            target = targetLanguage,
        };

        string json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);

        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Ответ API: " + responseBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Ошибка API: {response.StatusCode}, {responseBody}");
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<GoogleTranslateResponse>(responseBody, options);

            if (result?.Data?.Translations == null || !result.Data.Translations.Any())
            {
                throw new Exception("Ответ API не содержит переводов.");
            }

            return result.Data.Translations.First().TranslatedText;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка десериализации: {ex.Message}");
            throw new Exception("Ошибка обработки ответа API.");
        }
    }


    // Классы для десериализации ответа Google API
    public class GoogleTranslateResponse
    {
        public required Data Data { get; set; }

    }

    public class Data
    {
        public List<Translation> Translations { get; set; } = new List<Translation>();
    }

    public class Translation
    {
        public string TranslatedText { get; set; } = string.Empty;
    }


}
