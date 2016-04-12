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

    public GameObject _actionPanel;
    public GameObject _statsPanel;
    public Button _attackButton;
    public Button _waitButton;
    public Button _attackWhereButton;
    public GameObject damageText;

	private bool paused;

    private static List<Button> buttonList;

    private AudioSource[] buttonSounds;

    void Start()
    {
		paused = false;
        UIManager.Instance = this;

        _human = GameObject.Find("Human").GetComponent<HumanPlayer>();
        _humanUI = GameObject.FindGameObjectsWithTag("HumanUI");

        buttonSounds = GetComponents<AudioSource>();

        Transform canvas = GameObject.Find("Canvas").transform;
        _panel2 = Instantiate(_actionPanel, Vector3.zero, Quaternion.identity) as GameObject;
        _panel2.GetComponentInChildren<Text>().text = "";
        _panel2.transform.SetParent(canvas);
        _panel2.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
        initialAlpha = _panel2.GetComponent<Image>().color.a;
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

    public bool created = false;

    public void CreateAcceptButtonAttack(Character enemy)
    {
        if (!created)
        {
            created = true;
            // Kept for now. Possibly change
            // What it does : Checks if an accept attack button is already present. If not, add.
            if (!buttonList.Any(b => b.name == "Accept attack"))
            {
            
                Transform canvas = GameObject.Find("Canvas").transform;
                Button actionButton = Instantiate(_attackWhereButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
                actionButton.transform.SetParent(canvas);
                actionButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, -25, 0);
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

    public void CreateNewDamageLabel(int d, Vector3 target)
    {
        var go = CreateFloatingLabel(d.ToString() + "!", target, new Vector2(10, -20));

        StartCoroutine(ShowFloatingLabel(go));
    }

    public void CreateNewLevelUpLabel(string message, Vector3 target)
    {
        var go = CreateFloatingLabel(message, target, new Vector2(20, -20));
        go.GetComponent<Text>().color = Color.green;
        StartCoroutine(ShowFloatingLabel(go));
    }

    private GameObject CreateFloatingLabel(string message, Vector3 target, Vector2 offsets)
    {
        GameObject text = Instantiate(damageText, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        text.GetComponent<Text>().text = message;
        text.transform.SetParent(GameObject.Find("Canvas").transform);
        //text.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(20, 0, 0);

        var screenPos = Camera.main.WorldToScreenPoint(target);
        screenPos.x -= offsets.x;
        screenPos.y -= offsets.y;
        text.GetComponent<RectTransform>().position = screenPos;

        return text;
    }

    public IEnumerator ShowFloatingLabel(GameObject obj)
    {
        // Set live time
        var fct = obj.GetComponent<FloatingCombatText>();
        float liveTime = 2.0f;
        if (fct != null)
            liveTime = fct.float_time;
        yield return new WaitForSeconds(liveTime);
        Destroy(obj.gameObject);
    }

    public void ActivateUI()
    {
        if (_human.IsPlaying && !paused)
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
            int yPosition = -25;
            buttonList = new List<Button>();
            Transform canvas = GameObject.Find("Canvas").transform;

            if (character.HasEnemyNeighbours())
            {
                Button actionButton = Instantiate(_attackButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
                actionButton.transform.SetParent(canvas);
                actionButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(25, yPosition, 0);
                actionButton.onClick.AddListener(delegate()
                    {
                        UIManager.Instance.OnClickAttack();
                    });
                yPosition -= 40;
                buttonList.Add(actionButton);
            }

            Button waitButton = Instantiate(_waitButton, new Vector3(0, 0, 0), Quaternion.identity) as Button;
            waitButton.transform.SetParent(canvas);
            waitButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(25, yPosition, 0);
            waitButton.onClick.AddListener(delegate()
                {
                    UIManager.Instance.OnClickEndSelectedCharacterTurn();
                });
            buttonList.Add(waitButton);
        }
    }

    public void DeleteHumanPlayerActionUI()
    {
        created = false;
        if (buttonList != null)
        {
            foreach (Button b in buttonList)
            {
                if (b == null || b.gameObject == null)
                    continue;
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
            Experience exp = c.GetComponent<Experience>();
            var n = c.name.Split("("[0]);

            string statsString =
                n[0] +
                "\nType: " + stats.MyType +
                "\n Level: " + stats.Level +
                "\n Health: " + stats._currentHealth + "/" + stats.MaxHealth +
                "\n Attack: " + stats.Attack +
                "\n Defense: " + stats.Defense +
                (exp == null || exp.NextLevelXP == 0 ? "":
                string.Format("\n EXP: {0}%", exp.experience / (float)exp.NextLevelXP * 100));

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
    float initialAlpha;

    public void AddAction(string action)
    {
        //StopCoroutine(PanelDissapear());
        //_panel2.GetComponent<Image>().color = new Color(0, 0, 0, initialAlpha);
        //_panel2.GetComponentInChildren<Text>().color = new Color(1, 1, 1, 1);
        actionList.Add(action);

        if (actionList.Count > 8)
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
        //StartCoroutine(PanelDissapear());
    }

    private IEnumerator PanelDissapear()
    {
        Image panel = _panel2.GetComponent<Image>();
        Text text = _panel2.GetComponentInChildren<Text>();
        yield return new WaitForSeconds(5.0f);

        while (text.color.a > 0)
        {
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, panel.color.a - (0.02f * Time.deltaTime));
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (0.04f * Time.deltaTime));

            yield return new WaitForSeconds(0.005f);
        }
        actionList.Clear();
        text.text = "";
    }

	public GameObject escapeMenu;

	public void CreateHumanPlayerActionUI()
	{ 
		if (_human.SelectedCharacter != null)
			CreateHumanPlayerActionUI(_human.SelectedCharacter);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape)){
			paused = true;
			// Hide other UI
			HideUI();
			DeleteHumanPlayerActionUI();
			escapeMenu.gameObject.SetActive(true);
		}

	}

	public void Resume(){
		paused = false;
		escapeMenu.gameObject.SetActive(false);
		// Show other UI
		if (_human.IsPlaying){
			ShowUI();
			CreateHumanPlayerActionUI();
		}
	}

	public void Quit(){
		GameManager.Instance.QuitGame();
	}

    public GameEnd GameEndOptions;

    public void OnDestroy()
    {
        Instance = null;
    }
}

[System.Serializable]
public class GameEnd
{
    [SerializeField]
    Text Label,
        dynamicLabel;

    public void ShowVictory()
    {
        Label.gameObject.SetActive(true);
        Label.color = new Color(0, 1, 0, 0.5f);
        Label.text = "You win!";
    }

    public void ShowDefeat()
    {
        Label.gameObject.SetActive(true);
        Label.color = new Color(1, 0, 0, 0.5f);
        Label.text = "You lose...";
    }

    public void UpdateLabel(string text)
    {
        dynamicLabel.gameObject.SetActive(true);
        dynamicLabel.text = text;
    }
}
