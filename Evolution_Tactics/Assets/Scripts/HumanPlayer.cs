using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Picker))]
public class HumanPlayer : Player
{
    private bool _isPlaying;

    public Character SelectedCharacter;
    private Tile _selectedTile;
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
        SelectedCharacter.Deactivate();
        ClearSelection();
    }

    public override void StartTurn()
    {
        EnablePicker();

        ClearSelection();

        Debug.Log("Human Start turn");

        // Reactivate all units
        foreach (var c in _characters)
            c.Activate();

        // anything else ?
        _isPlaying = true;

        base.StartTurn();
    }

    void ClearSelection()
    {
        if (SelectedCharacter == null)
            return;

        _selectedTile.ClearMovementUI(SelectedCharacter.GetComponent<MovementRange>().Range);
        _selectedTile = null;
        SelectedCharacter = null;
    }

    void Update()
    {
        if (Input.GetKeyDown("e"))
            EndTurn();
        if (Input.GetMouseButtonDown(1))
            HandleRightMouse();
    }

    public void HandleRightMouse()
    {
        Debug.Log("Right mouse");
        if (SelectedCharacter != null)
        {
            if (SelectedCharacter.Moved)
            {
                // Possibly reset to initial position?   
            }
            else
            {
                ClearSelection();
            }
        }
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
            if (t._player == null)
            {
                // TODO: Something
                Debug.Log("NO CHARACTER");
            }
            else
            {
                // Check if mine or not
                if (IsMine(t._player))
                {
                    // Activated?
                    if (!t._player.IsActivated)
                    {
                        Debug.Log("DEACTIVATED");
                        return;
                    }

                    // Select
                    SelectedCharacter = t._player;

                    // TODO: Notify UI
                    Debug.Log("SELECTED MINE");

                    _selectedTile = t;
                    _selectedTile.MovementUI(SelectedCharacter.GetComponent<MovementRange>().Range);
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
            if (t._player != null)
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

                // Check movement range
                // TODO: Change
                if (!t.IsMovementTile())
                {
                    Debug.Log("INVALID MOVE");
                    return;
                }

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
