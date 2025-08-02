using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using VRCFaceTracking.Core.Params.Data.Mutation;

namespace VRCFaceTracking.Helpers;

public class ComponentTemplateSelector : DataTemplateSelector
{
    public DataTemplate CheckboxTemplate { get; set; }
    public DataTemplate TextInputTemplate { get; set; }
    public DataTemplate SliderTemplate { get; set; }
    public DataTemplate ButtonTemplate { get; set; }
    public DataTemplate RangeTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is MutationProperty property)
            return property.Type switch
            {
                MutationPropertyType.CheckBox => CheckboxTemplate,
                MutationPropertyType.TextBox => TextInputTemplate,
                MutationPropertyType.Slider => SliderTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        if (item is MutationRangeProperty)
            return RangeTemplate;
        if (item is MutationAction)
            return ButtonTemplate;

        return base.SelectTemplateCore(item, container);
    }
}
