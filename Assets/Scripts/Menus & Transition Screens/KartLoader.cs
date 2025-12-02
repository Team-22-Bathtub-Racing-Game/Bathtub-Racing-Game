using UnityEngine;

public class KartLoader : MonoBehaviour
{
    public GameObject kartBasePrefab;
    public string selectedKartName;

    void Start()
    {
        LoadKart();
    }

    void LoadKart()
    {
        selectedKartName = PlayerPrefs.GetString("SelectedKartName", "");

        if (string.IsNullOrEmpty(selectedKartName))
        {
            Debug.LogWarning("No kart selected — loading default kart.");
            Instantiate(kartBasePrefab);
            return;
        }

        CustomKart[] saved = KartSaveManager.LoadKarts();

        CustomKart target = null;
        foreach (var k in saved)
        {
            if (k.KartName == selectedKartName)
            {
                target = k;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning("Saved kart not found — loading default.");
            Instantiate(kartBasePrefab);
            return;
        }

        GameObject kart = Instantiate(kartBasePrefab);

        // APPLY COLORS TO CHILDREN OF THIS NEW INSTANCE
        var renderers = kart.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer r in renderers)
        {
            if (r.gameObject.name.Contains("Body"))
                r.material.color = target.MainColor;
            if (r.gameObject.name.Contains("Trim"))
                r.material.color = target.TrimColor;
            if (r.gameObject.name.Contains("Decal"))
                r.material.color = target.DecalColor;
        }

        Debug.Log("Loaded kart: " + target.KartName);
    }
}

