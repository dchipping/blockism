using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despwaner : MonoBehaviour
{
    // When something collides with belt
    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position = new Vector3(-7, 1.35f, 8.75f);
    }
}
