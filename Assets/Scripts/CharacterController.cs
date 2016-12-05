using UnityEngine;
using System.Collections;
using System.IO;

public class CharacterController : MonoBehaviour {

	private SpriteRenderer sprite;
	public float walkSpeed = 10f;
	public float runSpeed = 20f;
	private bool facingRight = false;
	private Rigidbody2D rb;
	private Animator anim;

	// walk direction & interact
	private float move;
	public float interactionRaycastLength = 5f;
	public LayerMask interactableLayer;
	private bool interacting = false;


	// for jumping and falling
	public float jumpForce = 700;
	public float ropePushForce = 70;
	private bool grounded = false;
	public Transform GroundCheck;
	private float groundRadius = 0.2f;
	public LayerMask whatIsGround;

	// climbing chain
	private int linkIndex;
	private bool jumpingOffChain = false;
	private bool climbingChain = false;
	private Vector2 chainLinkPos = Vector2.zero;
	private GameObject chainLink;
	private int previousLinkIndex;
	private bool climbedLink = false;

	// Push / Pull objects
	private Transform pullable = null;

	// Shooting
	private float aimX = 0f;
	private float aimY = 0f;
	public LayerMask shootableLayer;

	void Start () {
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		sprite = GetComponent<SpriteRenderer>();
	}

	void FixedUpdate ()
	{
		// checking for ground
		grounded = Physics2D.OverlapCircle (GroundCheck.position, groundRadius, whatIsGround);
		anim.SetBool ("Ground", grounded);

		anim.SetFloat ("vSpeed", rb.velocity.y);

		// moving
		move = Input.GetAxis ("Horizontal");
		float yVelocity = rb.velocity.y;


//		// ========================
//		// CHAIN CLIMBING
//		// ========================
//		// TODO clean up the climbing
//
		float climbing = Input.GetAxisRaw ("Vertical");

		// i.e. have to take finger off up button to climb to next link... just for now
//		if (climbing == 0) {
//			climbedLink = false;
//		}


		if (climbingChain) {
//			yVelocity = 0f;
			linkIndex = chainLink.transform.GetSiblingIndex ();
//			Debug.Log ("- - - Chain Link index: " + linkIndex);
//			Debug.Log ("- - - -going up? " + climbing);

			int numberOfLinks = chainLink.transform.parent.transform.childCount;

//			Debug.Log ("- - - - - Number Of Links......." + numberOfLinks);

			if (climbing != 0) {
				climbedLink = true;

				int targetIndex = linkIndex;

				if (climbing < 0) {
					targetIndex += 1;
				} else if (climbing > 0) {
					targetIndex -= 1;
				}
				;

				if (targetIndex < 0) {
					targetIndex = 0;
				} else if (targetIndex > (numberOfLinks - 1)) {
					targetIndex = numberOfLinks - 1;
				}

//				Debug.Log ("- - - - - - targetIndex" + targetIndex);

				// get next link
				GameObject oneLinkUp = chainLink.transform.parent.GetChild (targetIndex).gameObject;

				// create new hinge joint
				HingeJoint2D jointGrip = gameObject.AddComponent<HingeJoint2D> ();
				jointGrip.connectedBody = oneLinkUp.GetComponent<Rigidbody2D> ();
				chainLink = oneLinkUp;

				HingeJoint2D existingJoint = gameObject.GetComponent<HingeJoint2D> ();
				// destroy old hinge joint
				if (existingJoint) {
					Debug.Log("- - - - - - - Destroyed existing joint");
					Destroy (existingJoint);
				}

				// now Lerp character position between links
				bool posMatched = transform.position == oneLinkUp.transform.position;
				Debug.Log ("- - - posMatched?? " + posMatched);
//				transform.position = Vector2.Lerp (transform.position, oneLinkUp.transform.position, 0.5f);
				StartCoroutine (MoveToPosition (transform, oneLinkUp.transform.position, 0.5f));
			}
		}

		// =================================
		// RUNNING / WALKING
		// =================================

		if (Input.GetButton ("Fire3")) {
			rb.velocity = new Vector2 (move * runSpeed, yVelocity);
		} else {
			rb.velocity = new Vector2 (move * walkSpeed, yVelocity);	
		}

		anim.SetFloat ("Speed", Mathf.Abs (move));

		// =================
		// SHOOTING / AIMING
		// =================

		bool shootButton = Input.GetButtonDown("Fire1");

		RaycastHit2D shootableHit;

		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Quaternion rot = Quaternion.LookRotation(transform.position - mousePosition, -Vector3.forward);
		rot *= Quaternion.Euler(0,180f,0);

		shootableHit = Physics2D.Raycast (transform.position, rot * this.transform.forward, Mathf.Infinity, shootableLayer);

		Vector2 shootForce = rot * this.transform.forward * 50;
		Vector2 shootForceUp = Vector2.up * 5;

		if (shootableHit.collider != null && shootButton) {
			if (shootableHit.collider.gameObject.tag == "Shootable") {
				Rigidbody2D shootableRB = shootableHit.collider.gameObject.GetComponent<Rigidbody2D>();
				shootableRB.AddForce (shootForce, ForceMode2D.Impulse);
				shootableRB.AddForce (shootForceUp, ForceMode2D.Impulse);
			}
		}

		Debug.DrawRay(transform.position, rot * this.transform.forward * 400, Color.red);


		// =================================
		// MOVE / SPRITE DIRECTION
		// =================================

		if (move > 0) {
			facingRight = true;
			sprite.flipX = false;
		} else if (move < 0) {
			facingRight = false;
			sprite.flipX = true;
		}

	}

	public IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToMove){
      var currentPos = transform.position;
      var t = 0f;
       while(t < 1){
             t += Time.deltaTime / timeToMove;
             transform.position = Vector3.Lerp(currentPos, position, t);
             yield return null;
      }
    }

	void Update ()
	{

		bool interactionButton = Input.GetButton ("Fire2");
		Debug.Log("interactionButton: " + interactionButton);
		Debug.Log("interacting: " + interacting);

		if (grounded && Input.GetButtonDown ("Jump")) {
			anim.SetBool ("Ground", false);
			rb.AddForce (new Vector2 (0, jumpForce));
		} else if (climbingChain && Input.GetButtonDown ("Jump")) {
			Destroy (gameObject.GetComponent<HingeJoint2D> ());
			jumpingOffChain = true;
			Debug.Log("JUMP away form teh ROPE!!!!!: " + Mathf.Sign(move) * ropePushForce);
			rb.AddForce (new Vector2 (Mathf.Sign(move) * ropePushForce, jumpForce));
			linkIndex = 0;
			climbingChain = false;
		}

		RaycastHit2D interactableHit;
		Vector2 rayDirection = new Vector2 (1, 0);

		// =================
		// INTERACTIONS
		// =================
		if (facingRight) {
			interactableHit = Physics2D.Raycast (transform.position, rayDirection, interactionRaycastLength, interactableLayer);
		} else {
			rayDirection = new Vector2 (-1, 0);
			interactableHit = Physics2D.Raycast (transform.position, rayDirection, interactionRaycastLength, interactableLayer);
		}

		Debug.DrawRay (transform.position, rayDirection * interactionRaycastLength, Color.green);
		if (interactableHit.collider != null && interactionButton && !interacting) {
			if (interactableHit.collider.gameObject.tag == "Pullable") {
				FixedJoint2D jointGrip = gameObject.AddComponent<FixedJoint2D> ();
				jointGrip.connectedBody = interactableHit.collider.gameObject.GetComponent<Rigidbody2D> ();
				interacting = true;
			}
		}

		if (interacting && !interactionButton) {
			interacting = false;
			FixedJoint2D jointGrip = gameObject.GetComponent<FixedJoint2D> ();
			if (jointGrip){
				Destroy (jointGrip);
				Debug.Log("Destroy the joint grip.....");
			}
		}

	}

	void Flip(){
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		Debug.Log ("Entered Trigger " + col.gameObject.tag);
//		Debug.Log ("Parent: " + col.gameObject.transform.parent.name);
//
//		private GameObject parent = col.gameObject.transform.parent;

		if (col.gameObject.tag == "Chain" && !climbingChain && !jumpingOffChain) {
			Debug.Log("--- GRABBING CHAIN FOR FIRST TIME ---");
//			rb.isKinematic = false;
			climbingChain = true;
			chainLinkPos = gameObject.transform.position;
			if (col.gameObject.tag == "Chain") {
				HingeJoint2D jointGrip = gameObject.AddComponent<HingeJoint2D> ();
				jointGrip.connectedBody = col.gameObject.GetComponent<Rigidbody2D> ();
				chainLink = col.gameObject;
				previousLinkIndex = chainLink.transform.GetSiblingIndex ();
				Debug.Log("SHOULD BE ATTACHED TO CHAIN");
			}
		}
//		 else if (col.gameObject.tag == "PlayerMovable") {
//			Debug.Log ("Collided with object adn controller thingy........");
//			FixedJoint2D jointGrip = gameObject.AddComponent<FixedJoint2D> ();
//			jointGrip.connectedBody = col.gameObject.GetComponent<Rigidbody2D> ();
//		}
	}

	void OnTriggerExit2D (Collider2D col)
	{
		Debug.Log ("Exited Trigger " + col.gameObject.tag);
		if (col.gameObject.tag == "Chain") {
//			rb.isKinematic = true;
			jumpingOffChain = false;
		}
	}


}
