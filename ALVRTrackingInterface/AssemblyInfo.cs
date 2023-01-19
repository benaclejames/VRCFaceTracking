using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VRCFaceTracking.SDK;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ALVR Tracking Interface")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ALVR Tracking Interface")]
// [assembly: AssemblyCopyright("Copyright ©  2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// VRCFaceTracking Loading
[assembly: VRCFTModuleLoadPrior("SRanipalExtTrackingModule")]
[assembly: VRCFTModuleInfo(VRCFTModuleType.Both)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("47c84c9c-829f-4f72-bcf8-838c1c79ad77")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
