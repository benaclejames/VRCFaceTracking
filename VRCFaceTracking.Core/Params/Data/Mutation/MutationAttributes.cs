using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.SDK;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MutationPropertyAttribute : Attribute
{
    public string Name { get; }
    public Type EnumType { get; }
    public float Min { get; }
    public float Max { get; }

    public MutationPropertyAttribute(string name, float min = 0f, float max = 1f)
    {
        Name = name;
        Min = min;
        Max = max;
    }

    public MutationPropertyAttribute(Type enumType, float min = 0f, float max = 1f)
    {
        EnumType = enumType;
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