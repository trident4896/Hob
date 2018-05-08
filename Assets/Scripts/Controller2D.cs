using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Controller2D : RaycastController {
    //the climbing and descend angle  player can do
	float maxClimbAngle = 80;
	float maxDescendAngle = 80;
	//the characteristic of player
    public float health;
	public float damage;
	public GameObject protaganist;
	public GameObject theEndScreen;
	public GameObject instruction;
	public Text instruction_screen;
	private float instruct_count = 0;
    //collision detection
	public CollisionInfo collisions;
	[HideInInspector]
	public Vector2 playerInput;

	public override void Start () {
		base.Start ();
		theEndScreen.SetActive (false);
		instruction.SetActive (false);
		collisions.faceDir = 1;
	}
    //if player is detected on a platform and is pressing button to move, give velocity to player to move
	public void Move( Vector3 velocity, bool standingOnPlatform) {
		Move (velocity, Vector2.zero, standingOnPlatform);
	}
    //the basic movement of player standing on a platform
	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false) {
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.velocityOld = velocity;
		playerInput = input;

		if (velocity.x != 0) {
			collisions.faceDir = (int) Mathf.Sign (velocity.x);
		}

		if (velocity.y < 0) {
			DescendSlope (ref velocity);
		}
			
		HorizontalCollisions (ref velocity);
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);

		if (standingOnPlatform) {
			collisions.below = true;
		}
	}

	public void TheEnd () {
		theEndScreen.SetActive (true);
	}

    //detection of horizontal collisions
	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinwidth;

		if (Mathf.Abs (velocity.x) < skinwidth) {
			rayLength = 2 * skinwidth; 
		}

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            //if hit a trap, player will take damage
			if (hit) {

				if (hit.collider.tag == "Trap") {
					TakeDamage (damage);
				}

                //if distance between collision of player and objects around player is zero, then give player access to movement
				if (hit.distance == 0) {
					continue;
				}
                //for inclined platform
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
                //for descending the inclined platform
				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
             
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinwidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}
                //for climbing the inclined platfrom
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					
					velocity.x = (hit.distance - skinwidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}
    //detection of vertical collision
	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinwidth;

		for (int i = 0; i < verticalRayCount; i++) {
			
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            //if hit a trap, then player take damage
			if (hit) {
				if (hit.collider.tag == "Trap") {
					TakeDamage (damage);
				}

				if (hit.collider.tag == "End") {
					TheEnd ();

					if (Input.GetKeyDown (KeyCode.X)) {
						Application.Quit ();
					}
					if(Input.GetKeyDown(KeyCode.R)) {
						SceneManager.LoadScene ("2dver1");
					}
				}

				if (hit.collider.tag == "Instruction") {
					if (instruct_count == 0) {
						instruction.SetActive (true);
						instruction_screen.text = "Press <-- to move left." +" "+ "Press --> to move right.";
					}	
				}

				if (hit.collider.tag == "Instruction1") {
					if (instruct_count == 0) {
						instruction_screen.text = "Press 'Spacebar' to jump.";
					}
				}

				if (hit.collider.tag == "Instruction2") {
					if (instruct_count == 0) {
						instruction_screen.text = "Press 'Right' button to wall jump with spacebar.";
					}
				}

				if (hit.collider.tag == "Instruction3") {
					if (instruct_count == 0) {
						instruction_screen.text = "While on the wall, press opposite direction button to wall bounce along with spacebar.";
					}
				}

				if (hit.collider.tag == "Instruction4") {
					if (instruct_count == 0) {
						instruction_screen.text = "Go straight and wait for platform.";
					}
				}

				if (hit.collider.tag == "Instruction5") {
					if (instruct_count == 0) {
						instruction_screen.text = "Press 'Down'to drop below.";
					}
				}

				if (hit.collider.tag == "Instruction6") {
					if (instruct_count == 0) {
						instruction_screen.text = "Be careful of red cube, they are lethal.";
					}
				}

				if (hit.collider.tag == "Instruction7") {
					if (instruct_count == 0) {
						instruction_screen.text = "Enjoy!";
						Destroy (instruction, 3f);
					}
				}


					
            //if hit a platform with "through" tag, which means player can go through the platform
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke ("ResetFallingThroughPlatform", .5f);
						continue;
					}
				}

				velocity.y = (hit.distance - skinwidth) * directionY;
				rayLength = hit.distance;


				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}
					
				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
        //to detect collsions between player and inclined platform
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinwidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionY, rayLength, collisionMask);
           //if there is collision, give player access to movement
			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinwidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}
    //the there is collision detected on the inclined platform, give player velocity while climbing the inclined platform
	void ClimbSlope (ref Vector3 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}
    //when thre is collision detected on the inclined platform, give the player velocity while descending the inclined platform
	void DescendSlope( ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
      
		if (hit) {
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign (hit.normal.x) == directionX) {
					if (hit.distance - skinwidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) {
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}
    //after player fall through platform, reset back to initial state so player can fall through platform again
	void ResetFallingThroughPlatform() {
		collisions.fallingThroughPlatform = false;
	}
    //player takes damage when hit a trap
	public void TakeDamage(float dmg) {
		health -= dmg;
		if (health <= 0) {
			Die ();
		}
	}
    //reload  the level when dies
	public void Die() {
		SceneManager.LoadScene ("2dver1");
	}
	//declaration for collision detection and player movement
	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;
		public int faceDir;
		public bool fallingThroughPlatform;

		public void Reset() {
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}

