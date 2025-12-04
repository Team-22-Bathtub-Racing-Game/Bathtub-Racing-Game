using UnityEngine;

public class KartLoader : MonoBehaviour
{
    public GameObject kartBasePrefab;
    public string selectedKartName;

    void Start()
    {
        FindFirstObjectByType<RacePositionManager>().RefreshRacers();
        LoadKart();
    }
    

    void LoadKart()
    {
        selectedKartName = PlayerPrefs.GetString("SelectedKartName", "");

        GameObject kartToSpawn;

        // If no kart name stored, spawn default
        if (string.IsNullOrEmpty(selectedKartName))
        {
            Debug.LogWarning("No kart selected — loading default kart.");
            kartToSpawn = Instantiate(kartBasePrefab);
            return;
        }

        // Load all karts
        CustomKart[] saved = KartSaveManager.LoadKarts();
        CustomKart target = null;

        foreach (var k in saved)
            if (k.KartName == selectedKartName)
                target = k;

        if (target == null)
        {
            Debug.LogWarning("Saved kart not found — loading default kart.");
            kartToSpawn = Instantiate(kartBasePrefab);
            return;
        }

        // Spawn the customized kart
        GameObject kart = Instantiate(kartBasePrefab);
        var rpm = FindFirstObjectByType<RacePositionManager>();
        if (rpm != null)
            rpm.RefreshRacers();

        // Apply colors based on name of parts
        var renderers = kart.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer r in renderers)
        {
            if (r.gameObject.name.Contains("Body"))
                r.material.color = target.MainColor;

            if (r.gameObject.name.Contains("TubCap"))
                r.material.color = target.TrimColor;

            if (r.gameObject.name.Contains("Decal"))
                r.material.color = target.DecalColor;
        }

        Debug.Log("Loaded kart: " + target.KartName);
    }
}


