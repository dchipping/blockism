using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<AudioClip> clickSound;
    private static List<AudioClip> clickSoundsStatic;

    // Start is called before the first frame update
    void Start()
    {
        clickSoundsStatic = clickSound;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void StartGame()
    {

    }

    public static void PlayClickFromPoint(Vector3 position)
    {
        // Get random audio clip
        var random = new System.Random();
        int index = random.Next(clickSoundsStatic.Count-1);

        // Play clicking sound
        AudioSource.PlayClipAtPoint(clickSoundsStatic[index], position);
    }
}
