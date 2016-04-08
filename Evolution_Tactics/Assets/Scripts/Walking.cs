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
		if (vert > 0.1f || vert < -0.1f){
			anim.SetBool("Walk", true);
			transform.position += transform.forward * vert * speed * Time.deltaTime;
		}else{
			anim.SetBool("Walk", false);
		}


	}
}
