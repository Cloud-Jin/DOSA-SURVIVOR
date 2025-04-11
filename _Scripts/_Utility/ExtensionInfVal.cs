using InfiniteValue;
//ExtensionInfVal


public static class ExtensionInfVal
{
    public static string ToParseString(this InfVal value)
    {
        var maxDisplayedDigits = value.precision == 4 ? 4 : 
            value.precision % 3 == 1 ? 3 : 4;
        return MathInfVal.Floor(value).ToString(maxDisplayedDigits, DisplayOption.None);
    }
    
    public static string ToGoldString(this InfVal value)
    {
        // 골드, 경험치 용
        string[] arg = new[] { "K", "M", "G", "T" };
        
        var maxDisplayedDigits = value.precision == 4 ? 4 : 
                                    value.precision % 3 == 1 ? 3 : 4;
        
        return value.ToExponent(0).ToString(maxDisplayedDigits, arg, DisplayOption.AddSeparatorsBeforeDecimalPoint | DisplayOption.KeepZerosAfterDecimalPoint);
    }
    
    public static string ToGoodsString(this InfVal value)
    {
        return value.ToExponent(0).ToString(0, DisplayOption.AddSeparatorsBeforeDecimalPoint);
    }
}
