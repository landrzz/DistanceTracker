using Xamarin.Google.Crypto.Tink.Signature;

namespace DistanceTracker;

public partial class RecordDistancePage : ContentPage
{
    public RecordDistancePageViewModel _vm;
	public RecordDistancePage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        _vm = GetViewModel();

        base.OnAppearing();
    }

    public RecordDistancePageViewModel GetViewModel()
    {
        return BindingContext as RecordDistancePageViewModel;
    }
}