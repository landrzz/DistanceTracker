using System;
using System.Reactive.Subjects;

namespace DistanceTracker
{
	public class MyViewModel : BaseViewModel
	{
        private Subject<(INavigationParameters, bool)>? navSubj;

        public MyViewModel(BaseServices services) : base(services)
		{
		}

        public virtual Task InitializeAsync(INavigationParameters parameters)
        {
            return Task.CompletedTask;
        }

        public virtual void OnAppearing()
        {
        }

        public virtual void OnDisappearing()
        {
            Deactivate();
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
            ((SubjectBase<(INavigationParameters, bool)>)(object)navSubj)?.OnNext((parameters, false));
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
            ((SubjectBase<(INavigationParameters, bool)>)(object)navSubj)?.OnNext((parameters, true));
        }

        public virtual Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            return Task.FromResult(result: true);
        }

        public IObservable<(INavigationParameters Paramters, bool NavigatedTo)> WhenNavigation()
        {
            if (navSubj == null)
            {
                navSubj = new Subject<(INavigationParameters, bool)>();
            }
            return (IObservable<(INavigationParameters Paramters, bool NavigatedTo)>)Shiny.ObservableExtensions.DisposedBy<Subject<(INavigationParameters, bool)>>(navSubj, base.DestroyWith);
        }
    }
}

