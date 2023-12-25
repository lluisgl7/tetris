using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris {


    public class GameController : MonoBehaviour {

        public static GameController instance;

        public event Action<int> onLevelChanged;
        public event Action<int> onLinesCountChanged;
        public event Action<int> onScoreChanged;
        public event Action<bool> onGameInProgressChanged;
        public event Action onGameOver;
        

        [Header("Game Parameters")]
        [SerializeField] private int levelUpLines = 10;
        [SerializeField] private int base1LinePoints = 40;
        [SerializeField] private int base2LinePoints = 100;
        [SerializeField] private int base3LinePoints = 300;
        [SerializeField] private int base4LinePoints = 1200;

        [Header("Gravity Parameters")]
        [SerializeField] private float baseGravityTimeInterval = 1f;
        [SerializeField] private float levelGravityIntervalTimeMultiplier = 0.8f;


        private int level;
        public int Level { 
            
            get { return level; }

            private set {

                if (level != value) {

                    level = value;
                    onLevelChanged?.Invoke(level);
                }
            }
        }

        private int linesCount;
        public int LinesCount {

            get { return linesCount; }

            set {

                if (linesCount != value) {

                    linesCount = value;
                    onLinesCountChanged?.Invoke(linesCount);

                    if (linesCount >= levelUpLines * (Level + 1)) Level++;
                }
            }
        }

        private int score;
        public int Score {
            
            get { return score; }

            private set {

                if (score != value) {

                    score = value;
                    onScoreChanged?.Invoke(value);
                }
            }
        }

        private float GravityTimeInterval {
           
            get {

                return baseGravityTimeInterval * Mathf.Pow(levelGravityIntervalTimeMultiplier, Level);
            } 
        }

        private bool gameInProgress;
        public bool GameInProgress {

            get { return gameInProgress; }

            private set {

                if (gameInProgress != value) {

                    gameInProgress = value;
                    onGameInProgressChanged?.Invoke(gameInProgress);
                }
            } 
        }

        private Coroutine gravityCoroutine;

        private void Awake() {

            SingletonCheck();

            void SingletonCheck() {

                if (!instance) instance = this;
                else if (instance != this) Destroy(gameObject);
            }
        }

        private void Start() {

            if (GameSaver.SavedGameFound()) PromptUserLoadGame();

            else StartNewGame(); 
        }

        private void OnEnable() {

            SubscribeToEvents();
        }

        private void OnDisable() {

            UnsubscribeFromEvents();
        }

        public void StartNewGame() {

            StopGame();
            ResetGameValues();
            GameSaver.DeleteSavedGame();

            Grid.instance.ClearGrid();
            Grid.instance.SpawnRandomPiece();

            gravityCoroutine = StartCoroutine(GravityCoroutine());

            GameInProgress = true;
        }

        private void StopGame() {

            if (gravityCoroutine != null) StopCoroutine(gravityCoroutine);

            GameInProgress = false;
        }
             
        private void ResetGameValues() {

            Level = 0;
            LinesCount = 0;
            Score = 0;
        }

        private IEnumerator GravityCoroutine() {

            while (true) {

                yield return new WaitForSeconds(GravityTimeInterval);

                Grid.instance.ActivePiece.MoveDown();
            }
        }

        private void SubscribeToEvents() {

            StartCoroutine(WaitAndSubscribe());

            IEnumerator WaitAndSubscribe() {

                while (Grid.instance == null) yield return null; //make sure the Grid instance is created

                Grid.instance.onLinesCleared += OnLinesCleared;
                Grid.instance.onTopOut += OnTopOut;
            }
        }

        private void UnsubscribeFromEvents() {

            if (Grid.instance) {
         
                Grid.instance.onLinesCleared -= OnLinesCleared;
                Grid.instance.onTopOut -= OnTopOut;
            }
        }

        private void OnTopOut() {

            StopGame();

            onGameOver?.Invoke();
        }

        private void OnLinesCleared(int clearedLinesCount) {

            switch (clearedLinesCount) {

                case 1: Score += base1LinePoints * (Level + 1);
                    break;

                case 2: Score += base2LinePoints * (Level + 1);
                    break;

                case 3: Score += base3LinePoints * (Level + 1);
                    break;

                case 4: Score += base4LinePoints * (Level + 1);
                    break;
            }

            LinesCount += clearedLinesCount;
        }

        public void SaveGame() {

            if (GameInProgress) GameSaver.SaveGame();
        }
        public void LoadGame() {

            bool gameLoaded = GameSaver.LoadGame();

            if (gameLoaded) {

                GameSaver.DeleteSavedGame();

                if (!Grid.instance.ActivePiece) Grid.instance.SpawnRandomPiece();

                gravityCoroutine = StartCoroutine(GravityCoroutine());

                GameInProgress = true;
            }

            else StartNewGame(); //if couldn't load game (json corrupted, etc.), just start a new game.
        }        

        private void PromptUserLoadGame() {

            UI.instance.PromptUserLoadGame();
        }

        public SaveData GetSaveData() {

            SaveData saveData = new SaveData();
            saveData.PopulateSaveData(this);
            return saveData;
        }

        public void LoadSaveData(SaveData saveData) {

            Level = saveData.level;
            LinesCount = saveData.lines;
            Score = saveData.score;
        }

        public class SaveData {

            public int level, lines, score;

            public void PopulateSaveData(GameController gameController) {

                this.level = gameController.Level;
                this.lines = gameController.LinesCount;
                this.score = gameController.Score;
            }
        }

    }
}