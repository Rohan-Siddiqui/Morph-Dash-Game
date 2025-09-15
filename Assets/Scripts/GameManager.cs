using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static bool isGameStarted;
    public static GameManager Instance;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] shapes;

    [SerializeField] private string playerID;
    public int selectShapesQuantity = 1;

    [Header("GamePanel")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private TextMeshProUGUI scoreTxt;    // gamePanel score iconsImage and imagetext

    [SerializeField] private List<GameObject> gamePanelIconsImage;
    [SerializeField] private List<TextMeshProUGUI> gamePanelImageText;

    [SerializeField] private List<Sprite> imageList;
    [SerializeField] private Image nextButtonImage;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;

    [Header("GameOverPanel")]
    [SerializeField] private GameObject gameOverPanel;
    private Coroutine mainMenuRoutine;

    [SerializeField] private TextMeshProUGUI gameOverPanelScoreTxt; // gameOver panel score and high score text

    [SerializeField] private List<GameObject> gameOverPanelIconsImage;
    [SerializeField] private List<TextMeshProUGUI> gameOverPanelImageText;

    [SerializeField] private TextMeshProUGUI newHighestScoreTxt;

    [Header("MainMenuPanel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TextMeshProUGUI highScoreTxt;
    [SerializeField] private TextMeshProUGUI totalKillTxt;
    [SerializeField] private List<GameObject> mainMenuPanelIconsImage;
    [SerializeField] private List<TextMeshProUGUI> mainMenuPanelScoreTxt;

    private Coroutine rotateRoutine;
    private Coroutine shapeChangeRoutine;
    public bool isMainMenuActive = false;

    [Header("Enemy")]
    [SerializeField] private List<GameObject> enemySelectedPoint;
    public float enemySpeed = 2f;

    [Header("Particles Syste")]
    [SerializeField] private GameObject playerDestroyParticles;
    [SerializeField] private GameObject enemyDestroyParticles;

    [Header("UI Audio Source")]
    public AudioSource UISFXAudioSourceManager;

    [Header("UI Audio Clips")]
    [SerializeField] private AudioClip startButtonSound;
    [SerializeField] private AudioClip restartButtonSound;
    [SerializeField] private AudioClip quitButtonSound;
    [SerializeField] private AudioClip pauseButtonSound;
    [SerializeField] private AudioClip resumeButtonSound;
    [SerializeField] private AudioClip shapeChangeButtonSound;

    private HashSet<int> occupiedPoints = new HashSet<int>();
    private int currentPlayerIndex = 0;
    private int currentImageIndex = 1;
    private int totalScore = 0;
    private string checkId;
    private GameObject currentEnemyDestroyEffect;
    private GameObject currentEnemy;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreTxt.text = "HIGH SCORE : " + highScore.ToString();

        MainMenuPanelImagesOn();
        LoadSavedShapeScores();  // Main Menu panel ka score load
        TotalKill();

        SetMainMenuActive(true);
        EnemySpawner.Instance.GetComponent<EnemySpawner>().enabled = false;
        playerID = shapes[currentPlayerIndex].name.ToString();
    }
    public void StartGame()
    {
        isGameStarted = true;
        UISFXAudioSourceManager.PlayOneShot(startButtonSound);
        Thread.Sleep(500);

        GamePanelImagesOn();
        GameOverPanelImagesOn();
        ShowOnlyShape(currentPlayerIndex);
        UpdateButtonImage();
        SetMainMenuActive(false);    // Disable main menu logic (stops rotation, sets shape to index 1)
        gamePanel.SetActive(true);   // Enable the game UI

        Time.timeScale = 1;
        EnemySpawner.Instance.GetComponent<EnemySpawner>().enabled = true;
    }
    public GameObject GetAvailablePoint() // available points check kar raha
    {
        for (int i = 0; i < enemySelectedPoint.Count; i++)
        {
            if (!occupiedPoints.Contains(i) && enemySelectedPoint[i] != null)
            {
                occupiedPoints.Add(i);
                return enemySelectedPoint[i];
            }
        }

        return null; // No available point
    }
    public void ReleasePoint(GameObject point) // index remove ho raha go select ha
    {
        int index = enemySelectedPoint.IndexOf(point);
        if (index != -1)
        {
            occupiedPoints.Remove(index);
        }
    }
    private void ActivePlayerDestroyParticles() // player particles effect on
    {
        Instantiate(playerDestroyParticles, player.transform.position, Quaternion.identity);

    }
    private void ActiveEnemyDestroyParticles() // enemy particles effect on
    {
        Instantiate(enemyDestroyParticles, currentEnemyDestroyEffect.transform.position, Quaternion.identity);
    }
    public void CheckId(GameObject enemy)
    {
        currentEnemyDestroyEffect = enemy; // yaha ye assign kar raha ha currentEnemy for position for destroy enemy effect
        currentEnemy = enemy; // yaha ye currentEnemy assign kar rah taka position freeze karda

        Id enemyId = enemy.GetComponent<Id>();
        string playerCheck = playerID; // yah ye player ki currentID la raha ha

        if(enemyId != null && playerCheck != null)
        {
            if(enemyId.id == playerCheck)
            {
                checkId = enemyId.id;

                AudioManager.Instance.PlaySFX(AudioManager.Instance.enemy);
                DecideInWhichAdd();
                ActiveEnemyDestroyParticles();
                EnemySpawner.Instance.count--;
                Destroy(enemy.gameObject);

                if (enemySpeed <= 5f)
                {
                    enemySpeed += 0.2f;
                }

                Score(1); // score add ho raha
                print("Match");
            }
            else
            {
                StartCoroutine(HandlePlayerDestroy());
            }
        }
    }
    private IEnumerator HandlePlayerDestroy()
    {
        isGameStarted = false;

        if(currentEnemy != null)
        {
            Rigidbody2D rigidBody = currentEnemy.GetComponent<Rigidbody2D>();
            if(rigidBody != null)
            {
                rigidBody.velocity = Vector3.zero;
                rigidBody.isKinematic = true;
            }
        }
        ActivePlayerDestroyParticles(); // play particles effect

        yield return new WaitForSeconds(0.1f);

        AudioManager.Instance.PlaySFX(AudioManager.Instance.player); // sound

        shapes[currentPlayerIndex].SetActive(false); // shape deactive

        yield return new WaitForSeconds(0.5f);

        Destroy(player);

        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);

        print("UnMatch");
    }
    public void ChangeNextShape()
    {
        if(isGameStarted)
        {
            UISFXAudioSourceManager.PlayOneShot(shapeChangeButtonSound);
        }

        playerID = null;
        if (shapes[currentPlayerIndex] != null)
        {
            Vector3 currentPosition = shapes[currentPlayerIndex].transform.position;
            Quaternion currentRotation = shapes[currentPlayerIndex].transform.rotation;

            shapes[currentPlayerIndex].SetActive(false);
            currentPlayerIndex = (currentPlayerIndex + 1) % selectShapesQuantity;

            playerID = shapes[currentPlayerIndex].name; // currentIndex player ka name playerID ko assign
            currentImageIndex++;
            UpdateButtonImage();

            shapes[currentPlayerIndex].transform.position = currentPosition; // gab player change kara ga to previous ki current position or rotation 
            shapes[currentPlayerIndex].transform.rotation = currentRotation; // next player ko assign ho gi
           ShowOnlyShape(currentPlayerIndex);
        }
    }
    private void UpdateButtonImage() // image change shape change wala button sa selected quantity
    {
        currentImageIndex = (currentImageIndex) % selectShapesQuantity;
        if (currentImageIndex < imageList.Count)
        {
            nextButtonImage.sprite = imageList[currentImageIndex];
        }
    }
    public void ShowOnlyShape(int index) // sirf wohi shape on hogi gis ka index match kara
    {
        for (int i = 0; i < shapes.Length; i++)
        {
            shapes[i].SetActive(i == index);
        }
    }
    public void GamePanelImagesOn()    // gamePanel icon Image on ho rahi selected quantity ka 
    {
        for(int i = 0; i < gamePanelIconsImage.Count; i++)
        {
            if(i < selectShapesQuantity)
            {
                gamePanelIconsImage[i].SetActive(true);
            }
            else
            {
                gamePanelIconsImage[i].SetActive(false);
            }
        }
    }
    public void GameOverPanelImagesOn()     // gameOverPanel icon Image on ho rahi selected quantity ka 
    {
        for (int i = 0; i < gameOverPanelIconsImage.Count; i++)
        {
            if (i < selectShapesQuantity)
            {
                gameOverPanelIconsImage[i].SetActive(true);
            }
            else
            {
                gameOverPanelIconsImage[i].SetActive(false);
            }
        }
    }
    public void MainMenuPanelImagesOn()
    {
        for (int i = 0; i < mainMenuPanelIconsImage.Count; i++)
        {
            if (i < selectShapesQuantity)
            {
                mainMenuPanelIconsImage[i].SetActive(true);
            }
            else
            {
                mainMenuPanelIconsImage[i].SetActive(false);
            }
        }
    }
    public void TotalKill()
    {
        int totalKill = 0;

        for (int i = 0; i < mainMenuPanelIconsImage.Count; i++)
        {
            if (i < selectShapesQuantity && mainMenuPanelIconsImage[i].activeSelf)
            {
                if (i < mainMenuPanelScoreTxt.Count)
                {
                    int value;
                    if (int.TryParse(mainMenuPanelScoreTxt[i].text, out value))
                    {
                        totalKill += value;
                    }
                }
            }
        }

        totalKillTxt.text = "TOTAL KILLS : " + totalKill.ToString();
        PlayerPrefs.SetInt("TotalKill", totalKill);
        PlayerPrefs.Save();
    }
    private void LoadSavedShapeScores()
    {
        string[] keys = { "CircleScore", "HexagonScore", "SquareScore", "TriangleScore", "PentagonScore", "RectangleScore" };

        for (int i = 0; i < keys.Length; i++)
        {
            int savedScore = PlayerPrefs.GetInt(keys[i], 0);

            if (i < mainMenuPanelScoreTxt.Count)
            {
                mainMenuPanelScoreTxt[i].text = savedScore.ToString();
            }

            if (i < mainMenuPanelScoreTxt.Count)
            {
                mainMenuPanelScoreTxt[i].text = savedScore.ToString();
            }
        }
    }
    private IEnumerator RotatePlayer()
    {
        while (isMainMenuActive)
        {
            player.transform.Rotate(Vector3.forward * 90 * Time.deltaTime * 3); // smooth rotation around Z (2D)
            yield return null; // wait for next frame
        }
    }
    private IEnumerator ChangeShapeRoutine()
    {
        while (isMainMenuActive)
        {
            ChangeNextShape(); // change shape every 1 second
            yield return new WaitForSeconds(1f);
        }
    }
    public void SetMainMenuActive(bool isActive)
    {
        mainMenuPanel.SetActive(isActive);
        isMainMenuActive = isActive;

        if (isActive)
        {
            currentPlayerIndex = 0;
            playerID = shapes[currentPlayerIndex].name;
            ShowOnlyShape(currentPlayerIndex);

            rotateRoutine = StartCoroutine(RotatePlayer());
            shapeChangeRoutine = StartCoroutine(ChangeShapeRoutine());
        }
        else
        {
            if (rotateRoutine != null) StopCoroutine(rotateRoutine);
            if (shapeChangeRoutine != null) StopCoroutine(shapeChangeRoutine);

            currentPlayerIndex = 0;
            playerID = shapes[currentPlayerIndex].name;
            ShowOnlyShape(currentPlayerIndex);

            player.transform.rotation = Quaternion.identity;

            currentImageIndex = 1 % selectShapesQuantity;
            UpdateButtonImage();
        }
    }
    public void DecideInWhichAdd() // gamePanel or gameOverPanel dono ka text ma add kar raha
    {
        int index = -1;
        string shapeKey = " ";
        switch (checkId)
        {
            case "Circle":
                index = 0;
                shapeKey = "CircleScore";
                break;
            case "Hexagon":
                index = 1;
                shapeKey = "HexagonScore";
                break;
            case "Square":
                index = 2;
                shapeKey = "SquareScore";
                break;
            case "Triangle":
                index = 3;
                shapeKey = "TriangleScore";
                break;
            case "Pentagon":
                index = 4;
                shapeKey = "PentagonScore";
                break;
            case "Rectangle":
                index = 5;
                shapeKey = "RectangleScore";
                break;
            default:
                shapeKey = "OtherScore";
                break;
        }

        if (index >= 0 && index < gamePanelImageText.Count)
        {
            int currentValue;
            if (int.TryParse(gamePanelImageText[index].text, out currentValue))
            {
                int save = PlayerPrefs.GetInt(shapeKey, 0);
                save++;
                PlayerPrefs.SetInt(shapeKey, save);
                PlayerPrefs.Save();
                currentValue++;
                gamePanelImageText[index].text = currentValue.ToString();
                gameOverPanelImageText[index].text = currentValue.ToString();
            }
            else
            {
                gamePanelImageText[index].text = "1";
                gameOverPanelImageText[index].text = "1";
            }
        }
    }
    public void Score(int amount)
    {
        totalScore += amount;
        scoreTxt.text = "SCORE : " + totalScore.ToString();
        gameOverPanelScoreTxt.text = "SCORE : " + totalScore.ToString();

        PlayerPrefs.SetInt("LastScore", totalScore);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if(totalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", totalScore);
            newHighestScoreTxt.gameObject.SetActive(true);
        }
        else
        {
            newHighestScoreTxt.gameObject.SetActive(false);
        }
        PlayerPrefs.Save();
    }
    public void Pause()
    {
        UISFXAudioSourceManager.PlayOneShot(pauseButtonSound);
        Thread.Sleep(400);
        Time.timeScale = 0;
        gamePanel.SetActive(false);
        pausePanel.SetActive(true);

    }
    public void Resume()
    {
        UISFXAudioSourceManager.PlayOneShot(resumeButtonSound);
        Thread.Sleep(400);
        Time.timeScale = 1;
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
    }
    public void Restart()
    {
        isGameStarted = false;

        if(gameOverPanel.activeSelf)
            UISFXAudioSourceManager.PlayOneShot(restartButtonSound);
        else
        UISFXAudioSourceManager.PlayOneShot(restartButtonSound);
        Thread.Sleep(400);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnApplicationQuit()
    {
        if(gameOverPanel.activeSelf )
            UISFXAudioSourceManager.PlayOneShot(quitButtonSound);
        else
        UISFXAudioSourceManager.PlayOneShot(quitButtonSound);
        Thread.Sleep(100);
        Application.Quit();
    }
}
