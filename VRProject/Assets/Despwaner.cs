using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despwaner : MonoBehaviour
{
    // When something collides with belt
    private void OnCollisionEnter(Collision collision)
    {
        if (!GameManager.conveyerQueue.Contains(collision.gameObject.GetComponent<Block>()))
            GameManager.conveyerQueue.Enqueue(collision.gameObject.GetComponent<Block>());
    }
}
