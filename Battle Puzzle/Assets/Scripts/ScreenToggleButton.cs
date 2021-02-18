using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenToggleButton : MonoBehaviour
{
    public static ScreenToggleButton instance;
    public GameObject textObj;
    private Text text;
    
    void Awake()
    {
        if(instance == null)
            instance = this;
        
        else if(instance != this)
            Destroy(gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        text = textObj.GetComponent<Text>();
        instance.ToggleText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //
    public void ToggleText()
    {
        if(MasterController.fullscreen)
            text.text = "Fullscreen";
        
        else
            text.text = "Windowed";
    }
}
