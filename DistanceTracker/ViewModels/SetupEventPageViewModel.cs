using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class SetupEventPageViewModel : ViewModel
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }

        public string EventYear;

        private INavigationService _navigationService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        public SetupEventPageViewModel(BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }


        [Reactive] public string Property { get; set; }
    }
}
