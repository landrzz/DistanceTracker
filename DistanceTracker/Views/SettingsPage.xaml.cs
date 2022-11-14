using System.Diagnostics;
using CommunityToolkit.Maui;

namespace DistanceTracker;

public partial class SettingsPage : ContentPage
{
    SettingsPageViewModel _vm;

	public SettingsPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        _vm = GetViewModelInstance();
        base.OnAppearing();
    }

    private SettingsPageViewModel GetViewModelInstance()
    {
        return BindingContext as SettingsPageViewModel;
    }

    private async void SetupDistancesButton_Clicked(object sender, EventArgs e)
    {
        var currentDistances = Preferences.Default.Get("distances", string.Empty);
        var res = string.Empty;
        if (!string.IsNullOrWhiteSpace(currentDistances))
        {
            res = await this.DisplayPromptAsync("Distances should be integer/decimal values only!\n" +
            "Use comma separated values.\n\n" +
            "Example: 1.2, 3.1", "Enter Distances", "OK", "Cancel", "13.1, 26.2", -1, Keyboard.Default, currentDistances); //"OK", "Cancel", "13.1, 26.2",      //  ("Enter Distances",  -1, );
        }
        else
        {
            res = await this.DisplayPromptAsync("Enter Distances", "Distances should be integer/decimal values only!\n" +
            "Use comma separated values.\n\n" +
            "Example: 1.2, 3.1", "OK", "Cancel", "13.1, 26.2", -1, Keyboard.Default, currentDistances);
        }

        if (!string.IsNullOrWhiteSpace(res))
        {
            Preferences.Default.Set("distances", res);
            _vm.FinalizeSetDistancesCommand.Execute(null);
        }



        System.Diagnostics.Debug.WriteLine(res);
    }

    private void AddEventButton_Clicked(object sender, EventArgs e)
    {

    }

    async void ResetAppSettingsButton_Clicked(object sender, EventArgs e)
    {
        var res = await this.DisplayAlert("Reset App Settings", "Would you like to clear all preferences and caches?", "Yes", "No");

        if (res)
        {
            Preferences.Clear();
        }       
    }
}