using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingCombatText : MonoBehaviour {
    public int damage=10;
    static string text;
    static float timeToDeath = 100;
    Text display;
    Vector3 StartLoc;
    public int float_time = 2;
	// Use this for initialization
	void Start () {
        text = damage + "!";
        display = GetComponent<Text>();
        
        StartLoc = transform.position;
	}

    public static void setDamage(int damage)
    {
        text = damage + "!";
        timeToDeath = 0;
    }
	
	// Update is called once per frame
	void Update () {
        
        timeToDeath += Time.deltaTime;

        if (timeToDeath<=float_time)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), 1);
        }
        if (timeToDeath>=float_time)
        {
            text = " ";
            transform.position = StartLoc;
        }

              

        display.text = text;
    }
}
