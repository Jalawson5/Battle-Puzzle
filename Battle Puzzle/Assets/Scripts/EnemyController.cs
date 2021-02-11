using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public static EnemyController instance;

    private int aiChar;
    
    private int maxHealth;
    private int currHealth;
    private int x_correct; //convert block.x to board.x//
    private int y_correct; //convert block.y to board.y//
    private bool matching; //true if board is processing matches//
    private bool canSpawn; //true if board is ready for another block//
    private bool hasCombo; //true if block placed resulted in a combo, ignores comboHandler if false//
    private bool clearCombo;
    private bool checking;
    private bool hasDropped;
    private float timer; //timer for spawning a new block//
    private float comboTimer;
    private int numFalling;
    private int garbage;
    private int garbageIn;
    private int garbageType;
    private int nextBlock;
    private int totalCombo;
    private GameObject nextSprite;
    private int score;
    private int consecutive;
    
    private int shield;
    private float leech;
    
    public bool canQuick;
    public float quickDelay;
    private bool smartDrop; //Does the AI build for combos? (Stage 5+)
    
    public GameObject healthBar;
    public GameObject charText;
    public GameObject damageLoc;
    public GameObject damageText;
    public GameObject comboText;
    public GameObject statusText;
    public GameObject shieldEffect;
    private GameObject activeShield;
    public GameObject scoreText;
    
    GameObject[,] board;
    private int[] combo; //array representing combo count: atk, mag, sup, con//
    private int[] currStats;
    
    private Stack matchStack;
    private Stack fallStack;
    private Stack garbageStack;
    
    private bool playerControlled;
    
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
        board = new GameObject[GameController.board_w, GameController.board_h];
        
        matchStack = new Stack();
        fallStack = new Stack();
        garbageStack = new Stack();
        
        combo = new int[]{0, 0, 0, 0};
        
        x_correct = -13;
        y_correct = 4;
        
        matching = false;
        canSpawn = true;
        hasDropped = false;
        checking = false;
        
        timer = 0.75f;
        InitDifficulty();
        
        numFalling = 0;
        garbage = 0;
        garbageIn = 0;
        
        shield = 0;
        leech = 0;
        
        /*if(MasterController.Stage == 7)
            aiChar = -1;
        
        else if(MasterController.Stage == 8)
            aiChar = -2;
            
        else if(MasterController.Stage == 9)
            aiChar = -3;
        
        else
            aiChar = new System.Random().Next(7);*/
        if(MasterController.IsMultiplayer)
        {
            aiChar = MasterController.OppCharacter;
            currStats = CharacterData.GetStats(aiChar);
            
            maxHealth = CharacterData.InitializeHealth(currStats[4]);
            currHealth = maxHealth;
            
            playerControlled = true;
        }
        
        else
        {
            aiChar = MasterController.Opponents[MasterController.Stage];

            currStats = CharacterData.GetStats(aiChar);

            maxHealth = CharacterData.InitializeHealth(currStats[4]);
            currHealth = maxHealth;
            
            playerControlled = false;
        }
        
        charText.GetComponent<Text>().text = CharacterData.GetName(aiChar);
        
        //Disabled until I find good royalty free music. Sorry!//
        //MusicController.instance.SetTheme(aiChar);
        
        score = 0;
        consecutive = 0;
        StartCoroutine(ScoreTracker());
    }
    
    private void InitDifficulty()
    {
        int baseDiff = MasterController.Stage;
        if(baseDiff >= 7)
        {
            canQuick = true;
            if(baseDiff == 7)
                quickDelay = 1f;
            
            else if(baseDiff == 8)
                quickDelay = 0.7f;
            
            else
                quickDelay = 0.3f;
        }
        
        else
        {
            canQuick = false;
        }
        
        if(baseDiff >= 4)
        {
            smartDrop = false;
        }
        
        else
        {
            smartDrop = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(currHealth == 0 && !matching && MasterController.IsPlaying)
        {
            MasterController.instance.MatchOver(false);
        }
        
        else if(MasterController.IsPlaying)
        {
            if(numFalling == 0 && !matching)
            {
                checking = true;
                CheckOverflow();
                checking = false;
            }

            if(garbage > 0 && canSpawn && !hasDropped && !checking)
            {
                hasDropped = true;
                StartCoroutine(GarbageHandler());
                //canSpawn = true;
            }

            if(garbageIn > 0)
            {
                //probably unnecessary, to prevent race conditions//
                int temp = garbageIn;
                garbage += temp;
                garbageIn -= temp;
            }

            if(matching || !canSpawn)
                timer = 0.75f;
            
            if(clearCombo)
            {
                comboTimer -= Time.deltaTime;
                
                if(comboTimer <= 0)
                {
                    ClearComboText();
                    clearCombo = false;
                }
            }
            
            else if(canSpawn)
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
                    
                    block = Instantiate(MasterController.Blocks[choice], new Vector3(16, 5, 0), Quaternion.identity);

                    block.GetComponent<BlockBehavior>().playerBlock = false;
                    
                    if(playerControlled)
                        block.GetComponent<BlockBehavior>().mpBlock = true;
                    
                    else
                        block.GetComponent<BlockBehavior>().mpBlock = false;
                        
                    if(nextSprite != null)
                        Destroy(nextSprite);
                    
                    nextSprite = Instantiate(MasterController.Sprites[nextBlock], new Vector3(11, 0, 0), Quaternion.identity);
                        
                    hasDropped = false;
                }
            }
        }
    }
    
    public void Notify(GameObject caller)
    {
        StartCoroutine(BlockHandler(caller));
    }
    
    ////////////////////////////////////////////////////////////////////
    //void notify(BlockBehavior)                                      //
    //handles the notifying BlockBehavior object                      //
    //checks for matches and handles falls until the combo is resolved//
    ////////////////////////////////////////////////////////////////////
    IEnumerator BlockHandler(GameObject caller)
    {
        int new_x = (int)(caller.transform.position.x + x_correct);
        int new_y = (int)(caller.transform.position.y + y_correct);
        
        //Check if stack is too high. Delete bottom rows until stack is low enough.//
        /*if(new_y >= GameController.board_h - 1)
        {
            yield return Drop();
        }*/
        
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
    
    ///////////////////////////////////////////////////////////////////////////
    //void match(int, int)                                                   //
    //accepts board coordinates and checks all neighboring blocks for matches//
    ///////////////////////////////////////////////////////////////////////////
    void Match(int x, int y)
    {
        //Copy board to temp board//
        GameObject[,] temp = new GameObject[GameController.board_w, GameController.board_h];
        
        for(int i = 0; i < GameController.board_w; i++)
            for(int k = 0; k < GameController.board_h; k++)
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
    
    void RecurMatch(int x, int y, string type, GameObject[,] copy)
    {
        GameObject curr = copy[x, y];
        copy[x, y] = null;
        
        if(curr.tag.Equals(type))
        {
            matchStack.Push(curr);
            
            if(x != 0 && copy[x-1, y] != null)
                RecurMatch(x - 1, y, type, copy);
            
            if(x != GameController.board_w - 1 && copy[x+1, y] != null)
                RecurMatch(x + 1, y, type, copy);
                
            if(y != 0 && copy[x, y-1] != null)
                RecurMatch(x, y - 1, type, copy);
                
            if(y != GameController.board_h - 1 && copy[x, y+1] != null)
                RecurMatch(x, y + 1, type, copy);
        }
        
        else if(curr.tag.Equals("Block_Dead"))
        {
            garbageStack.Push(curr);
        }
    }
    
    /*void checkFalls()
    {
        for(int i = 0; i < board_w; i++)
            for(int k = 0; k < board_h; k++)
            {
                if(board[i, k] == null)
                    continue;
                    
                GameObject fallen = board[i, k];
                bool hasFallen = fallen.GetComponent<BlockBehavior>().fall();
                
                if(hasFallen)
                {
                    fallStack.Push(fallen);
                    board[i, k] = null;
                }
                
                int new_x = (int)fallen.transform.position.x + x_correct;
                int new_y = (int)fallen.transform.position.y + y_correct;
                
                board[new_x, new_y] = fallen;
            }
    }*/
    
    IEnumerator CheckFalls()
    {
        for(int i = 0; i < GameController.board_h; i++)
        {
            for(int k = 0; k < GameController.board_w; k++)
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
            tempStack.Push(tempObj);
            
            int new_x = (int) tempObj.transform.position.x + x_correct;
            int new_y = (int) tempObj.transform.position.y + y_correct;
            
            board[new_x, new_y] = tempObj;
        }
        
        fallStack = tempStack;
    }
    
    public void StopFalling(GameObject caller)
    {
        if(fallStack.Contains(caller))
        {
            numFalling--;
        }
    }
    
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
        
        int totalDamage = comboVals[0] + comboVals[1];
        
        if(totalDamage > 0)
            GameController.instance.TakeDamage(totalDamage);
        
        if(currStats[5] == 0 && comboVals[2] > 0)
            HealDamage(comboVals[2]);
            
        else if(currStats[5] == 1)
        {
            if(shield < comboVals[2])
            {
                shield = comboVals[2];
                DamageTextHandler(shield, 2);
                GenerateShield();
            }
        }
        
        else if(totalDamage > 0)
        {
            if(leech > 0)
                HealDamage((int) (leech * totalDamage));
            leech = 0;
        }
        
        if(tempLeech != 0)
            leech = tempLeech;
            
        GameController.instance.SendGarbage(comboVals[3], currStats[6]);
        
        clearCombo = true;
        comboTimer = 0.5f;
        
        //Scoring//
        int incScore = 0;
        incScore = numBlocks * 100 * consecutive * (int)(1.25 * numTypes);
        
        score += incScore;
    }
    
    public int FindTarget(string type)
    {
        GameObject temp;
                
        for(int i = 0; i < GameController.board_h; i++)
        {
            for(int k = 0; k < GameController.board_w; k++)
            {
                temp = board[k, i];
                if(temp == null)
                    continue;
                if(temp.tag.Equals(type))
                {
                    if(k > 0 && board[k - 1, i] == null)
                        return k - 1 - x_correct;//gives block x position to aim for//
                    
                    else if((k < GameController.board_w - 1) && board[k + 1, i] == null)
                        return k + 1 - x_correct;
                        
                    else if((i < GameController.board_h - 1) && board[k, i + 1] == null)
                        return k - x_correct;
                    
                    //If cannot connect blocks, and can smart drop, try to build upwards//
                    else if(smartDrop)
                    {
                        if((i < GameController.board_h - 1) && board[k, GameController.board_h - 1] == null)
                            return k - x_correct;
                            
                        else if(k > 0 && board[k - 1, GameController.board_h - 1] == null)
                            return k - 1 - x_correct;
                        
                        else if((k < GameController.board_w - 1) && board[k + 1, GameController.board_h - 1] == null)
                            return k + 1 - x_correct;
                    }
                }
            }
        }
        
        System.Random rand = new System.Random();
        return rand.Next(GameController.board_w) - x_correct;
    }
    
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
    
    private void HealDamage(int heal)
    {
        currHealth += heal;
        if(currHealth > maxHealth)
            currHealth = maxHealth;
            
        healthBar.GetComponent<HealthHandler>().SetScale((float)currHealth / maxHealth);
        DamageTextHandler(heal, 1);
    }
    
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
    
    public void SendGarbage(int blocks, int type)
    {
        garbageIn += blocks;
        garbageType = type;
    }
    
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
                    GameObject block = Instantiate(MasterController.Blocks[4], new Vector3(13 + i, 6, 0), Quaternion.identity);
                    block.GetComponent<BlockBehavior>().playerBlock = false;
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
            for(int i = 0; i < GameController.board_h; i++)
            {
                for(int k = 0; k < GameController.board_w; k++)
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
                tempObj.GetComponent<BlockBehavior>().playerBlock = false;
                
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
            for(int i = 0; i < GameController.board_h; i++)
            {
                for(int k = 0; k < GameController.board_w; k++)
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
                GameObject tempObj = Instantiate(MasterController.Blocks[4], temp[i].transform.position, Quaternion.identity);
                tempObj.GetComponent<BlockBehavior>().playerBlock = false;
                
                int new_x = (int) tempObj.transform.position.x + x_correct;
                int new_y = (int) tempObj.transform.position.y + y_correct;
                
                board[new_x, new_y] = tempObj;
                Destroy(temp[i]);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    public void DropBoard()
    {
        StartCoroutine(Drop());
    }
    
    private IEnumerator Drop()
    {
        for(int i = 0; i < GameController.board_w; i++)
        {
            GameObject sorry = board[i, 0];
            
            if(sorry == null)
                continue;
                
            Destroy(sorry.gameObject);
            board[i, 0] = null;
        }
        
        yield return CheckFalls();
        fallStack.Clear();
    }
    
    private void CheckOverflow()
    {
        for(int i = 0; i < GameController.board_w; i++)
        {
            if(board[i, GameController.board_h - 1] != null)
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
