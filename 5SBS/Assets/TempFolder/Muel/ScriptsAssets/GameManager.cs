using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    public GameObject player;
    public Transform playerSpawnPoint;

    [Header("Components")]
    public TransformHistory transformHistory;
    public DoorHistory doorHistory;
    public ObjectManager objectManager;

    [Header("Timer")]
    public float stageTime = 5f;
    [HideInInspector] public float currentTime;
    public TextMeshProUGUI timeText;

    [Header("WinState")]
    public GameObject uiWinScreen;

    [Header("Door System")]
    public Transform doorsParent;               // <<=== Parent Doors
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;

    private List<GameObject> doors = new List<GameObject>();
    private bool hasPlayerStartedMoving = false;
    private bool isLevelPaused = false;


    // ===========================================================
    // INITIALIZATION
    // ===========================================================
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        AutoDetectDoors();
        SetupDoorHistory();

        RespawnPlayer();
        ResetTimer();
    }


    // ===========================================================
    // AUTO-DETECT DOORS
    // ===========================================================
    private void AutoDetectDoors()
    {
        doors.Clear();

        if (doorsParent == null)
        {
            Debug.LogError("GameManager: doorsParent belum di-assign!");
            return;
        }

        foreach (Transform child in doorsParent)
            doors.Add(child.gameObject);

        Debug.Log($"Auto-detected {doors.Count} doors.");
    }

    private void SetupDoorHistory()
    {
        if (doorHistory == null)
        {
            Debug.LogError("DoorHistory belum terpasang!");
            return;
        }

        doorHistory.doors = doors;
        doorHistory.openSprite = openDoorSprite;
        doorHistory.closedSprite = closedDoorSprite;

        doorHistory.SaveState(); // initial state
    }


    // ===========================================================
    // TIMER
    // ===========================================================
    public void SetPlayerMoving()
    {
        hasPlayerStartedMoving = true;
    }

    public void TimeStart()
    {
        if (isLevelPaused || player == null || !hasPlayerStartedMoving) return;

        currentTime -= Time.deltaTime;
        UpdateTimerText();

        if (currentTime <= 0)
        {
            currentTime = stageTime;
            UpdateTimerText();
            OnPlayerDeath();
            RespawnPlayer();
            ResetTimer();
        }
    }

    private void UpdateTimerText()
    {
        int seconds = Mathf.FloorToInt(currentTime);
        int milliseconds = Mathf.FloorToInt((currentTime - seconds) * 100);

        timeText.text = $"{seconds:00}:{milliseconds:00}";
    }

    public void ResetTimer()
    {
        currentTime = stageTime;
        hasPlayerStartedMoving = false;
    }


    // ===========================================================
    // PLAYER DEATH
    // ===========================================================
    public void OnPlayerDeath()
    {
        if (player == null) return;

        transformHistory?.SaveLog();

        player.SetActive(false);

        doorHistory?.SaveState();
    }


    // ===========================================================
    // PLAYER RESPAWN
    // ===========================================================
    public void RespawnPlayer()
    {
        if (player == null) return;

        player.transform.position = playerSpawnPoint.position;
        player.SetActive(true);
    }


    // ===========================================================
    // RESET LEVEL
    // ===========================================================
    public void ResetLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }


    // ===========================================================
    // UNDO SYSTEM
    // ===========================================================
    public void UndoLastAction()
    {
        transformHistory?.Undo();
        doorHistory?.UndoState();

        RespawnPlayer();
        ResetTimer();
    }


    // ===========================================================
    // PAUSE / RESUME
    // ===========================================================
    public void PauseGame()
    {
        isLevelPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isLevelPaused = false;
        Time.timeScale = 1f;
    }


    // ===========================================================
    // WIN LEVEL
    // ===========================================================
    public void OnPlayerWin()
    {
        PauseGame();

        if (uiWinScreen != null)
            uiWinScreen.SetActive(true);
    }


    // ===========================================================
    // DOOR INTERACTION
    // ===========================================================
    public void doorInteraction(int doorIndex)
    {
        if (doorIndex < 0 || doorIndex >= doors.Count) return;

        GameObject door = doors[doorIndex];
        if (door == null) return;

        Collider2D col = door.GetComponent<Collider2D>();

        // Toggle collider
        col.enabled = !col.enabled;

        // Update sprite via dedicated function
        UpdateDoorSprite(doorIndex);
    }


    public bool isDoorOpen(int doorIndex)
    {
        if (doorIndex < 0 || doorIndex >= doors.Count) return false;

        Collider2D col = doors[doorIndex].GetComponent<Collider2D>();
        return (col != null && col.enabled == false);
    }

    public void UpdateDoorSprite(int doorIndex)
    {
        if (doorIndex < 0 || doorIndex >= doors.Count) return;

        GameObject door = doors[doorIndex];
        if (door == null) return;

        Collider2D col = door.GetComponent<Collider2D>();
        SpriteRenderer sr = door.GetComponent<SpriteRenderer>();

        if (sr == null) return;

        if (col.enabled == false)
            sr.sprite = openDoorSprite;
        else
            sr.sprite = closedDoorSprite;
    }

    void ApplyDoorHistory(List<bool> doorStates)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            Collider2D col = doors[i].GetComponent<Collider2D>();
            col.enabled = doorStates[i];

            // wajib update sprite!
            UpdateDoorSprite(i);
        }
    }


}
