﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.XR;
using System;
using Ubiq.Rooms;

public class SubmissionArea : MonoBehaviour, INetworkComponent, INetworkObject
{
    public TMPro.TextMeshProUGUI scoreText;
    public static TMPro.TextMeshProUGUI scoreTextStatic;
    int submittedStructures = 0;
    List<string> alreadySubmitted = new List<string>();

    // Networking 
    private NetworkContext context;
    NetworkId INetworkObject.Id => new NetworkId("58e19a43a15f5f1a");

    struct Message
    {
        public NetworkId blockId;
        public int score;

        public Message(NetworkId blockId, int score)
        {
            this.blockId = blockId;
            this.score = score;
        }
    }

    void INetworkComponent.ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();
        NetworkId blockId = msg.blockId;
        foreach (Block b in GameManager.allBlocksStatic)
        {
            if (b.shared_id == blockId)
            {
                SubmitStructure(b.gameObject, msg.score);
                break;
            }
        }
    }

    public void SendMessageUpdate(Block block, int score)
    {
        Message message;
        message.blockId = block.shared_id;
        message.score = score;
        context.SendJson(message);
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = "$0";
        scoreTextStatic = scoreText;
        // Networking
        context = NetworkScene.Register(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        /// Check if its a root objects
        if (other.gameObject.transform.parent == null)
        {
            // if the structure isnt being held
            Block b = other.gameObject.GetComponent<Block>();
            if (b != null && b.grasped != null && !alreadySubmitted.Contains(other.gameObject.name))
            {
                b.Release();
                int score = SubmitStructure(b.gameObject);
                SendMessageUpdate(b, score);
            }
        }
    }

    public static void EndGame()
    {
        scoreTextStatic.fontSize = 0.3f;
        scoreTextStatic.text = "GAME OVER $" + (GameManager.score * 10).ToString();
    }

    private void SubmitStructure(GameObject other, int score)
    {
        

        // Update score
        GameManager.score += score;
        scoreText.text = "$" + (GameManager.score * 10).ToString();

        // Teleport object somewhere else
        other.transform.position = new Vector3(0, -9.5f, 28.75f);

        // Go to next level if enough structures have been submitted
        submittedStructures++;
        if (submittedStructures >= GameManager.currLevel)
        {
            submittedStructures = 0;
            GameManager.NextLevel();
        }

        alreadySubmitted.Add(other.name);
    }

    private int SubmitStructure(GameObject other)
    {
        // How many block were misplaced in the structure?
        int correctBlocks = 0;
        if (!other.name.Contains("Table"))
        {
            foreach (Transform child in other.transform)
            {
                HitBox hb = child.gameObject.GetComponent<HitBox>();
                if (hb != null)
                {
                    foreach (Transform hitboxChild in child)
                    {
                        Block block = hitboxChild.gameObject.GetComponent<Block>();
                        if (block != null && block.colourIdx == hb.correctColourIdx)
                            correctBlocks++;
                    }
                }
            }
        }
        else
        {
            correctBlocks = 2;
        }

        // Update score
        GameManager.score += correctBlocks;
        scoreText.text = "$" + (GameManager.score * 10).ToString();

        // Teleport object somewhere else
        other.transform.position = new Vector3(0, -9.5f, 28.75f);

        // Go to next level if enough structures have been submitted
        submittedStructures++;
        if (submittedStructures >= GameManager.currLevel)
        {
            submittedStructures = 0;
            GameManager.NextLevel();
        }

        alreadySubmitted.Add(other.name);

        return correctBlocks;
    }
}
