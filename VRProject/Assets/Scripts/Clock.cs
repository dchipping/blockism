using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static float timeRemaining = 0;
    private TMPro.TextMeshProUGUI clockText;
    private static TMPro.TextMeshProUGUI clockTextStatic;
    private static bool endGame = false;

    // Start is called before the first frame update
    void Start()
    {
        clockText = GetComponent<TMPro.TextMeshProUGUI>();
        clockTextStatic = clockText;
    }

    void Update()
    {
        if (!endGame)
        {
            if (timeRemaining > 0.1)
            {
                timeRemaining -= Time.deltaTime;
                int mins = (int)Math.Floor(timeRemaining / 60f);
                int secs = ((int)Math.Floor(timeRemaining)) % 60;

                if (secs < 10)
                    clockText.text = mins.ToString() + ":0" + secs.ToString();
                else
                    clockText.text = mins.ToString() + ":" + secs.ToString();
            }
            else
            {
                clockText.text = "0:00";
                if (GameManager.currLevel > 0 && GameManager.currLevel < 4)
                    GameManager.NextLevel();
            }
        }
    }

    public static void ResetClock()
    {
        timeRemaining = 180;
    }

    public static void EndGame()
    {
        timeRemaining = -1;
        endGame = true;
        clockTextStatic.fontSize = 0.3f;
        clockTextStatic.text = "GAME OVER $" + (GameManager.score * 10).ToString();
    }
}
