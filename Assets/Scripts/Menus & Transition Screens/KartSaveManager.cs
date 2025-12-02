using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class KartSaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "customKarts.json");

    // Save kart
    public static void SaveKart(CustomKart kart)
    {
        string json = JsonUtility.ToJson(new CustomKartSerializable(kart), true);

        // Check if file exists, load existing array
        CustomKartSerializableList kartList;
        if (File.Exists(SavePath))
        {
            string existing = File.ReadAllText(SavePath);
            kartList = JsonUtility.FromJson<CustomKartSerializableList>(existing) ?? new CustomKartSerializableList();
        }
        else
        {
            kartList = new CustomKartSerializableList();
        }

        // Add new kart and save
        kartList.Karts.Add(new CustomKartSerializable(kart));
        File.WriteAllText(SavePath, JsonUtility.ToJson(kartList, true));
    }

    // Load all karts
    public static CustomKart[] LoadKarts()
    {
        if (!File.Exists(SavePath)) return new CustomKart[0];

        string json = File.ReadAllText(SavePath);
        CustomKartSerializableList list = JsonUtility.FromJson<CustomKartSerializableList>(json);
        if (list == null) return new CustomKart[0];

        CustomKart[] karts = new CustomKart[list.Karts.Count];
        for (int i = 0; i < list.Karts.Count; i++)
            karts[i] = list.Karts[i].ToCustomKart();

        return karts;
    }
}

// Serializable wrapper for Color and enums
[System.Serializable]
public class CustomKartSerializable
{
    public string KartName;
    public string DriverName;
    public int Wheel;
    public int RollCage;
    public int ExtraDetail;
    public int Decal;
    public string MainColor;
    public string TrimColor;
    public string DecalColor;

    public CustomKartSerializable() { }

    public CustomKartSerializable(CustomKart kart)
    {
        KartName = kart.KartName;
        DriverName = kart.DriverName;
        Wheel = (int)kart.Wheel;
        RollCage = (int)kart.RollCage;
        ExtraDetail = (int)kart.ExtraDetail;
        Decal = (int)kart.Decal;
        MainColor = ColorUtility.ToHtmlStringRGB(kart.MainColor);
        TrimColor = ColorUtility.ToHtmlStringRGB(kart.TrimColor);
        DecalColor = ColorUtility.ToHtmlStringRGB(kart.DecalColor);
    }

    public CustomKart ToCustomKart()
    {
        CustomKart kart = new CustomKart();
        kart.KartName = KartName;
        kart.DriverName = DriverName;
        kart.Wheel = (WheelType)Wheel;
        kart.RollCage = (RollCageType)RollCage;
        kart.ExtraDetail = (ExtraDetailType)ExtraDetail;
        kart.Decal = (DecalType)Decal;

        ColorUtility.TryParseHtmlString("#" + MainColor, out kart.MainColor);
        ColorUtility.TryParseHtmlString("#" + TrimColor, out kart.TrimColor);
        ColorUtility.TryParseHtmlString("#" + DecalColor, out kart.DecalColor);

        return kart;
    }
}

[System.Serializable]
public class CustomKartSerializableList
{
    public List<CustomKartSerializable> Karts = new List<CustomKartSerializable>();
}