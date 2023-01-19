using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.SDK
{
    public enum VRCFTModuleType
    {
        Eye,
        Expression,
        Both
    }
    public enum VRCFTFieldValueType
    {
        None,
        Float,
        Bool
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class VRCFTModuleInfoAttribute : Attribute
    {
        public VRCFTModuleType ModuleType { get; private set; }

        public VRCFTModuleInfoAttribute(VRCFTModuleType moduleType)
        {
            ModuleType = moduleType;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class VRCFTModuleLoadPriorAttribute : Attribute
    {
        public string PostModule { get; private set; }

        public VRCFTModuleLoadPriorAttribute(string moduleName)
        {
            PostModule = moduleName;
        }
    }

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
}
