using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

namespace Tetris {

    public class UI : MonoBehaviour {

        public static UI instance;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI linesCountText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Prompt Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject loadGameQuestionPanel;

        [Header("Buttons")]
        [SerializeField] private Button saveExitButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button loadYesButton;
        [SerializeField] private Button loadNoButton;

        private void Awake() {

            SingletonCheck();

            void SingletonCheck() {

                if (!instance) instance = this;
                else if (instance != this) Destroy(gameObject);
            }
        }

        private void Start() {

            Initialize();
        }

        private void OnEnable() {

            SubscribeToEvents();
        }

        private void OnDisable() {

            UnsubscribeFromEvents();
        }

        public void PromptUserLoadGame() {

            loadGameQuestionPanel.SetActive(true);
        }

        private void Initialize() {

            if (GameController.instance) {

                UpdateScoreText(GameController.instance.Score);
                UpdateLinesCountText(GameController.instance.LinesCount);
                UpdateLevelText(GameController.instance.Level);
            }
        }

        private void SubscribeToEvents() {

            SubscribeToGameControllerEvents();
            SubscribeToButtonEvents();

            void SubscribeToGameControllerEvents() {

                StartCoroutine(WaitAndSubscribe());

                IEnumerator WaitAndSubscribe() {

                    while (GameController.instance == null) yield return null; //make sure the GameController instance is created

                    GameController.instance.onScoreChanged += UpdateScoreText;
                    GameController.instance.onLinesCountChanged += UpdateLinesCountText;
                    GameController.instance.onLevelChanged += UpdateLevelText;
                    GameController.instance.onGameInProgressChanged += OnGameInProgressChanged;
                    GameController.instance.onGameOver += OnGameOver;
                }
            }

            void SubscribeToButtonEvents() {

                saveExitButton.onClick.AddListener(SaveExitButton_onClick);
                exitButton.onClick.AddListener(ExitButton_onClick);
                restartButton.onClick.AddListener(RestartButton_onClick);
                loadYesButton.onClick.AddListener(LoadYesButton_onClick);
                loadNoButton.onClick.AddListener(LoadNoButton_onClick);
            }
        }        

        private void UnsubscribeFromEvents() {

            if (GameController.instance) {

                GameController.instance.onScoreChanged -= UpdateScoreText;
                GameController.instance.onLinesCountChanged -= UpdateLinesCountText;
                GameController.instance.onLevelChanged -= UpdateLevelText;
                GameController.instance.onGameInProgressChanged -= OnGameInProgressChanged;
                GameController.instance.onGameOver -= OnGameOver;
            }

            saveExitButton.onClick.RemoveListener(SaveExitButton_onClick);
            exitButton.onClick.RemoveListener(ExitButton_onClick);
            restartButton.onClick.RemoveListener(RestartButton_onClick);
            loadYesButton.onClick.RemoveListener(LoadYesButton_onClick);
            loadNoButton.onClick.RemoveListener(LoadNoButton_onClick);
        }

        private void UpdateScoreText(int score) { scoreText.text = score.ToString(); }        
        private void UpdateLinesCountText(int linesCount) { linesCountText.text = linesCount.ToString(); }
        private void UpdateLevelText(int level) { levelText.text = level.ToString(); }

        private void SaveExitButton_onClick() { 
            
            GameController.instance.SaveGame(); 
            QuitGame(); 
        }

        private void ExitButton_onClick() { QuitGame(); }        
        private void RestartButton_onClick() { GameController.instance.StartNewGame(); }
        private void LoadYesButton_onClick() { GameController.instance.LoadGame(); }
        private void LoadNoButton_onClick() { GameController.instance.StartNewGame(); }

        private void OnGameInProgressChanged(bool gameInProgress) {

            if (gameInProgress) { //deactivate panels when game is in progress

                gameOverPanel.SetActive(false);
                loadGameQuestionPanel.SetActive(false);
            }
        }

        private void OnGameOver() { gameOverPanel.SetActive(true); }

        private void QuitGame() {

#if UNITY_EDITOR

            UnityEditor.EditorApplication.isPlaying = false;
#else

            Application.Quit();

#endif
            
        }
    }
}