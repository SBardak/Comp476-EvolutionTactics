using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    private HumanPlayer _human;
    private bool _activateUI = false;
    private GameObject[] _humanUI;

    void Start()
    {
        _human = GameObject.Find("Human").GetComponent<HumanPlayer>();
        _humanUI = GameObject.FindGameObjectsWithTag("HumanUI");
    }

    void Update()
    {
    }

    public void OnClickEndTurn()
    {
        _human.EndTurn();
    }

    public void ActivateUI()
    {
        if (_human.IsPlaying)
        {
            foreach (GameObject o in _humanUI)
            {
                o.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject o in _humanUI)
            {
                o.SetActive(false);
            }
        }
    }

}
