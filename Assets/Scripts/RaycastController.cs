using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    //declaration for player/platform collider box
	public LayerMask  collisionMask;
    //the amount and detection range of rays
	public const float skinwidth = .015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D collider;
	public RaycastOrigins raycastOrigins;
    //get player collider box before any further function execute
	public virtual void Awake () {
		collider = GetComponent <BoxCollider2D> ();
	}
    //calculate and generate raycasting to detect collision
	public virtual void Start() {
		CalculateRaySpacing ();
	}
    //update every frame for the raycast so that detection of collisions will continue
	public void UpdateRaycastOrigins() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinwidth * -2);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);

	}
    //calculate the detection range and amount of rays
	public void CalculateRaySpacing () {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinwidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
    //type of rays
	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
