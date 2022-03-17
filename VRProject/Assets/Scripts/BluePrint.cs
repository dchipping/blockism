using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePrint : MonoBehaviour
{
    public float RotationSpeed = 40.0f;

    // Stores which level the blueprint should be visible in
    public int whichLevel;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);
    }

    public void UpdateVisibility()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (whichLevel == GameManager.currLevel)
        {
            foreach (var r in renderers)
            {
                r.enabled = true;
            }
        }
        else
        {
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }
    }
}
