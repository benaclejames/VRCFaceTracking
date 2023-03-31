using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking_Next.Core.Contracts.Services;
public interface IOSCService
{
    int InPort { get; set; }
    int OutPort { get; set; }
    string Address { get; set; }

    Task SaveSettings();
    Task LoadSettings();
    Task<(bool, bool)> InitializeAsync();
}
