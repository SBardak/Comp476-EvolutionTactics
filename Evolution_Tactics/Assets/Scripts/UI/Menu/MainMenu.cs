using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public Button[] _initialButtons;
    public Button[] _newGameButtons;

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
    }

    public void NewGame()
    {
        Init(2);
    }

    public void  ChoosePokemon()
    {
        
    }

    public void RandomMap()
    {
        
    }

    public void DefaultMap()
    {
        
    }

    public void Back()
    {
        Init(1);
    }
}
