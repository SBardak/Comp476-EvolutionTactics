using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Picker))]
public class HumanPlayer : Player
{
    private bool _isPlaying;

    public Character SelectedCharacter;
    private Tile _selectedTile;
    [SerializeField]
    private Character[] _characters;
    private List<Character> _charactersList;

    private Picker _pickerScript;
    public List<Tile> selectableTiles = null;

    bool _finishedStart = false, _awaitingTurn = false;

    void Awake()
    {
        _pickerScript = GetComponent<Picker>();
        _characters = GetComponentsInChildren<Character>();
        _charactersList = new List<Character>(_characters);

        foreach (var c in _charactersList)
        {
            c.GetComponent<Pathfinding>().OnReachEnd += HumanPlayer_OnReachEnd;
            c.OnDeath += Character_OnDeath;
            c.ControllingPlayer = this;
        }

        // TODO: Make a hashtable with the characters?
    }

    private void Character_OnDeath(Character c)
    {
        _charactersList.Remove(c);
    }

    void Start()
    {
        PositionCharacter(_charactersList);

        _finishedStart = true;
        if (_awaitingTurn)
            StartTurn();
    }

    /// <summary>
    /// Handle event from reaching end of walk
    /// </summary>
    public void HumanPlayer_OnReachEnd()
    {
        isInMovement = false;
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
        if (!_finishedStart)
        {
            _awaitingTurn = true;
            return;
        }

        EnablePicker();

        ClearSelection();

        Debug.Log("Human Start turn");

        // Reactivate all units
        foreach (var c in _charactersList)
            c.Activate();

        // Select first character
        SelectCharacter(_charactersList[0]);
        Map_Movement.Instance.CenterOn(_charactersList[0].gameObject);

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
            if (SelectedCharacter.Moved && SelectedCharacter.IsActivated)
            {
                // Possibly reset to initial position?   
                SelectedCharacter.Moved = false;
                SelectedCharacter._currentTile.Deselect();
                var pt = SelectedCharacter._currentTile;
                ClearCharacterRange(pt);

                SelectedCharacter.SetCurrentTile(_selectedTile);
                var pos  = _selectedTile.transform.position;
                pos.y = SelectedCharacter.transform.position.y;
                SelectedCharacter.transform.position = pos;
                _selectedTile.SetSelected();

                ShowCharacterRange(_selectedTile);
            }
            else
            {
                if (isInMovement)
                    return;
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

    void SelectCharacter(Character c)
    {
        // Select
        SelectedCharacter = c;

        // TODO: Notify UI
        Debug.Log("SELECTED MINE");

        _selectedTile = c._currentTile;
        _selectedTile.SetSelected();
        ShowCharacterRange(_selectedTile);
        UIManager.Instance.CreateHumanPlayerActionUI(c);
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

                    SelectCharacter(t._player);
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
                if (selectableTiles != null && CanBeSelected(t) && !IsMine(t._player))
                {
                    UIManager.Instance.CreateAcceptButtonAttack(t._player);
                    return;
                }

                if (IsMine(t._player) && t._player.IsActivated)
                {
                    ClearSelection();
                    SelectCharacter(t._player);
                }

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
        //FinishCharacterMove();
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

        if (_selectedTile != null)
            _selectedTile.Deselect();
        _selectedTile = null;
        SelectedCharacter._currentTile.Deselect();
        Debug.Log("Clear selection");
        SelectedCharacter = null;
    }

    #endregion


}
