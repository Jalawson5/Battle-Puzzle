using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    private Transform healthBar;
    private SpriteRenderer sr;
    private bool isScaling;
    private float endScale;
    private float scaleTime = 0.2f;
    private float vel = 0f; //needed by SmoothDamp?//
    
    // Start is called before the first frame update
    void Start()
    {
        healthBar = transform.Find("Bar");
        sr = healthBar.GetComponent<SpriteRenderer>();
        isScaling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isScaling)
        {
            float newScale = Mathf.SmoothDamp(healthBar.localScale.x, endScale, ref vel, scaleTime);
            healthBar.localScale = new Vector3(newScale, 1f, 0);
            
            if(healthBar.localScale.x == endScale)
                isScaling = false;
        }
    }
    
    public void SetScale(float scale)
    {
        //healthBar.localScale = new Vector3(scale, 1f, 0);
        isScaling = true;
        endScale = scale;
    }
}
