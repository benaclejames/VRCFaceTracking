using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

[AttributeUsage(AttributeTargets.Field)]
public class MutationPropertyAttribute : Attribute
{
    public string Name { get; }
    public bool SavedImmediate { get; }
    public Type EnumType { get; }
    public float Min { get; }
    public float Max { get; }

    public MutationPropertyAttribute(string name, bool savedImmediate = false, float min = 0f, float max = 1f)
    {
        Name = name;
        SavedImmediate = savedImmediate;
        Min = min;
        Max = max;
    }

    public MutationPropertyAttribute(Type enumType, bool savedImmediate = false, float min = 0f, float max = 1f)
    {
        EnumType = enumType;
        SavedImmediate = savedImmediate;
        Min = min;
        Max = max;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MutationButtonAttribute : Attribute
{
    public string Name
    {
        get;
    }

    public MutationButtonAttribute(string name)
    {
        Name = name;
    }
}