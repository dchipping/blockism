using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.XR;
using System;

public class Block : MonoBehaviour, IGraspable
{

    private Hand grasped;

    public GameObject m_MyObject, m_NewObject;
    Collider m_Collider, m_Collider2;

    //private Rigidbody rigidbody;
    //public List<HitBox> hitBoxes;

    //enum BlockClass { RED, YELLOW };
    //BlockClass colour;

    // Start is called before the first frame update
    void Start()
    {
        // Set the rigidbody from the block - rigidbody controls the physics
        //rigidbody = gameObject.GetComponent<Rigidbody>();

        //Check that the first GameObject exists in the Inspector and fetch the Collider
        if (m_MyObject != null)
            m_Collider = m_MyObject.GetComponent<Collider>();

        //Check that the second GameObject exists in the Inspector and fetch the Collider
        if (m_NewObject != null)
            m_Collider2 = m_NewObject.GetComponent<Collider>();
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

            // Check hitboxes
            //for (HitBox hitbox in hitBoxes){
            //    hitbox.
            //}
        }

        GetIntersectionPercent(m_Collider, m_Collider2);
        //If the first GameObject's Bounds enters the second GameObject's Bounds, output the message

    }

    private float GetIntersectionPercent(Collider c1, Collider c2)
    {
        if (c1.bounds.Intersects(c2.bounds))
        {
            // Get minimums and maximums of the bounding boxes
            Vector3 min1 = c1.bounds.min;
            Vector3 max1 = c1.bounds.max;
            Vector3 min2 = c2.bounds.min;
            Vector3 max2 = c2.bounds.max;

            // Calculate intersection in each axis
            float x_intersection = Math.Max(max1.x - min2.x, max2.x - min1.x);
            float y_intersection = Math.Max(max1.y - min2.y, max2.y - min1.y);
            float z_intersection = Math.Max(max1.z - min2.z, max2.z - min1.z);

            // Calculate volumn of each bounding box
            float volumn1 = (max1.x - min1.x) * (max1.y - min1.y) * (max1.z - min1.z);
            float volumn2 = (max2.x - min2.x) * (max2.y - min2.y) * (max2.z - min2.z);

            // Take the smallest bouding box (a bounding box is aligned with world coordinates so is bigger than the block)
            float smallestVolumn = Math.Min(volumn1, volumn2);

            // Return an estimate of the percent of two colliders are intersecting each other
            Debug.Log(smallestVolumn / (x_intersection * y_intersection * z_intersection));
            return smallestVolumn / (x_intersection * y_intersection * z_intersection);
        }

        return 0;
    }
}
