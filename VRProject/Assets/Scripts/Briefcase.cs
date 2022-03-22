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
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void Use(Hand controller)
    {
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
        if (!isStarted)
        {
            pointLight.intensity = 3f * Mathf.Abs(Mathf.Sin(Time.time));
        }
    }
}
