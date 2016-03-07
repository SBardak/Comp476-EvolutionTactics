using UnityEngine;
using System.Collections;

public class Walking : MonoBehaviour {

	private Animator anim;
	private float vert;
	public float speed;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		vert = Input.GetAxis("Vertical");
		anim.SetFloat("Walk", vert);

		if (vert > 0.1f){
			transform.position += transform.forward * vert * speed * Time.deltaTime;
		}
	}
}
