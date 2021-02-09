using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Behavior for the floating damage numbers//
//Move upwards a short distance and disappear//
public class DamageNumBehavior : MonoBehaviour
{
    private float timer = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += new Vector3(0f, 0.05f, 0f);
        timer -= Time.deltaTime;
        
        if(timer <= 0)
            Destroy(gameObject);
    }
}
