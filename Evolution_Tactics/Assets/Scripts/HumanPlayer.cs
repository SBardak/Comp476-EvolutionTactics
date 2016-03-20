using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Picker))]
public class HumanPlayer : Player
{
    private bool _isPlaying;

    public Character SelectedCharacter;
    public Character SelectedEnemyCharacter;
    private Tile _selectedTile;
    [SerializeField]
    private Character[] _characters;

    private Picker _pickerScript;
    public List<Tile> selectableTiles = null;

    void Awake()
    {
        _pickerScript = GetComponent<Picker>();
        _characters = GetComponentsInChildren<Character>();

        foreach (var c in _characters)
        {
            c.GetComponent<Pathfinding>().OnReachEnd += HumanPlayer_OnReachEnd;
            c.ControllingPlayer = this;
        }

        // TODO: Make a hashtable with the characters?
    }

    /// <summary>
    /// Handle event from reaching end of walk
    /// </summary>
    public void HumanPlayer_OnReachEnd()
    {
        SelectedCharacter.Moved = true;
        Debug.Log("Human reach end");
        EnablePicker();

        EndMovement();
    }

    /// <summary>
    /// Finish the active character's move
    /// Call from UI or after attacking ?
    /// </summary>
    public void FinishCharacterMove()
    {
        isInMovement = false;

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





    #region Tile stuff

    /// <summary>
    /// Handles selection of a tile
    /// Maybe move to another script
    /// </summary>
    /// <param name="t"></param>
    public void HandleSelection(Tile t)
    {
        if (selectableTiles != null && CanBeSelected(t))
        {
            HandleEnemySelection(t);
            return;
        }

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
                    _selectedTile.SetSelected();
                    ShowCharacterRange(_selectedTile);
                    UIManager.Instance.CreateHumanPlayerActionUI(t._player);
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
            else
            {
                // Check if already moved
                if (SelectedCharacter.Moved)
                    return;

                // TODO: Change
                // Check movement range
                if (!t.IsMovementTile())
                {
                    // Unreachable terrain (further than walkable)
                    Debug.Log("INVALID MOVE");
                    return;
                }

                // Reachable terrain
                BeginMovement(t);
            }
        }
    }

    public void HandleEnemySelection(Tile t)
    {
        // No selected units
        if (SelectedEnemyCharacter == null)
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
                if (!IsMine(t._player))
                {
                    // Select
                    SelectedEnemyCharacter = t._player;

                    // TODO: Notify UI
                    Debug.Log("SELECTED ENEMY");

                    _selectedTile = t;
                    _selectedTile.SetSelected();
                    ShowCharacterRange(_selectedTile);
                    UIManager.Instance.CreateAcceptButtonAttack(t._player);
                }
                // Not mine, check stats?
                else
                {
                    // TODO: Notify UI
                    Debug.Log("SELECTED MINE");
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
            else
            {
                // Check if already moved
                if (SelectedCharacter.Moved)
                    return;

                // TODO: Change
                // Check movement range
                if (!t.IsMovementTile())
                {
                    // Unreachable terrain (further than walkable)
                    Debug.Log("INVALID MOVE");
                    return;
                }

                // Reachable terrain
                BeginMovement(t);
            }
        }
    }

    private bool CanBeSelected(Tile tile)
    {
        return selectableTiles.Contains(tile);
    }

    bool isInMovement = false;

    void BeginMovement(Tile t)
    {
        isInMovement = true;

        _selectedTile.Deselect();
        ClearCharacterRange(_selectedTile);
        DisablePicker();

        var pathFinder = SelectedCharacter.GetComponent<Pathfinding>();
        pathFinder._endNode = t;
        pathFinder.CalculateNewPath();
    }

    /// <summary>
    /// Options when movement end:
    /// Cancel movement
    /// Attack player
    /// End character turn
    /// </summary>
    void EndMovement()
    {
        // Activate reached tile
        SelectedCharacter._currentTile.SetSelected();

        // TODO: Remove
        // For now, deactivate
        FinishCharacterMove();
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

    public void ShowCharacterRange(Tile t)
    {
        if (t == null)
            return;
        t.MovementUI();
    }

    public void ClearCharacterRange(Tile t)
    {
        if (t == null)
            return;
        t.ClearMovementUI();
    }

    public void HandleHover(Tile t)
    {
        if (isInMovement || t._player == null)
            return;

        if (SelectedCharacter != null)
            ClearCharacterRange(_selectedTile);
        ShowCharacterRange(t);
    }

    public void HandleHoverOut(Tile t)
    {
        if (isInMovement || t._player == null)
            return;

        ClearCharacterRange(t);
        if (SelectedCharacter != null)
            ShowCharacterRange(_selectedTile);
    }

    void ClearSelection()
    {
        if (SelectedCharacter == null)
            return;

        ClearCharacterRange(_selectedTile);
        _selectedTile.Deselect();
        _selectedTile = null;
        SelectedCharacter._currentTile.Deselect();
        Debug.Log("Clear selection");
        SelectedCharacter = null;
        SelectedEnemyCharacter = null;
    }

    #endregion


}
