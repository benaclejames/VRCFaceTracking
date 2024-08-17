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

public static class MutationPropertyFactory
{
    public static ObservableCollection<MutationProperty> CreateProperties(object instance)
    {
        var properties = new ObservableCollection<MutationProperty>();
        var processedObjects = new HashSet<object>();
        ProcessFields(instance, properties, processedObjects);
        return properties;
    }


    public static void ProcessFields(object instance, ObservableCollection<MutationProperty> properties, HashSet<object> processedObjects)
    {
        if (instance == null || processedObjects.Contains(instance))
            return;

        processedObjects.Add(instance);

        var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<MutationPropertyAttribute>();
            if (attribute != null)
            {
                var value = field.GetValue(instance);
                var mutationProperty = new MutationProperty 
                (
                    attribute.Name,
                    value,
                    DeterminePropertyType(value),
                    newValue => field.SetValue(instance, Convert.ChangeType(newValue, field.FieldType))
                );
                properties.Add(mutationProperty);
            }

            var fieldType = field.FieldType;
            if (fieldType.IsClass || fieldType.IsValueType)
            {
                if (fieldType.IsArray)
                {
                    var array = (Array)field.GetValue(instance);
                    if (array != null)
                    {
                        foreach (var element in array)
                        {
                            ProcessFields(element, properties, processedObjects);
                        }
                    }
                }
                else
                {
                    var nestedInstance = field.GetValue(instance);
                    if (nestedInstance != null)
                    {
                        ProcessFields(nestedInstance, properties, processedObjects);
                    }
                }
            }
        }
    }

    private static MutationPropertyType DeterminePropertyType(object value)
    {
        return value switch
        {
            bool => MutationPropertyType.CheckBox,
            float => MutationPropertyType.Slider,
            string => MutationPropertyType.TextBox,
            // Add more type checks as needed
            _ => MutationPropertyType.TextBox // default input
        };
    }

}
