using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class RecordDistancePageViewModel : ViewModel
    {
        private INavigationService _navigationService { get; }
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        public RecordDistancePageViewModel(Shiny.BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            var distancesSet = CheckAreDistancesSet();
            var raceEvent = CheckIsEventSet();

            if (!raceEvent || !distancesSet)
            {
                await _dialogService.Alert("You must specify both a default event and default event distances before recording distance!", "Set Event First!");
                await _navigationService.GoBackAsync();
            }

            var currentDistances = Preferences.Default.Get("distances", string.Empty);
            if (!string.IsNullOrWhiteSpace(currentDistances))
            {
                //set up action buttons / popup?
                //set the buttons
            }
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }

        public bool CheckIsEventSet()
        {
            var raceEvent = Preferences.Get("currenteventname", string.Empty);
            if (string.IsNullOrWhiteSpace(raceEvent))
            {
                return false;
            }
            else
                return true;
        }

        public bool CheckAreDistancesSet()
        {
            var distances = Preferences.Get("distances", string.Empty);
            if (string.IsNullOrWhiteSpace(distances))
            {
                return false;
            }
            else
                return true;
        }


        [Reactive] public string Property { get; set; }
    }
}
