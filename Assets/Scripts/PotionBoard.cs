using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    //define the size of the board
    public int width = 8;
    public int height = 8;
    //define some spacing for the board
    public float spacingX = 1f;
    public float spacingY = 1f;
    //offset variables to control board position
    private float offsetX;
    private float offsetY;
    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;
    //get a reference to the collection nodes potionBoard + GO
    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;

    [SerializeField]
    private Potion selectedPotion;

    [SerializeField]
    private bool isProcessingMove;

    [SerializeField]
    List<Potion> potionsToRemove = new();

    //layoutArray
    public ArrayLayout arrayLayout;

    public List<GameObject> potionsToPause = new List<GameObject>(); // Track potions to pause

    //public static of potionboard
    public static PotionBoard Instance;

    private Dictionary<Vector2Int, MatchDirection> matchDirections = new Dictionary<Vector2Int, MatchDirection>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove)
                    return;

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("I have a clicked a potion it is: " + potion.gameObject);

                SelectPotion(potion);
            }
        }
    }

    // Helper method to calculate position consistently
    private Vector3 CalculatePosition(int x, int y, float zPos = 0)
    {
        return new Vector3(
            (x * spacingX) - offsetX,
            (y * spacingY) - offsetY,
            zPos
        );
    }

    private bool IsValidPotionPlacement(int x, int y, PotionType potionType)
    {
        // Check horizontal matches (look left)
        if (x >= 2 &&
            potionBoard[x - 1, y].isUsable && potionBoard[x - 1, y].potion != null &&
            potionBoard[x - 2, y].isUsable && potionBoard[x - 2, y].potion != null)
        {
            if (potionBoard[x - 1, y].potion.GetComponent<Potion>().potionType == potionType &&
                potionBoard[x - 2, y].potion.GetComponent<Potion>().potionType == potionType)
            {
                return false;
            }
        }

        // Check vertical matches (look down)
        if (y >= 2 &&
            potionBoard[x, y - 1].isUsable && potionBoard[x, y - 1].potion != null &&
            potionBoard[x, y - 2].isUsable && potionBoard[x, y - 2].potion != null)
        {
            if (potionBoard[x, y - 1].potion.GetComponent<Potion>().potionType == potionType &&
                potionBoard[x, y - 2].potion.GetComponent<Potion>().potionType == potionType)
            {
                return false;
            }
        }

        return true;
    }

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];

        if (arrayLayout.rows.Length != height)
        {
            Debug.LogError("ArrayLayout rows do not match the height of the board.");
            return;
        }

        for (int i = 0; i < arrayLayout.rows.Length; i++)
        {
            if (arrayLayout.rows[i].row.Length != width)
            {
                Debug.LogError($"Row {i} in ArrayLayout does not match the width of the board.");
                return;
            }
        }

        // Calculate the center offset for the board
        offsetX = ((float)(width - 1) * spacingX) / 2;
        offsetY = ((float)(height - 1) * spacingY) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = CalculatePosition(x, y);

                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    // Try to find a valid potion type that doesn't create matches
                    bool validPotionFound = false;
                    List<int> availableIndices = new List<int>();

                    // Initialize list of available potion indices
                    for (int i = 0; i < potionPrefabs.Length; i++)
                    {
                        availableIndices.Add(i);
                    }

                    // Keep trying different random potions until we find one that doesn't create a match
                    while (availableIndices.Count > 0 && !validPotionFound)
                    {
                        int randomIndex = Random.Range(0, availableIndices.Count);
                        int potionIndex = availableIndices[randomIndex];

                        // Create temporary potion to check its type
                        GameObject tempPotion = Instantiate(potionPrefabs[potionIndex], position, Quaternion.identity);
                        PotionType potionType = tempPotion.GetComponent<Potion>().potionType;
                        Destroy(tempPotion);

                        if (IsValidPotionPlacement(x, y, potionType))
                        {
                            // Create the actual potion
                            GameObject potion = Instantiate(potionPrefabs[potionIndex], position, Quaternion.identity);
                            potion.transform.SetParent(potionParent.transform);
                            potion.GetComponent<Potion>().SetIndicies(x, y);
                            potionBoard[x, y] = new Node(true, potion);
                            potionsToDestroy.Add(potion);
                            potionsToPause.Add(potion);
                            validPotionFound = true;
                        }
                        else
                        {
                            availableIndices.RemoveAt(randomIndex);
                        }
                    }

                    if (!validPotionFound)
                    {
                        Debug.LogError($"Could not find valid potion for position [{x}, {y}]. This shouldn't happen with enough potion types.");
                        // Fall back to random potion as last resort
                        int randomIndex = Random.Range(0, potionPrefabs.Length);
                        GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                        potion.transform.SetParent(potionParent.transform);
                        potion.GetComponent<Potion>().SetIndicies(x, y);
                        potionBoard[x, y] = new Node(true, potion);
                        potionsToDestroy.Add(potion);
                        potionsToPause.Add(potion);
                    }
                }
            }
        }

        Debug.Log("Board initialized with completely random potions (no matches)!");
    }


    private void DestroyPotions()
    {
        if (potionsToDestroy != null)
        {
            foreach (GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    public bool CheckBoard()
    {
        if (GameManager.Instance.isGameEnded)
            return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        potionsToRemove.Clear();
        matchDirections.Clear(); // Clear match directions

        foreach (Node nodePotion in potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    if (!potion.isMatched)
                    {
                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);

                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                            {
                                pot.isMatched = true;
                                Vector2Int position = new Vector2Int(pot.xIndex, pot.yIndex);
                                if (!matchDirections.ContainsKey(position))
                                {
                                    matchDirections[position] = superMatchedPotions.direction;
                                }
                            }

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }


    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Potion potionToRemove in potionsToRemove)
        {
            potionToRemove.isMatched = false;
        }

        RemoveAndRefill(potionsToRemove);
        GameManager.Instance.ProcessTurn(potionsToRemove, _subtractMoves);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        // Map columns to match directions
        Dictionary<int, MatchDirection> columnMatchDirections = new Dictionary<int, MatchDirection>();
        // Map columns to firstElAdded flag
        Dictionary<int, bool> columnFirstElAdded = new Dictionary<int, bool>();

        foreach (Potion potion in _potionsToRemove)
        {
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            // Store the match direction for this column
            MatchDirection matchDir = matchDirections[new Vector2Int(_xIndex, _yIndex)];

            if (!columnMatchDirections.ContainsKey(_xIndex))
            {
                columnMatchDirections[_xIndex] = matchDir;
                columnFirstElAdded[_xIndex] = false; // Initialize firstElAdded to false
            }
            else
            {
                // Prioritize vertical matches over horizontal
                if (matchDir == MatchDirection.Vertical || matchDir == MatchDirection.LongVertical || matchDir == MatchDirection.Super)
                {
                    columnMatchDirections[_xIndex] = matchDir;
                }
            }

            Destroy(potion.gameObject);
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x = 0; x < width; x++)
        {
            // Get the match direction for this column
            MatchDirection matchDir = columnMatchDirections.ContainsKey(x) ? columnMatchDirections[x] : MatchDirection.None;
            bool firstElAdded = columnFirstElAdded.ContainsKey(x) ? columnFirstElAdded[x] : false;

            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                    RefillPotion(x, y, matchDir, ref firstElAdded);
                }
            }

            // Update the firstElAdded flag for the column
            columnFirstElAdded[x] = firstElAdded;
        }
    }



    private void RefillPotion(int x, int y, MatchDirection matchDirection, ref bool firstElAdded)
    {
        int yOffset = 1;

        while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
            yOffset++;
        }

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();
            Vector3 targetPos = CalculatePosition(x, y, potionAbove.transform.position.z);

            potionAbove.MoveToTarget(targetPos);
            potionAbove.SetIndicies(x, y);
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }
        else if (y + yOffset == height)
        {
            // No potion above, need to generate a new potion
            bool isFirstTile = (y == 0);
            SpawnPotionAtPosition(x, y, matchDirection, isFirstTile, ref firstElAdded);
        }
    }

    private void SpawnPotionAtPosition(int x, int y, MatchDirection matchDirection, bool isFirstTile, ref bool firstElAdded)
    {
        Vector3 spawnPos = CalculatePosition(x, height);

        // Determine the potion index based on the color selection logic
        int potionIndex = GetPotionIndexBasedOnBelowTile(x, y, matchDirection, isFirstTile, firstElAdded);

        GameObject newPotion = Instantiate(potionPrefabs[potionIndex], spawnPos, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.GetComponent<Potion>().SetIndicies(x, y);
        potionBoard[x, y] = new Node(true, newPotion);

        Vector3 targetPosition = CalculatePosition(x, y, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);

        // Add the newly spawned potion to the pause list
        potionsToPause.Add(newPotion);

        // After placing the first tile, set firstElAdded to true
        if (!firstElAdded)
        {
            firstElAdded = true;
        }
    }

    private int GetPotionIndexBasedOnBelowTile(int x, int y, MatchDirection matchDirection, bool isFirstTile, bool firstElAdded)
    {
        PotionType? belowPotionType = null;
        if (y > 0 && potionBoard[x, y - 1].potion != null)
        {
            belowPotionType = potionBoard[x, y - 1].potion.GetComponent<Potion>().potionType;
        }

        int chanceOfSameColor = 10; // Default probability reduced from 20% to 10%

        if (matchDirection == MatchDirection.Vertical || matchDirection == MatchDirection.LongVertical || matchDirection == MatchDirection.Super)
        {
            if (isFirstTile)
            {
                if (y == 0) // First tile at the bottom of the grid
                {
                    // Equal chance for each color
                    return Random.Range(0, potionPrefabs.Length);
                }
                else
                {
                    if (!firstElAdded)
                    {
                        // First tile not at the bottom, reduced from 40% to 20%
                        chanceOfSameColor = 20;
                    }
                    else
                    {
                        // Subsequent tiles, reduced from 60% to 30%
                        chanceOfSameColor = 30;
                    }
                }
            }
            else
            {
                // Subsequent tiles, reduced from 60% to 25%
                chanceOfSameColor = 25;
            }
        }
        else if (matchDirection == MatchDirection.Horizontal || matchDirection == MatchDirection.LongHorizontal)
        {
            if (y == 0)
            {
                // Tile at the bottom, equal chance
                return Random.Range(0, potionPrefabs.Length);
            }
            else
            {
                if (!firstElAdded)
                {
                    // First tile reduced from 40% to 15%
                    chanceOfSameColor = 15;
                }
                else
                {
                    // Each tile reduced from 60% to 25%
                    chanceOfSameColor = 25;
                }
            }
        }
        else
        {
            // No specific match direction, use uniform probability
            return Random.Range(0, potionPrefabs.Length);
        }

        if (belowPotionType.HasValue)
        {
            int randomNum = Random.Range(0, 100);
            if (randomNum < chanceOfSameColor)
            {
                // Choose the same color as the tile below
                for (int i = 0; i < potionPrefabs.Length; i++)
                {
                    PotionType potionType = potionPrefabs[i].GetComponent<Potion>().potionType;
                    if (potionType == belowPotionType.Value)
                    {
                        return i;
                    }
                }
                // If no matching color is found, select a random potion
                return Random.Range(0, potionPrefabs.Length);
            }
            else
            {
                // Choose one of the other colors
                List<int> availableIndices = new List<int>();
                for (int i = 0; i < potionPrefabs.Length; i++)
                {
                    PotionType potionType = potionPrefabs[i].GetComponent<Potion>().potionType;
                    if (potionType != belowPotionType.Value)
                    {
                        availableIndices.Add(i);
                    }
                }

                if (availableIndices.Count > 0)
                {
                    // Randomly select from remaining colors
                    int index = Random.Range(0, availableIndices.Count);
                    return availableIndices[index];
                }
                else
                {
                    // If all colors are the same as belowPotionType, select a random potion
                    return Random.Range(0, potionPrefabs.Length);
                }
            }
        }
        else
        {
            // If there is no potion below, select a random potion
            return Random.Range(0, potionPrefabs.Length);
        }
    }




    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMoveTo = 8 - index;

        int randomIndex = Random.Range(0, potionPrefabs.Length);
        Vector3 spawnPos = CalculatePosition(x, height);

        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], spawnPos, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        potionBoard[x, index] = new Node(true, newPotion);

        Vector3 targetPosition = CalculatePosition(x, height - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);

        // Add the newly spawned potion to the pause list
        potionsToPause.Add(newPotion);
    }


    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--)
        {
            if (potionBoard[x, y].potion == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    #region MatchingLogic
    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Horizontal Match");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Vertical Match");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        return null;
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);

        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }

        connectedPotions.Clear();
        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                if (!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
    #endregion

    #region Swapping Potions

    public void SelectPotion(Potion _potion)
    {
        if (selectedPotion == null)
        {
            selectedPotion = _potion;
        }
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
        }
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion, _potion);
            selectedPotion = null;
        }
    }

    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        if (!IsAdjacent(_currentPotion, _targetPotion))
        {
            return;
        }

        DoSwap(_currentPotion, _targetPotion);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            DoSwap(_currentPotion, _targetPotion);
        }
        isProcessingMove = false;
    }

    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    #endregion
}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}


