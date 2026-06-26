namespace OverblikPlus.Shared.Helpers;

public static class JsonHelper
{
    public static string Serialize<T>(T value)
    {
        if (value == null)
            return "[]";
        
        return System.Text.Json.JsonSerializer.Serialize(value);
    }

    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}


