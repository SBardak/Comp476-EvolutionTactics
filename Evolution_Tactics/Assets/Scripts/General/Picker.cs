using UnityEngine;
using System.Collections;

public class Picker : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            GetClickObjects();
        }
    }

    void GetClickObjects()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] rayHits = Physics.RaycastAll(mouseRay);

        int cpt = 0;
        foreach (RaycastHit rayHit in rayHits)
        {
            var selector = rayHit.collider.gameObject.GetComponent<Selector>();
            if (selector != null)
                selector.RayHit(cpt);
            ++cpt;
        }
    }
}
