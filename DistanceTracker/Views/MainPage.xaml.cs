using Humanizer;
using System.Diagnostics;
using System.Timers;

namespace DistanceTracker
{

    public partial class MainPage : ContentPage
    {
        System.Timers.Timer myTimer;
        DateTime dtStarted;
        public MainPage(MainViewModel viewModel)
        {
            this.InitializeComponent();
            this.BindingContext = viewModel;


            myTimer = new System.Timers.Timer(1000); //Timer(Callback, null, 1000, Timeout.Infinite);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var startedWhen = GetTimeSinceEventTimerStarted(dtStarted);
            Debug.WriteLine(startedWhen);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ElapsedTimeLabel.Text = $"Elapsed Time: {startedWhen}";
            });
        }

        protected override void OnAppearing()
        {
            try
            {
                var timeStarted = Preferences.Get(Keys.CurrentEventTimestamp, string.Empty);
                if (!string.IsNullOrWhiteSpace(timeStarted))
                {
                    //convert to DT
                    
                    var dtValid = DateTime.TryParse(timeStarted, out dtStarted);
                    if (dtValid)
                    {
                        ElapsedTimeLabel.IsVisible = true;
                        
                        myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                        myTimer.Enabled = true;
                        myTimer.Start();                        
                    }
                }
            }
            catch (Exception ex)
            {
            }
            

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            myTimer.Stop();

            base.OnDisappearing();
        }

        public string GetTimeSinceEventTimerStarted(DateTime dtStarted)
        {
            var tsElapsed = (DateTime.Now - dtStarted).TotalMilliseconds;
            return TimeSpan.FromMilliseconds(tsElapsed).Humanize(3, countEmptyUnits: true, minUnit: Humanizer.Localisation.TimeUnit.Second);
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