using UnityEngine;
using System.Collections;

public class Evolve : MonoBehaviour {

    public int Evolve_Level_Req=5;
    public GameObject NextStage;

	

	void Start () {
	
	}
	public void EvolveCheck()
    {
        if (transform.GetComponent<PokemonStats>().Level == Evolve_Level_Req)
        {
            evolve();
        }
    }

    void evolve()
    {

    }

	// Update is called once per frame
	void Update () {
	
	}
}
