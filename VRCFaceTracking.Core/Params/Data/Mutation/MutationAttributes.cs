using System;
using System.Collections.Generic;
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

public static class MutationPropertyFactory
{
    public static List<MutationProperty<object>> CreateProperties(object instance)
    {
        var properties = new List<MutationProperty<object>>();

        var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<MutationPropertyAttribute>();
            if (attribute != null)
            {
                var value = field.GetValue(instance);
                var mutationProperty = new MutationProperty<object> 
                {
                    Name = attribute.Name,
                    Value = value,
                    Type = DeterminePropertyType(value)
                };
                properties.Add(mutationProperty);
            }
        }

        return properties;
    }

    private static MutationPropertyType DeterminePropertyType(object value)
    {
        return value switch
        {
            bool => MutationPropertyType.CheckBox,
            int => MutationPropertyType.Slider,
            string => MutationPropertyType.TextBox,
            // Add more type checks as needed
            _ => MutationPropertyType.TextBox // default input
        };
    }

}
