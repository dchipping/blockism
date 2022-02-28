using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.XR;
using System;
using Ubiq.Rooms;

public class Block : MonoBehaviour, IGraspable, INetworkComponent, INetworkObject
{

    private Hand grasped;

    private NetworkContext context;

    // 16-digit hex
    NetworkId INetworkObject.Id => new NetworkId("f8cdefa3a15f5e6d");

    public NetworkId shared_id; 

    private RoomClient client;

    private bool owner = false; 

    struct Message
    {
        public NetworkId who; 
        public Vector3 position;
        public Quaternion rotation;
        public bool owner; 

        public Message(Vector3 pos, Quaternion rot, NetworkId who, bool owner)
        {
            this.who = who;
            this.position = pos;
            this.rotation = rot;
            this.owner = owner; 
        }
    }

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // update position only if
        // the message comes from an object of the same shared ID
        if (msg.who == shared_id)
        {
            transform.localPosition = msg.position;
            transform.rotation = msg.rotation;
            owner = msg.owner; 
        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        client = context.scene.GetComponentInChildren<RoomClient>();

        client.OnPeerAdded.AddListener(OnPeerAdded);

        shared_id = new NetworkId((uint)(Math.Pow(10, 3)*(transform.localPosition.x + 
                                        transform.localPosition.y + 
                                        transform.localPosition.z) ));
    }

    private void SendMessageUpdate()
    {
        Message message;
        message.position = transform.localPosition;
        message.rotation = transform.rotation;
        message.who = shared_id;
        message.owner = owner;

        context.SendJson(message);
    }

    private void OnPeerAdded(IPeer peer)
    {
        Debug.Log("peer added ID : " + peer.UUID);

        Debug.Log("client peer ID : " + client.Me.UUID);

        if (peer.UUID == client.Me.UUID)
        {
            return; 
        }

        SendMessageUpdate();
    }

    void IGraspable.Grasp(Hand controller)
    {
        if (owner)
        {
            return; 
        }

        grasped = controller;
        owner = true;

        SendMessageUpdate();
    }

    void IGraspable.Release(Hand controller)
    {
        grasped = null;
        owner = false;

        SendMessageUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        // If the block is held by a player
        if (grasped && owner)
        {
            // Match the position and orientation of the hand
            transform.localPosition = grasped.transform.position;
            transform.rotation = grasped.transform.rotation;

            // Networking code
            SendMessageUpdate();
        }
    }
}
