using Wyoming.Net.Satellite.App.Maui.ViewModels;

namespace Wyoming.Net.Satellite.App.Maui;

public partial class VadSettingsPage : ContentPage
{
    private readonly SatelliteSettingsViewModel viewModel;

    public VadSettingsPage(SatelliteSettingsViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm.VadSettings;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        viewModel.Save();
        base.OnNavigatedFrom(args);
    }
}
