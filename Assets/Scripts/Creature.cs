using UnityEngine;
using System.Collections;

public class Creature : MonoBehaviour {

	private Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	void OnTriggerEnter2D (Collider2D col)
	{
		Debug.Log("Creature trigger fired " + col.gameObject.name);
		if (col.gameObject.name == "Character") {
			Debug.Log("Creature trigger fired... by Player");
			anim.SetTrigger("composeCreature");
		}
	}

}
