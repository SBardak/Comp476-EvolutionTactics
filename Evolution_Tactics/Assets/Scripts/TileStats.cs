using UnityEngine;
using System.Collections;

//Started by Aram
//Determines types for tiles and manages tile list array.


 //manipulating pathfinding can be done by using the array "TileList".
 //Obstacle type implies impassable terrain.

public class TileStats : MonoBehaviour {
    Vector3 position;
    Renderer rend;
    public Texture GrassTexture;
    public Texture GroundTexture;
    public Texture FireTexture;
    public Texture WaterTexture;
    public Texture ElectricTexture;
    public Texture FlyingTexture;

    public enum type
    {
        Fire,
        Water,
        Grass,
        Electric,
        Ground,
        Flying,
        Obstacle
    }

    public type MyType=type.Fire; //by default set to fire.


    public static type[,] TileList;



	void Start () {
        TileList = new type[20, 20];

        position.x = this.transform.position.x;
        position.y = 0;
        position.z = this.transform.position.z;

        position.x = (int)(position.x+0.1f);
        position.z = (int)(position.z+0.1f);

        transform.position = position;
        TileList[(int)transform.position.x, (int)transform.position.z-1] = MyType;
        rend = transform.GetComponent<Renderer>(); 
        
          
        switch (MyType)
        {
            case type.Fire:
                rend.material.color = Color.red;
                rend.material.mainTexture = FireTexture;
                break;
            case type.Water:
                rend.material.color = Color.white;
                rend.material.mainTexture = WaterTexture;
                break;
            case type.Grass:
                rend.material.mainTexture = GrassTexture;
                rend.material.color = Color.white;
                break;
            case type.Electric:
                rend.material.color = Color.yellow;
                rend.material.mainTexture = ElectricTexture;
                break;
            case type.Flying:
                rend.material.mainTexture = FlyingTexture;
                rend.material.color = Color.white;
                break;
            case type.Ground:
                rend.material.mainTexture = GroundTexture;
                rend.material.color = Color.white;
                break;
            default:
                break;
        }
	
	}

	void Update () {
        
	
	}
}
