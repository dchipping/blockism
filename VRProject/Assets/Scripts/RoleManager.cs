using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;


public class RoleManager : MonoBehaviour, INetworkComponent, INetworkObject
{
    private List<string> roles = new List<string> { "red", "yellow", "blue", "green" };

    private NetworkContext context;

    private RoomClient room_client;

    private Ubiq.Avatars.AvatarManager avatar_manager;

    // set of variables for messages
    private List<string> avatar_ids;
    private List<string> avatar_roles;
    private string master_peer_id;
    // set of variables for messages

    NetworkId INetworkObject.Id => new NetworkId("a15ca05dbb9ef8ec");

    private string room_id;

    private static System.Random rng;

    struct Message
    {
        public List<string> avatar_ids;
        public List<string> avatar_roles;
        public string master_peer_id;
        public string room_id;

        public Message(List<string> ai, List<string> ar, string mpi, string rid)
        {
            this.avatar_ids = ai;
            this.avatar_roles = ar;
            this.master_peer_id = mpi;
            this.room_id = rid;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rng = new System.Random();

        context = NetworkScene.Register(this);

        room_client = context.scene.GetComponentInChildren<RoomClient>();

        room_client.OnPeerAdded.AddListener(OnPeerAdded);

        room_client.OnJoinedRoom.AddListener(OnJoinedRoom);

        /*room_client.OnRoomUpdated.AddListener(OnRoomUpdated);*/

        /*room_client.OnRoomsDiscovered.AddListener(OnRoomsDiscovered);*/

        avatar_manager = GameObject.Find("Avatar Manager").GetComponent<Ubiq.Avatars.AvatarManager>();
    }

    void Update()
    {
        /*if (Time.realtimeSinceStartup > next_room_refresh_time)
        {
            next_room_refresh_time = Time.realtimeSinceStartup + room_refresh_rate;

            room_client.DiscoverRooms();

            discover_rooms_count++;

            if (discover_rooms_count == 2 && string.IsNullOrEmpty(room_client.Room.Name))
            {
                room_client.Join(name: room_name, publish: true);
            }
        }*/

        // check if master peer has left and pick a new one 
        if (room_client.Room.UUID == room_id && !string.IsNullOrEmpty(master_peer_id))
        {
            var avatars = avatar_manager.Avatars;
            bool found_master = false;

            foreach (var avatar in avatars)
            {
                if (avatar.Peer.UUID == master_peer_id)
                {
                    found_master = true;
                    break;
                }
            }

            if (!found_master)
            {
                RemoveAvatarAndRole(master_peer_id);

                // select self as master peer 
                master_peer_id = avatars.First().Peer.UUID;

                SendMessageUpdate();
            }
        }

        // check if the a peer has left and update lists if they have (only for master peer)
        if ((room_client.Room.UUID == room_id) && (room_client.Me.UUID == master_peer_id))
        {
            var avatars = avatar_manager.Avatars;

            foreach (var id in avatar_ids)
            {
                bool id_found = false;

                foreach (var avatar in avatars)
                {
                    if (avatar.Peer.UUID == id)
                    {
                        id_found = true;
                        break;
                    }
                }

                if (!id_found)
                {
                    RemoveAvatarAndRole(id);
                }
            }

            SendMessageUpdate();
        }
    }

    /*private void OnRoomsDiscovered(List<IRoom> rooms, RoomsDiscoveredRequest request)
    {
        if (string.IsNullOrEmpty(request.joincode))
        {
            foreach (var room in rooms)
            {
                if (room.Name == room_name)
                {
                    room_client.Join(room.JoinCode);
                }
            }
        }
    }*/

    private void AddAvatarAndRole(string avatar_id, string color)
    {
        if (!avatar_ids.Contains(avatar_id))
        {
            avatar_ids.Add(avatar_id);
            avatar_roles.Add(color);
        }
        else
        {
            avatar_roles[avatar_ids.IndexOf(avatar_id)] = color;
        }
    }

    private void RemoveAvatarAndRole(string id)
    {
        int peer_index = avatar_ids.IndexOf(id);
        avatar_roles.RemoveAt(peer_index);
        avatar_ids.RemoveAt(peer_index);
    }

    // randomly shuffle the roles of the players 
    public void ShuffleRoles()
    {
        // only master peer can change roles and send message updates 
        if (room_client.Me.UUID != master_peer_id)
        {
            return;
        }

        var avatars = avatar_manager.Avatars;
        var role_check = avatar_roles;
        int no_of_avatars = avatars.Count();

        // shuffle elements in avatar roles 
        avatar_roles.OrderBy(role => rng.Next()).ToList();

        var local_avatar = avatar_manager.LocalAvatar;

        var prefab = avatar_manager.AvatarCatalogue.prefabs[roles.IndexOf(local_avatar.color)];

        room_client.Me["ubiq.avatar.prefab"] = prefab.name;

        SendMessageUpdate();
    }

    private void SendMessageUpdate()
    {
        Message message;
        message.avatar_ids = avatar_ids;
        message.avatar_roles = avatar_roles;
        message.master_peer_id = master_peer_id;
        message.room_id = room_id;

        context.SendJson(message);
    }

    private void OnJoinedRoom(IRoom room)
    {
        var avatars = avatar_manager.Avatars;

        // room with more than 1 avatar already has master peer
        if (avatars.Count() > 1)
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
        var owner_avatar = avatars.First();
        master_peer_id = owner_avatar.Peer.UUID;
        owner_avatar.color = roles.First();

        var prefab = avatar_manager.AvatarCatalogue.prefabs[roles.IndexOf(owner_avatar.color)];

        room_client.Me["ubiq.avatar.prefab"] = prefab.name;

        avatar_ids = new List<string>();
        avatar_roles = new List<string>();

        avatar_ids.Add(owner_avatar.Peer.UUID);
        avatar_roles.Add(owner_avatar.color);
        room_id = room.UUID;

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
                AddAvatarAndRole(avatar.Peer.UUID, avatar.color);
            }
        }

        // choose role with min count as current avatar's role 
        var new_role = role_count.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

        // update lists 
        AddAvatarAndRole(peer.UUID, new_role);

        SendMessageUpdate();
    }

    /*private void OnRoomUpdated(IRoom room)
    {
        // peer is leaving room if room name, uuid and join code are null 
        if (string.IsNullOrEmpty(room.JoinCode) 
                && string.IsNullOrEmpty(room.Name) 
                && string.IsNullOrEmpty(room.UUID))
        {
            // if peer used to be master, pick a new master and send a messge update 
            if (master_peer_id == room_client.Me.UUID)
            {
                // remove ourselves from lists
                RemoveAvatarAndRole(room_client.Me.UUID);

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
            master_peer_id = "";
        }

    }*/

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // utilize message update only if not master peer 
        if (msg.master_peer_id == room_client.Me.UUID)
        {
            return;
        }

        // unload message updates to client variables 
        avatar_ids = msg.avatar_ids;
        avatar_roles = msg.avatar_roles;
        master_peer_id = msg.master_peer_id;
        room_id = msg.room_id;

        // change prefab if at the client of an avatar
        foreach (var avatar in avatar_manager.Avatars)
        {
            var avatar_index = avatar_ids.IndexOf(avatar.Peer.UUID);

            if (avatar_index == -1)
            {
                continue;
            }

            if (avatar.Peer.UUID == room_client.Me.UUID)
            {
                // get the index of the role that was picked out for this avatar 
                var avatar_role_index = roles.IndexOf(avatar_roles[avatar_index]);
                // use the above index to pick out the correct prefab 
                var prefab = avatar_manager.AvatarCatalogue.prefabs[avatar_role_index];

                // set the room client peer's prefab name and
                // avatar manager will update the prefab on all clients
                room_client.Me["ubiq.avatar.prefab"] = prefab.name;
            }
        }
    }

    public List<int> GetAvatarColourIndexes()
    {
        List<int> avatar_roles_indexs = new List<int>();
        foreach (string avatar_role in avatar_roles)
            avatar_roles_indexs.Add(roles.IndexOf(avatar_role));
        return avatar_roles_indexs;
    }

    public List<string> GetAvatarRoles()
    {
        return avatar_roles;
    }
}
