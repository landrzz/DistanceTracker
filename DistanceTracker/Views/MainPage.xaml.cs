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
            try
            {
                if (!string.IsNullOrWhiteSpace(timeStarted))
                {
                    var startedWhen = GetTimeSinceEventTimerStarted(dtStarted);
                    Debug.WriteLine(startedWhen);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ElapsedTimeLabel.Text = $"Elapsed Time: {startedWhen}";
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ElapsedTimeLabel.Text = $"Elapsed Time: 00:00:00";
                    });
                }                  
            }
            catch (Exception ex)
            {
                //eat it
            }           
        }

        string timeStarted;
        protected override void OnAppearing()
        {
            try
            {
                timeStarted = Preferences.Default.Get(Keys.CurrentEventTimestamp, string.Empty);
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
                    else
                    {
                        ElapsedTimeLabel.Text = $"Elapsed Time: UNKNOWN";
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