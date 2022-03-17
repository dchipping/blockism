using System.Collections;
using System.Collections.Generic;
using Ubiq.XR;
using UnityEngine;

public class Briefcase : MonoBehaviour, IUseable
{
    public Light pointLight;

    private Hand follow;
    private bool isStarted = false;
    private Rigidbody body;

    public void Use(Hand controller)
    {
        if (!isStarted)
        {
            GameManager.StartGame();
            isStarted = true;
            pointLight.intensity = 0f;
        }
    }

    public void UnUse(Hand controller)
    {
        
    }

    public void FixedUpdate()
    {
        if (!isStarted)
        {
            pointLight.intensity = 3f * Mathf.Abs(Mathf.Sin(Time.time));
        }
    }
}
