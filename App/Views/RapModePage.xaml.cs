using App.ViewModels;

namespace App.Views;

public partial class RapModePage : ContentPage
{
    public RapModePage(RapModeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
