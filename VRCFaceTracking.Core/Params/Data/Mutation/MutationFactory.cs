using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.SDK;
public static class MutationComponentFactory
{
    public static ObservableCollection<IMutationComponent> CreateComponents(object instance)
    {
        var components = new ObservableCollection<IMutationComponent>();
        var processedObjects = new HashSet<object>();
        ProcessFields(instance, components, processedObjects);
        return components;
    }


    public static void ProcessFields(object instance, ObservableCollection<IMutationComponent> components, HashSet<object> processedObjects)
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
                components.Add(mutationProperty);
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
                            ProcessFields(element, components, processedObjects);
                        }
                    }
                }
                else
                {
                    var nestedInstance = field.GetValue(instance);
                    if (nestedInstance != null)
                    {
                        ProcessFields(nestedInstance, components, processedObjects);
                    }
                }
            }
        }

        ProcessMethods(instance, components, processedObjects);
    }

    public static void ProcessMethods(object instance, ObservableCollection<IMutationComponent> components, HashSet<object> processedObjects)
    {
        var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<MutationButtonAttribute>();
            if (attribute != null)
            {
                var mutationProperty = new MutationAction
                (
                    attribute.Name,
                    () => method.Invoke(instance, null)
                );
                components.Add(mutationProperty);
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
