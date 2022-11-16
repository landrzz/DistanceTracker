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
        private IDialogs _dialogService { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        
        public ICommand FinalizeSetDistancesCommand => new Command(FinalizeSetDistances);

        public SettingsPageViewModel(BaseServices services) : base(services)
        {
            _navigationService = services.Navigation;
            _dialogService = services.Dialogs;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }
        public async void FinalizeSetDistances()
        {
            await _dialogService.Snackbar("Distances Set Successfully!");
        }

        [Reactive] public string Property { get; set; }
    }
}
