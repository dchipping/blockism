using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ubiq.Samples
{
    [CustomEditor(typeof(TabletopSpawner))]
    public class SampleSimpleTabletopSpawnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Spawn"))
            {
                (target as TabletopSpawner).SpawnTabletop();
            }
        }
    }
}