namespace ShockTheirOsc;

public static class ExtensionMethods
{
    public static int ConvertVrcFloatToRange(this float value, float min, float max)
    {
        return (int)(value * (max - min) + min);
    }
}
