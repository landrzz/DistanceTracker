using System.Diagnostics;
using CommunityToolkit.Maui;

namespace DistanceTracker;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

    private async void SetupDistancesButton_Clicked(object sender, EventArgs e)
    {
		var currentDistances = Preferences.Default.Get("distances", string.Empty);
        var res = string.Empty;
		if (!string.IsNullOrWhiteSpace(currentDistances))
		{
            res = await this.DisplayPromptAsync("Enter Distances", "Distances should be integer/decimal values only!\n" +
            "Use comma separated values.\n\n" +
            "Example: 1.2, 3.1", "OK", "Cancel", "13.1, 26.2", -1, Keyboard.Default, initialValue: currentDistances);
        }
		else
		{
            res = await this.DisplayPromptAsync("Enter Distances", "Distances should be integer/decimal values only!\n" +
            "Use comma separated values.\n\n" +
            "Example: 1.2, 3.1", "OK", "Cancel", "13.1, 26.2", -1, Keyboard.Default);
        }

        if (!string.IsNullOrWhiteSpace(res))
        {
            Preferences.Default.Set("distances", res);
        }

		

		Debug.WriteLine(res);
    }

    private void AddEventButton_Clicked(object sender, EventArgs e)
    {

    }
}