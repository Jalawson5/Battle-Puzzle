using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController instance;
    
    public AudioClip ThemeBarb; //0
    public AudioClip ThemeArcher; //1
    public AudioClip ThemeRogue; //2
    public AudioClip ThemeWitch; //3
    public AudioClip ThemeCleric; //4
    public AudioClip ThemeMonk; //5
    public AudioClip ThemeDruid; //6
    public AudioClip ThemeMonster; //-1 & -2
    public AudioClip ThemeBoss; //-3
    private AudioSource current;
    
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
        current = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetTheme(int choice)
    {
        if(choice == 0)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeBarb;
            current.Play();
        }
        
        else if(choice == 1)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeArcher;
            current.Play();
        }
        
        else if(choice == 2)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeRogue;
            current.Play();
        }
        
        else if(choice == 3)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeWitch;
            current.Play();
        }
        
        else if(choice == 4)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeCleric;
            current.Play();
        }
        
        else if(choice == 5)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeMonk;
            current.Play();
        }
        
        else if(choice == 6)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeDruid;
            current.Play();
        }
        
        else if(choice == -3)
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeBoss;
            current.Play();
        }
        
        else
        {
            if(current.isPlaying)
                current.Stop();
            
            current.clip = ThemeMonster;
            current.Play();
        }
    }
    
    public void Stop()
    {
        if(current.isPlaying)
            current.Stop();
    }
}
