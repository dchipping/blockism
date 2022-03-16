using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;

public class GameManager : MonoBehaviour, INetworkComponent, INetworkObject
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
    static int numOfPlayers = 4;

    
    // Networking/avatar components
    private NetworkContext context;

    private RoomClient room_client; 

    private Ubiq.Avatars.AvatarManager avatar_manager;

    public List<string> avatar_ids;

    public List<string> avatar_roles;

    private string last_owner_id;

    NetworkId INetworkObject.Id => new NetworkId("a15ca05dbb9ef8ec");

    struct Message
    {
        
        public List <string> avatar_ids;
        public List<string> avatar_roles;
        public string last_owner_id;

        public Message(List<string> ai, List<string> ar, string lo_id)
        {
            this.avatar_ids = ai;
            this.avatar_roles = ar;
            this.last_owner_id = lo_id;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initalize static variables
        clickSoundsStatic = clickSound;
        blockColoursStatic = blockColours;
        allBlocksStatic = allBlocks;

        context = NetworkScene.Register(this);

        room_client = context.scene.GetComponentInChildren<RoomClient>();

        room_client.OnPeerAdded.AddListener(OnPeerAdded);

        room_client.OnJoinedRoom.AddListener(OnJoinedRoom);

        avatar_manager = GameObject.Find("Avatar Manager").GetComponent<Ubiq.Avatars.AvatarManager>();

        StartGame();
    }

    private void SendMessageUpdate()
    {
        Message message;
        message.avatar_ids = avatar_ids;
        message.avatar_roles = avatar_roles;
        message.last_owner_id = last_owner_id;

        context.SendJson(message);
    }

    private void OnJoinedRoom(IRoom room)
    {
        if (avatar_manager.Avatars.Count() > 1)
        {
            return;
        }

        // set first avatar as owner
        var owner_avatar = avatar_manager.Avatars.First();
        last_owner_id = owner_avatar.Peer.UUID;
        owner_avatar.color = "red";

        avatar_ids = new List<string>();
        avatar_roles = new List<string>();

        avatar_ids.Add(owner_avatar.Peer.UUID);
        avatar_roles.Add(owner_avatar.color);

        SendMessageUpdate();
    }

    private void OnPeerAdded(IPeer peer) 
    {
        
        // Attach roles and modify dict only if it is owner's room 
        if (room_client.Me.UUID != last_owner_id)
        {
            return; 
        }

        var avatars = avatar_manager.Avatars;
        var i = 0;

        foreach (var avatar in avatars) { 
            if (!avatar_ids.Contains(avatar.Peer.UUID))
            {
                if (i % 2 == 0)
                {
                    avatar.color = "red";
                } else
                {
                    avatar.color = "yellow";
                }

                avatar_ids.Add(avatar.Peer.UUID);
                avatar_roles.Add(avatar.color);
            }
            i++;
        }

        SendMessageUpdate();
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

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // do not update if GM same as last owner's  
        /*if (room_client.Me.UUID == msg.last_owner_id)
        {
            return;
        }*/

        avatar_ids = msg.avatar_ids;
        avatar_roles = msg.avatar_roles;
        last_owner_id = msg.last_owner_id;

        var avatars = avatar_manager.Avatars;

        foreach (var avatar in avatars)
        {
            if (avatar_ids.Contains(avatar.Peer.UUID))
            {
                avatar.color = avatar_roles[avatar_ids.IndexOf(avatar.Peer.UUID)];
            }
        }
    }
}
