using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmissionArea : MonoBehaviour
{
    public TMPro.TextMeshProUGUI scoreText;
    int submittedStructures = 0;

    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "$0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if its a root objects
        if (other.gameObject.transform.parent == null)
        {
            // if the structure isnt being held
            Block b = other.gameObject.GetComponent<Block>();
            if (b != null && b.grasped == null)
            {
                // How many block were misplaced in the structure?
                int correctBlocks = 0;
                if (!other.gameObject.name.Contains("Table"))
                {
                    foreach (Transform child in other.gameObject.transform)
                    {
                        HitBox hb = child.gameObject.GetComponent<HitBox>();
                        if (hb != null)
                        {
                            foreach (Transform hitboxChild in child)
                            {
                                Block block = hitboxChild.gameObject.GetComponent<Block>();
                                if (block != null && block.colourIdx == hb.correctColourIdx)
                                    correctBlocks++;
                            }
                        }
                    }
                }
                else
                {
                    correctBlocks = 2;
                }

                // Update score
                GameManager.score += correctBlocks;
                scoreText.text = "$" + GameManager.score.ToString();

                // Teleport object somewhere else
                other.gameObject.transform.position = new Vector3(0, -99.5f, 28.75f);

                // Go to next level if enough structures have been submitted
                submittedStructures++;
                if (submittedStructures >= GameManager.currLevel)
                {
                    submittedStructures = 0;
                    GameManager.NextLevel();
                }
            }
        }
    }
}
