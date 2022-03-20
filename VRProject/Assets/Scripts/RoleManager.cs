using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;


public class RoleManager : MonoBehaviour, INetworkComponent, INetworkObject
{
    private List<string> roles = new List<string> {"red", "yellow", "green", "blue"};

    private enum Roles
    {
        red, 
        yellow, 
        green, 
        blue
    };

    private NetworkContext context;

    private RoomClient room_client;

    private Ubiq.Avatars.AvatarManager avatar_manager;

    // set of variables for messages
    private List<string> avatar_ids;
    private List<string> avatar_roles;
    private string master_peer_id;
    private string room_id;
    private string join_code;
    // set of variables for messages

    NetworkId INetworkObject.Id => new NetworkId("a15ca05dbb9ef8ec");

    private string last_requested_join_code;

    private string room_name = "Blockism";

    private uint join_tries = 0;

    struct Message
    {

        public List<string> avatar_ids;
        public List<string> avatar_roles;
        public string master_peer_id;
        public string room_id;
        public string join_code;

        public Message(List<string> ai, List<string> ar, string mpi, string rid, string jcode)
        {
            this.avatar_ids = ai;
            this.avatar_roles = ar;
            this.master_peer_id = mpi;
            this.room_id = rid;
            this.join_code = jcode;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        room_client = context.scene.GetComponentInChildren<RoomClient>();

        room_client.OnPeerAdded.AddListener(OnPeerAdded);

        room_client.OnJoinedRoom.AddListener(OnJoinedRoom);

        room_client.OnRoomUpdated.AddListener(OnRoomUpdated);

        room_client.OnJoinRejected.AddListener(OnJoinRejected);

        avatar_manager = GameObject.Find("Avatar Manager").GetComponent<Ubiq.Avatars.AvatarManager>();

        Join();
    }

    private void Join()
    {
        if (string.IsNullOrEmpty(join_code))
        {
            room_client.Join(name: room_name, publish: true);
        } 
        else
        {
            room_client.Join(joincode: join_code);
        }
    }

    private void OnJoinRejected(Rejection rejection)
    {
        join_tries++;

        if (join_tries > 100)
        {
            return; 
        }

        Join();
    }

    private void AddAvatarAndRole(Ubiq.Avatars.Avatar avatar)
    {
        if (!avatar_ids.Contains(avatar.Peer.UUID))
        {
            avatar_ids.Add(avatar.Peer.UUID);
            avatar_roles.Add(avatar.color);
        }
    }

    private void RemoveAvatarAndRole(IPeer peer)
    {
        int peer_index = avatar_ids.IndexOf(peer.UUID);
        avatar_roles.RemoveAt(peer_index);
        avatar_ids.RemoveAt(peer_index);
    }

    private void SendMessageUpdate()
    {
        Message message;
        message.avatar_ids = avatar_ids;
        message.avatar_roles = avatar_roles;
        message.master_peer_id = master_peer_id;
        message.room_id = room_id;
        message.join_code = join_code;

        context.SendJson(message);
    }

    private void OnJoinedRoom(IRoom room)
    {
        // room with more than 1 avatar already has master peer
        if (avatar_manager.Avatars.Count() > 1)
        {
            return;
        }

        // do not set master peer for empty room 
        if (string.IsNullOrEmpty(room.JoinCode)
                && string.IsNullOrEmpty(room.Name)
                && string.IsNullOrEmpty(room.UUID))
        {
            return;
        }

        // set first avatar as master peer      
        var owner_avatar = avatar_manager.Avatars.First();
        master_peer_id = owner_avatar.Peer.UUID;
        owner_avatar.color = roles.First();

        var result = avatar_manager.AvatarCatalogue.prefabs[roles.IndexOf(owner_avatar.color)];

        /*avatar_manager.CreateLocalAvatar(result);*/

        room_client.Me["ubiq.avatar.prefab"] = result.name;

        avatar_ids = new List<string>();
        avatar_roles = new List<string>();
        room_id = room.UUID;
        join_code = room.JoinCode;

        avatar_ids.Add(owner_avatar.Peer.UUID);
        avatar_roles.Add(owner_avatar.color);

        SendMessageUpdate();
    }

    private void OnPeerAdded(IPeer peer)
    {
        // Attach roles and modify dict only if it is master peer's room 
        if (room_client.Me.UUID != master_peer_id)
        {
            return;
        }

        var avatars = avatar_manager.Avatars;
        Dictionary<string, int> role_count = new Dictionary<string, int>();

        // loop through roles and initiate Dict  
        roles.ForEach(role => role_count.Add(role, 0));

        foreach (var avatar in avatars)
        {
            // avatar already has a role 
            if (!string.IsNullOrEmpty(avatar.color))
            {
                // register the role with the Dict 
                role_count[avatar.color] += 1;

                // mainain an internal list of ids and roles 
                AddAvatarAndRole(avatar);
            }
        }
        
       Ubiq.Avatars.Avatar current_avatar = null;
       foreach (var avatar in avatars)
       {
            if (avatar.Peer.UUID == peer.UUID)
            {
                current_avatar = avatar; 
            }
       }

        // choose role with min count as current avatar's role 
        current_avatar.color = role_count.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

        // peer["ubiq.avatar.prefab"] = GameObject.FindGameObjectWithTag(current_avatar.color).name;

        AddAvatarAndRole(current_avatar);

        SendMessageUpdate();
    }

    private void OnRoomUpdated(IRoom room)
    {
        // peer is leaving room if room name, uuid and join code are null 
        if (string.IsNullOrEmpty(room.JoinCode) 
                && string.IsNullOrEmpty(room.Name) 
                && string.IsNullOrEmpty(room.UUID))
        {
            // if peer used to be master, do something
            if (master_peer_id == room_client.Me.UUID)
            {
                // remove ourselves from lists
                var peer_index = avatar_ids.IndexOf(room_client.Me.UUID);
                avatar_ids.RemoveAt(peer_index);
                avatar_roles.RemoveAt(peer_index);

                // choose someone else as master
                if (avatar_ids.Count() > 0)
                {
                    master_peer_id = avatar_ids[0];

                    // send message update
                    SendMessageUpdate();
                }

            }

            // reset variables since we are entering empty room
            avatar_ids = new List<string>();
            avatar_roles = new List<string>();
            room_id = "";
            join_code = "";
            master_peer_id = "";
        }

    }

    // runs every frame 
    void Update()
    {
        // send message every timestep
        if (room_client.Me.UUID == master_peer_id)
        {
            SendMessageUpdate();
        }
    }

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // utilize message update only if sent my master peer in the same room 
        /*if (msg.room_id != room_client.Room.UUID)
        {
            return; 
        }*/

        avatar_ids = msg.avatar_ids;
        avatar_roles = msg.avatar_roles;
        master_peer_id = msg.master_peer_id;
        room_id = msg.room_id;
        join_code = msg.join_code;

        if (avatar_ids.Count() == 0 && avatar_roles.Count() == 0)
        {
            return; 
        }

        var avatars = avatar_manager.Avatars;

        if (avatars.Count() != avatar_roles.Count())
        {
            return;
        }

        foreach (var avatar in avatars)
        {
            avatar.color = avatar_roles[avatar_ids.IndexOf(avatar.Peer.UUID)];
        
            if (avatar.Peer.UUID == room_client.Me.UUID)
            {
                var prefab = avatar_manager.AvatarCatalogue.prefabs[roles.IndexOf(avatar.color)];

                room_client.Me["ubiq.avatar.prefab"] = prefab.name;
            }
        }
    }
}
