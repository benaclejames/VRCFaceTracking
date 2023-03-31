using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking_Next.Core.Contracts.Services;
public interface IMainService
{
    void Teardown();
    Task InitializeAsync();
}
