using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VisualLog.Desktop
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>Folder of the diagnostic log, see NLog.config.</summary>
    public static string LogFolder => Path.Combine(AppContext.BaseDirectory, "log");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      this.SetupExceptionHandling();
      log.Info($"Started. Version {typeof(App).Assembly.GetName().Version}, .NET {Environment.Version}, " +
               $"OS {Environment.OSVersion}, folder {AppContext.BaseDirectory}");

      var mainViewModel = new MainViewModel();
      var mainView = new MainView();
      mainView.DataContext = mainViewModel;
      mainView.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
      log.Info($"Exit, code {e.ApplicationExitCode}");
      NLog.LogManager.Shutdown();
      base.OnExit(e);
    }

    private void SetupExceptionHandling()
    {
      // UI thread: log, tell the user and keep the application running
      this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;

      // Any other thread: the process is going down, at least leave a trace
      AppDomain.CurrentDomain.UnhandledException += (s, e) =>
      {
        log.Fatal(e.ExceptionObject as Exception, $"Unhandled exception in a background thread (terminating: {e.IsTerminating})");
        NLog.LogManager.Flush();
      };

      TaskScheduler.UnobservedTaskException += (s, e) =>
      {
        log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
      };

      // WPF binding errors go to the log too (as warnings), otherwise they are visible only under a debugger
      PresentationTraceSources.Refresh();
      PresentationTraceSources.DataBindingSource.Listeners.Add(new NLogTraceListener("WpfBinding"));
      PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;
    }

    private bool isShowingError;

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      log.Error(e.Exception, "Unhandled exception in the UI thread");
      NLog.LogManager.Flush();
      e.Handled = true;

      // An exception may repeat on every layout pass: show the message only once at a time
      if (this.isShowingError)
        return;
      this.isShowingError = true;
      try
      {
        MessageBox.Show(
          $"Something went wrong:\n\n{e.Exception.GetBaseException().Message}\n\nDetails are written to the log:\n{LogFolder}",
          "VisualLog", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        this.isShowingError = false;
      }
    }

    /// <summary>Forwards System.Diagnostics trace output (WPF binding errors) to NLog.</summary>
    private sealed class NLogTraceListener : TraceListener
    {
      private readonly Logger logger;

      public NLogTraceListener(string loggerName)
      {
        this.logger = NLog.LogManager.GetLogger(loggerName);
      }

      public override void Write(string message) { }

      public override void WriteLine(string message) => this.logger.Warn(message);

      public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
      {
        if (eventType <= TraceEventType.Warning)
          this.logger.Warn(message);
      }

      public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
      {
        if (eventType <= TraceEventType.Warning)
          this.logger.Warn(args == null || args.Length == 0 ? format : string.Format(format, args));
      }
    }
  }
}
