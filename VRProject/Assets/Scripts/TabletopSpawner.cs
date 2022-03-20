using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;

namespace Ubiq.Samples
{
    public class TabletopSpawner : MonoBehaviour
    {
        public PrefabCatalogue catalogue;
        public System.Random rnd = new System.Random();

        void Start()
        {
            //InvokeRepeating("SpawnTabletop", 5f, 5f);
        }


        public void SpawnTabletop()
        {
            //int idx = rnd.Next(0, catalogue.prefabs.Count);
            //NetworkSpawner.Spawn(this, catalogue.prefabs[idx]);
        }
    }
}