using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController instance;

    public const int SoundDestroy = 0;
    public const int SoundPlace = 1;

    public AudioClip blockDestroy;
    public AudioClip blockPlace;
    
    private AudioSource source;
    
    void Awake()
    {        
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        
        else if(instance != this)
            Destroy(gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /////////////////////////////////////////////////////////////////////////////////////////
    //void PlaySound(int)                                                                  //
    //plays the specified sound, using the SoundController constants to determine the sound//
    //see constants above for sounds                                                       //
    /////////////////////////////////////////////////////////////////////////////////////////
    public void PlaySound(int sound)
    {
        switch(sound)
        {
            case SoundDestroy:
                source.PlayOneShot(blockDestroy);
                break;
            case SoundPlace:
                source.PlayOneShot(blockPlace);
                break;
            default:
                Debug.Log("Invalid Sound code: " + sound);
                break;
        }
    }
}
