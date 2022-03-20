using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // Sound effects
    public List<AudioClip> clickSound;
    private static List<AudioClip> clickSoundsStatic;

    // Materials
    public List<Material> blockColours;
    public static List<Material> blockColoursStatic;

    // A list of all blocks in the game world
    public List<Block> allBlocks;
    private static List<Block> allBlocksStatic;

    // Game state
    public static int numOfPlayers = 2;

    // Event that randomly changes roles of the players 
    UnityEvent role_change;

    RoleManager role_manager; 

    // Start is called before the first frame update
    void Start()
    {
        // Initalize static variables
        clickSoundsStatic = clickSound;
        blockColoursStatic = blockColours;
        allBlocksStatic = allBlocks;

        role_manager = GameObject.Find("RoleManager").GetComponent<RoleManager>();

        role_change = new UnityEvent();
        role_change.AddListener(OnRoleChange);

        StartGame();
    }

    void OnRoleChange()
    {
        role_manager.ShuffleRoles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void StartGame()
    {
        // Colour all blocks in scene
        for (int i = 0; i < allBlocksStatic.Count; i++)
        {
            // How many colours will be used
            int numberOfColours = numOfPlayers;
            if (numOfPlayers > blockColoursStatic.Count)
                numberOfColours = blockColoursStatic.Count;

            // Tell blocks what materials they should use
            int colourIdx = i % numberOfColours;
            allBlocksStatic[i].SetColour(colourIdx);
        }
    }

    public static void PlayClickFromPoint(Vector3 position)
    {
        // Get random audio clip
        var random = new System.Random();
        int index = random.Next(clickSoundsStatic.Count-1);

        // Play clicking sound
        AudioSource.PlayClipAtPoint(clickSoundsStatic[index], position);
    }
}
