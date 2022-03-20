using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public List<GameObject> onBelt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<string> alreadyUpdated = new List<string>();
        for (int i = 0; i < onBelt.Count; i++)
        {
            if (!alreadyUpdated.Contains(onBelt[i].name))
            {
                onBelt[i].transform.position += (speed * direction * Time.deltaTime);
                alreadyUpdated.Add(onBelt[i].name);
            }
        }
    }

    // When something collides with belt
    private void OnCollisionEnter(Collision collision)
    {
        onBelt.Add(collision.gameObject);
    }

    // When something leaves belt
    private void OnCollisionExit(Collision collision)
    {
        onBelt.Remove(collision.gameObject);
    }
}
