using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{

    private GameObject thisHitBox;
    public List<GameObject> pieces;
    Collider thisCollider;
    List<Collider> pieceColliders = new List<Collider>();
    public bool filled;
    int fillingObject;


    // Start is called before the first frame update
    void Start()
    {
        //Check that the peice GameObject exists and fetch the Collider
        foreach (GameObject obj in pieces) {
            pieceColliders.Add(obj.GetComponent<Collider>());
        }

        //Check that the hitbox GameObject exists in the Inspector and fetch the Collider
        thisHitBox = gameObject;
        if (thisHitBox != null)
            thisCollider = thisHitBox.GetComponent<Collider>();

        filled = false;
    }

    // Update is called once per frame
    void Update()
    {
        bool filledFlag = false;
        int filledIdx = -1;
        for (int i = 0; i < pieceColliders.Count; i++)
        {
            Collider collider = pieceColliders[i];
            if (GetIntersectionPercent(collider, thisCollider) > 0.75)
            {
                filledFlag = true;
                filledIdx = i;
            }
        }
        filled = filledFlag;
        fillingObject = filledIdx;
    }

    public void FillHitBox()
    {
        pieces[fillingObject].transform.position = thisHitBox.transform.position;
        pieces[fillingObject].transform.rotation = thisHitBox.transform.rotation;
        pieces[fillingObject].transform.SetParent(thisHitBox.transform);
    }

    // Estimates percentage of two colliders that are intersecting (may need to be improved)
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
