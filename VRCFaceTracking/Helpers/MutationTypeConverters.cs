using System.Globalization;
using Avalonia.Data.Converters;
using VRCFaceTracking.Core.Params.Data.Mutation;

namespace VRCFaceTracking.Helpers;

public abstract class MutationTypeEqualityConverter : IValueConverter
{
    protected abstract MutationPropertyType Expected { get; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MutationPropertyType t && t == Expected;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// We're not in kansas (winui) anymore :c
public sealed class MutationTypeCheckboxConverter : MutationTypeEqualityConverter
{
    protected override MutationPropertyType Expected => MutationPropertyType.CheckBox;
}

public sealed class MutationTypeSliderConverter : MutationTypeEqualityConverter
{
    protected override MutationPropertyType Expected => MutationPropertyType.Slider;
}

public sealed class MutationTypeTextConverter : MutationTypeEqualityConverter
{
    protected override MutationPropertyType Expected => MutationPropertyType.TextBox;
}
