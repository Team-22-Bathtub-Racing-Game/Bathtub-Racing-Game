using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Update()
    {
        if (cam == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cam = mainCam.transform;
            else
                return;
        }
        transform.LookAt(cam);

        transform.Rotate(0, 180f, 0);
    }
}