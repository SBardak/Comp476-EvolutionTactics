using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private static HumanPlayer _human;
    private bool _activateUI = false;
    private GameObject[] _humanUI;

    public Button _attackButton;
    public Button _waitButton;
    public Button _attackWhereButton;

    private static List<Button> buttonList;

    void Start()
    {
        UIManager.Instance = this;

        _human = GameObject.Find("Human").GetComponent<HumanPlayer>();
        _humanUI = GameObject.FindGameObjectsWithTag("HumanUI");
    }

    void Update()
    {
    }

    public void OnClickEndHumanTurn()
    {
        _human.EndTurn();
        DeleteHumanPlayerActionUI();
    }

    public void OnClickEndSelectedCharacterTurn()
    {
        Debug.Log("Wait Button clicked");

        EndCurrentPokemonAction();
    }

    private int damage;
    private Character selectedEnemy;

    public void OnClickAttack()
    {
        Debug.Log("Attack button clicked");
        buttonList[0].enabled = false;
        buttonList[0].GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        Character selected = _human.SelectedCharacter;

        _human.selectableTiles = selected.GetNeighbourTiles();
        _human.EnablePicker();
        Debug.LogWarning("Select an enemy to attack");
    }

    public void CreateAcceptButtonAttack(Character enemy)
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        Button actionButton = Instantiate(_attackWhereButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
        actionButton.transform.SetParent(canvas);
        actionButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
        actionButton.GetComponentInChildren<Text>().text = "Accept attack";

        actionButton.onClick.AddListener(delegate
            {
                UIManager.Instance.Attack();
            });
        buttonList.Add(actionButton);

        selectedEnemy = enemy;

        Character selected = _human.SelectedCharacter;

        AttackAlgorithm attack = selected.GetComponent<AttackAlgorithm>();
        damage = attack.GetDamage(enemy);

        Debug.LogWarning("Press middle button to accept");
    }

    public void Attack()
    {
        _human.SelectedCharacter.Attack(selectedEnemy);
        EndCurrentPokemonAction();
    }

    private void EndCurrentPokemonAction()
    {
        _human.FinishCharacterMove();
        DeleteHumanPlayerActionUI();
    }

    private static IEnumerator WaitForKeyDown(string key)
    {
        while (!Input.GetKeyDown(key))
        {
            yield return null;
        }
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
                actionButton.onClick.AddListener(delegate()
                    {
                        UIManager.Instance.OnClickAttack();
                    });
                yPosition -= 30;
                buttonList.Add(actionButton);
            }

            Button waitButton = Instantiate(_waitButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
            waitButton.transform.SetParent(canvas);
            waitButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, yPosition, 0);
            waitButton.onClick.AddListener(delegate()
                {
                    UIManager.Instance.OnClickEndSelectedCharacterTurn();
                });
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

    private void CreateAttackButtons(List<Character> neighbours)
    {
        int total = 0;
        int yPosition = 0;
        Transform canvas = GameObject.Find("Canvas").transform;
        foreach (Character c in neighbours)
        {
            if (c != null)
            {   
                Button actionButton = Instantiate(_attackWhereButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
                actionButton.transform.SetParent(canvas);
                actionButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, yPosition, 0);
                /*actionButton.onClick.AddListener(delegate
                    {
                        UIManager.Instance.Attack(c);
                    });*/
                //actionButton.onClick.AddListener(() => UIManager.Instance.Attack(c));
                Debug.Log(c.name);
                yPosition -= 30;
                if (total == 0)
                {
                    actionButton.GetComponentInChildren<Text>().text = "Attack Left";
                    Debug.Log(c + " left");
                }
                else if (total == 1)
                {
                    actionButton.GetComponentInChildren<Text>().text = "Attack Right";
                    Debug.Log(c + " right");
                }
                else if (total == 2)
                {
                    actionButton.GetComponentInChildren<Text>().text = "Attack Down";
                    Debug.Log(c + " down");
                }
                else if (total == 3)
                {
                    actionButton.GetComponentInChildren<Text>().text = "Attack Up";
                    Debug.Log(c + " up");
                } 
                buttonList.Add(actionButton);
            }
            total++;
        }
    }
}
