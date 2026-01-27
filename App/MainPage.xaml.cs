namespace App;

public partial class MainPage : ContentPage
{
    WhisperApiService _whisperApiService;
    public MainPage(WhisperApiService whisperApiService)
    {
        InitializeComponent();
        _whisperApiService = whisperApiService;
    }

    private async void OnLLMChatClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LLMPage");
    }

    private async void OnTranscriptionClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//WhisperPage");
        //var modelDetailsResult = await _whisperApiService.GetModelDetailsAsync();
        //Debug.WriteLine(modelDetailsResult);
    }

    private async void OnRapModeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RapModePage");
    }
}
