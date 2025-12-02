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

        // FACE the camera
        transform.LookAt(cam);

        // FIX: Many billboards need a 180° rotation to face correctly
        transform.Rotate(0, 180f, 0);
    }
}