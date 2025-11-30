using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectCustomizations : MonoBehaviour
{
    //Live update objects
    public GameObject kart;
    public GameObject wheels;
    public GameObject rollCage;
    public GameObject extraDetail;
    public GameObject decal;
    
    private CustomKart _customKart;

    //Lists of customization options
    public GameObject[] wheelOptions;
    public GameObject[] rollCageOptions;
    public GameObject[] extraDetailOptions;
    public GameObject[] decalOptions;

    //Kart materials
    public MeshRenderer bodyMaterial;
    public MeshRenderer trimMaterial;
    public MeshRenderer decalMaterial;

    //UI input fields
    public TMP_Dropdown rollCageInput;
    public TMP_Dropdown extraDetailInput;
    public TMP_Dropdown decalInput;
    public TMP_Dropdown wheelInput;
    public TMP_InputField kartNameInput;
    public TMP_InputField driverNameInput;
    
    void Start()
    {
        _customKart = new CustomKart();
        //Load in pre-existing kart or reset to defaults
    }

    public void SetWheels()
    {
        Destroy(wheels.gameObject);
        int wheelType = wheelInput.value;
        wheels = wheelType switch
        {
            0 => Instantiate(wheelOptions[0], kart.transform),
            1 => Instantiate(wheelOptions[1], kart.transform),
            2 => Instantiate(wheelOptions[2], kart.transform),
            _ => Instantiate(wheelOptions[0], kart.transform)
        };
        
        _customKart.Wheel = (WheelType)wheelType;
    }

    public void SetRollCage()
    {
        Destroy(rollCage.gameObject);
        int rollCageType = rollCageInput.value;
        rollCage = rollCageType switch
        {
            0 => Instantiate(rollCageOptions[0], kart.transform),
            1 => Instantiate(rollCageOptions[1], kart.transform),
            2 => Instantiate(rollCageOptions[2], kart.transform),
            _ => Instantiate(rollCageOptions[0], kart.transform)
        };
        
        _customKart.RollCage = (RollCageType)rollCageType;
    }

    public void SetExtraDetail()
    {
        Destroy(extraDetail.gameObject);
        int extraDetailType = extraDetailInput.value;
        extraDetail = extraDetailType switch
        {
            0 => Instantiate(extraDetailOptions[0], kart.transform),
            1 => Instantiate(extraDetailOptions[1], kart.transform),
            //2 => Instantiate(extraDetailOptions[2], kart.transform),
            _ => Instantiate(extraDetailOptions[0], kart.transform)
        };
        
        _customKart.ExtraDetail = (ExtraDetailType)extraDetailType;
    }

    public void SetDecal()
    {
        Destroy(decal.gameObject);
        int decalType = decalInput.value;
        decal = decalType switch
        {
            0 => Instantiate(decalOptions[0], kart.transform),
            1 => Instantiate(decalOptions[1], kart.transform),
            2 => Instantiate(decalOptions[2], kart.transform),
            _ => Instantiate(decalOptions[0], kart.transform)
        };
        
        _customKart.Decal = (DecalType)decalType;
    }

    public void SetKartName()
    {
        _customKart.KartName = kartNameInput.text;
    }

    public void SetDriverName()
    {
        _customKart.DriverName = driverNameInput.text;
    }

    public void SelectMainColor(string color)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString("#" + color, out newColor);
        bodyMaterial.material.color = newColor;
        _customKart.MainColor = newColor;
    }

    public void SelectTrimColor(string color)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString("#" + color, out newColor);
        trimMaterial.material.color = newColor;
        _customKart.TrimColor = newColor;
    }

    public void SelectDecalColor(string color)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString("#" + color, out newColor);
        decalMaterial.material.color = newColor;
        _customKart.DecalColor = newColor;
    }

    public void ConfirmChanges()
    {
        
    }

    public void CancelChanges()
    {
        
    }
}

public class CustomKart
{
    public RollCageType RollCage;
    public WheelType Wheel;
    public ExtraDetailType ExtraDetail;
    public DecalType Decal;
    public string KartName;
    public string DriverName;
    public Color MainColor;
    public Color TrimColor;
    public Color DecalColor;

    public CustomKart()
    {
        
    }
}

public enum WheelType
{
    Large = 0, Small = 1, Combo = 2
}

public enum RollCageType
{
    Round = 0, Box = 1, Slim = 2
}

public enum ExtraDetailType
{
    None = 0, FrontWing = 1
}

public enum DecalType
{
    None = 0
}