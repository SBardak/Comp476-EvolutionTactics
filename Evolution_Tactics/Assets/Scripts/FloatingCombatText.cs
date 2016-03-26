using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingCombatText : MonoBehaviour
{
    public int damage = 10;
    static string text;
    Text display;
    Vector3 StartLoc;
    public int float_time = 2;

    void Start()
    {
        text = damage + "!";
        display = GetComponent<Text>();
        
        StartLoc = transform.position;
    }
	
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), 1);
    }
}
