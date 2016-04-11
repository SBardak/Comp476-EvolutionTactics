using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public Button[] _initialButtons;
    public Button[] _newGameButtons;
    public Button[] _pokemonButtons;
    public Text[] _pokemonTexts;

    public List<string> _selectedCharacters = new List<string>();

    private List<string> selectable = new List<string>(new string[]
        { "charmander", "squirtle", "bulbasaur",
            "voltorb", "hoppip", "diglett",
            "charmeleon", "wartotle", "ivysaur",
            "electrode", "skiploom", "dugtrio"
        });

    void Start()
    {
        Init(1);
    }

    private void Init(int option)
    {
        foreach (Button b in _initialButtons)
        {
            if (option == 1)
                b.gameObject.SetActive(true);
            else
                b.gameObject.SetActive(false);
        }

        foreach (Button b in _newGameButtons)
        {
            if (option == 2)
                b.gameObject.SetActive(true);
            else
                b.gameObject.SetActive(false);
        } 

        foreach (Button b in _pokemonButtons)
        {
            if (option == 3)
            {
                b.gameObject.SetActive(true);
                if (_selectedCharacters.Contains(b.name))
                {
                    b.interactable = false;
                }
                if (b.name.Equals("Accept") && _selectedCharacters.Count < 6)
                {
                    b.interactable = false;
                }
            }
            else
                b.gameObject.SetActive(false);
        }

        foreach (Text t in _pokemonTexts)
        {
            if (option == 3)
                t.gameObject.SetActive(true);
            else
                t.gameObject.SetActive(false);
        }
    }

    public void NewGame()
    {
        Init(2);
    }

    public void  ChoosePokemon()
    {
        Init(3);
    }

    public void RandomMap()
    {
        
    }

    public void DefaultMap()
    {
        GameManager.Instance.isPlaying = true;
        //SceneManager.LoadScene(1);
        Application.LoadLevel(1);
    }

    public void Back()
    {
        Init(1);
    }

    int lowLevel = 0, highLevel = 0;

    public void Add(string pokemon)
    {
        int index = selectable.IndexOf(pokemon);
        _pokemonButtons[selectable.IndexOf(pokemon)].interactable = false;
        if (index < 6)
            lowLevel++;
        else
            highLevel++;
        
        if (_selectedCharacters.Count < 6)
        {
            _selectedCharacters.Add(pokemon);
            string s = pokemon;
            if (_selectedCharacters.Count <= 5)
                s += ", ";
            _pokemonTexts[2].GetComponent<Text>().text += s;
        }
        if (lowLevel == 4)
        {
            for (int i = 0; i < 6; i++)
            {
                _pokemonButtons[i].interactable = false;
            }
        }
        if (highLevel == 2)
        {
            for (int i = 6; i < 12; i++)
            {
                _pokemonButtons[i].interactable = false;
            }
        }
        if (_selectedCharacters.Count == 6)
        {
            int i = 0;
            foreach (Button b in _pokemonButtons)
            {
                if (i < 12)
                    b.interactable = false;
                else if (i == 14)
                    b.interactable = true;
                i++;
            }
        }
    }

    public void Reset()
    {
        _selectedCharacters.Clear();
        lowLevel = 0;
        highLevel = 0;
        int i = 0;
        foreach (Button b in _pokemonButtons)
        {
            if (i < 12)
                b.interactable = true;
            else if (i == 14)
                b.interactable = false;
            i++;
        }
        _pokemonTexts[2].GetComponent<Text>().text = "Chosen: ";
    }

    public void Accept()
    {
        if (_selectedCharacters.Count == 6)
        {
            Init(1);
        }
    }
}
