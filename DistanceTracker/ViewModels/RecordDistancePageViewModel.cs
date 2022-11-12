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

        public DelegateCommand<string> NavigateCommand { get; }

        public RecordDistancePageViewModel(BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            var currentDistances = Preferences.Default.Get("distances", string.Empty);
            if (!string.IsNullOrWhiteSpace(currentDistances))
            {
                //set up action buttons / popup?
            }
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }


        [Reactive] public string Property { get; set; }
    }
}
