using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void StartArcade()
    {
        MasterController.instance.StartArcade();
    }
    
    public void Selected(int choice)
    {
        MasterController.instance.Selected(choice);
    }
}
