using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Services;

namespace VRCFaceTracking.ViewModels;

public class OutputViewModel : ObservableRecipient
{
    public ObservableCollection<LogLine> Logs => OutputPageLogger.FilteredLogs;

    public string AllLogsText =>
        string.Join(Environment.NewLine, OutputPageLogger.AllLogs.Select(l => l.Message));
}