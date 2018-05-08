   using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlatformController : RaycastController {
    //declaration for platform collider box
	public LayerMask passengerMask;
    //declaration for location that platform will travel
	public Vector3[] localWaypoints;
	Vector3[] globalWaypoints;
    //declaratioon for the mechanism and physics for moving platform
	public float speed;
	public bool cylic;
	public float waitTime;
	[Range(0,2)]
	public float easeAmount;
    //declaration for the variation of moving platform
	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform,Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D> ();
    //main function to call and link from other scripts
	public override void Start () {
		base.Start ();

		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++) {
			globalWaypoints [i] = localWaypoints [i] + transform.position;
		}
	}
    //Update raycast every frame to detect collisions with platform
	void Update() {
		UpdateRaycastOrigins ();

		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengerMovement (velocity);

		MovePassengers (true);
		transform.Translate (velocity);
		MovePassengers (false);
	}

	float Ease(float x) {
		float a = easeAmount + 1;
		return Mathf.Pow ( x, a) / (Mathf.Pow( x, a) + Mathf.Pow( 1-x, a));
	}

//The settings for the path of moving platform
	Vector3 CalculatePlatformMovement() {

		if (Time.time < nextMoveTime) {
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints [fromWaypointIndex],globalWaypoints [toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01 (percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease (percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(globalWaypoints [fromWaypointIndex] , globalWaypoints [toWaypointIndex], easedPercentBetweenWaypoints);

        //Do travelling cycle of path for moving platform
		if (percentBetweenWaypoints >= 1) {
			percentBetweenWaypoints = 0;
			fromWaypointIndex++; 
			if (!cylic) {
				if (fromWaypointIndex >= globalWaypoints.Length - 1) {
					fromWaypointIndex = 0;
					System.Array.Reverse (globalWaypoints);
				}
			}
			nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;
	}

    //if this is true, player can walk on moving platform
	void MovePassengers(bool beforeMovePlatform) {
		foreach (PassengerMovement passenger in passengerMovement) {
			if (!passengerDictionary.ContainsKey(passenger.transform)) {
				passengerDictionary.Add (passenger.transform, passenger.transform.GetComponent<Controller2D> ());
			}
				
			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary [passenger.transform].Move (passenger.velocity, passenger.standingOnPlatform);	
			}
		}
	}
    //When the movement speed and direction when player is on the moving platform
	void CalculatePassengerMovement(Vector3 velocity) {
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerMovement = new List<PassengerMovement> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);

		//vertically moving platform
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinwidth;

			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinwidth) * directionY;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), directionY == 1, true));
					}
				}
			}
		}
		//Horizontally moving platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinwidth;

			for (int i = 0; i < horizontalRayCount; i++) {
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x - (hit.distance - skinwidth) * directionX;
						float pushY = -skinwidth;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), false, true));
					}
				}

			}
		}

		//Passenger on top of horizontally descending platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = skinwidth * 2;

			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit && hit.distance != 0) {
					if (!movedPassengers.Contains (hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;

						passengerMovement.Add (new PassengerMovement (hit.transform, new Vector3 (pushX, pushY), true, false));
					}
				}
			}
		}
	}

    //check whether player is on platform or not
	struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}


   //Draw the path of moving platform
	void OnDrawGizmos() {
		if (localWaypoints != null) {
			Gizmos.color = Color.red;
			float size = .3f;

			for (int i = 0; i < localWaypoints.Length; i++) {
				Vector3 globalWaypointsPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints [i] + transform.position;
				Gizmos.DrawLine (globalWaypointsPos - Vector3.up * size, globalWaypointsPos + Vector3.up * size);
				Gizmos.DrawLine (globalWaypointsPos - Vector3.left * size, globalWaypointsPos + Vector3.left * size);
			}
		}
	}
}