﻿using UnityEngine;
using System.Collections;


public class PlayerControl : MonoBehaviour 
{

    ///reset position 
    public Vector3 retryPos;

    public const float EDGEBUFFER = 0.4f; // Percentage of screen to validate mouse click/mobile tap
    // for edge detection
    Vector2 edgeLeft;
    Vector2 edgeRight;
    Vector2 screenWidth;

    // For click/tap detection
    Vector2 clickPosition;

    // for references to player
    public bool isAlive = true;
    private SpriteRenderer sprite;
    public Color initialColor;
    // for player movement
    public Vector2 movement;
    public bool facingRight = true;
    //speed variables
    public float playerSpeed;               // final magnitude of speed, the player's speed
    public bool slowMo;                     //boolean that toggles slow motion
    public bool sprint;                     //boolean that toggles sprint
    public float defaultSpeed = 10.0f;      //same as normal speed
    public float normalSpeed;       //normal speed magnitude
    public float slowMoSpeed;        //speed magnitude when slowMo is activaed
    public float sprintSpeed;

	//Fading Darkness Script variable
	public FadingDarkness fadingDarknessScript;

    //point variables
    public static int itemCounter;//to count item pickups

    //hiding variables
    public bool hide = false;
    int hidingOrder = 0;//sorting layer when hidden
    int sortingOrder = 2;//sorting layer normally

    //teleport walls
    GameObject wallR;
    GameObject wallL;

    // hunter variables
    public GameObject HunterMonster;
    private bool isHunterActive = false;
    float hunterYOffset = 10f;

    // estimated position.y of each floor
        /* Wilburn TODO: I feel like I can grab the floor positions from the floors that's
           is generated or somehow calculate the required measurements instead of eyeballing
           and writing it down. It would be cleaner
        */
    float floorOne = -13f;
    float floorTwo = 13.5f;
    float floorThree = 40.6f;
    float floorFour = 67.6f;
    float floorFive = 94.7f;
    float floorAttic = 121.8f;

    // Stage Difficulty Variable
    bool standardLevel = true;
    int stageLevel;
    int hunterInactiveDuration;

    // Use this for initialization
    void Awake()
    {
        normalSpeed = defaultSpeed;
        clickPosition = new Vector2(0f, 0f);
        screenWidth = new Vector2((float)Screen.width, 0f);
        edgeLeft = new Vector2(screenWidth.x * EDGEBUFFER, 0f);
        edgeRight = new Vector2(screenWidth.x - edgeLeft.x, 0f);
        sprite = GetComponent<SpriteRenderer>();
        slowMoSpeed = normalSpeed / 2;
        sprintSpeed = normalSpeed * 2;
        wallR = GameObject.Find("RightWall");
        wallL = GameObject.Find("LeftWall");
    }
    // Use this for initialization
    void Start() //what happens as soon as player is created
    {
        initialColor = sprite.color;
        slowMo = false;  //slowMo starts out as false since the player hasn't hit the button yet

        // Formula to calculate the hunter duration
        Debug.Log("Stage Level: " + Application.loadedLevel);
        if(Application.loadedLevelName == "Ending Level" || Application.loadedLevelName == "Tutorial Stage")
        {
            standardLevel = false;
        }
        hunterInactiveDuration = 40 - 18 * (Application.loadedLevel - 1);
        if (hunterInactiveDuration < 8)
            hunterInactiveDuration = 8;
    }
    void Update()
    {
        //if player is killed, set player object's alpha to 0 (make invisible)
        /*if(isAlive==false)
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        }
        */
        //code for sprinting
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //it shift key is hit, players speed is 2 times the speed
            sprint = true;
        } 
        else
        {
            sprint = false;
        }

        // Code to spawn a HunterMonster targeting the player at a random location within
        // the same floor.
        if (!isHunterActive && standardLevel == true)//check if hunter is currently inactive and if this level should spawn a hunter
        {
            if (transform.position.y > floorOne && transform.position.y < floorTwo)
            {
                isHunterActive = true;
                StartCoroutine(SpawnHunterMonster(hunterInactiveDuration, floorOne, floorTwo));
            }
            if (transform.position.y > floorTwo && transform.position.y < floorThree)
            {
                isHunterActive = true;
                StartCoroutine(SpawnHunterMonster(hunterInactiveDuration, floorTwo, floorThree));
            }
            if (transform.position.y > floorThree && transform.position.y < floorFour)
            {
                isHunterActive = true;
                StartCoroutine(SpawnHunterMonster(hunterInactiveDuration, floorThree, floorFour));
            }
            if (transform.position.y > floorFour && transform.position.y < floorFive)
            {
                isHunterActive = true;
                StartCoroutine(SpawnHunterMonster(hunterInactiveDuration, floorFour, floorFive));
            }
            if (transform.position.y > floorFive && transform.position.y < floorAttic)
            {
                isHunterActive = true;
                StartCoroutine(SpawnHunterMonster(hunterInactiveDuration, floorFive, floorAttic));
            }
        }
    }

    void LateUpdate()
    {
        //handles player movement based upon mouse clicks (or taps)
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < edgeLeft.x)
                Move(-playerSpeed);
            else if (Input.mousePosition.x > edgeRight.x)
                Move(playerSpeed);
        }
        //handles player movement based upon keyboard input
        else
        {
           //calls move, sends a value of the speed multiplied by the axis (which will either be -1, 0, or 1)
            Move(Input.GetAxis("Horizontal")*playerSpeed);
        }
        
        ///code for slow motion movement
       
        if (slowMo) //when slowMo is true, the player will move at half speed
        {
            playerSpeed = slowMoSpeed;
        }
        else if(sprint)
        {
            playerSpeed = sprintSpeed;
        }

        else  //when slowMo is false, the player will move normally
        {
            playerSpeed = normalSpeed;
        }

    }
   
    void Move(float h)
    {
        //prevent player from moving when hidden
        if (!hide)
        {
            // Set movement and normalize in terms of time passed from previous frame
            // (Assuming we will be frame rate dependent)
            movement.x = h;
            movement *= Time.deltaTime;

            // apply movement to player
            transform.Translate(movement);
            //this checks which direction the player is moving and flips the player based upon that
            if (movement.x > 0 && facingRight == false)
            {
                FlipPlayer();
            }
            if (movement.x < 0 && facingRight == true)
            {
                FlipPlayer();
            }

        }

    }
    //handle collisions with level objects
    void OnTriggerEnter2D(Collider2D col)
    {
        //if player colliders with an enemy and is not hidden
        if (col.gameObject.tag == "PatrolEnemy" && hide == false)
        {
            //player is dead
            isAlive = false;
            //prevent player from moving
            normalSpeed = 0f;
        }
        //level warp
       
       
        //checks for collision
        if (col.gameObject.name == "LeftWall")
        {
            transform.position = new Vector3(wallR.transform.position.x - 4, transform.position.y, transform.position.z);
        }
        if (col.gameObject.name == "RightWall")
        {
            transform.position = new Vector3(wallL.transform.position.x + 4, transform.position.y, transform.position.z);
        }
       
    }
    //allows actions when staying within collision area
    void OnTriggerStay2D(Collider2D col)
    {
        // OverlapPoint refers to world space instead of screen space, adjusting accordingly
        clickPosition.x = (Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
        clickPosition.y = (Camera.main.ScreenToWorldPoint(Input.mousePosition).y);


        //Toggle Hide/Unhide
        if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && col.OverlapPoint(clickPosition)))) //if player activates hiding spot
            //|| ((hide && ((Input.GetAxis("Horizontal") > 0.9)||(Input.GetAxis("Horizontal") < -0.9)))  //or if player is hidden and moves using the keyboard
            //|| (hide && Input.GetMouseButton(0)&& (Input.mousePosition.x < edgeLeft.x || Input.mousePosition.x > edgeRight.x)))) //or if player is hidden and moves using the mouse
        {
            if (col.gameObject.tag == "Cover")
            {
                if (!hide)
                {
                    sprite.sortingOrder = hidingOrder;
                    hide = true;
                }
                else if (hide)
                {
                    sprite.sortingOrder = sortingOrder;
                    hide = false;

                    if (slowMo) //Disables slowmotion speed upon hiding
                    {
                        slowMo = false;
                    }
                }
            }
            //if player is trying to hide, and the object is the trap enemy
            /*
            else if (col.gameObject.tag == "Enemy")
            {
               
                //player dies
                isAlive = false;
                //prevents movement
                normalSpeed = 0f;
            }
            */
               
          }     
        
        //if player colliders with an enemy and is not hidden
        if (col.gameObject.tag == "PatrolEnemy" && hide == false)
        {
            //player is dead
            isAlive = false;
            //prevent player from moving
            normalSpeed = 0f;
        }
       ///gameover for stationary enemies handled in their own code
    }
    void FlipPlayer()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    // adds points in  the Player Script
    public static void AddPoints(int itemAdd)
    {
        itemCounter += itemAdd; //adds amount to current score
        Debug.Log("Score: " + itemCounter); //confirms the player has picked up the object (track amount). this is removeable.
    }
    public void HidePlayer()
    {
        sprite.color = new Color(1f, 1f, 1f, 0f);
    }
    IEnumerator SpawnHunterMonster(int time, float bottomFloor, float topFloor)
    {
        // waits time seconds before spawning
        for (int i = 0; i < time; i++)
        {
            if (transform.position.y > bottomFloor && transform.position.y < topFloor)
                yield return new WaitForSeconds(1); // waits up to 'time' seconds
            else
                break;
        }

        // hunter spawning code
        if (transform.position.y > bottomFloor && transform.position.y < topFloor)
        {
            float xHunterPos = Random.Range(wallL.transform.position.x, wallR.transform.position.x);
            while (transform.position.x - 10 < xHunterPos && xHunterPos < transform.position.x + 10)
            {
                xHunterPos = Random.Range(wallL.transform.position.x, wallR.transform.position.x);
            }
            var hunter = Instantiate(HunterMonster, new Vector2(xHunterPos, transform.position.y + hunterYOffset), Quaternion.identity);

            for (int i = 0; i < 60; i++)    // 'i' controlls the duration of the hunter existance
            {
                if (transform.position.y > bottomFloor && transform.position.y < topFloor)
                    yield return new WaitForSeconds(1);
                else
                    break;
            }

            // goodbye hunter
            if (hunter != null)
                Destroy(hunter, 1f);
        }

        isHunterActive = false;
    }
}