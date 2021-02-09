using System.Collections;
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
    
    public int playerScore;
    
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
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
    
    IEnumerator PlayerLoss()
    {
        GameOver = true;
        GameController.instance.SendStatus(StatusLose);
        EnemyController.instance.SendStatus(StatusWin);
        instance.playerScore = 0;
        yield return new WaitForSeconds(1);
        Instantiate(instance.ContinueCanvas, new Vector3(0, 0, 0), Quaternion.identity);
    }
    
    IEnumerator PlayerWin()
    {
        GameController.instance.SendStatus(StatusWin);
        EnemyController.instance.SendStatus(StatusLose);
        
        if(!IsMultiplayer)
            instance.playerScore = GameController.instance.score;
            
        yield return new WaitForSeconds(1);
        
        if(Stage != 9)
        {
            StartCoroutine(StartStage());
        }
        
        else
        {
            Stage = -1;
            ToMainMenu();
        }
    }
    
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
    
    private void ContinueStage()
    {
        Stage--;
        StartCoroutine(StartStage());
    }
    
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
    
    public void StartArcade()
    {
        OpponentSequence();
        SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
    }
    
    public void StartMP()
    {
        SceneManager.LoadScene("CharacterSelectMP", LoadSceneMode.Single);
        MusicController.instance.Stop();
    }
    
    private void ToMainMenu()
    {
        Stage = -1;
        MusicController.instance.Stop();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    
    /////////////////
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
    
    public void SelectedMP(int p1, int p2)
    {
        PlayerCharacter = p1;
        OppCharacter = p2;
        IsMultiplayer = true;
        StartCoroutine(StartStage());
    }
    
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
    
    public void QuitButton()
    {
        ToMainMenu();
    }
    
    public void ContinueButton()
    {
        GameOver = false;
        //SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        instance.ContinueStage();
    }
    
    public void CloseGame()
    {
        Application.Quit();
    }
    
    public void HelpButton()
    {
        SceneManager.LoadScene("HelpMenu");
    }
    
    public void CharHelpButton()
    {
        SceneManager.LoadScene("CharacterHelp");
    }
    
    public void BackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
