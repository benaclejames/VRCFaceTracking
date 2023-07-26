using System.ComponentModel;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface IParamSupervisor : INotifyPropertyChanged
{
    bool AllParametersRelevant { get; set; }
}