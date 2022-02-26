using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{

    public List<GameObject> children;

    public void RemoveGrasp()
    {
        foreach (GameObject go in children)
        {
            Block block = go.GetComponent<Block>();
            block.grasped = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
