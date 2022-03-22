using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPRoomDoor : MonoBehaviour
{
    private List<string> colours;
    private List<int> colour_indexes;

    RoleManager role_manager;

    // Start is called before the first frame update
    public void Init()
    {
        MeshRenderer mesh_rend = gameObject.GetComponent<MeshRenderer>();
        mesh_rend.material = GameManager.blockColoursStatic[0];
        role_manager = GameObject.Find("RoleManager").GetComponent<RoleManager>();
        colours = role_manager.GetAvatarRoles();
        colour_indexes = role_manager.GetAvatarColourIndexes();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeColour(int index)
    {
        index = (index - 1) % GameManager.numOfPlayers;
        int col_idx = colour_indexes[index];
        MeshRenderer mesh_rend = gameObject.GetComponent<MeshRenderer>();
        mesh_rend.material = GameManager.blockColoursStatic[col_idx];

       /* if (index > 0)
        {
            int prev_col_idx = colour_indexes[index - 1];
            GameObject avatarNotAllowed = GameObject.FindGameObjectWithTag(colours[prev_col_idx]);
            Physics.IgnoreCollision(gameObject.GetComponent<BoxCollider>(), avatarNotAllowed.gameObject.GetComponent<BoxCollider>(), false);
        }*/

        GameObject avatarAllowed = GameObject.FindGameObjectWithTag(colours[col_idx]);
        Physics.IgnoreCollision(gameObject.GetComponent<BoxCollider>(), avatarAllowed.gameObject.GetComponent<BoxCollider>(), true);
    }
}