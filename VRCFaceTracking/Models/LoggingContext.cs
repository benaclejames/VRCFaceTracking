using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VRCFaceTracking.Models;

public class LoggingContext : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private bool _visible = true;
    public bool Visible 
    { 
        get => _visible;
        set
        {
            _visible = value;
            NotifyPropertyChanged();
        } 
    }

    private string _context;
    public string Context 
    { 
        get => _context;
        set
        {
            _context = value;
            NotifyPropertyChanged();
        } 
    }
    private string _logs;

    public string Logs 
    { 
        get => _logs;
        set
        {
            _logs = value;
            NotifyPropertyChanged();
        } 
    }
}
