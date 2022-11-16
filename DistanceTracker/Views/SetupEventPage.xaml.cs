namespace DistanceTracker;

public partial class SetupEventPage : ContentPage
{
	public SetupEventPage()
	{
		InitializeComponent();

		eventdatepicker.MinimumDate = DateTime.Today;

    }
}