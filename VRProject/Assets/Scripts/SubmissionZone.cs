using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmissionZone : MonoBehaviour
{

    // List of blocks that are currently in the zone
    public HashSet<GameObject> blocksInZone = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Loop through children and compare them to the hitboxes
        // If all colours correct, increment score
        // What to do with completed structure?
        foreach (GameObject block in blocksInZone)
        {
            if (block.name.Contains("Root") && !block.GetComponent<Block>().being_grasped)
            {
                HitBox[] hitBoxes = block.GetComponentsInChildren<HitBox>();
                foreach (HitBox hb in hitBoxes)
                {
                    if (hb.GetComponent<MeshRenderer>().material.color != hb.GetComponent<Block>().GetComponent<MeshRenderer>().material.color)
                    {
                        GameManager.submissionsRemaining[GameManager.currLevel - 1]--;
                        if (GameManager.submissionsRemaining[GameManager.currLevel - 1] == 0)
                        {
                            GameManager.NextLevel();
                        }
                        return;
                    }
                }
                blocksInZone.Remove(block);
                Score.UpdateScore();
                GameManager.submissionsRemaining[GameManager.currLevel - 1]--;
                if (GameManager.submissionsRemaining[GameManager.currLevel - 1] == 0)
                {
                    GameManager.NextLevel();
                }
            }
            else
            {
                // Possibly some sound effect
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Block>() != null)
        {
            blocksInZone.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<Block>() != null)
        {
            blocksInZone.Add(collision.gameObject);
        }
    }
}
