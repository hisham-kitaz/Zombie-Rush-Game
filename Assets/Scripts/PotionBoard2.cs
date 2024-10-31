using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard2 : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource2;
    [SerializeField] private AudioClip[] matchSounds2 = new AudioClip[3]; // Array of 3 match sounds
    private int currentMatchSoundIndex2 = 0; // Track which sound to play next
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
    public GameObject[] potionPrefabs2; // Renamed
    //get a reference to the collection nodes potionBoard2 + GO
    public Node[,] potionBoard2; // Renamed
    public GameObject potionBoardGO2; // Renamed

    public List<GameObject> potionsToDestroy2 = new(); // Renamed
    public GameObject potionParent2; // Renamed

    [SerializeField]
    private Potion selectedPotion2; // Renamed

    [SerializeField]
    private bool isProcessingMove2; // Renamed

    [SerializeField]
    List<Potion> potionsToRemove2 = new(); // Renamed

    //layoutArray
    public ArrayLayout arrayLayout2; // Renamed

    public List<GameObject> potionsToPause2 = new List<GameObject>(); // Renamed

    //public static of potionboard2
    public static PotionBoard2 Instance2; // Renamed

    private void Awake()
    {
        Instance2 = this;
        if (audioSource2 == null)
        {
            audioSource2 = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        InitializeBoard2(); // Renamed
    }
    private void PlayNextMatchSound2()
    {
        if (audioSource2 != null && matchSounds2 != null && matchSounds2.Length > 0)
        {
            // Check if we have a valid sound at the current index
            if (matchSounds2[currentMatchSoundIndex2] != null)
            {
                audioSource2.PlayOneShot(matchSounds2[currentMatchSoundIndex2]);

                // Move to next sound index, wrap around to 0 if we reach the end
                currentMatchSoundIndex2 = (currentMatchSoundIndex2 + 1) % matchSounds2.Length;
            }
            else
            {
                Debug.LogWarning($"Match sound 2 at index {currentMatchSoundIndex2} is null!");
            }
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove2) // Renamed
                    return;

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("I have clicked a potion, it is: " + potion.gameObject);

                SelectPotion2(potion); // Renamed
            }
        }
    }

    // Helper method to calculate position consistently
    private Vector3 CalculatePosition2(int x, int y, float zPos = 0) // Renamed
    {
        return new Vector3(
            (x * spacingX) - offsetX,
            (y * spacingY) - offsetY,
            zPos
        );
    }

    private bool IsValidPotionPlacement2(int x, int y, PotionType potionType) // Renamed
    {
        // Check horizontal matches (look left)
        if (x >= 2 &&
            potionBoard2[x - 1, y].isUsable && potionBoard2[x - 1, y].potion != null &&
            potionBoard2[x - 2, y].isUsable && potionBoard2[x - 2, y].potion != null)
        {
            if (potionBoard2[x - 1, y].potion.GetComponent<Potion>().potionType == potionType &&
                potionBoard2[x - 2, y].potion.GetComponent<Potion>().potionType == potionType)
            {
                return false;
            }
        }

        // Check vertical matches (look down)
        if (y >= 2 &&
            potionBoard2[x, y - 1].isUsable && potionBoard2[x, y - 1].potion != null &&
            potionBoard2[x, y - 2].isUsable && potionBoard2[x, y - 2].potion != null)
        {
            if (potionBoard2[x, y - 1].potion.GetComponent<Potion>().potionType == potionType &&
                potionBoard2[x, y - 2].potion.GetComponent<Potion>().potionType == potionType)
            {
                return false;
            }
        }

        return true;
    }

    void InitializeBoard2() // Renamed
    {
        DestroyPotions2(); // Renamed
        potionBoard2 = new Node[width, height]; // Renamed

        // Initialize all nodes to avoid null references
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                potionBoard2[x, y] = new Node(false, null);
            }
        }


        // Calculate the center offset for the board
        offsetX = ((float)(width - 1) * spacingX) / 2;
        offsetY = ((float)(height - 1) * spacingY) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = CalculatePosition2(x, y); // Renamed

                if (arrayLayout2.rows[y].row[x]) // Renamed
                {
                    potionBoard2[x, y] = new Node(false, null); // Renamed
                }
                else
                {
                    bool validPotionFound = false;

                    // Keep trying until we find a valid potion type
                    while (!validPotionFound)
                    {
                        int potionIndex = GetAdjustedRandomPotionIndex(x, y);

                        // Create temporary potion to check its type
                        PotionType potionType = potionPrefabs2[potionIndex].GetComponent<Potion>().potionType;

                        if (IsValidPotionPlacement2(x, y, potionType)) // Renamed
                        {
                            // Create the actual potion
                            GameObject potion = Instantiate(potionPrefabs2[potionIndex], position, Quaternion.identity); // Renamed
                            potion.transform.SetParent(potionParent2.transform); // Renamed
                            potion.GetComponent<Potion>().SetIndicies(x, y);
                            potionBoard2[x, y] = new Node(true, potion); // Renamed
                            potionsToDestroy2.Add(potion); // Renamed
                            potionsToPause2.Add(potion); // Renamed
                            validPotionFound = true;
                        }
                    }
                }
            }
        }

        Debug.Log("Board2 initialized with no initial matches!"); // Renamed
    }


    private void DestroyPotions2() // Renamed
    {
        if (potionsToDestroy2 != null) // Renamed
        {
            foreach (GameObject potion in potionsToDestroy2) // Renamed
            {
                Destroy(potion);
            }
            potionsToDestroy2.Clear(); // Renamed
        }
    }

    public bool CheckBoard2() // Renamed
    {
        if (GameManager.Instance.isGameEnded)
            return false;
        Debug.Log("Checking Board2"); // Renamed
        bool hasMatched = false;

        potionsToRemove2.Clear(); // Renamed

        foreach (Node nodePotion in potionBoard2) // Renamed
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
                if (potionBoard2[x, y].isUsable) // Renamed
                {
                    Potion potion = potionBoard2[x, y].potion.GetComponent<Potion>(); // Renamed

                    if (!potion.isMatched)
                    {
                        MatchResult2 matchedPotions = IsConnected2(potion); // Correctly Renamed

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult2 superMatchedPotions = SuperMatch2(matchedPotions); // Correctly Renamed

                            potionsToRemove2.AddRange(superMatchedPotions.connectedPotions); // Renamed

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                                pot.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard2(bool _subtractMoves) // Renamed
    {
        foreach (Potion potionToRemove in potionsToRemove2) // Renamed
        {
            potionToRemove.isMatched = false;
        }

        RemoveAndRefill2(potionsToRemove2); // Renamed
        GameManager.Instance.ProcessTurn(potionsToRemove2, _subtractMoves); // Renamed
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard2()) // Renamed
        {
            PlayNextMatchSound2();
            StartCoroutine(ProcessTurnOnMatchedBoard2(false)); // Renamed
        }
    }

    private void RemoveAndRefill2(List<Potion> _potionsToRemove) // Renamed
    {
        foreach (Potion potion in _potionsToRemove) // Renamed
        {
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            Destroy(potion.gameObject);
            potionBoard2[_xIndex, _yIndex] = new Node(true, null); // Renamed
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard2[x, y].potion == null) // Renamed
                {
                    Debug.Log("The location X: " + x + " Y: " + y + " is empty, attempting to refill it.");
                    RefillPotion2(x, y); // Renamed
                }
            }
        }
    }

    private void RefillPotion2(int x, int y) // Renamed
    {
        int yOffset = 1;

        while (y + yOffset < height && potionBoard2[x, y + yOffset].potion == null) // Renamed
        {
            yOffset++;
        }

        if (y + yOffset < height && potionBoard2[x, y + yOffset].potion != null) // Renamed
        {
            Potion potionAbove = potionBoard2[x, y + yOffset].potion.GetComponent<Potion>(); // Renamed
            Vector3 targetPos = CalculatePosition2(x, y, potionAbove.transform.position.z); // Renamed

            potionAbove.MoveToTarget(targetPos);
            potionAbove.SetIndicies(x, y);
            potionBoard2[x, y] = potionBoard2[x, y + yOffset]; // Renamed
            potionBoard2[x, y + yOffset] = new Node(true, null); // Renamed
        }

        if (y + yOffset == height)
        {
            SpawnPotionAtTop2(x); // Renamed
        }
    }
    // Add these methods to your PotionBoard2 class

    private List<PotionType> GetNeighboringPotionTypes(int x, int y)
    {
        List<PotionType> neighboringPotionTypes = new List<PotionType>();

        // Define the directions for the 8 neighbors
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                if (potionBoard2[nx, ny] != null && potionBoard2[nx, ny].isUsable && potionBoard2[nx, ny].potion != null)
                {
                    PotionType neighborType = potionBoard2[nx, ny].potion.GetComponent<Potion>().potionType;
                    neighboringPotionTypes.Add(neighborType);
                }
            }
        }

        return neighboringPotionTypes;
    }


    private int GetAdjustedRandomPotionIndex(int x, int y)
    {
        List<PotionType> neighboringPotionTypes = GetNeighboringPotionTypes(x, y);

        // Initialize counts to 1 (x), as per the logic
        Dictionary<PotionType, int> colorCounts = new Dictionary<PotionType, int>();
        int totalPotionTypes = potionPrefabs2.Length;

        // Get all possible potion types from potionPrefabs2
        List<PotionType> allPotionTypes = new List<PotionType>();
        for (int i = 0; i < totalPotionTypes; i++)
        {
            PotionType potionType = potionPrefabs2[i].GetComponent<Potion>().potionType;
            if (!allPotionTypes.Contains(potionType))
            {
                allPotionTypes.Add(potionType);
                colorCounts[potionType] = 1; // Start with x
            }
        }

        // Count neighboring potions
        foreach (PotionType neighborType in neighboringPotionTypes)
        {
            if (colorCounts.ContainsKey(neighborType))
            {
                colorCounts[neighborType] += 1; // Add x for each neighbor of that type
            }
        }

        // Now sum up the total adjusted probabilities
        int totalX = 0;
        foreach (int count in colorCounts.Values)
        {
            totalX += count;
        }

        // Build a list of potion types and their cumulative probabilities
        List<PotionType> potionTypesList = new List<PotionType>();
        List<float> cumulativeProbabilities = new List<float>();
        float cumulative = 0f;
        foreach (PotionType potionType in allPotionTypes)
        {
            int count = colorCounts[potionType];
            float probability = (float)count / totalX;
            cumulative += probability;
            potionTypesList.Add(potionType);
            cumulativeProbabilities.Add(cumulative);
        }

        // Generate a random float between 0 and 1
        float rand = UnityEngine.Random.value;

        // Find which potion type corresponds to rand
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            if (rand <= cumulativeProbabilities[i])
            {
                PotionType selectedPotionType = potionTypesList[i];

                // Return the index of this potion type in potionPrefabs2
                for (int j = 0; j < potionPrefabs2.Length; j++)
                {
                    PotionType prefabPotionType = potionPrefabs2[j].GetComponent<Potion>().potionType;
                    if (prefabPotionType == selectedPotionType)
                    {
                        return j;
                    }
                }
            }
        }

        // Fallback in case something went wrong
        return Random.Range(0, potionPrefabs2.Length);
    }

    private void SpawnPotionAtTop2(int x) // Renamed
    {
        int index = FindIndexOfLowestNull2(x); // Renamed
        int locationToMoveTo = 8 - index;

        int potionIndex = GetAdjustedRandomPotionIndex(x, index);

        Vector3 spawnPos = CalculatePosition2(x, height); // Renamed

        GameObject newPotion = Instantiate(potionPrefabs2[potionIndex], spawnPos, Quaternion.identity); // Renamed
        newPotion.transform.SetParent(potionParent2.transform); // Renamed
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        potionBoard2[x, index] = new Node(true, newPotion); // Renamed

        Vector3 targetPosition = CalculatePosition2(x, height - locationToMoveTo, newPotion.transform.position.z); // Renamed
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);

        // Add the newly spawned potion to the pause list
        potionsToPause2.Add(newPotion); // Renamed
    }


    private int FindIndexOfLowestNull2(int x) // Renamed
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--)
        {
            if (potionBoard2[x, y].potion == null) // Renamed
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    #region MatchingLogic

    private MatchResult2 SuperMatch2(MatchResult2 _matchedResults) // Correctly Renamed
    {
        if (_matchedResults.direction == MatchDirection2.Horizontal || _matchedResults.direction == MatchDirection2.LongHorizontal) // Correctly Renamed
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection2(pot, new Vector2Int(0, 1), extraConnectedPotions); // Renamed
                CheckDirection2(pot, new Vector2Int(0, -1), extraConnectedPotions); // Renamed

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Horizontal Match2"); // Renamed
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    return new MatchResult2
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection2.Super // Correctly Renamed
                    };
                }
            }
            return new MatchResult2
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        else if (_matchedResults.direction == MatchDirection2.Vertical || _matchedResults.direction == MatchDirection2.LongVertical) // Correctly Renamed
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection2(pot, new Vector2Int(1, 0), extraConnectedPotions); // Renamed
                CheckDirection2(pot, new Vector2Int(-1, 0), extraConnectedPotions); // Renamed

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("I have a super Vertical Match2"); // Renamed
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    return new MatchResult2
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection2.Super // Correctly Renamed
                    };
                }
            }
            return new MatchResult2
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        return null;
    }

    MatchResult2 IsConnected2(Potion potion) // Correctly Renamed
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        CheckDirection2(potion, new Vector2Int(1, 0), connectedPotions); // Renamed
        CheckDirection2(potion, new Vector2Int(-1, 0), connectedPotions); // Renamed

        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult2 // Correctly Renamed
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection2.Horizontal // Correctly Renamed
            };
        }
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult2 // Correctly Renamed
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection2.LongHorizontal // Correctly Renamed
            };
        }

        connectedPotions.Clear();
        connectedPotions.Add(potion);

        CheckDirection2(potion, new Vector2Int(0, 1), connectedPotions); // Renamed
        CheckDirection2(potion, new Vector2Int(0, -1), connectedPotions); // Renamed

        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult2 // Correctly Renamed
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection2.Vertical // Correctly Renamed
            };
        }
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult2 // Correctly Renamed
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection2.LongVertical // Correctly Renamed
            };
        }
        else
        {
            return new MatchResult2 // Correctly Renamed
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection2.None // Correctly Renamed
            };
        }
    }

    void CheckDirection2(Potion pot, Vector2Int direction, List<Potion> connectedPotions) // Renamed
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard2[x, y].isUsable) // Renamed
            {
                Potion neighbourPotion = potionBoard2[x, y].potion.GetComponent<Potion>(); // Renamed

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

    public void SelectPotion2(Potion _potion) // Renamed
    {
        if (selectedPotion2 == null) // Renamed
        {
            selectedPotion2 = _potion; // Renamed
        }
        else if (selectedPotion2 == _potion) // Renamed
        {
            selectedPotion2 = null; // Renamed
        }
        else if (selectedPotion2 != _potion) // Renamed
        {
            SwapPotion2(selectedPotion2, _potion); // Renamed
            selectedPotion2 = null; // Renamed
        }
    }

    private void SwapPotion2(Potion _currentPotion, Potion _targetPotion) // Renamed
    {
        if (!IsAdjacent2(_currentPotion, _targetPotion)) // Renamed
        {
            return;
        }

        DoSwap2(_currentPotion, _targetPotion); // Renamed

        isProcessingMove2 = true; // Renamed

        StartCoroutine(ProcessMatches2(_currentPotion, _targetPotion)); // Renamed
    }

    private void DoSwap2(Potion _currentPotion, Potion _targetPotion) // Renamed
    {
        GameObject temp = potionBoard2[_currentPotion.xIndex, _currentPotion.yIndex].potion; // Renamed

        potionBoard2[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard2[_targetPotion.xIndex, _targetPotion.yIndex].potion; // Renamed
        potionBoard2[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp; // Renamed

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(potionBoard2[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position); // Renamed
        _targetPotion.MoveToTarget(potionBoard2[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position); // Renamed
    }

    private IEnumerator ProcessMatches2(Potion _currentPotion, Potion _targetPotion) // Renamed
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard2()) // Renamed
        {
            PlayNextMatchSound2();
            StartCoroutine(ProcessTurnOnMatchedBoard2(true)); // Renamed
        }
        else
        {
            DoSwap2(_currentPotion, _targetPotion); // Renamed
        }
        isProcessingMove2 = false; // Renamed
    }

    private bool IsAdjacent2(Potion _currentPotion, Potion _targetPotion) // Renamed
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    #endregion
}

public class MatchResult2 // Correctly Renamed
{
    public List<Potion> connectedPotions;
    public MatchDirection2 direction; // Correctly Renamed
}

public enum MatchDirection2 // Correctly Renamed
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
