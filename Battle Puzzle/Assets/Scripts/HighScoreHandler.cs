using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreHandler : MonoBehaviour
{
    int score;

    // Start is called before the first frame update
    void Start()
    {
        score = MasterController.instance.highScore;
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameController.instance.score > score)
        {
            score = GameController.instance.score;
            UpdateText();
        }
    }
    
    ///////////////////////////////
    //void UpdateText()          //
    //Updates the high score text//
    ///////////////////////////////
    private void UpdateText()
    {
        gameObject.GetComponent<Text>().text = "High Score: " + score.ToString();
    }
}
