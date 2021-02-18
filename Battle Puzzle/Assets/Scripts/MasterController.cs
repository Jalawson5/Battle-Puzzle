using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterController : MonoBehaviour
{
    public const string StatusWin = "Win!!";
    public const string StatusLose = "Lose...";
    public const string StatusReady = "Ready?";
    public const string StatusGo = "GO!!";

    public static MasterController instance;
    public static int PlayerCharacter;
    public static int OppCharacter;
    public static bool IsPlaying;
    public static bool IsMultiplayer;
    public static int Stage;
    public static int[] Opponents;
    public static bool GameOver;
    
    public static bool fullscreen;
    
    public static DataEntry Dat;
    
    public int playerScore;
    public int highScore;
    
    public static bool[] Clears;
    
    public GameObject ContinueCanvas;
    public GameObject MPCanvas;
    
    public GameObject block_atk;
    public GameObject block_mag;
    public GameObject block_sup;
    public GameObject block_con;
    public GameObject block_garbage;
    
    public GameObject sprite_atk;
    public GameObject sprite_mag;
    public GameObject sprite_sup;
    public GameObject sprite_con;
    
    public static GameObject[] Blocks;
    public static GameObject[] Sprites;
    
    void Awake()
    {
        PlayerCharacter = 0;
        
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
        Stage = -1;
        Opponents = new int[10];
        Blocks = new GameObject[]{instance.block_atk, instance.block_mag, instance.block_sup, instance.block_con, instance.block_garbage};
        Sprites = new GameObject[]{instance.sprite_atk, instance.sprite_mag, instance.sprite_sup, instance.sprite_con};
        playerScore = 0;
        Dat = new DataEntry();
        Clears = new bool[]{false, false, false, false, false, false, false};
        
        InitData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    ///////////////////////////////////////////////////////////////////////
    //void MatchOver(bool)                                               //
    //Handles the end of a match                                         //
    //Determines the winner and offers a rematch or starts the next stage//
    ///////////////////////////////////////////////////////////////////////
    public void MatchOver(bool player)
    {
        IsPlaying = false;
        if(!IsMultiplayer)
        {
            if(player)
            {
                StartCoroutine(PlayerLoss());
            }

            else
            {
                StartCoroutine(PlayerWin());
            }
        }
        
        else
        {
            StartCoroutine(MPOver(player));
        }
    }
    
    //////////////////////////////////////////////////////////////////////
    //IEnumerator PlayerLoss()                                          //
    //Handles a player loss, offering a rematch or to return to the menu//
    //////////////////////////////////////////////////////////////////////
    IEnumerator PlayerLoss()
    {
        GameOver = true;
        GameController.instance.SendStatus(StatusLose);
        EnemyController.instance.SendStatus(StatusWin);
        instance.playerScore = 0;
        yield return new WaitForSeconds(1);
        Instantiate(instance.ContinueCanvas, new Vector3(0, 0, 0), Quaternion.identity);
    }
    
    ///////////////////////////////////////////////////////////////////////////
    //IEnumerator PlayerWin()                                                //
    //Handles a player win, moving to the next stage or returning to the menu//
    ///////////////////////////////////////////////////////////////////////////
    IEnumerator PlayerWin()
    {
        GameController.instance.SendStatus(StatusWin);
        EnemyController.instance.SendStatus(StatusLose);
        
        if(!IsMultiplayer)
        {
            instance.playerScore = GameController.instance.score;
            if(instance.playerScore > instance.highScore)
                instance.highScore = instance.playerScore;
            Debug.Log("Highscore: " + instance.highScore);
        }
        
        yield return new WaitForSeconds(1);
        
        if(Stage != 9)
        {
            StartCoroutine(StartStage());
        }
        
        else
        {
            Stage = -1;
            Ending();
            ToMainMenu();
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////
    //IEnumerator StartStage()                                                      //
    //Starts the next stage by incrementing the stage number and reloading GameScene//
    //////////////////////////////////////////////////////////////////////////////////
    IEnumerator StartStage()
    {
        Stage++;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        yield return new WaitForSeconds(1);
        GameController.instance.SendStatus(StatusReady);
        EnemyController.instance.SendStatus(StatusReady);
        yield return new WaitForSeconds(1);
        GameController.instance.SendStatus(StatusGo);
        EnemyController.instance.SendStatus(StatusGo);
        yield return new WaitForSeconds(1);
        GameController.instance.SendStatus("");
        EnemyController.instance.SendStatus("");
        IsPlaying = true;
    }
    
    ///////////////////////////////////////////
    //void ContinueStage()                   //
    //repeats the current stage for a rematch//
    ///////////////////////////////////////////
    private void ContinueStage()
    {
        Stage--;
        StartCoroutine(StartStage());
    }
    
    ///////////////////////////////////////////////////
    //IEnumerator MPOver(bool)                       //
    //Handles the end of match for a multiplayer game//
    //Allows for a rematch or back to menu           //
    ///////////////////////////////////////////////////
    IEnumerator MPOver(bool player)
    {
        GameOver = true;
        
        if(player)
        {
            GameController.instance.SendStatus(StatusLose);
            EnemyController.instance.SendStatus(StatusWin);
        }
        
        else
        {
            GameController.instance.SendStatus(StatusWin);
            GameController.instance.SendStatus(StatusLose);
        }
        
        yield return new WaitForSeconds(1);        
        Instantiate(instance.MPCanvas, new Vector3(0, 0, 0), Quaternion.identity);
    }
    
    ///////////////////////////////////////////////////////////////////////////////////
    //void StartArcade()                                                             //
    //Starts arcade mode by setting the opponent sequence and loading CharacterSelect//
    ///////////////////////////////////////////////////////////////////////////////////
    public void StartArcade()
    {
        OpponentSequence();
        SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
    }
    
    ///////////////////////////////////////////////////
    //void StartMP()                                 //
    //Starts Versus mode by loading CharacterSelectMP//
    ///////////////////////////////////////////////////
    public void StartMP()
    {
        SceneManager.LoadScene("CharacterSelectMP", LoadSceneMode.Single);
        MusicController.instance.Stop();
    }
    
    ////////////////////////////////////////////////////////
    //void ToMainMenu()                                   //
    //resets the stage number and returns to the main menu//
    ////////////////////////////////////////////////////////
    private void ToMainMenu()
    {
        Stage = -1;
        MusicController.instance.Stop();
        SaveData();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    
    /////////////////////////////////////////////////
    //void Selected(int)                           //
    //Sets the player character and loads GameScene//
    /////////////////////////////////////////////////
    //0 = barbarian//
    //1 = archer   //
    //2 = rogue    //
    //3 = witch    //
    //4 = cleric   //
    //5 = monk     //
    //6 = druid    //
    /////////////////
    public void Selected(int choice)
    {
        PlayerCharacter = choice;
        IsMultiplayer = false;
        StartCoroutine(StartStage());
    }
    
    //////////////////////////////////////////////////////////////
    //void SelectedMP(int, int)                                 //
    //Sets the player and player2 characters and loads GameScene//
    //////////////////////////////////////////////////////////////
    public void SelectedMP(int p1, int p2)
    {
        PlayerCharacter = p1;
        OppCharacter = p2;
        IsMultiplayer = true;
        StartCoroutine(StartStage());
    }
    
    ////////////////////////////////////////////////
    //void OpponentSequence()                     //
    //Randomizes the opponents for arcade mode    //
    //Last three opponents will always be the same//
    ////////////////////////////////////////////////
    public void OpponentSequence()
    {
        Opponents[7] = -1;
        Opponents[8] = -2;
        Opponents[9] = -3;
        
        int[] temp = new int[]{0, 1, 2, 3, 4, 5, 6};
        temp = MathStuff.Shuffle(temp);
        
        for(int i = 0; i < temp.Length; i++)
        {
            Opponents[i] = temp[i];
        }
    }
    
    ///////////////////////////////
    //void QuitButton()          //
    //handler for the Quit button//
    //returns to the main menu   //
    ///////////////////////////////
    public void QuitButton()
    {
        ToMainMenu();
    }
    
    /////////////////////////////////////////
    //void ContinueButton()                //
    //Handler for the continue button      //
    //starts a rematch of the current stage//
    /////////////////////////////////////////
    public void ContinueButton()
    {
        GameOver = false;
        //SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        instance.ContinueStage();
    }
    
    ////////////////////////////////////
    //void CloseGame()                //
    //Handler for the Quit Menu button//
    //Quits the game                  //
    ////////////////////////////////////
    public void CloseGame()
    {
        Application.Quit();
    }
    
    ///////////////////////////////
    //void HelpButton()          //
    //Handler for the Help button//
    //opens the help menu        //
    ///////////////////////////////
    public void HelpButton()
    {
        SceneManager.LoadScene("HelpMenu");
    }
    
    ///////////////////////////////////////
    //void GeneralHelpButton()           //
    //Handler for the Instructions button//
    //opens the instructions menu        //
    ///////////////////////////////////////
    public void GeneralHelpButton()
    {
        SceneManager.LoadScene("GeneralHelp");
    }
    
    /////////////////////////////////////////
    //void CharHelpButton()                //
    //Handler for the Character menu button//
    //opens the character tutorial menu    //
    /////////////////////////////////////////
    public void CharHelpButton()
    {
        SceneManager.LoadScene("CharacterHelp");
    }
    
    ////////////////////////////////////////
    //void SettingsButton()               //
    //Handler for the Settings menu button//
    //opens the settings menu             //
    ////////////////////////////////////////
    public void SettingsButton()
    {
        SceneManager.LoadScene("SettingsMenu");
    }
    
    ////////////////////////////////////
    //void BackToMenuButton()         //
    //Handler for the Back menu button//
    //returns to the main menu        //
    ////////////////////////////////////
    public void BackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    ///////////////////////////
    //void Ending()          //
    //Handles the end of game//
    ///////////////////////////
    private void Ending()
    {
        Clears[PlayerCharacter] = true;
    }
    
    ///////////////////////////////
    //void InitData()            //
    //Initializes persistent data//
    ///////////////////////////////
    private void InitData()
    {
        if(Dat.LoadData())
        {
            highScore = Dat.score;
            Clears[0] = Dat.barbClear;
            Clears[1] = Dat.archerClear;
            Clears[2] = Dat.rogueClear;
            Clears[3] = Dat.witchClear;
            Clears[4] = Dat.clericClear;
            Clears[5] = Dat.monkClear;
            Clears[6] = Dat.druidClear;
        }
        else
        {
            highScore = 0;
        }
        
        if(PlayerPrefs.HasKey("full"))
        {
            int temp = PlayerPrefs.GetInt("full");
            
            if(temp == 1)
            {
                fullscreen = true;
                Screen.fullScreen = true;
            }
            
            else
            {
                fullscreen = false;
                Screen.fullScreen = false;
            }
        }
        
        else
            fullscreen = Screen.fullScreen;
    }
    
    /////////////////////////////////////
    //void SaveData()                  //
    //prepares DataEntry and saves data//
    /////////////////////////////////////
    private void SaveData()
    {
        //Prepare data//
        Dat.score = instance.highScore;
        Dat.barbClear = Clears[0];
        Dat.archerClear = Clears[1];
        Dat.rogueClear = Clears[2];
        Dat.witchClear = Clears[3];
        Dat.clericClear = Clears[4];
        Dat.monkClear = Clears[5];
        Dat.druidClear = Clears[6];
        
        //Save data//
        Dat.SaveData();
        
        if(fullscreen)
            PlayerPrefs.SetInt("full", 1);
        
        else
            PlayerPrefs.SetInt("full", 0);
            
        PlayerPrefs.Save();
    }
    
    ////////////////////////////////
    //void ToggleScreen()         //
    //toggles the fullscreen value//
    ////////////////////////////////
    public void ToggleScreen()
    {
        fullscreen = !fullscreen;
        ScreenToggleButton.instance.ToggleText();
    }
    
    /////////////////////////////////////////////
    //void UpdateSettings()                    //
    //updates values based on changed settings //
    //currently includes fullscreen vs windowed//
    /////////////////////////////////////////////
    public void UpdateSettings()
    {
        Screen.fullScreen = fullscreen;
    }
}
