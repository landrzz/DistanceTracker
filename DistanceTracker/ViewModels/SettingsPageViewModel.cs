using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceTracker
{
    public class SettingsPageViewModel : ViewModel
    {
        private INavigationService _navigationService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        public SettingsPageViewModel(BaseServices services) : base(services)
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
