using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverSingleton : MonoBehaviour
{
    private GameObject contButton;
    
    public static GameOverSingleton instance;
    void Awake()
    {
        if(instance == null)
            instance = this;
            
        else if(instance != this)
            Destroy(gameObject);
    }
    
    public void Start()
    {
        contButton = gameObject.transform.Find("Continue").transform.Find("Button_Continue").gameObject;
        contButton.GetComponent<UnityEngine.UI.Button>().Select();
    }
    public void Enable()
    {
        gameObject.SetActive(true);
    }
}
