using App.ViewModels;
using App.Models;

namespace App.Views;

public partial class MetricsPage : ContentPage
{
    public MetricsPage(MetricsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is MetricsViewModel viewModel)
        {
            await viewModel.LoadMetricsAsync();
        }
    }

    private void OnViewButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is TranscriptionMetrics metric)
        {
            if (BindingContext is MetricsViewModel viewModel)
            {
                viewModel.ViewTextCommand.Execute(metric);
            }
        }
    }
}
