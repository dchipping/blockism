using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPRoomDoor : MonoBehaviour
{
    private string[] colours = { "red", "yellow", "blue", "green" };

    public Material doorColour;

    // Start is called before the first frame update
    void Start()
    {
        doorColour = GameManager.blockColoursStatic[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColour(int index)
    {
        doorColour = GameManager.blockColoursStatic[index];

        GameObject avatarAllowed = GameObject.FindGameObjectsWithTag(colours[index])[0];
        Physics.IgnoreCollision(gameObject.GetComponent<MeshCollider>(), avatarAllowed.gameObject.GetComponent<MeshCollider>(), true);

        if (index > 0)
        {
            GameObject avatarNotAllowed = GameObject.FindGameObjectsWithTag(colours[index - 1])[0];
            Physics.IgnoreCollision(gameObject.GetComponent<MeshCollider>(), avatarNotAllowed.gameObject.GetComponent<MeshCollider>(), false);
        }
    }
}
