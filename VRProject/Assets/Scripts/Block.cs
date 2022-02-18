using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.XR;
using System;

public class Block : MonoBehaviour, IGraspable
{

    private Hand grasped;

    //private Rigidbody rigidbody;
    //public List<HitBox> hitBoxes;

    //enum BlockClass { RED, YELLOW };
    //BlockClass colour;

    // Start is called before the first frame update
    void Start()
    {
        // Set the rigidbody from the block - rigidbody controls the physics
        //rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void IGraspable.Grasp(Hand controller)
    {
        grasped = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        grasped = null;
    }

    // Update is called once per frame
    void Update()
    {
        // If the block is held by a player
        if (grasped)
        {
            // Match the position and orientation of the hand
            transform.localPosition = grasped.transform.position;
            transform.rotation = grasped.transform.rotation;
        }


        //If the first GameObject's Bounds enters the second GameObject's Bounds, output the message

    }
}
