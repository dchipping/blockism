using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPRoomDoor : MonoBehaviour
{
    private string[] colours = { "red", "yellow", "blue", "green" };
    private List<int> colour_indexes;

    private RoleManager role_manager;
    private Ubiq.Avatars.AvatarManager avatar_manager;
    private Ubiq.Avatars.Avatar local_avatar = null;

    // Start is called before the first frame update
    public void Init()
    {
        role_manager = GameObject.Find("RoleManager").GetComponent<RoleManager>();
        avatar_manager = GameObject.Find("Avatar Manager").GetComponent<Ubiq.Avatars.AvatarManager>();
        colour_indexes = role_manager.GetAvatarColourIndexes();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeColour(int lvl)
    {
        int currIndex = (lvl - 1) % GameManager.numOfPlayers;
        int col_idx = colour_indexes[currIndex];
        MeshRenderer mesh_rend = gameObject.GetComponent<MeshRenderer>();
        mesh_rend.material = GameManager.blockColoursStatic[col_idx];

        var avatars = avatar_manager.Avatars;

        foreach (var avatar in avatars)
        {
            if (avatar.IsLocal)
            {
                local_avatar = avatar;
            }
        }

        if (GameManager.currLevel > 1)
        {
            int prev_col_idx = colour_indexes[(currIndex - 1 + GameManager.numOfPlayers) % GameManager.numOfPlayers];
            if (local_avatar.gameObject.tag == colours[prev_col_idx])
            {
                Physics.IgnoreCollision(GameObject.Find("Player").GetComponent<BoxCollider>(), gameObject.GetComponent<BoxCollider>(), false);
            }
        }

        if (local_avatar.gameObject.tag == colours[col_idx])
        {
            Physics.IgnoreCollision(GameObject.Find("Player").GetComponent<BoxCollider>(), gameObject.GetComponent<BoxCollider>(), true);
        }        
    }
}