using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Params.Data.Mutation;
public static class MutationComponentFactory
{
    public static ObservableCollection<IMutationComponent> CreateComponents(object instance)
    {
        var components = new ObservableCollection<IMutationComponent>();
        var processedObjects = new HashSet<object>();
        ProcessFields(instance, components, processedObjects);
        return components;
    }

    public static void CreateComponent(
        string name,
        float min,
        float max,
        object instance, 
        object value, 
        FieldInfo field,
        ObservableCollection<IMutationComponent> components)
    {
        if (value is ValueTuple || value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(ValueTuple<,>))
        {
            var mutationRange = new MutationRangeProperty
            (
                name,
                (((float, float))value).Item1,
                (((float, float))value).Item2,
                newValue => field.SetValue(instance, newValue),
                min,
                max
            );
            components.Add(mutationRange);
        }
        else
        {
            var mutationProperty = new MutationProperty
            (
                name,
                value,
                DeterminePropertyType(value),
                newValue => field.SetValue(instance, Convert.ChangeType(newValue, field.FieldType)),
                min,
                max
            );
            components.Add(mutationProperty);
        }
    }

    public static void ProcessFields(object instance, ObservableCollection<IMutationComponent> components, HashSet<object> processedObjects)
    {
        if (instance == null || processedObjects.Contains(instance))
            return;

        processedObjects.Add(instance);

        var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<MutationPropertyAttribute>();

            if (attribute != null)
            {
                var value = field.GetValue(instance);
                if (field.FieldType.IsArray)
                {
                    var valueArray = value as Array;
                    Type enumType = attribute.EnumType;
                    
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        var name = Enum.GetName(enumType, i);
                        if (name == null) break;
                        CreateComponent(name, attribute.Min, attribute.Max, instance, valueArray.GetValue(i), field, components);
                    }
                    processedObjects.Add(valueArray);
                }
                else CreateComponent(attribute.Name, attribute.Min, attribute.Max, instance, value, field, components);
            }

            if (field.FieldType.IsClass || field.FieldType.IsValueType)
            {
                var nestedInstance = field.GetValue(instance);
                if (nestedInstance != null)
                {
                    ProcessFields(nestedInstance, components, processedObjects);
                }
            }
        }

        ProcessMethods(instance, components, processedObjects);
    }

    public static void ProcessMethods(object instance, ObservableCollection<IMutationComponent> components, HashSet<object> processedObjects)
    {
        var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

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
