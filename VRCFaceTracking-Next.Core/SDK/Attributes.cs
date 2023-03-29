using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking
{
    /*
    public enum VRCFTModuleType
    {
        Eye,
        Expression,
        Both
    }

    /// <summary>
    /// Specifies the module type info relevant for VRCFaceTracking.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class VRCFTModuleTypeAttribute : Attribute
    {
        /// <summary>
        /// Container of the type of data the module is expected to send.
        /// </summary>
        public VRCFTModuleType ModuleType { get; private set; }

        public VRCFTModuleTypeAttribute(VRCFTModuleType moduleType)
        {
            ModuleType = moduleType;
        }
    }
    */

    /// <summary>
    /// Specifies loading this module prior to the specified module GUID.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class VRCFTModuleEnsureLoadPriorAttribute : Attribute
    {
        /// <summary>
        /// Container of the referenced module GUID.
        /// </summary>
        public string ModuleGUID { get; private set; }

        public VRCFTModuleEnsureLoadPriorAttribute(string moduleGUID)
        {
            ModuleGUID = moduleGUID;
        }
    }

    /// <summary>
    /// Specifies loading this module directly after the specified module GUID.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class VRCFTModuleEnsureLoadAfterAttribute : Attribute
    {
        /// <summary>
        /// Container of the referenced module GUID.
        /// </summary>
        public string ModuleGUID { get; private set; }

        public VRCFTModuleEnsureLoadAfterAttribute(string moduleGUID)
        {
            ModuleGUID = moduleGUID;
        }
    }
    /*
    [AttributeUsage(AttributeTargets.Field)]
    public class VRCFTModuleFieldAttribute : Attribute
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public VRCFTFieldValueType Type { get; set; }

        public VRCFTModuleFieldAttribute(string name, float value) 
        {
            Name = name;
            Value = value; 
            Type = VRCFTFieldValueType.Float;
        }

        public VRCFTModuleFieldAttribute(string name, bool value)
        {
            Name = name;
            Value = value == true ? 1.0f : 0.0f;
            Type = VRCFTFieldValueType.Bool;
        }
        public VRCFTModuleFieldAttribute(string name)
        {
            Name = name;
            Value = 0.0f;
            Type = VRCFTFieldValueType.None;
        }
    }
    */
}
