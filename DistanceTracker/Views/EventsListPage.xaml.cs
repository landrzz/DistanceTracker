namespace DistanceTracker;

public partial class EventsListPage : ContentPage
{
	public EventsListPage()
	{
		InitializeComponent();
	}

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (((ListView)sender).SelectedItem == null)
            return;

        //Do stuff here with the SelectedItem ...
        ((ListView)sender).SelectedItem = null;
    }
}