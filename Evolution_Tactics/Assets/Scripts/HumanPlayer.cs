using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Picker))]
public class HumanPlayer : Player
{
    #region Fields

    public Character SelectedCharacter;
    private Tile _selectedTile;
    private Tile _hoveredTile = null;
    [SerializeField]
    private Character[] _characters;
    private List<Character> _charactersList;

    private Picker _pickerScript;
    public List<Tile> selectableTiles = null;

    bool _finishedStart = false, 
        _awaitingTurn = false, 
        _showingAttack = false,
        _isPlaying;

    #endregion Fields

    #region Properties

    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
    }

    public int AverageLevel
    {
        get
        {
            if (_charactersList.Count == 0)
                return 1;

            int avg = 0;
            foreach (var c in _charactersList)
                avg += c.GetComponent<PokemonStats>().Level;
            return Mathf.Max(avg / _charactersList.Count, 1);
        }
    }

    #endregion Properties

    #region Methods

    #region Init/Preperation

    void Awake()
    {
        _pickerScript = GetComponent<Picker>();
        _characters = GetComponentsInChildren<Character>();

        PrepareCharacters(new List<Character>(_characters));
    }

    public void PrepareCharacters(List<Character> chars)
    {
        _charactersList = chars;
        PrepareCharacters();
    }
    public void PrepareCharacters()
    {
        foreach (var c in _charactersList)
        {
            c.GetComponent<Pathfinding>().OnReachEnd += HumanPlayer_OnReachEnd;
            c.OnDeath += Character_OnDeath;
            c.ControllingPlayer = this;
        }

        PositionCharacter(_charactersList);
    }

    private void Character_OnDeath(Character c)
    {
        _charactersList.Remove(c);
    }

    void Start()
    {
        _finishedStart = true;
        if (_awaitingTurn)
            StartTurn();
    }

    #endregion Init/Preperation

    #region Turn

    /// <summary>
    /// Handle event from reaching end of walk
    /// </summary>
    public void HumanPlayer_OnReachEnd()
    {
        UIManager.Instance.ShowUI();

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
        ClearAttackRange();
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

        if (_charactersList.Count == 0)
            return;

        // Reactivate all units
        foreach (var c in _charactersList)
            c.Activate();

        // Select first character
        UIManager.Instance.DeleteHumanPlayerActionUI();
        SelectCharacter(_charactersList[0]);
        Map_Movement.Instance.CenterOn(_charactersList[0].gameObject);

        // anything else ?
        _isPlaying = true;

        UIManager.Instance.ShowUI();

        base.StartTurn();
    }

    public void EndTurn()
    {
        Debug.Log("Human end turn");
        ClearSelection();

        DisablePicker();

        _isPlaying = false;
        GameManager.Instance.NextTurn();
    }

    #endregion Turn

    #region Input

    void Update()
    {
        if (IsPlaying)
        {
            if (Input.GetKeyDown("e"))
                EndTurn();
            if (Input.GetMouseButtonDown(1))
                HandleRightMouse();

            if (Input.GetKeyDown("1"))
                GameManager.Instance.DeadPlayer(this);
            if (Input.GetKeyDown("2"))
                GameManager.Instance.DeadAI(new AIPlayer());
        }
    }

    public void HandleRightMouse()
    {
        Debug.Log("Right mouse");
        if (SelectedCharacter != null && !isInMovement)
        {
            if (_showingAttack)
            {
                UIManager.Instance.CancelAttack();

                ClearAttackRange(SelectedCharacter._currentTile);
            }
            else if (SelectedCharacter.Moved && SelectedCharacter.IsActivated)
            {
                UIManager.Instance.DeleteHumanPlayerActionUI();

                // Resets character to initial position
                SelectedCharacter.Moved = false;
                SelectedCharacter._currentTile.Deselect();
                var pt = SelectedCharacter._currentTile;
                ClearCharacterRange(pt);

                SelectedCharacter.SetCurrentTile(_selectedTile);
                var pos = _selectedTile.transform.position;
                pos.y = SelectedCharacter.transform.position.y;
                SelectedCharacter.transform.position = pos;
                _selectedTile.SetSelected();

                ShowCharacterRange(_selectedTile);

                UIManager.Instance.CreateHumanPlayerActionUI(SelectedCharacter);
            }
            else
            {
                UIManager.Instance.DeleteHumanPlayerActionUI();

                // End of line. Clear all selections
                ClearSelection();
            }
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

    #endregion Input

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

        var v = c.GetComponent<CharacterVoice>();
        if (v != null)
            v.PlayVoice();
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
            if (!t.HasPlayer)
            {
                // TODO: Something
                Debug.Log("NO CHARACTER");
            }
            else
            {
                // Check if mine or not
                if (IsMine(t._character))
                {
                    // Activated?
                    if (!t._character.IsActivated)
                    {
                        Debug.Log("DEACTIVATED");
                        return;
                    }

                    SelectCharacter(t._character);
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
            if (t.HasPlayer)
            {
                if (selectableTiles != null && CanBeSelected(t) && !IsMine(t._character))
                {
                    if (!UIManager.Instance.created)
                        UIManager.Instance.CreateAcceptButtonAttack(t._character);
                    return;
                }

                if (IsMine(t._character) && !SelectedCharacter.Moved && t._character.IsActivated)
                {
                    ClearSelection();
                    SelectCharacter(t._character);
                }

                Debug.Log("CONTAINS CHARACTER");
                return;
            }
            else if (t.HasObstacle)
            {
                // ?
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
        UIManager.Instance.HideUI();

        isInMovement = true;

        _selectedTile.Deselect();
        ClearCharacterRange(_selectedTile);
        DisablePicker();

        var pathFinder = SelectedCharacter.GetComponent<Pathfinding>();
        /* pathFinder._endNode = t;
        pathFinder.CalculateNewPath();*/
        pathFinder.SetPath(t);
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
        foreach (var c in _charactersList)
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

    public void ShowAttackRange()
    {
        if (SelectedCharacter != null)
            ShowAttackRange(SelectedCharacter._currentTile);
    }

    public void ShowAttackRange(Tile t)
    {
        if (t == null)
            return;

        if (SelectedCharacter != null)
            ClearCharacterRange(SelectedCharacter._currentTile);

        t.AttackUI();
        _showingAttack = true;
    }

    public void ClearAttackRange()
    {
        if (SelectedCharacter != null)
            ClearAttackRange(SelectedCharacter._currentTile);
    }

    public void ClearAttackRange(Tile t)
    {
        if (t == null)
            return;
        t.ClearAttackUI();
        _showingAttack = false;
    }

    public void HandleHover(Tile t)
    {
        if (isInMovement || _showingAttack || !t.HasPlayer)
            return;

        // 'Cache' hovered tile
        if (_hoveredTile == t)
            return;
        _hoveredTile = t;

        if (SelectedCharacter != null)
            ClearCharacterRange(_selectedTile);
        ShowCharacterRange(t);
    }

    public void HandleHoverOut(Tile t)
    {
        if (isInMovement || _showingAttack || !t.HasPlayer)
            return;

        _hoveredTile = null;

        ClearCharacterRange(t);
        if (SelectedCharacter != null)
            ShowCharacterRange(_selectedTile);
    }

    void ClearSelection()
    {
        ClearCharacterRange(_selectedTile);

        if (_selectedTile != null)
            _selectedTile.Deselect();
        _selectedTile = null;

        if (SelectedCharacter == null)
            return;
        SelectedCharacter._currentTile.Deselect();
        Debug.Log("Clear selection");
        SelectedCharacter = null;

        if (_hoveredTile != null)
            HandleHoverOut(_hoveredTile);
    }

    #endregion

    #endregion Methods
}
