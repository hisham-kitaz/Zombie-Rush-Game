using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject backgroundPanel;
    public GameObject victoryPanel;
    public GameObject losePanel;
    public GameObject PotionBoardImg;

    [Header("Game Parameters")]
    public int goal;
    public int moves;
    public int points;
    public bool isGameEnded;

    [Header("Main UI Elements")]
    public TMP_Text pointsTxt;
    public TMP_Text pointsTxt1;
    public TMP_Text pointsTxt2;
    public TMP_Text movesTxt;
    public TMP_Text goalTxt;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float remainingTime;

    [Header("Multiplier Display")]
    [SerializeField] private TMP_Text multiplierText;

    [Header("Star Rewards")]
    [SerializeField] private GameObject[] starObjects;
    [SerializeField] private int oneStarThreshold = 1000;
    [SerializeField] private int twoStarThreshold = 2000;
    [SerializeField] private int threeStarThreshold = 3000;

    [Header("Score Multiplier")]
    private int chainReactionCount = 0;
    private const int BASE_MATCH_SCORE = 100;
    private const float MULTIPLIER_INCREMENT = 0.5f;
    private bool isProcessingChain = false;

    [Header("Color Match Counters")]
    [SerializeField] private TMP_Text redMatchesText;
    [SerializeField] private TMP_Text blueMatchesText;
    [SerializeField] private TMP_Text purpleMatchesText;
    [SerializeField] private TMP_Text greenMatchesText;
    [SerializeField] private TMP_Text whiteMatchesText;

    [Header("Required Matches")]
    private const int REQUIRED_MATCHES = 3;
    private int redMatches = 0;
    private int blueMatches = 0;
    private int purpleMatches = 0;
    private int greenMatches = 0;
    private int whiteMatches = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int _moves, int _goal, float _gameTime)
    {
        moves = _moves;
        goal = _goal;
        remainingTime = _gameTime;
        points = 0;
        chainReactionCount = 0;
        isProcessingChain = false;

        redMatches = 0;
        blueMatches = 0;
        purpleMatches = 0;
        greenMatches = 0;
        whiteMatches = 0;

        UpdateMatchCounters();
        multiplierText.text = "Multiplier: x1.0";
    }

    void Update()
    {
        pointsTxt.text = "Points: " + points.ToString();
        pointsTxt1.text = "Points: " + points.ToString();
        pointsTxt2.text = "Points: " + points.ToString();
        movesTxt.text = "Moves: " + moves.ToString();
        goalTxt.text = "Goal: " + goal.ToString();

        if (!isGameEnded)
        {
            UpdateTimer();
        }
    }

    private void UpdateMatchCounters()
    {
        redMatchesText.text = $"{redMatches}/{REQUIRED_MATCHES}";
        blueMatchesText.text = $"{blueMatches}/{REQUIRED_MATCHES}";
        purpleMatchesText.text = $"{purpleMatches}/{REQUIRED_MATCHES}";
        greenMatchesText.text = $"{greenMatches}/{REQUIRED_MATCHES}";
        whiteMatchesText.text = $"{whiteMatches}/{REQUIRED_MATCHES}";
    }

    private bool CheckAllWinConditions()
    {
        bool allMatchesComplete = redMatches >= REQUIRED_MATCHES &&
                                  blueMatches >= REQUIRED_MATCHES &&
                                  purpleMatches >= REQUIRED_MATCHES &&
                                  greenMatches >= REQUIRED_MATCHES &&
                                  whiteMatches >= REQUIRED_MATCHES;

        bool hasEnoughPoints = points >= goal;
        bool hasTimeAndMoves = remainingTime > 0 && moves > 0;

        return allMatchesComplete && hasEnoughPoints && hasTimeAndMoves;
    }

    public void ProcessTurn(List<Potion> matchedPotions, bool _subtractMoves)
    {
        if (_subtractMoves)
        {
            chainReactionCount = 0;
            isProcessingChain = true;
            moves--;
        }
        else if (!isProcessingChain)
        {
            chainReactionCount = 0;
            isProcessingChain = true;
        }

        float multiplier = 1 + (chainReactionCount * MULTIPLIER_INCREMENT);
        int matchScore = Mathf.RoundToInt(BASE_MATCH_SCORE * matchedPotions.Count * multiplier);
        points += matchScore;

        chainReactionCount++;
        Debug.Log($"Chain Reaction #{chainReactionCount}! Multiplier: {multiplier}x");

        multiplierText.text = $"Multiplier: x{multiplier:F1}";

        if (matchedPotions.Count >= 3)
        {
            PotionType matchType = matchedPotions[0].potionType;
            switch (matchType)
            {
                case PotionType.Red:
                    if (redMatches < REQUIRED_MATCHES) redMatches++;
                    break;
                case PotionType.Blue:
                    if (blueMatches < REQUIRED_MATCHES) blueMatches++;
                    break;
                case PotionType.Purple:
                    if (purpleMatches < REQUIRED_MATCHES) purpleMatches++;
                    break;
                case PotionType.Green:
                    if (greenMatches < REQUIRED_MATCHES) greenMatches++;
                    break;
                case PotionType.White:
                    if (whiteMatches < REQUIRED_MATCHES) whiteMatches++;
                    break;
            }

            UpdateMatchCounters();
        }

        if (CheckAllWinConditions())
        {
            EndGame(true);
            return;
        }

        if (moves <= 0 || remainingTime <= 0)
        {
            EndGame(false);
            return;
        }

        int width = PotionBoard.Instance?.width ?? PotionBoard2.Instance2.width;
        int height = PotionBoard.Instance?.height ?? PotionBoard2.Instance2.height;
        int potentialRemainingPoints = moves * width * height;

        if (points + potentialRemainingPoints < goal)
        {
            EndGame(false);
            return;
        }

        // Reset multiplier immediately if no more matches are found
        bool board1HasMatches = PotionBoard.Instance != null && PotionBoard.Instance.CheckBoard();
        bool board2HasMatches = PotionBoard2.Instance2 != null && PotionBoard2.Instance2.CheckBoard2();

        if (!board1HasMatches && !board2HasMatches)
        {
            Debug.Log("No more matches found, resetting multiplier");
            isProcessingChain = false;
            chainReactionCount = 0;
            multiplierText.text = "Multiplier: x1.0";
        }
    }

    private IEnumerator CheckBoardSettled()
    {
        yield return new WaitForSeconds(0.5f);

        // This coroutine is now only used for debug logging
        bool board1Settled = PotionBoard.Instance != null && !PotionBoard.Instance.CheckBoard();
        bool board2Settled = PotionBoard2.Instance2 != null && !PotionBoard2.Instance2.CheckBoard2();

        if (board1Settled && board2Settled)
        {
            Debug.Log("Board has settled, chain reaction complete");
        }
    }

    private void EndGame(bool victory)
    {
        isGameEnded = true;
        backgroundPanel.SetActive(true);

        if (victory)
        {
            PotionBoardImg.SetActive(false);
            victoryPanel.SetActive(true);
            AwardStars();
        }
        else
        {
            PotionBoardImg.SetActive(false);
            losePanel.SetActive(true);
        }

        if (PotionBoard.Instance != null)
        {
            PotionBoard.Instance.potionParent.SetActive(false);
        }
        if (PotionBoard2.Instance2 != null)
        {
            PotionBoard2.Instance2.potionParent2.SetActive(false);
        }
    }

    private void AwardStars()
    {
        foreach (GameObject star in starObjects)
        {
            star.SetActive(false);
        }

        if (points >= oneStarThreshold)
        {
            starObjects[0].SetActive(true);
        }

        if (points >= twoStarThreshold)
        {
            starObjects[1].SetActive(true);
        }

        if (points >= threeStarThreshold)
        {
            starObjects[2].SetActive(true);
        }

        SaveLevelProgress();
    }

    private void SaveLevelProgress()
    {
        int stars = 0;
        if (points >= threeStarThreshold) stars = 3;
        else if (points >= twoStarThreshold) stars = 2;
        else if (points >= oneStarThreshold) stars = 1;

        string levelKey = $"Level_{SceneManager.GetActiveScene().buildIndex}_Stars";
        int previousStars = PlayerPrefs.GetInt(levelKey, 0);
        if (stars > previousStars)
        {
            PlayerPrefs.SetInt(levelKey, stars);
            PlayerPrefs.Save();
        }
    }

    private void UpdateTimer()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else
        {
            remainingTime = 0;
            EndGame(false);
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = "Time Left: " + string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
}
