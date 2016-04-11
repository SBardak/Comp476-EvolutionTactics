using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public GameObject logo;
    public Button[] _initialButtons;
    public Button[] _newGameButtons;
    public Button[] _pokemonButtons;
    public Text[] _pokemonTexts;

    public List<string> _selectedCharacters = new List<string>();

    void Start()
    {
        Init(1);
    }

    private void Init(int option)
    {
        if (option == 1 || option == 2)
        {
            logo.gameObject.SetActive(true);
        }
        else if (option == 3)
        {
            logo.gameObject.SetActive(false);
        }

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

        int i = 0;
        foreach (Button b in _pokemonButtons)
        {
            if (option == 3)
            {
                b.gameObject.SetActive(true);
                if (b.name.Equals("Accept") && _selectedCharacters.Count < 6)
                {
                    b.interactable = false;
                }
                else
                {
                    b.interactable = true;
                }
                if (i < 6 && _selectedCharacters.Count == 6)
                {
                    b.interactable = false;
                }
            }
            else
                b.gameObject.SetActive(false);
            
            i++;
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
        GameManager.Instance.isPlaying = true;

        if (_selectedCharacters.Count == 6)
            GameManager.Instance._selectedCharacters = new List<string>(_selectedCharacters);
        
        Application.LoadLevel(2);
    }

    public void DefaultMap()
    {
        GameManager.Instance.isPlaying = true;

        if (_selectedCharacters.Count == 6)
            GameManager.Instance._selectedCharacters = new List<string>(_selectedCharacters);
        
        //SceneManager.LoadScene(1);
        Application.LoadLevel(1);
    }

    public void Back()
    {
        Init(1);
    }

    public void Add(string pokemon)
    {
        
        if (_selectedCharacters.Count < 6)
        {
            _selectedCharacters.Add(pokemon);
            string s = pokemon;
            if (_selectedCharacters.Count <= 5)
                s += ", ";
            _pokemonTexts[1].GetComponent<Text>().text += s;
        }

        if (_selectedCharacters.Count == 6)
        {
            int i = 0;
            foreach (Button b in _pokemonButtons)
            {
                if (i < 6)
                    b.interactable = false;
                else if (i == 8)
                    b.interactable = true;
                i++;
            }
        }
    }

    public void Reset()
    {
        _selectedCharacters.Clear();

        int i = 0;
        foreach (Button b in _pokemonButtons)
        {
            if (i < 6)
                b.interactable = true;
            else if (i == 8)
                b.interactable = false;
            i++;
        }
        _pokemonTexts[1].GetComponent<Text>().text = "Chosen: ";
    }

    public void Accept()
    {
        if (_selectedCharacters.Count == 6)
        {
            Init(1);
        }
    }
}
