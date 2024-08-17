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

    public MutationPropertyAttribute(string name)
    {
        Name = name;
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