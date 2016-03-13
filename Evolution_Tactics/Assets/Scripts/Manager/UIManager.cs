using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private HumanPlayer _human;
    private bool _activateUI = false;
    private GameObject[] _humanUI;

    public Button _attackButton;
    public Button _waitButton;

    void Start()
    {
        _human = GameObject.Find("Human").GetComponent<HumanPlayer>();
        _humanUI = GameObject.FindGameObjectsWithTag("HumanUI");
    }

    void Update()
    {
    }

    public void OnClickEndHumanTurn()
    {
        GameObject.Find("Human").GetComponent<HumanPlayer>().EndTurn();
    }

    public void OnClickEndSelectedCharacterTurn()
    {
        //Only works for one human character for now
        // We should add that it works only with selected character
        Debug.Log("Wait button not yet implemented");
    }

    public void OnClickAttack()
    {
        // Select next nearby enemy to attack
        Debug.Log("Attack button not yet implemented");
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

    private List<Button> buttonList;

    public void CreateHumanPlayerActionUI(Character character)
    {
        if (buttonList == null)
        {
            int yPosition = 0;
            buttonList = new List<Button>();
            Transform canvas = GameObject.Find("Canvas").transform;

            if (character.HasEnemyNeighbours())
            {
                Button actionButton = Instantiate(_attackButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
                actionButton.transform.SetParent(canvas);
                actionButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, yPosition, 0);
                yPosition -= 30;
                buttonList.Add(actionButton);
            }

            Button waitButton = Instantiate(_waitButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
            waitButton.transform.SetParent(canvas);
            waitButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, yPosition, 0);
            buttonList.Add(waitButton);
        }
    }

    public void DeleteHumanPlayerActionUI()
    {
        if (buttonList != null)
        {
            foreach (Button b in buttonList)
            {
                Destroy(b.gameObject);
            }
        }
        buttonList = null;
    }
}
