using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private static HumanPlayer _human;
    private bool _activateUI = false;
    private GameObject[] _humanUI;

    public Button _attackButton;
    public Button _waitButton;
    public Button _attackWhereButton;
    public GameObject damageText;

    private static List<Button> buttonList;

    void Start()
    {
        UIManager.Instance = this;

        _human = GameObject.Find("Human").GetComponent<HumanPlayer>();
        _humanUI = GameObject.FindGameObjectsWithTag("HumanUI");
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
        _human.ShowAttackRange();
        Debug.LogWarning("Select an enemy to attack");
    }
    public void CancelAttack()
    {
        buttonList[0].enabled = true;
        buttonList[0].GetComponent<Image>().color = new Color(1, 1, 1);

        for (int i = buttonList.Count - 1; i > 0; i--)
        {
            if (buttonList[i].name == "Accept attack")
            {
                Destroy(buttonList[i].gameObject);
                buttonList.RemoveAt(i);
            }
        }

        _human.selectableTiles = null;
        _human.ClearAttackRange();
    }

    public void CreateAcceptButtonAttack(Character enemy)
    {
        // Kept for now. Possibly change
        // What it does : Checks if an accept attack button is already present. If not, add.
        if (!buttonList.Any(b => b.name == "Accept attack"))
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
        }

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

    public IEnumerator CreateNewDamageLabel(int d)
    {
        GameObject text = Instantiate(damageText, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        text.GetComponent<Text>().text = d.ToString() + "!";
        text.transform.SetParent(GameObject.Find("Canvas").transform);
        text.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(20, 0, 0);
        yield return new WaitForSeconds(2.0f);
        Destroy(text.gameObject);
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
}
