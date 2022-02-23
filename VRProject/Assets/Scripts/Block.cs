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

        public Message(Vector3 pos, Quaternion rot, NetworkId who)
        {
            this.who = who;
            this.position = pos;
            this.rotation = rot; 
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
        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        client = context.scene.GetComponentInChildren<RoomClient>();

        client.OnPeerAdded.AddListener(OnPeerAdded);

        shared_id = new NetworkId((uint)(Math.Pow(10, 5)*(transform.localPosition.x + 
                                        transform.localPosition.y + 
                                        transform.localPosition.z) ));
    }

    private void OnPeerAdded(IPeer peer)
    {
        if (peer.UUID == client.Me.UUID)
        {
            return; 
        }

        Message message;
        message.position = transform.localPosition;
        message.rotation = transform.rotation;
        message.who = shared_id;

        context.SendJson(message);
    }

    void IGraspable.Grasp(Hand controller)
    {
        grasped = controller;
        owner = true; 
    }

    void IGraspable.Release(Hand controller)
    {
        grasped = null;
        owner = false; 
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
            Message message;
            message.position = transform.localPosition;
            message.rotation = transform.rotation;
            message.who = shared_id; 

            context.SendJson(message);
        }
    }
}
