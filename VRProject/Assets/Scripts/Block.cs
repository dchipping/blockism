using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.XR;
using System;

public class Block : MonoBehaviour, IGraspable, INetworkComponent, INetworkObject
{

    public GameObject structureObject;

    public Hand grasped;


    NetworkId INetworkObject.Id => new NetworkId(1001);
    private NetworkContext context;

    struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();
        transform.position = msg.position;
        transform.rotation = msg.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
    }

    void IGraspable.Grasp(Hand controller)
    {
        grasped = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        grasped = null;
    }

    // Update is called once per frame
    void Update()
    {
        // If the block is held by a player
        if (grasped)
        {
            if (structureObject == null)
            {
                // Match the position and orientation of the hand
                transform.position = grasped.transform.position;
                transform.rotation = grasped.transform.rotation;
            }
            else
            {
                // Match the position and orientation of the hand
                structureObject.transform.position = grasped.transform.position;
                structureObject.transform.rotation = grasped.transform.rotation;
            }

            // Networking code
            Message message;
            message.position = transform.position;
            message.rotation = transform.rotation;
            context.SendJson(message);
        }
    }
}
