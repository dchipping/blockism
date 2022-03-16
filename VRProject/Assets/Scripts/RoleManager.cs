using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;


public class RoleManager : MonoBehaviour, INetworkComponent, INetworkObject
{
    public List<string> roles = new List<string> {"red", "yellow", "green", "blue"};

    private NetworkContext context;

    private RoomClient room_client;

    private Ubiq.Avatars.AvatarManager avatar_manager;

    private List<string> avatar_ids;

    private List<string> avatar_roles;

    private string master_peer_id;

    NetworkId INetworkObject.Id => new NetworkId("a15ca05dbb9ef8ec");

    struct Message
    {

        public List<string> avatar_ids;
        public List<string> avatar_roles;
        public string master_peer_id;

        public Message(List<string> ai, List<string> ar, string lo_id)
        {
            this.avatar_ids = ai;
            this.avatar_roles = ar;
            this.master_peer_id = lo_id;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        room_client = context.scene.GetComponentInChildren<RoomClient>();

        room_client.OnPeerAdded.AddListener(OnPeerAdded);

        room_client.OnJoinedRoom.AddListener(OnJoinedRoom);

        room_client.OnPeerRemoved.AddListener(OnPeerRemoved);

        avatar_manager = GameObject.Find("Avatar Manager").GetComponent<Ubiq.Avatars.AvatarManager>();
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
        master_peer_id = owner_avatar.Peer.UUID;
        owner_avatar.color = roles.First();

        avatar_ids = new List<string>();
        avatar_roles = new List<string>();

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

        AddAvatarAndRole(current_avatar);

        SendMessageUpdate();
    }

    private void OnPeerRemoved(IPeer peer)
    {
        // Remove peer and broadcast message only if master peer 
        if (room_client.Me.UUID != master_peer_id)
        {
            return;
        }

        // if current master peer is leaving, then select a new one
        if (peer.UUID == master_peer_id)
        {
            var avatars = avatar_manager.Avatars;

            foreach (var avatar in avatars)
            {
                if (avatar.Peer.UUID != master_peer_id)
                {
                    master_peer_id = avatar.Peer.UUID;

                    // remove leaving peer id from lists 
                    RemoveAvatarAndRole(peer);

                    SendMessageUpdate();

                    return; 
                }
            }
        } else
        {
            RemoveAvatarAndRole(peer);

            SendMessageUpdate();
        }
    }

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // do not update if GM same as last owner's  
        /*if (room_client.Me.UUID == msg.master_peer_id)
        {
            return;
        }*/

        avatar_ids = msg.avatar_ids;
        avatar_roles = msg.avatar_roles;
        master_peer_id = msg.master_peer_id;

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
