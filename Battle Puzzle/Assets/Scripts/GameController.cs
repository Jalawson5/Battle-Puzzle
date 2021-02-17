using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//This script is an awkward combination between the player script and an assistant to the MasterController.//
//Controls player 1's board and communicates with the MasterController to control the game state.          //
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
public class GameController : MonoBehaviour
{
    public static GameController instance;
    
    public const int board_w = 7 ; //Width of the board (subject to change)//
    public const int board_h = 10; //Height of the board (subject to change)//
    
    public static bool isShuttingDown = false;
    
    private int maxHealth; //Currently selected character's max health//
    private int currHealth; //Current health. Game is over if currHealth reaches 0//
    private int currChar; //Currently selected character (see Character Data for list of characters//
    private int x_correct; //convert block.x to board.x//
    private int y_correct; //convert block.y to board.y//
    private bool matching; //true if board is processing matches//
    private bool canSpawn; //true if board is ready for another block//
    private bool hasCombo; //true if block placed resulted in a combo, ignores comboHandler if false//
    private bool clearCombo; //true if combo is finished, start timer to clear combo text//
    private int totalCombo; //number of blocks in current combo, used for status text//
    private bool handlingGarbage; //true if the controller is currently handling garbage//
    private bool hasDropped; //true if the controller has dropped garbage blocks this cycle//
    private float timer; //timer for spawning a new block//
    private float comboTimer; //timer for clearing combo text//
    private int numFalling; //counter keeping track of how many blocks are currently falling//
    private int garbage; //number of garbage blocks//
    private int garbageIn; //buffer containing new garbage blocks, used to prevent race conditions//
    private int garbageType; //0 = drop, 1 = convert physical, 2 = convert magical//
    private int nextBlock; //next block to be dropped//
    private int consecutive;
    private GameObject nextSprite; //sprite prefab showing the next block to be dropped//
    
    private int shield; //current shield value. Only used if current character has the shield support type//
    private float leech; //current leech multiplier. Only used if current character has the leech support type//
    
    public GameObject healthBar; //pointer to this player's health bar//
    public GameObject charText; //pointer to this player's character name text//
    public GameObject stageText; //pointer to the stage counter text//
    public GameObject damageLoc; //pointer to a placeholder object for creating damage text//
    public GameObject damageText; //prefab for Damage Text, to be instantiated in DamageTextHandler()//
    public GameObject comboText; //pointer to this player's combo text//
    public GameObject statusText; //pointer to this player's status text//
    public GameObject shieldEffect; //shield particle effect prefab//
    private GameObject activeShield; //pointer to the current shield object//
    public GameObject scoreText; //pointer to this player's score text//
    
    public int score; //current score//
    
    GameObject[,] board; //array representing this player's board//
    private int[] combo; //array representing combo count: atk, mag, sup, con//
    private int[] currStats; //array holding the currently chosen character's stats//
    
    private Stack matchStack; //Stack containing any blocks involved in the current match//
    private Stack fallStack; //Stack containing any blocks that are currently falling//
    private Stack garbageStack; //Stack containing garbage blocks that need to be cleared//
    
    void OnApplicationQuit()
    {
        isShuttingDown = true;
    }
    
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
        board = new GameObject[board_w, board_h];
        
        matchStack = new Stack();
        fallStack = new Stack();
        garbageStack = new Stack();
        
        combo = new int[]{0, 0, 0, 0};
        
        x_correct = 3;
        y_correct = 4;
        
        matching = false;
        canSpawn = true;
        handlingGarbage = false;
        hasDropped = false;
        
        timer = 0.35f;
        
        numFalling = 0;
        garbage = 0;
        garbageIn = 0;
        
        nextBlock = -1;
        
        shield = 0;
        leech = 0;
        
        currChar = MasterController.PlayerCharacter;
        currStats = CharacterData.GetStats(currChar);
        
        maxHealth = CharacterData.InitializeHealth(currStats[4]);
        currHealth = maxHealth;
        
        charText.GetComponent<Text>().text = CharacterData.GetName(currChar);
        stageText.GetComponent<Text>().text = ("Stage\n" + (MasterController.Stage + 1));
        
        score = MasterController.instance.playerScore;
        consecutive = 0;
        StartCoroutine(ScoreTracker());
        
        //MasterController.IsPlaying = true;
    }

    // Update is called once per frame
    void Update()
    {   
        if(currHealth == 0 && !matching && MasterController.IsPlaying)
        {
            MasterController.instance.MatchOver(true);
        }
        
        else if(MasterController.IsPlaying)
        {
            CheckOverflow();

            if(garbage > 0 && canSpawn && !handlingGarbage && !hasDropped)
            {
                handlingGarbage = true;
                StartCoroutine(GarbageHandler());
                //canSpawn = true;
                handlingGarbage = false;
                hasDropped = true;
            }

            if(garbageIn > 0)
            {
                //probably unnecessary, to prevent race conditions//
                int temp = garbageIn;
                garbage += temp;
                garbageIn -= temp;
            }

            if(matching || !canSpawn)
                timer = 0.35f;
            
            if(clearCombo)
            {
                comboTimer -= Time.deltaTime;
                
                if(comboTimer <= 0)
                {
                    ClearComboText();
                    clearCombo = false;
                }
            }
            
            else if(canSpawn && !handlingGarbage)
            {
                timer -= Time.deltaTime;
                if(timer <= 0)
                {
                    System.Random rand = new System.Random();
                    canSpawn = false;
                    
                    int choice;
                    
                    if(nextBlock == -1)
                        choice = rand.Next(4);
                        
                    else
                        choice = nextBlock;
                    
                    nextBlock = rand.Next(4);
                    
                    GameObject block;
                    
                    block = Instantiate(MasterController.Blocks[choice], new Vector3(0, 5, 0), Quaternion.identity);
                    block.GetComponent<BlockBehavior>().playerBlock = true;
                    
                    if(nextSprite != null)
                        Destroy(nextSprite);
                    
                    nextSprite = Instantiate(MasterController.Sprites[nextBlock], new Vector3(5, 0, 0), Quaternion.identity);
                    
                    hasDropped = false;
                }
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////
    //void notify(GameObject)                                        //
    //message receiver for blocks in this player's board.            //
    //calls the BlockHandler to handle the object that called Notify.//
    ///////////////////////////////////////////////////////////////////
    public void notify(GameObject caller)
    {
        StartCoroutine(BlockHandler(caller));
    }
    /////////////////////////////////////////////////////////////////////////////
    //IEnumerator BlockHandler(GameObject)                                     //
    //handles the object that called Notify.                                   //
    //places the block in the board array, handles matching, and handles falls.//
    /////////////////////////////////////////////////////////////////////////////
    IEnumerator BlockHandler(GameObject caller)
    {
        int new_x = (int)(caller.transform.position.x + x_correct);
        int new_y = (int)(caller.transform.position.y + y_correct);
        
        board[new_x, new_y] = caller;
        
        matching = true;
        Match(new_x, new_y);
        
        while(matching)
        {
            if(hasCombo)
                yield return new WaitForSeconds(0.5f);
                
            yield return CheckFalls();
            
            if(fallStack.Count == 0)
            {
                matching = false;
                break;
            }
            
            while(fallStack.Count > 0)
            {
                GameObject fallen = (GameObject)fallStack.Pop();
                
                if(fallen == null || fallen.tag.Equals("Block_Dead"))
                    continue; //if current block does not exist (possible if block was matched after falling), skip and continue//
                    
                new_x = (int)fallen.transform.position.x + x_correct;
                new_y = (int)fallen.transform.position.y + y_correct;
                
                if(board[new_x, new_y] != null)                    
                    Match(new_x, new_y);
            }
        }
        
        if(hasCombo)
        {
            consecutive++;
            ComboHandler();
            hasCombo = false;
            totalCombo = 0;
        }
        
        else
            consecutive = 0;
        
        matchStack.Clear();
        fallStack.Clear();
        canSpawn = true;
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////
    //void match(int, int)                                                                //
    //accepts board coordinates and recursively checks all neighboring blocks for matches.//
    ////////////////////////////////////////////////////////////////////////////////////////
    void Match(int x, int y)
    {
        //Copy board to temp board//
        GameObject[,] temp = new GameObject[board_w, board_h];
        
        for(int i = 0; i < board_w; i++)
            for(int k = 0; k < board_h; k++)
                temp[i,k] = board[i, k];
                
        string type = temp[x, y].tag;
        int pos; //represents index of combo array for the current block type//
        if(type.Equals("Block_Atk"))
            pos = 0;
        else if(type.Equals("Block_Mag"))
            pos = 1;
        else if(type.Equals("Block_Sup"))
            pos = 2;
        else
            pos = 3;
            
        RecurMatch(x, y, type, temp);
        
        if(matchStack.Count >= 4)
        {
            while(matchStack.Count > 0)
            {
                GameObject curr = (GameObject)matchStack.Pop();
                int old_x = (int) curr.transform.position.x + x_correct;
                int old_y = (int) curr.transform.position.y + y_correct;
                board[old_x, old_y] = null;
                combo[pos]++;
                totalCombo++;
                Destroy(curr.gameObject);
                
                hasCombo = true;
            }
            
            while(garbageStack.Count > 0)
            {
                GameObject curr = (GameObject)garbageStack.Pop();
                int old_x = (int) curr.transform.position.x + x_correct;
                int old_y = (int) curr.transform.position.y + y_correct;
                board[old_x, old_y] = null;
                Destroy(curr.gameObject);
            }
            
            SoundController.instance.PlaySound(SoundController.SoundDestroy);
        }
        
        if(totalCombo > 4)
            SetComboText();
            
        matchStack.Clear();
        garbageStack.Clear();
    }
    
    ////////////////////////////////////////////////////////////////
    //void SetComboText()                                         //
    //sets this player's combo text according to the current combo//
    //only combos of more than 4 blocks will show as a combo      //
    ////////////////////////////////////////////////////////////////
    void SetComboText()
    {
        Text statText = comboText.GetComponent<Text>();
        
        statText.text = "Combo!\n" + totalCombo + " Blocks";
    }
    
    //////////////////////////////////////////////////
    //void ClearComboText()                         //
    //clears this player's combo text (empty string)//
    //////////////////////////////////////////////////
    void ClearComboText()
    {
        Text statText = comboText.GetComponent<Text>();
        
        statText.text = "";
    }
    
    //////////////////////////////////////////////////////////////////////////
    //void RecurMatch(int, int, string, GameObject)                         //
    //recursively checks all neighboring blocks for blocks of the same type.//
    //////////////////////////////////////////////////////////////////////////
    void RecurMatch(int x, int y, string type, GameObject[,] copy)
    {
        GameObject curr = copy[x, y];
        copy[x, y] = null;
        
        if(curr.tag.Equals(type))
        {
            matchStack.Push(curr);
            
            if(x != 0 && copy[x-1, y] != null)
                RecurMatch(x - 1, y, type, copy);
            
            if(x != board_w - 1 && copy[x+1, y] != null)
                RecurMatch(x + 1, y, type, copy);
                
            if(y != 0 && copy[x, y-1] != null)
                RecurMatch(x, y - 1, type, copy);
                
            if(y != board_h - 1 && copy[x, y+1] != null)
                RecurMatch(x, y + 1, type, copy);
        }
        
        else if(curr.tag.Equals("Block_Dead"))
        {
            garbageStack.Push(curr);
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////////
    //IEnumerator CheckFalls()                                                      //
    //Scans the board for blocks that must fall due to any cleared blocks.          //
    //Forces the appropriate blocks to fall, returning when all blocks have settled.//
    //////////////////////////////////////////////////////////////////////////////////
    IEnumerator CheckFalls()
    {
        for(int i = 0; i < board_h; i++)
        {
            for(int k = 0; k < board_w; k++)
            {
                if(board[k, i] == null)
                    continue;
                    
                GameObject fallen = board[k, i];
                bool isFalling = fallen.GetComponent<BlockBehavior>().fall();
                
                if(isFalling)
                {
                    numFalling++;
                    fallStack.Push(fallen);
                    board[k, i] = null;
                    //Debug.Log("numFalling increased to " + numFalling);
                }
            }
            yield return null;
        }
        
        while(numFalling > 0)
        {
            yield return null; //wait until next update//
        }
        
        Stack tempStack = new Stack();
        while(fallStack.Count > 0)
        {
            GameObject tempObj = (GameObject)fallStack.Pop();
            tempStack.Push(tempObj); //Null reference here when rogue drops blocks while opponent overflows, looking into it//
            
            int new_x = (int) tempObj.transform.position.x + x_correct;
            int new_y = (int) tempObj.transform.position.y + y_correct;
            
            board[new_x, new_y] = tempObj;
        }
        
        fallStack = tempStack;
    }
    
    ///////////////////////////////////////////////////////////////////
    //void StopFalling(GameObject)                                   //
    //Message receiver for blocks that settle in this player's board.//
    //decrements numFalling for every block settles.                 //
    ///////////////////////////////////////////////////////////////////
    public void StopFalling(GameObject caller)
    {
        if(fallStack.Contains(caller))
        {
            numFalling--;
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////
    //void ComboHandler()                                                                     //
    //Handles the results of the matches for the previous drop.                               //
    //Calculates the damage dealt, healing done or damage reduced, and garbage blocks to send.//
    ////////////////////////////////////////////////////////////////////////////////////////////
    void ComboHandler()
    {
        int[] comboVals = new int[4];
        float tempLeech = 0;
        int numBlocks = 0;
        int numTypes = 0;
        //Not the cleanest way to do this, I know. I'm having a brainfart today.//
        for(int i = 0; i < combo.Length; i++)
        {
            if(combo[i] == 0)
                continue;
                
            int baseVal = 0;
            if(i == 0 || i == 1)
            {
                if(combo[i] < 8)
                    baseVal = CharacterData.BaseDamageLow;
                
                else if(combo[i] < 12)
                    baseVal = CharacterData.BaseDamageMid;
                
                else
                    baseVal = CharacterData.BaseDamageHigh;
                    
                comboVals[i] = (int)System.Math.Ceiling((baseVal + (combo[i] - 4.0) * 50) * (((currStats[i] - 1.0) * 0.1) + 1));
            }
            
            else if(i == 2)
            {
                if(currStats[5] == 0 || currStats[5] == 1)
                {
                    if(combo[i] < 8)
                        baseVal = CharacterData.BaseHealLow;
                
                    else if(combo[i] < 12)
                        baseVal = CharacterData.BaseHealMid;
                
                    else
                        baseVal = CharacterData.BaseHealHigh;
                    
                    comboVals[i] = (int)System.Math.Ceiling((baseVal + (combo[i] - 4.0) * 50) * (((currStats[i] - 1.0) * 0.1) + 1));
                }
                
                else
                {
                    float baseLeech ;
                    
                    if(combo[i] < 8)
                        baseLeech = CharacterData.BaseLeechLow;
                    
                    else if(combo[i] < 12)
                        baseLeech = CharacterData.BaseLeechMid;
                        
                    else
                        baseLeech = CharacterData.BaseLeechHigh;
                        
                    tempLeech = baseLeech + ((combo[i] - 4.0f) * 0.05f) + ((currStats[i] - 1.0f) * 0.08f);
                }
            }
            
            else
            {
                if(combo[i] < 8)
                    baseVal = CharacterData.BaseContLow;
                
                else if(combo[i] < 12)
                    baseVal = CharacterData.BaseContMid;
                
                else
                    baseVal = CharacterData.BaseContHigh;
                    
                comboVals[i] = baseVal + (int)System.Math.Ceiling(((combo[i] - 4.0) + currStats[i]) / 2.0);
            }
            
            numBlocks += combo[i];
            if(combo[i] > 0)
                numTypes++;
            combo[i] = 0;
        }
        
        //sum physical and magical damage and apply//
        int totalDamage = comboVals[0] + comboVals[1];
        
        if(totalDamage > 0)
            EnemyController.instance.TakeDamage(totalDamage);
        
        //support type = heal//
        if(currStats[5] == 0 && comboVals[2] > 0)
            HealDamage(comboVals[2]);
        
        //support type = shield//
        else if(currStats[5] == 1)
        {
            if(shield < comboVals[2])
            {
                shield = comboVals[2];
                DamageTextHandler(shield, 2);
                GenerateShield();
            }
        }
        
        //support type = leech, apply if damage is dealt//
        else if(totalDamage > 0)
        {
            HealDamage((int) (leech * totalDamage));
            leech = 0;
        }
        
        //reset leech//
        if(tempLeech != 0)
            leech = tempLeech;
            
        EnemyController.instance.SendGarbage(comboVals[3], currStats[6]);
        
        clearCombo = true;
        comboTimer = 0.5f;
        
        //Scoring//
        int incScore = 0;
        incScore = numBlocks * 100 * consecutive * (int)(1.25 * numTypes);
        
        score += incScore;
    }
    
    /////////////////////////////////////////////////////////////////////////////////////////////////
    //void TakeDamage(int)                                                                         //
    //Receives an amount of damage to take and removes health accordingly.                         //
    //If the player has a shield, damage is applied to the shield first.                           //
    //If the shield has taken any damage, even if it has health remaining, the shield is destroyed.//
    /////////////////////////////////////////////////////////////////////////////////////////////////
    public void TakeDamage(int damage)
    {
        if(damage == 0)
            return;
        
        int actual = damage - shield;
        if(actual < 0)
            actual = 0;
            
        shield = 0;
        DestroyShield();
        
        currHealth -= actual;
        
        if(currHealth < 0)
            currHealth = 0;
            
        healthBar.GetComponent<HealthHandler>().SetScale((float)currHealth / maxHealth);
        DamageTextHandler(damage, 0);
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////
    //void HealDamage(int)                                                                //
    //Receives an amount of healing and restores health accordingly.                      //
    //Regardless of how much healing is received, health cannot be raised above maxHealth.//
    ////////////////////////////////////////////////////////////////////////////////////////
    private void HealDamage(int heal)
    {
        currHealth += heal;
        if(currHealth > maxHealth)
            currHealth = maxHealth;
            
        healthBar.GetComponent<HealthHandler>().SetScale((float)currHealth / maxHealth);
        DamageTextHandler(heal, 1);
    }
    
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    //void DamageTextHandler(int, bool)                                                                    //
    //Receives an amount of damage or healing and creates text showing how much.                           //
    //If isDamage is true, text will be white to show damage, otherwise text will be green to show healing.//
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void DamageTextHandler(int amount, int type)
    {
        GameObject tempText = Instantiate(damageText);
        
        tempText.transform.SetParent(damageLoc.transform);
        
        Text statText = tempText.GetComponent<Text>();
        
        //If not damage, set color to green for healing//
        if(type == 1) //Healing = green//
            statText.color = Color.green;
            
        else if(type == 2) //Shield = blue//
            statText.color = Color.blue;
            
        else //default to damage = white//
            statText.color = Color.white;
        
        tempText.transform.localPosition = new Vector2(0f, 0f);
        tempText.transform.localScale = new Vector2(1f, 1f);
        statText.text = amount.ToString();
    }
    
    ///////////////////////////////////////////////////////////////////////////////////////
    //void GenerateShield()                                                              //
    //Creates a particle effect underneath the health bar showing that a shield is active//
    ///////////////////////////////////////////////////////////////////////////////////////
    private void GenerateShield()
    {
        if(activeShield == null)
        {
            activeShield = Instantiate(shieldEffect);
            activeShield.transform.SetParent(healthBar.transform);
            activeShield.transform.localPosition = new Vector2(0f, 0f);
            activeShield.GetComponent<ParticleSystem>().Play();
        }
    }
    
    ///////////////////////////////////////////////////
    //void DestroyShield()                           //
    //If there is an active shield effect, destroy it//
    ///////////////////////////////////////////////////
    private void DestroyShield()
    {
        if(activeShield != null)
            Destroy(activeShield.gameObject);
    }
    
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //void SendGarbage(int, int)                                                                        //
    //Revieces an amount of garbage blocks to handle.                                                   //
    //Garbage blocks are added to a buffer and a maximum of 7 garbage blocks will be received per cycle.//
    //Also receives a garbage type, changing how garbage blocks are applied to the board.               //
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    public void SendGarbage(int blocks, int type)
    {
        garbageIn += blocks;
        garbageType = type;
    }
    
    ///////////////////////////////////////////////////////
    //IEnumerator GarbageHandler()                       //
    //Handles incoming garbage blocks.                   //
    //Will apply no more than 7 garbage blocks per cycle.//
    ///////////////////////////////////////////////////////
    private IEnumerator GarbageHandler()
    {
        if(garbageType == 0)
        {
            bool[] temp = new bool[7];
            int drop = 0;
            if(garbage >= 7)
            {
                drop = 7;
                garbage -= 7;
            }
            
            else
            {
                drop = garbage;
                garbage = 0;
            }
            
            for(int i = 0; i < drop; i++)
            {
                temp[i] = true;
            }
            
            MathStuff.Shuffle(temp);
            
            for(int i = 0; i < temp.Length; i++)
            {
                if(temp[i])
                {
                    GameObject block = Instantiate(MasterController.Blocks[4], new Vector3(-3 + i, 6, 0), Quaternion.identity);
                    block.GetComponent<BlockBehavior>().playerBlock = true;
                    bool isFalling = block.GetComponent<BlockBehavior>().fall();
                    
                    if(isFalling)
                    {
                        numFalling++;
                        fallStack.Push(block);
                    }
                }
            }
            
            while(numFalling > 0)
            {
                yield return null;
            }
            
            while(fallStack.Count > 0)
            {
                GameObject tempObj = (GameObject)fallStack.Pop();
                
                int new_x = (int) tempObj.transform.position.x + x_correct;
                int new_y = (int) tempObj.transform.position.y + y_correct;
                
                board[new_x, new_y] = tempObj;
            }
        }
        
        else if(garbageType == 1)
        {
            int drop = 0;
            if(garbage >= 7)
            {
                drop = 7;
                garbage -= 7;
            }
            
            else
            {
                drop = garbage;
                garbage = 0;
            }
            
            Stack tempStack = new Stack();
            for(int i = 0; i < board_h; i++)
            {
                for(int k = 0; k < board_w; k++)
                {
                    if(board[k, i] != null && (board[k, i].tag.Equals("Block_Atk") || board[k, i].tag.Equals("Block_Con")))
                        tempStack.Push(board[k, i]);
                }
            }
            
            if(tempStack.Count < drop)
                drop = tempStack.Count;
                
            GameObject[] temp = new GameObject[tempStack.Count];
            
            for(int i = 0; i < temp.Length; i++)
            {
                temp[i] = (GameObject)tempStack.Pop();
            }
            
            MathStuff.Shuffle(temp);
            
            for(int i = 0; i < drop; i++)
            {
                GameObject tempObj = Instantiate(MasterController.Blocks[4], temp[i].transform.position, Quaternion.identity);;
                tempObj.GetComponent<BlockBehavior>().playerBlock = true;
                
                int new_x = (int) tempObj.transform.position.x + x_correct;
                int new_y = (int) tempObj.transform.position.y + y_correct;
                
                board[new_x, new_y] = tempObj;
                Destroy(temp[i]);
            }
        }
        
        else if(garbageType == 2)
        {
            int drop = 0;
            if(garbage >= 7)
            {
                drop = 7;
                garbage -= 7;
            }
            
            else
            {
                drop = garbage;
                garbage = 0;
            }
            
            Stack tempStack = new Stack();
            for(int i = 0; i < board_h; i++)
            {
                for(int k = 0; k < board_w; k++)
                {
                    if(board[k, i] != null && (board[k, i].tag.Equals("Block_Mag") || board[k, i].tag.Equals("Block_Sup")))
                        tempStack.Push(board[k, i]);
                }
            }
            
            if(tempStack.Count < drop)
                drop = tempStack.Count;
                
            GameObject[] temp = new GameObject[tempStack.Count];
            
            for(int i = 0; i < temp.Length; i++)
            {
                temp[i] = (GameObject)tempStack.Pop();
            }
            
            MathStuff.Shuffle(temp);
            
            for(int i = 0; i < drop; i++)
            {
                GameObject tempObj = Instantiate(MasterController.Blocks[4], temp[i].transform.position, Quaternion.identity);;
                tempObj.GetComponent<BlockBehavior>().playerBlock = true;
                
                int new_x = (int) tempObj.transform.position.x + x_correct;
                int new_y = (int) tempObj.transform.position.y + y_correct;
                
                board[new_x, new_y] = tempObj;
                Destroy(temp[i]);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    ////////////////////////////////////////////////////////////////////////
    //void DropBoard()                                                    //
    //Message receiver for when a block is placed above the overflow line.//
    //Calls Drop() to handle the board drop.                              //
    ////////////////////////////////////////////////////////////////////////
    public void DropBoard()
    {
        StartCoroutine(Drop());
    }
    
    //////////////////////////////////////////////////////////////////////////
    //IEnumerator Drop()                                                    //
    //Clears the bottom row of the board to prevent overflowing the board.  //
    //Blocks cleared in this way are not matched and will not start a combo.//
    //////////////////////////////////////////////////////////////////////////
    private IEnumerator Drop()
    {
        for(int i = 0; i < board_w; i++)
        {
            GameObject sorry = board[i, 0];
            
            if(sorry == null)
                continue;
                
            Destroy(sorry.gameObject);
            board[i, 0] = null;
        }
        
        yield return CheckFalls();
    }
    
    ////////////////////////////////////////////////////////////////////////
    //void CheckOverflow()                                                //
    //Scans the top of the board to determine if the board needs to drop. //
    //If any blocks are above the overflow line, drops the board one line.//
    ////////////////////////////////////////////////////////////////////////
    private void CheckOverflow()
    {
        for(int i = 0; i < board_w; i++)
        {
            if(board[i, board_h - 1] != null)
            {
                DropBoard();
                return;
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////////
    //void SendStatus(string)                                            //
    //Receives a status from the MasterController to display on the board//
    ///////////////////////////////////////////////////////////////////////
    public void SendStatus(string status)
    {
        Text statText = statusText.GetComponent<Text>();
        statText.text = status;
    }
    
    /////////////////////////////////////////////////////////
    //IEnumerator ScoreTracker()                           //
    //ticks up the score counter to show the player's score//
    /////////////////////////////////////////////////////////
    private IEnumerator ScoreTracker()
    {
        int tempScore = score;
        Text text = scoreText.GetComponent<Text>();
        text.text = "Score: " + tempScore.ToString();
        
        while(true)
        {
            if(tempScore < score)
            {
                tempScore += 10;
                text.text = "Score: " + tempScore.ToString();
            }
            yield return new WaitForSeconds(0.001f);
        }
    }
}
