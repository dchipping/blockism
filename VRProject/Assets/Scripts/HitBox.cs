using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{

    private GameObject thisHitBox;
    private Block parentBlock;
    public List<GameObject> pieces;
    public float hitboxThreshold;
    Collider thisCollider;
    List<Collider> pieceColliders = new List<Collider>();

    bool attached;
    


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

        try
        {
            parentBlock = transform.parent.gameObject.GetComponent<Block>();
        }
        catch (Exception ex)
        {
            parentBlock = null;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < pieceColliders.Count; i++)
        {
            Collider collider = pieceColliders[i];

            //if (GetIntersectionPercent(collider, thisCollider) > hitboxThreshold && !attached)
            if (!attached)
            {
                float lengthThreshold = 0.1f;
                if ((collider.transform.position - thisCollider.transform.position).magnitude < lengthThreshold)
                FillHitBox(pieces[i]);
            }
        }
    }

    public void FillHitBox(GameObject other)
    {



        // Define blocks and any structures they have
        Block otherBlock = other.GetComponent<Block>();
        GameObject parentBlockStruct = (parentBlock.structureObject != null ? parentBlock.structureObject : null);
        GameObject otherBlockStruct = (otherBlock.structureObject != null ? otherBlock.structureObject : null);

        if (!attached)
        {
            if (parentBlockStruct == null && otherBlockStruct == null)
            {
                // Remove grasping hand
                otherBlock.grasped = null;

                // Fill hitbox
                other.transform.position = thisHitBox.transform.position;
                other.transform.rotation = thisHitBox.transform.rotation;

                // Create new structure game object
                GameObject newStruct = new GameObject();
                newStruct.transform.position = thisHitBox.transform.position;
                newStruct.transform.rotation = thisHitBox.transform.rotation;
                otherBlock.structureObject = newStruct;
                parentBlock.structureObject = newStruct;

                // Add script to game object
                newStruct.AddComponent<Structure>();
                Structure structScript = newStruct.GetComponent<Structure>();
                structScript.children = new List<GameObject>();

                // Set a common parent
                other.transform.SetParent(newStruct.transform);
                transform.parent.SetParent(newStruct.transform);
                structScript.children.Add(other);
                structScript.children.Add(transform.parent.gameObject);

            }
            else if (parentBlockStruct != null && otherBlockStruct == null)
            {

                // Get structure object that has already been defined
                Structure structure = parentBlockStruct.GetComponent<Structure>();

                // Remove grasping hand
                otherBlock.grasped = null;
                structure.RemoveGrasp();

                // Fill hitbox
                other.transform.position = thisHitBox.transform.position;
                other.transform.rotation = thisHitBox.transform.rotation;


                // Set parents
                other.transform.SetParent(parentBlockStruct.transform);
                otherBlock.structureObject = parentBlockStruct;
                structure.children.Add(other);
            }
            else if (parentBlockStruct == null && otherBlockStruct != null)
            {
                // Get structure object that has already been defined
                Structure structure = otherBlockStruct.GetComponent<Structure>();

                // Remove grasping hand
                parentBlock.grasped = null;
                structure.RemoveGrasp();

                
                // Change position of entire sturcture  
                Vector3 positionOffset = other.transform.position - thisHitBox.transform.position;
                otherBlockStruct.transform.position = otherBlockStruct.transform.position - positionOffset;

                // Change rotation of the entire structure
                Vector3 rotationalOffset = other.transform.rotation.eulerAngles - thisHitBox.transform.rotation.eulerAngles;
                rotationalOffset.x = ClampTo180DegreeRange(rotationalOffset.x);
                rotationalOffset.y = ClampTo180DegreeRange(rotationalOffset.y);
                rotationalOffset.z = ClampTo180DegreeRange(rotationalOffset.z);
                otherBlockStruct.transform.RotateAround(other.transform.position, new Vector3(1, 0, 0), -rotationalOffset.x);
                otherBlockStruct.transform.RotateAround(other.transform.position, new Vector3(0, 1, 0), -rotationalOffset.y);
                otherBlockStruct.transform.RotateAround(other.transform.position, new Vector3(0, 0, 1), -rotationalOffset.z);

                // Set parents/children
                transform.parent.gameObject.transform.SetParent(otherBlockStruct.transform);
                parentBlock.structureObject = parentBlockStruct;
                structure.children.Add(transform.parent.gameObject);
            }
            else
            {

                // Get structure objects that has already been defined
                Structure otherStructure = otherBlockStruct.GetComponent<Structure>();
                Structure parentStructure = parentBlockStruct.GetComponent<Structure>();


                // Remove grasping hand
                otherStructure.RemoveGrasp();
                parentStructure.RemoveGrasp();

                // Change position of entire sturcture  
                Vector3 positionOffset = other.transform.position - thisHitBox.transform.position;
                otherBlockStruct.transform.position = otherBlockStruct.transform.position - positionOffset;

                // Change rotation of the entire structure
                Vector3 rotationalOffset = other.transform.rotation.eulerAngles - thisHitBox.transform.rotation.eulerAngles;
                rotationalOffset.x = ClampTo180DegreeRange(rotationalOffset.x);
                rotationalOffset.y = ClampTo180DegreeRange(rotationalOffset.y);
                rotationalOffset.z = ClampTo180DegreeRange(rotationalOffset.z);
                otherBlockStruct.transform.RotateAround(other.transform.position, new Vector3(1, 0, 0), -rotationalOffset.x);
                otherBlockStruct.transform.RotateAround(other.transform.position, new Vector3(0, 1, 0), -rotationalOffset.y);
                otherBlockStruct.transform.RotateAround(other.transform.position, new Vector3(0, 0, 1), -rotationalOffset.z);
                

                // Move all children GameObjects into a common GameObject
                foreach (GameObject child in parentStructure.children)
                {
                    child.transform.SetParent(otherBlockStruct.transform);
                    Block childBlock = child.GetComponent<Block>();
                    childBlock.structureObject = otherBlockStruct;
                }
            }


            attached = true;
        }
    }

    private float ClampTo180DegreeRange(float degs)
    {
        while (degs < -90 || degs > 90)
        {
            if (degs > 90)
                degs -= 180;
            else
                degs += 180;
        }
        return degs;
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
