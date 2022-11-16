namespace DistanceTracker;

public partial class AddRunnerPage : ContentPage
{
	public AddRunnerPage()
	{
		InitializeComponent();
	}

    private void SaveRunner_Clicked(object sender, EventArgs e)
    {

    }

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        var birthdate = BirthDatePicker.Date;

        // Save today's date.
        var today = DateTime.Today;

        // Calculate the age.
        var age = today.Year - birthdate.Year;

        // Go back to the year in which the person was born in case of a leap year
        if (birthdate.Date > today.AddYears(-age)) age--;

        AgeEntry.Text = age.ToString();
    }
}