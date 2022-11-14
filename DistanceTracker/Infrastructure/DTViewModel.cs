using System;
using System.Reactive.Disposables;

namespace DistanceTracker
{
    public abstract class DTViewModel : ReactiveObject, IInitializeAsync, IConfirmNavigationAsync, INavigationAware, IDisposable
    {
        protected DTViewModel(BaseServices services) => this.Services = services;

        //protected DTViewModel(BaseServices services)
        //    : base(services)
        //{
        //}

        [Reactive] public string Title { get; protected set; } = "";
        [Reactive] public bool IsBusy { get; protected set; }
        protected IPlatform Platform => this.Services.Platform;
        protected IDialogs Dialogs => this.Services.Dialogs;
        protected INavigationService Navigation => this.Services.Navigation;
        protected BaseServices Services { get; }

        ILogger? logger;
        protected ILogger Logger
        {
            get
            {
                this.logger ??= this.Services.LoggerFactory.CreateLogger(this.GetType());
                return this.logger;
            }
        }

        CancellationTokenSource? cancelSrc;
        public CancellationToken CancelToken
        {
            get
            {
                this.cancelSrc ??= new();
                return this.cancelSrc.Token;
            }
        }

        CompositeDisposable? destroyWith;

        //protected DTViewModel(BaseServices services)
        //{
        //    //
        //


        public CompositeDisposable DestroyWith
        {
            get
            {
                this.destroyWith ??= new();
                return this.destroyWith;
            }
        }
        public virtual Task InitializeAsync(INavigationParameters parameters) => Task.CompletedTask;
        public virtual Task<bool> CanNavigateAsync(INavigationParameters parameters) => Task.FromResult(true);
        public virtual void OnNavigatedFrom(INavigationParameters parameters) { }
        public virtual void OnNavigatedTo(INavigationParameters parameters) { }
        public virtual void Dispose()
        {
            this.cancelSrc?.Cancel();
            this.cancelSrc = null;
            this.destroyWith?.Dispose();
            this.destroyWith = null;
        }


        protected virtual ICommand LoadingCommand(Func<Task> task, IObservable<bool>? canExecute = null)
        {
            var cmd = ReactiveCommand.CreateFromTask(task, canExecute);
            cmd.Subscribe(
                x => this.IsBusy = true,
                _ => this.IsBusy = false,
                () => this.IsBusy = false
            );
            return cmd;
        }


        protected virtual ICommand ConfirmCommand(string question, Func<Task> func, IObservable<bool>? canExecute = null) => ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await this.Dialogs.Confirm("", question, "Yes", "No");
            if (result)
                await func.Invoke();
        }, canExecute);


        protected virtual Task Alert(string message, string title = "ERROR", string okBtn = "OK")
            => this.Dialogs.Alert(title, message, okBtn);

        protected virtual Task<bool> Confirm(string question, string title = "Confirm")
            => this.Dialogs.Confirm(title, question, "Yes", "No");

        protected virtual async Task SafeExecute(Func<Task> task)
        {
            try
            {
                await task.Invoke();
            }
            catch (Exception ex)
            {
                await this.DisplayError(ex);
            }
        }


        protected virtual async Task DisplayError(Exception ex)
        {
            this.Logger.LogError(ex, "Error");
            await this.Dialogs.Alert("Error", ex.ToString(), "OK");
        }


        public bool IsApple => this.IsIos || this.IsMacCatalyst;
#if IOS
    public bool IsIos => true;
    public bool IsMacCatalyst => false;
    public bool IsAndroid => false;
#elif MACCATALYST
    public bool IsIos => false;
    public bool IsMacCatalyst => true;
    public bool IsAndroid => false;
#elif ANDROID
        public bool IsIos => false;
        public bool IsMacCatalyst => false;
        public bool IsAndroid => true;
#endif
    }


    public record MyBaseServices(
        IPlatform Platform,
        ILoggerFactory LoggerFactory,
        IPageDialogService Dialogs,
        INavigationService Navigation
    );
}

