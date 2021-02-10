using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToHelp : MonoBehaviour
{
    private bool done;
    
    // Start is called before the first frame update
    void Start()
    {
        done = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("return"))
        {
            done = true;
        }
        
        if(done)
        {
            MasterController.instance.HelpButton();
        }
    }
}
