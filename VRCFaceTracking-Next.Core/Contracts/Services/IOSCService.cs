using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking_Next.Core.Contracts.Services;
public interface IOSCService
{
    (bool senderSuccess, bool receiverSuccess) Bind(string address, int outPort, int inPort);
}
