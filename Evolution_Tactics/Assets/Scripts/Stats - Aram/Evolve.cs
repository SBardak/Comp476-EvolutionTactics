using UnityEngine;
using System.Collections;

public class Evolve : MonoBehaviour {

    public int Evolve_Level_Req=5;
    public GameObject NextStage;
    public bool DoIEvolve=false;
	public bool evolved = false;
	

	void Start () {
	
	}
	public void EvolveCheck()
    {
        if (transform.GetComponent<PokemonStats>().Level == Evolve_Level_Req && !evolved)
        {
            evolve();
        }
    }

    void evolve()
    {
        Debug.Log("Evolving");
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);                  //Destroy all childs of the object
        }
        Vector3 TempPosition = transform.position;
        //TempPosition.y += 1;   //If needed, move new model
        GameObject newModel = (GameObject)Instantiate(NextStage);//, TempPosition, Quaternion.identity);//NextStage.transform.rotation);
        newModel.transform.SetParent(transform);                    //attach new model

        newModel.transform.localPosition = NextStage.transform.position;
        newModel.transform.localRotation = NextStage.transform.rotation;

        DoIEvolve = false;



        //transform.gameObject.name = "Charmeleon";

    }

	// Update is called once per frame
	void Update () {
		EvolveCheck();
		if (DoIEvolve==true){
            evolve();
     	}
	}
}
