using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    //main UI declaration
	public Text countText;
	public Text showText;
	private int count = 0;
	private int exitCount = 0;
	public GameObject loadingScreen;
	public GameObject userScreen;
	public GameObject objectiveScreen;
	public GameObject exitScreen;

    //main walking and jumping physics
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirbone = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 9;

    //wall jumping variable declaration
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick; 

    //pause screen declaration
	public GameObject pauseScreen;
	bool paused;

    //main gravity physics declaration
	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

    //main function to set a world with gravity, walking and jumping physics
	void Start () {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		print ("Gravity: " + gravity + " Jump Velocity: " + maxJumpVelocity);

		paused = false;
		pauseScreen = GameObject.Find ("PauseScreen");

	}
    //Update every frame of player movement
	void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirbone);

		bool wallSliding = false;
        //when player hits a wall using left side or right side, collisions happen and activate the wall riding features
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				} 
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}
		}
        //when a spacebar is pressed, if ther isd velocity on player movenment, player ascend; when there is no velocity, player jumps off;
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (wallSliding) {
				if (wallDirX == input.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				}
				else {
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}
			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
		}
        //to set the jump height of the player, when player press longer , they jump higher
		if (Input.GetKeyUp (KeyCode.Space)) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}
        
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime, input);
        //player detects collisions from above or below
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
        //UI desgin, when 'c' is pressed, activate narrative intro
		if (Input.GetKeyDown(KeyCode.C) && count == 0 ){
			countText.text = "Hello there, little data!";
			showText.text = "Press C to continue";
		}
			
		if (Input.GetKeyDown (KeyCode.C) && count == 1) {
			countText.text = "Welcome to the world of digital.";
			showText.text = "Press C to continue";
		}

		if (Input.GetKeyDown (KeyCode.C) && count == 2) {
			countText.text = "Oh No! The 'User' is going to whipe the data!";
			showText.text = "Press C to continue";
		}

		if(Input.GetKeyDown (KeyCode.C) && count == 3) {
			countText.text = "Quick! Run! Run!";
			showText.text = "----------------->";
			Destroy (loadingScreen, .5f);
			Destroy (userScreen, 1f);
			Destroy (objectiveScreen, 1f);
		}
		
		if(Input.GetKeyDown (KeyCode.C)) {
		count++;
		}

		//If 'Esc' is pressed, pause menu is activated	
		if (Input.GetKeyDown (KeyCode.Escape)) {
			paused = !paused;
			exitCount++;
		}

        //Inside pasue menu, if 'X; is pressed, quit the game
		if (Input.GetKeyDown (KeyCode.X) && exitCount == 1) {
			Application.Quit ();
			exitCount = 0;
		}

        //Insde the pause menu, if 'Esc' is press again, game resume
		if (paused) {
			pauseScreen.SetActive (true);
			exitScreen.SetActive (true);
			Time.timeScale = 0;
		}

		if (!paused) {
			pauseScreen.SetActive (false);
			exitScreen.SetActive (false);
			Time.timeScale = 1;
		}
			

	}





}
