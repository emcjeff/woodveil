using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Capital 'T' for the Type
    public Transform cam;

    void LateUpdate()
    {
        if (cam != null)
        {
            // Lowercase 't' for the current object's property
            transform.LookAt(transform.position + cam.forward);
        }
    }
}