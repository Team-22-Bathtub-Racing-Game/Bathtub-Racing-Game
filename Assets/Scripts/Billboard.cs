using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _pov;

    void Start()
    {
        _pov = Camera.main.transform;
    }

    void Update()
    {
        Vector3 newRotation = _pov.transform.eulerAngles;

        newRotation.x = 0;
        newRotation.z = 0;

        transform.eulerAngles = newRotation;
    }
}