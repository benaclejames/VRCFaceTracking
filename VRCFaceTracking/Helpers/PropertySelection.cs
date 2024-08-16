using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.Helpers;

public class MutationPropertyTemplateSelector : DataTemplateSelector
{
    public DataTemplate CheckboxTemplate { get; set; }
    public DataTemplate TextInputTemplate { get; set; }
    public DataTemplate SliderTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is MutationProperty property)
        {
            return property.Type switch
            {
                MutationPropertyType.CheckBox => CheckboxTemplate,
                MutationPropertyType.TextBox => TextInputTemplate,
                MutationPropertyType.Slider => SliderTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
        return base.SelectTemplateCore(item, container);
    }
}
