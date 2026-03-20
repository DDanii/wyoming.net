namespace Wyoming.Net.Satellite.App.Tz.Platform;

public class TizenFeature
{
    public TizenFeature(string key, string type)
    {
        Key = key;
        Type = type;
    }

    public string Key { get; private set;}
    public string Type { get; private set;}
}
