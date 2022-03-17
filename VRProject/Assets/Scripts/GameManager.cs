using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // A list of blueprints
    public List<BluePrint> bluePrints;
    private static List<BluePrint> bluePrintsStatic;

    // Game state
    static int numOfPlayers = 2;
    static Queue<Block> conveyerQueue = new Queue<Block>();

    // Current level
    public static int currLevel;

    // Start is called before the first frame update
    void Start()
    {
        // Initalize static variables
        clickSoundsStatic = clickSound;
        blockColoursStatic = blockColours;
        allBlocksStatic = allBlocks;
        bluePrintsStatic = bluePrints;

        StartGame();
        
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
        currLevel = 0;

        conveyerQueue.Enqueue(allBlocksStatic[0]);
        conveyerQueue.Enqueue(allBlocksStatic[1]);
        conveyerQueue.Enqueue(allBlocksStatic[2]);
        NextLevel();
        SpawnBlocks();
    }

    static void SpawnBlocks()
    {
        float lastClockValue = Clock.timeRemaining;
        while (conveyerQueue.Count > 0)
        {
            while (lastClockValue - Clock.timeRemaining > 1)
            {
                lastClockValue = Clock.timeRemaining;
                Block nextBlock = conveyerQueue.Dequeue();
                nextBlock.gameObject.transform.position = new Vector3(-7, 1.35f, 8.75f);
            }
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

    public static void NextLevel()
    {
        currLevel++;
        Clock.ResetClock();
        foreach (BluePrint bp in bluePrintsStatic)
        {
            bp.UpdateVisibility();
        }
    }
}
