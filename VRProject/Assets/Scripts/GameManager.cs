using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;
using Ubiq.Rooms;


public class GameManager : MonoBehaviour, INetworkComponent, INetworkObject
{
    // Helper Signs
    public List<GameObject> helperSigns;
    private static List<GameObject> helperSignsStatic;

    // Sound effects
    public List<AudioClip> clickSound;
    private static List<AudioClip> clickSoundsStatic;

    // Materials
    public List<Material> blockColours;
    public List<Material> bluePrintColours;
    public static List<Material> blockColoursStatic;

    // A list of all blocks in the game world
    public List<Block> allBlocks;
    private static List<Block> allBlocksStatic;

    // A list of all blue print blocks in the game world
    public List<MeshRenderer> bluePrintBlocks;
    private static List<MeshRenderer> bluePrintBlocksStatic;

    // A list of blueprints
    public List<BluePrint> bluePrints;
    private static List<BluePrint> bluePrintsStatic;

    // A list of blueprints
    public List<HitBox> hitboxes;
    private static List<HitBox> hitboxesStatic;

    // Game state
    public static int numOfPlayers = 3;

    // Event that randomly changes roles of the players 
    UnityEvent role_change;

    RoleManager role_manager; 

    public static int score = 0;
    public static Queue<Block> conveyerQueue = new Queue<Block>();

    // Current level
    public static int currLevel;

    // Networking components
    NetworkId INetworkObject.Id => new NetworkId("a13ba05dbb9ef8fc");
    private NetworkContext context;

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        // Initalize static variables
        helperSignsStatic = helperSigns;
        clickSoundsStatic = clickSound;
        blockColoursStatic = blockColours;
        allBlocksStatic = allBlocks;

        role_manager = GameObject.Find("RoleManager").GetComponent<RoleManager>();

        role_change = new UnityEvent();
        role_change.AddListener(OnRoleChange);

        bluePrintsStatic = bluePrints;
        bluePrintBlocksStatic = bluePrintBlocks;
        hitboxesStatic = hitboxes;
    }

    void OnRoleChange()
    {
        role_manager.ShuffleRoles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    struct Message
    {
        public bool start;

        public Message(bool start)
        {
            this.start = start;
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting Game!");
        foreach (GameObject sign in helperSignsStatic) // Loop through List with foreach
        {
            Destroy(sign);
        }

        // Colour all blocks in scene
        List<int> colours = role_manager.GetAvatarColourIndexes();
        numOfPlayers = colours.Count();
        for (int i = 0; i < allBlocksStatic.Count; i++)
        {
            // How many colours will be used
            int numberOfColours = numOfPlayers;
            if (numOfPlayers > blockColoursStatic.Count)
                numberOfColours = blockColoursStatic.Count;

            // Tell blocks what materials they should use
            int avatarIdx = i % numberOfColours;
            int colourIdx = colours[avatarIdx];
            allBlocksStatic[i].SetColour(colourIdx);

            
            // Tell blue print blocks what materials they should use
            bluePrintBlocks[i].material = bluePrintColours[colourIdx];

            // Set any child blocks to be the same colour
            foreach (Transform child in bluePrintBlocks[i].gameObject.transform)
            {
                MeshRenderer mesh_rend = child.GetComponent<MeshRenderer>();
                mesh_rend.material = bluePrintColours[colourIdx];
            }

            // Set hitbox correct colours
            if (hitboxes[i] != null)
                hitboxes[i].correctColourIdx = colourIdx;

        }
        currLevel = 0;

        NextLevel();
        StartCoroutine(SpawnBlocks());
    }

    // Unity allows to use Start as IEnumerator instead of a void
    private IEnumerator SpawnBlocks()
    {
        while (true)
        {
            if (conveyerQueue.Count > 0)
            {
                Block nextBlock = conveyerQueue.Dequeue();
                nextBlock.gameObject.transform.position = new Vector3(-7, 1.35f, 8.75f);
            }
            yield return new WaitForSeconds(2);
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

        if (currLevel == 1)
            Level1();
        else if (currLevel == 2)
            Level2();
        else if (currLevel == 3)
            Level3();
        else
            EndGame();
    }

    private static void Level1()
    {
        conveyerQueue.Clear();

        for (int i = 0; i < 3; i++)
            conveyerQueue.Enqueue(allBlocksStatic[i]);
    }

    private static void Level2()
    {

        conveyerQueue.Clear();

        for (int i = 0; i < 3; i++)
        {
            Block nextBlock = allBlocksStatic[i].GetComponent<Block>();
            nextBlock.gameObject.transform.position = new Vector3(0, -9.5f, 8.75f);
        }

        for (int i = 3; i < 15; i++)
            conveyerQueue.Enqueue(allBlocksStatic[i]);
    }

    private static void Level3()
    {
        conveyerQueue.Clear();

        for (int i = 3; i < 15; i++)
        {
            Block nextBlock = allBlocksStatic[i].GetComponent<Block>();
            nextBlock.gameObject.transform.position = new Vector3(0, -9.5f, 8.75f);
        }

        for (int i = 15; i < 30; i++)
            conveyerQueue.Enqueue(allBlocksStatic[i]);
    }

    private static void EndGame()
    {
        for (int i = 15; i < 30; i++)
        {
            Block nextBlock = allBlocksStatic[i].GetComponent<Block>();
            nextBlock.gameObject.transform.position = new Vector3(0, -9.5f, 8.75f);
        }

        Clock.EndGame();
    }

    public void SendMessageUpdate()
    {
        Message startMessage = new Message(true);
        context.SendJson(startMessage);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();
        if (msg.start == true)
            StartGame();
    }
}
