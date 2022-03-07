using System.Collections;
using System.Collections.Generic;
using Ubiq.XR;
using UnityEngine;

public class TestBlock : MonoBehaviour, IUseable, IGraspable
{
    private Hand follow;
    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Grasp(Hand controller)
    {
        follow = controller;
    }

    public void Release(Hand controller)
    {
        follow = null;
    }

    public void UnUse(Hand controller)
    {

    }

    public void Use(Hand controller)
    {

    }

    private void OnKeys()
    {
        Debug.Log("Test!");
        Vector3 movement = new Vector3(0f, 0f, 0f);
        if (Input.GetKey(KeyCode.A))
        {
            movement += new Vector3(-0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += new Vector3(0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.W))
        {
            movement += new Vector3(0f, 0f, 0.1f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += new Vector3(0f, 0f, -0.1f);
        }
        //movement = movement.normalized * (movementSpeed) * Time.deltaTime;
        //movement = headCamera.transform.TransformDirection(movement);
        movement.y = 0f;

        transform.position += movement;
    }

    private void Update()
    {
        if (follow != null)
        {
            transform.position = follow.transform.position;
            transform.rotation = follow.transform.rotation;
            body.isKinematic = true;
        }
        else
        {
            body.isKinematic = false;
        }
        //OnKeys();
    }
}