namespace DistanceTracker
{

    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            this.InitializeComponent();
            this.BindingContext = viewModel;
        }

        private void AddRunnerButton_Clicked(object sender, EventArgs e)
        {
            
        }

        private void AddEvent_Clicked(object sender, EventArgs e)
        {

        }

        private void RecordDistances_Clicked(object sender, EventArgs e)
        {

        }

        private void Settings_Clicked(object sender, EventArgs e)
        {

        }
    }
}