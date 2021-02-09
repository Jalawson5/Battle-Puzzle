using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    private float timer;
    private float maxTimer;
    private float quickTimer;
    private float maxQuick;
    private float quickDelay;
    private float aiTimer;
    private Rigidbody2D rb;
    private bool movable;
    private bool isFalling;
    public GameObject boom;
    public bool playerBlock;
    public bool mpBlock;
    private int aiTargetCol;
    
    // Start is called before the first frame update
    void Start()
    {
        maxTimer = 0.3f;
        timer = maxTimer;
        
        maxQuick = 0.05f;
        quickTimer = maxQuick;
        
        quickDelay = 0f;
        
        if(tag == "Block_Dead")
            movable = false;
        
        else
            movable = true;

        rb = GetComponent<Rigidbody2D>();
        
        aiTargetCol = 0;
        if(!playerBlock)
        {
            FindTarget();
            aiTimer = 0.2f;
            if(EnemyController.instance.canQuick)
            {
                quickDelay = EnemyController.instance.quickDelay;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(MasterController.IsPlaying)
        {
            if(playerBlock)
            {
                if(movable)
                {
                    if(Input.GetKeyDown("left"))
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(-0.6f, 0, 0), Vector2.left, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(-1, 0, 0);
                        }
                    }
                    else if(Input.GetKeyDown("right"))
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0.6f, 0, 0), Vector2.right, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(1, 0, 0);
                        }
                    }

                    timer -= Time.deltaTime;
                    quickTimer -= Time.deltaTime;

                    if(timer <= 0 || (Input.GetKey("down")) && quickTimer <= 0) //one second has passed, move block//
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.6f, 0), Vector2.down, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(0, -1, 0); //timer reaches 1 second, move down 1 block//
                        }

                        else
                        {
                            movable = false; //block has settled, cannot be moved.
                            GameController.instance.notify(gameObject);
                        }
                        timer = 0.3f;
                        quickTimer = 0.05f;
                    }
                }

                if(isFalling)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.6f, 0), Vector2.down, 0.1f);

                    if(hit.collider == null)
                    {
                        transform.position += new Vector3(0, -0.5f, 0);
                    }

                    else
                    {
                        isFalling = false;
                        GameController.instance.StopFalling(gameObject);
                    }
                }
            }
            
            else if(mpBlock)
            {
                if(movable)
                {
                    if(Input.GetKeyDown("a"))
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(-0.6f, 0, 0), Vector2.left, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(-1, 0, 0);
                        }
                    }
                    else if(Input.GetKeyDown("d"))
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0.6f, 0, 0), Vector2.right, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(1, 0, 0);
                        }
                    }

                    timer -= Time.deltaTime;

                    if(timer <= 0 || Input.GetKey("s")) //one second has passed, move block//
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.6f, 0), Vector2.down, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(0, -1, 0); //timer reaches 1 second, move down 1 block//
                        }

                        else
                        {
                            movable = false; //block has settled, cannot be moved.
                            EnemyController.instance.Notify(gameObject);
                        }
                        timer = 0.3f;
                    }
                }

                if(isFalling)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.6f, 0), Vector2.down, 0.1f);

                    if(hit.collider == null)
                    {
                        transform.position += new Vector3(0, -0.5f, 0);
                    }

                    else
                    {
                        isFalling = false;
                        EnemyController.instance.StopFalling(gameObject);
                    }
                }
            }

            else //AI block//
            {
                if(movable)
                {
                    aiTimer -= Time.deltaTime;
                    if(aiTimer <= 0)
                    {
                        if(aiTargetCol < transform.position.x)
                        {
                            RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(-0.6f, 0, 0), Vector2.left, 0.1f);
                            if(hit.collider == null)
                            {
                                transform.position += new Vector3(-1, 0, 0);
                            }
                        }
                        else if(aiTargetCol > transform.position.x)
                        {
                            RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0.6f, 0, 0), Vector2.right, 0.1f);
                            if(hit.collider == null)
                            {
                                transform.position += new Vector3(1, 0, 0);
                            }
                        }
                        aiTimer = 0.2f;
                    }
                        
                    timer -= Time.deltaTime;
                    if(EnemyController.instance.canQuick)
                        quickDelay -= Time.deltaTime;
                        

                    if(timer <= 0) //one second has passed, move block//
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.6f, 0), Vector2.down, 0.1f);
                        if(hit.collider == null)
                        {
                            transform.position += new Vector3(0, -1, 0); //timer reaches 1 second, move down 1 block//
                        }

                        else
                        {
                            movable = false; //block has settled, cannot be moved.
                            EnemyController.instance.Notify(gameObject);
                        }
                        
                        if(aiTargetCol == transform.position.x && EnemyController.instance.canQuick && quickDelay <= 0)
                            timer = maxQuick;
                        
                        else
                            timer = maxTimer;
                    }
                }

                if(isFalling)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.6f, 0), Vector2.down, 0.1f);

                    if(hit.collider == null)
                    {
                        transform.position += new Vector3(0, -0.5f, 0);
                    }

                    else
                    {
                        isFalling = false;
                        if(playerBlock)
                        {
                            GameController.instance.StopFalling(gameObject);
                        }

                        else
                        {
                            EnemyController.instance.StopFalling(gameObject);
                        }
                    }
                }
            }
        }
    }
    
    public bool fall()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(0, -0.55f, 0), Vector2.down, 0.1f);
        if(hit.collider == null)
        {
            isFalling = true;
            return true;
        }
        
        else
            return false;
    }
    
    private void FindTarget()
    {
        aiTargetCol = EnemyController.instance.FindTarget(gameObject.tag);
    }
    
    void OnDestroy()
    {
        if(!GameController.isShuttingDown && MasterController.IsPlaying)
            Instantiate(boom, transform.position, Quaternion.identity);
    }
}
