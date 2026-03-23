using System.Text.Json;

namespace WorkTimer;

internal sealed class Settings
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkTimer");

    private static readonly string SettingsFile =
        Path.Combine(SettingsDir, "settings.json");

    public string VirtualKey { get; set; } = "F16";
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>Map of display name to virtual key code for unused high function keys.</summary>
    public static readonly Dictionary<string, ushort> AvailableKeys = new()
    {
        ["F13"] = 0x7C,
        ["F14"] = 0x7D,
        ["F15"] = 0x7E,
        ["F16"] = 0x7F,
        ["F17"] = 0x80,
        ["F18"] = 0x81,
        ["F19"] = 0x82,
        ["F20"] = 0x83,
        ["F21"] = 0x84,
        ["F22"] = 0x85,
        ["F23"] = 0x86,
        ["F24"] = 0x87,
    };

    public ushort GetVirtualKeyCode() =>
        AvailableKeys.TryGetValue(VirtualKey, out var code) ? code : AvailableKeys["F16"];

    public static Settings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch
        {
            // Corrupted file — fall back to defaults
        }
        return new Settings();
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFile, json);
    }
}
