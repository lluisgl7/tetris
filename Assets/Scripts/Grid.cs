using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris {

    public class Grid : MonoBehaviour {

        public static Grid instance;
        public event Action<int> onLinesCleared;
        public event Action onTopOut;

        public Piece ActivePiece { get; private set; }

        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 20);
        [SerializeField] private Transform spawnPosition;
        [SerializeField] private Transform stackedBlocksParent;
        [SerializeField] private Transform activePieceParent;

        private Block[,] stackedBlocks;

        private void Awake() {

            SingletonCheck();

            void SingletonCheck() {

                if (!instance) instance = this;
                else if (instance != this) Destroy(gameObject);
            }

            InitializeGrid();
        }

        private void OnEnable() {

            SubscribeToEvents();
        }

        private void OnDisable() {

            UnsubscribeFromEvents();
        }

        private void InitializeGrid() {

            stackedBlocks = new Block[gridSize.x, gridSize.y];
        }

        public void ClearGrid() {

            foreach (Block block in stackedBlocks) RemoveBlock(block);

            foreach (Transform child in activePieceParent) Destroy(child.gameObject);

            ActivePiece = null;
        }

        public bool PositionIsAvailable(Vector2Int position) {
            
            //check borders:
            if (position.x < 0 || position.x >= gridSize.x) return false;
            if (position.y < 0 || position.y >= gridSize.y) return false;

            //check stacked blocks:
            if (stackedBlocks[position.x, position.y]) return false;
            
            return true;
        }       

        private void SpawnPiece(Piece piecePrefab) {

            ActivePiece = Instantiate(piecePrefab, spawnPosition.position, Quaternion.identity, activePieceParent);

            if (!ActivePiece.IsOnAvailablePosition()) {

                bool moved = ActivePiece.MoveUp();

                if (!moved) {

                    ActivePiece = null;
                    onTopOut?.Invoke();
                }
            }
        }

        public void SpawnRandomPiece() {

            SpawnPiece(PiecesManager.instance.GetRandomPiecePrefab());
        }

        public void AddToStackedBlocks(Piece piece) {

            foreach (Block block in piece.Blocks) AddToStackedBlocks(block);

            Destroy(piece.gameObject);
        }

        private void AddToStackedBlocks(Block block) {

            block.transform.parent = stackedBlocksParent;
            stackedBlocks[block.Position.x, block.Position.y] = block;
        }

        public void CheckLines() {

            List<int> fullLinePositions = new List<int>();

            for (int y = 0; y < gridSize.y; y++)
                if (LineIsfull(y)) fullLinePositions.Add(y);

            if (fullLinePositions.Count > 0) ClearLines(fullLinePositions); //we clear them after checking them all instead of after checking each one so that we could add proper visual effects if we wanted to
        }

        private bool LineIsfull(int yPosition) {

            for (int x = 0; x < gridSize.x; x++) {

                if (!stackedBlocks[x, yPosition]) return false;
            }

            return true;
        }

        private void ClearLines(List<int> linePositions) {

            int clearedLinesCount = 0;

            foreach (int linePosition in linePositions) {

                ClearLine(linePosition - clearedLinesCount); //for every line that has been cleared before, upper lines have moved down one position.
                clearedLinesCount++;
            }

            if (clearedLinesCount > 0) onLinesCleared?.Invoke(clearedLinesCount);

            void ClearLine(int lineToClearYPosition) {

                //remove blocks in line:
                for (int x = 0; x < gridSize.x; x++) RemoveBlock(stackedBlocks[x, lineToClearYPosition]);

                //settle (move down) upper lines:
                for (int y = lineToClearYPosition + 1; y < gridSize.y; y++) MoveLineDown(y);

                void MoveLineDown(int lineToMoveYPosition) {

                    for (int x = 0; x < gridSize.x; x++) MoveBlockDown(stackedBlocks[x, lineToMoveYPosition]);

                    void MoveBlockDown(Block block) {

                        if (!block) return;

                        stackedBlocks[block.Position.x, block.Position.y] = null; //clear current position
                        
                        block.transform.position += Vector3Int.down;
                        
                        stackedBlocks[block.Position.x, block.Position.y] = block;
                    }
                }
            }            
        }

        private void RemoveBlock(Block block) {

            if (!block) return;
                
            stackedBlocks[block.Position.x, block.Position.y] = null;
            Destroy(block.gameObject);
        }

        private void SubscribeToEvents() {

            Piece.onPieceTouchdown += OnPieceTouchdown;
        }

        private void UnsubscribeFromEvents() {

            Piece.onPieceTouchdown -= OnPieceTouchdown;
        }

        private void OnPieceTouchdown(Piece piece) {

            AddToStackedBlocks(piece);
            CheckLines();

            SpawnRandomPiece();
        }

        public SaveData GetSaveData() {

            SaveData saveData = new SaveData();
            saveData.PopulateSaveData(this);
            return saveData;
        }

        public void LoadSaveData(SaveData saveData) {

            ClearGrid();
            LoadStackedBlocks();
            LoadActivePiece();           

            void LoadStackedBlocks() {

                foreach (var blockSaveData in saveData.stackedBlocks) {

                    Block block = Instantiate(PiecesManager.instance.GetBlockPrefab());
                    block.LoadSaveData(blockSaveData);

                    AddToStackedBlocks(block);
                }
            }

            void LoadActivePiece() {

                Piece.SaveData activePieceSaveData = saveData.activePiece;
                if (activePieceSaveData == null) return;

                Piece piecePrefab = PiecesManager.instance.GetPiecePrefab(activePieceSaveData.type);
                if (!piecePrefab) throw new Exception("Couldn't load piece of type '" + activePieceSaveData.type + "'.");
    
                ActivePiece = Instantiate(piecePrefab, activePieceParent);
                ActivePiece.LoadSaveData(saveData.activePiece);
            }
        }

        public class SaveData {

            public List<Block.SaveData> stackedBlocks = new List<Block.SaveData>();
            public Piece.SaveData activePiece;

            public void PopulateSaveData(Grid grid) {

                foreach (Block block in grid.stackedBlocks)
                    if (block) this.stackedBlocks.Add(block.GetSaveData());

                if (grid.ActivePiece) this.activePiece = grid.ActivePiece.GetSaveData();
            }
        }
    }
}