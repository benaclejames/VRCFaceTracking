using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Models;
using VRCFaceTracking.Services.Logging;

namespace VRCFaceTracking.ViewModels;

public class OutputViewModel : ObservableRecipient
{
    public ObservableCollection<LogLine> Logs => OutputPageLogger.AllLogs;
}