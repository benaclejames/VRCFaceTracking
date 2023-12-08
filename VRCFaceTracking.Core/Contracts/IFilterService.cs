using System.ComponentModel;

namespace VRCFaceTracking.Core.Contracts;
public interface IFilterService : INotifyPropertyChanged
{
    public bool Enabled { get; set; }
    public double Alpha { get; set; }
    public double Beta { get; set; }
    public double Frequency { get; set; }
    public void Filter();
    public Task LoadCalibration();
    public Task SaveCalibration();

}
