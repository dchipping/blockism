using System.Collections;
using System.Collections.Generic;
using Ubiq.XR;
using UnityEngine;

public class Briefcase : MonoBehaviour, IUseable, IGraspable
{
    public Light pointLight;
    public GameManager gameManager;

    private Hand follow;
    private bool isStarted = false;
    private Rigidbody body;

    public void Start()
    {
        // Find game manager object in scene
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void Use(Hand controller)
    {
        // Start the game if not already started
        if (!isStarted && gameManager != null)
        {
            gameManager.SendMessageUpdate();
            gameManager.StartGame();
            isStarted = true;
            pointLight.intensity = 0f;
        }
    }

    public void UnUse(Hand controller)
    {
        
    }

    public void Grasp(Hand controller)
    {
        // Start the game if not already started
        if (!isStarted)
        {
            gameManager.SendMessageUpdate();
            gameManager.StartGame();
            isStarted = true;
            pointLight.intensity = 0f;
        }
    }

    public void Release(Hand controller) 
    {

    }

    public void FixedUpdate()
    {
        // Make light pulse
        if (!isStarted)
        {
            pointLight.intensity = 3f * Mathf.Abs(Mathf.Sin(Time.time));
        }
    }
}
