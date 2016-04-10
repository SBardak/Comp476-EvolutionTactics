﻿using UnityEngine;
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

    public GameObject _actionPanel;
    public GameObject _statsPanel;
    public Button _attackButton;
    public Button _waitButton;
    public Button _attackWhereButton;
    public GameObject damageText;

    private static List<Button> buttonList;

    private AudioSource[] buttonSounds;

    void Start()
    {
        UIManager.Instance = this;

        _human = GameObject.Find("Human").GetComponent<HumanPlayer>();
        _humanUI = GameObject.FindGameObjectsWithTag("HumanUI");

        buttonSounds = GetComponents<AudioSource>();

        Transform canvas = GameObject.Find("Canvas").transform;
        _panel2 = Instantiate(_actionPanel, Vector3.zero, Quaternion.identity) as GameObject;
        _panel2.GetComponentInChildren<Text>().text = "";
        _panel2.transform.SetParent(canvas);
        _panel2.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
    }

    public void OnClickEndHumanTurn()
    {
        _human.EndTurn();
        DeleteHumanPlayerActionUI();
    }

    public void OnClickEndSelectedCharacterTurn()
    {
        buttonSounds[0].Play();
        Debug.Log("Wait Button clicked");

        EndCurrentPokemonAction();
    }

    private int damage;
    private Character selectedEnemy;

    public void OnClickAttack()
    {
        buttonSounds[1].Play();
        
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
        //buttonSounds[1].Play();
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
            ShowUI();
        else
            HideUI();
    }

    public void ShowUI()
    {
        ChangeUI(true);
    }

    public void HideUI()
    {
        ChangeUI(false);
    }

    void ChangeUI(bool enabled)
    {
        foreach (GameObject o in _humanUI)
        {
            o.SetActive(enabled);
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

    private GameObject _currentStats = null;
    private GameObject _panel = null;

    public void ShowStats(Character c)
    {
        if (_currentStats != c.gameObject)
        {
            RemoveStats();
            _currentStats = c.gameObject;
            Transform canvas = GameObject.Find("Canvas").transform;
            PokemonStats stats = c.GetComponent<PokemonStats>();
            var n = c.name.Split("("[0]);

            string statsString =
                n[0] +
                "\nType: " + stats.MyType +
                "\n Level: " + stats.Level +
                "\n Health: " + stats._currentHealth + "/" + stats.MaxHealth +
                "\n Attack: " + stats.Attack +
                "\n Defense: " + stats.Defense;

            InstantiatePanel(statsString, canvas);
        }
    }

    public void ShowCollectible(HealingCollectible c)
    {
        if (_currentStats != c.gameObject)
        {
            RemoveStats();
            _currentStats = c.gameObject;
            Transform canvas = GameObject.Find("Canvas").transform;

            string statsString = 
                "Potion" +
                "\nGive " + c.healthGiven + " health.";

            InstantiatePanel(statsString, canvas);
        }
    }

    public void ShowObstacle(TileObstacle t)
    {
        if (_currentStats != t.gameObject)
        {
            RemoveStats();
            _currentStats = t.gameObject;
            Transform canvas = GameObject.Find("Canvas").transform;
            var n = t.name.Split("("[0]);

            string statsString = 
                n[0];

            InstantiatePanel(statsString, canvas);
        }
    }

    private void InstantiatePanel(string message, Transform canvas)
    {
        _panel = Instantiate(_statsPanel, Vector3.zero, Quaternion.identity) as GameObject;
        _panel.GetComponentInChildren<Text>().text = message;
        _panel.transform.SetParent(canvas);
        _panel.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
    }

    public void RemoveStats()
    {
        _currentStats = null;
        Destroy(_panel);
    }

    private List<string> actionList = new List<string>();
    private GameObject _panel2 = null;

    public void AddAction(string action)
    {
        actionList.Add(action);

        if (actionList.Count > 7)
        {
            _panel2.GetComponentInChildren<Text>().text = "";
            for (int i = 0; i < actionList.Count; i++)
            {
                if (i < actionList.Count - 1)
                {
                    actionList[i] = actionList[i + 1];
                    _panel2.GetComponentInChildren<Text>().text += "- " + actionList[i] + "\n";
                }
                else
                    actionList.RemoveAt(i);
            }
        }
        else
        {
            _panel2.GetComponentInChildren<Text>().text += "- " + action + "\n";
        }
    }
}
