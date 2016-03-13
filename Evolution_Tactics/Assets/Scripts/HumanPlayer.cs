using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Picker))]
public class HumanPlayer : Player
{
    private bool _isPlaying;

    public Character SelectedCharacter;
    [SerializeField]
    private Character[] _characters;

    private Picker _pickerScript;

    void Awake()
    {
        _pickerScript = GetComponent<Picker>();
        _characters = GetComponentsInChildren<Character>();

        foreach (var c in _characters)
        {
            c.GetComponent<Pathfinding>().OnReachEnd += HumanPlayer_OnReachEnd;
        }

        // TODO: Make a hashtable with the characters?
    }

    /// <summary>
    /// Handle event from reaching end of walk
    /// </summary>
    private void HumanPlayer_OnReachEnd()
    {
        SelectedCharacter.Moved = true;
        EnablePicker();

        // TODO: Remove
        // For now, deactivate
        FinishCharacterMove();
    }

    /// <summary>
    /// Finish the active character's move
    /// Call from UI or after attacking ?
    /// </summary>
    public void FinishCharacterMove()
    {
        if (SelectedCharacter == null)
            return;

        SelectedCharacter.Deactivate();
        SelectedCharacter = null;
    }

    public override void StartTurn()
    {
        EnablePicker();

        SelectedCharacter = null;

        Debug.Log("Human Start turn");

        // Reactivate all units
        foreach (var c in _characters)
            c.Activate();

        // anything else ?
        _isPlaying = true;

        base.StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown("e"))
            EndTurn();
    }

    public void EndTurn()
    {
        Debug.Log("Human end turn");

        DisablePicker();

        _isPlaying = false;
        GameManager.Instance.NextTurn();
    }

    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
    }

    /// <summary>
    /// Enables picking
    /// </summary>
    public void EnablePicker()
    {
        _pickerScript.enabled = true;
    }

    /// <summary>
    /// Disables picking
    /// </summary>
    public void DisablePicker()
    {
        _pickerScript.enabled = false;
    }

    /// <summary>
    /// Handles selection of a tile
    /// Maybe move to another script
    /// </summary>
    /// <param name="t"></param>
    public void HandleSelection(Tile t)
    {
        // No selected units
        if (SelectedCharacter == null)
        {
            // Nothing on tile
            if (t.player == null)
            {
                // TODO: Something
                Debug.Log("NO CHARACTER");
            }
            else
            {
                // Check if mine or not
                if (IsMine(t.player))
                {
                    // Activated?
                    if (!t.player.IsActivated)
                    {
                        Debug.Log("DEACTIVATED");
                        return;
                    }

                    // Select
                    SelectedCharacter = t.player;

                    // TODO: Notify UI
                    Debug.Log("SELECTED MINE");
                }
                // Not mine, check stats?
                else
                {
                    // TODO: Notify UI
                    Debug.Log("SELECTED ENEMY");
                }
            }
        }
        // Selected unit
        else
        {
            // Selected tile contains something
            // TODO: Add more, check for chars only right now
            if (t.player != null)
            {
                Debug.Log("CONTAINS CHARACTER");
                return;
            }
            // Reachable terrain
            else
            {
                // Check if already moved
                if (SelectedCharacter.Moved)
                    return;

                DisablePicker();

                var pathFinder = SelectedCharacter.GetComponent<Pathfinding>();
                pathFinder._endNode = t;
                pathFinder.CalculateNewPath();
            }
            //else if (_pathFinder == null)
            //{
            //    //_pathFinder = GameObject.Find("PathFinding").GetComponent<Pathfinding>();
            //    _pathFinder = GameObject.FindGameObjectWithTag("Human").GetComponentInChildren<Pathfinding>();
            //}
            //_pathFinder._endNode = _myTile;
            //_pathFinder.CalculateNewPath();
        }

        // Unreachable terrain (further than walkable)
    }

    public bool IsMine(Character o)
    {
        bool mine = false;
        foreach (var c in _characters)
        {
            if (c == o)
            {
                mine = true;
                break;
            }
        }
        return mine;
    }

}
