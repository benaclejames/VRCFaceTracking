using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VRCFaceTracking
{
    internal static class ModuleAttributeHandler
    {
        private static void HandleLoadPriorAttribute(Assembly assembly, ref List<Assembly> assemblies, List<Attribute> attributes)
        {
            Assembly tempAssembly;

            // Get 'prior to' attributes.
            var loadPriorTo = attributes.OfType<VRCFTModuleEnsureLoadPriorAttribute>().ToList();

            // Iterating modules in order of how they appear from Lib Manager in front of main indexer
            for (int j = 0; j < assemblies.IndexOf(assembly); j++)
            {
                // Iterating through each 'prior to' attribute and checking if the module's GUID is contained.
                foreach (VRCFTModuleEnsureLoadPriorAttribute a in loadPriorTo)
                    if (a.ModuleGUID.Equals(assemblies[j].GetCustomAttribute<GuidAttribute>().Value))
                    {
                        int t = assemblies.IndexOf(assembly);
                        while (t > j)
                        {
                            Logger.Msg("Pushed " + assemblies[t].GetCustomAttribute<AssemblyTitleAttribute>().Title +
                            " into " + assemblies[t - 1].GetCustomAttribute<AssemblyTitleAttribute>().Title +
                            " via LoadPrior. \nt Value: " + t + "j Value: " + j);
                            tempAssembly = assemblies[t];
                            assemblies[t] = assemblies[t - 1];
                            assemblies[t - 1] = tempAssembly;
                            t--;
                        }
                    }
            }
        }

        public static void HandleModuleAttributes(ref List<Assembly> assemblies)
        {
            //HandleLoadPriorAttribute(assembly, ref assemblies, attributes);
            for (int i = 0; i < assemblies.Count; i++)
            {
                // Get all attributes.
                var attributes = assemblies[i].GetCustomAttributes().ToList();

                HandleLoadPriorAttribute(assemblies[i], ref assemblies, attributes);
            }
        }
    }
}
