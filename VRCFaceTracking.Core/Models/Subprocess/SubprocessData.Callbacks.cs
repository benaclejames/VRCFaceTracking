using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Models;

/// <summary>
/// We use callbacks to intercept all invocations to the ExtTrackingModule interface
/// </summary>
public partial class SubprocessData
{

    public void OnInitializedCallback(bool eyeSuccess, bool expressionSuccess)
    {
        // @TODO: Verify if we can consider the module to be valid
        _isDataTrustworthy = eyeSuccess || expressionSuccess;
    }

    public void OnUpdateCallback()
    {

    }

    public void OnSupportedCallback(bool SupportsEye, bool SupportsExpression)
    {

    }

    public void OnTeardownCallback()
    {

    }

    public void OnLoggerCallback()
    {

    }

    public void OnModuleMetadataChangedCallback()
    {

    }
}
