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
        try
        {
            var currentDistances = Preferences.Default.Get(Keys.Distances, string.Empty);
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
                "Example: 1.2, 3.1", "OK", "Cancel", "13.1, 26.2", -1, Keyboard.Default);
            }

            if (!string.IsNullOrWhiteSpace(res))
            {
                Preferences.Default.Set(Keys.Distances, res);
                _vm.FinalizeSetDistancesCommand.Execute(null);
            }
            System.Diagnostics.Debug.WriteLine(res);
        }
        catch (Exception ex) 
        {
        }
    }

    private void AddEventButton_Clicked(object sender, EventArgs e)
    {

    }

    async void ResetAppSettingsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var res = await this.DisplayAlert("Reset App Settings", "Would you like to clear all preferences and caches?", "Yes", "No");

            if (res)
            {
                Preferences.Clear();
                Preferences.Default.Clear();
            }
        }
        catch (Exception ex)
        {
        }      
    }

    private async void SetDashboardRefresh_Clicked(object sender, EventArgs e)
    {
        try
        {
            var currentInterval = Preferences.Default.Get(Keys.RefreshInterval, 0);
            var res = string.Empty;

            if (currentInterval != 0)
            {
                res = await this.DisplayPromptAsync("Set Refresh Interval", "Refresh interval in seconds. Use whole number values only.", "OK", "Cancel", maxLength: -1, keyboard: Keyboard.Numeric, initialValue: currentInterval.ToString()); //"OK", "Cancel", "13.1, 26.2",      //  ("Enter Distances",  -1, );
            }
            else
            {
                res = await this.DisplayPromptAsync("Set Refresh Interval", "Refresh interval in seconds. Use whole number values only.", "OK", "Cancel", maxLength: -1, keyboard: Keyboard.Numeric);
            }

            if (!string.IsNullOrWhiteSpace(res))
            {
                var resInt = 60;
                var worked = int.TryParse(res, out resInt);

                Preferences.Default.Set(Keys.RefreshInterval, resInt);
                _vm.FinalizeSetDistancesCommand.Execute(null);
            }
            System.Diagnostics.Debug.WriteLine(res);
        }
        catch (Exception ex)
        {

        } 
    }

    public async void SetEventTimeFrame_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            var eventTimeLimit = Preferences.Default.Get(Keys.TimeLimitHours, 12);
            var res = string.Empty;

            if (eventTimeLimit != 0)
            {
                res = await this.DisplayPromptAsync("Set Event Time Limit", "Set the time limit for the event. For example, 12 hours, 8 hours, etc.\nUse whole numbers only.", "OK", "Cancel", maxLength: 2, keyboard: Keyboard.Numeric, initialValue: eventTimeLimit.ToString()); //"OK", "Cancel", "13.1, 26.2",      //  ("Enter Distances",  -1, );
            }
            else
            {
                res = await this.DisplayPromptAsync("Set Event Time Limit", "Set the time limit for the event. For example, 12 hours, 8 hours, etc.\nUse whole numbers only.", "OK", "Cancel", maxLength: 2, keyboard: Keyboard.Numeric);
            }

            if (!string.IsNullOrWhiteSpace(res))
            {
                var resInt = 12;
                var worked = int.TryParse(res, out resInt);

                Preferences.Default.Set(Keys.TimeLimitHours, resInt);
                _vm.FinalizeEventTimeLimitCommand.Execute(null);
            }
            System.Diagnostics.Debug.WriteLine(res);
        }
        catch (Exception ex)
        {
        }
    }

    
}