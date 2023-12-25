using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Tetris {

    public class Piece : MonoBehaviour {

        public static event Action<Piece> onPieceTouchdown;

        public Vector2Int Position {

            get { return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }
            private set { transform.position = (Vector3Int)value; }
        }

        [SerializeField] private string typeIdentifier;
        [SerializeField] private List<Block> blocks = new List<Block>();
        [SerializeField] private Transform rotationPoint;

        public ReadOnlyCollection<Block> Blocks => new ReadOnlyCollection<Block>(blocks);

        public string TypeIdentifier => typeIdentifier;

        public bool MoveLeft() { return Move(Vector2Int.left); }
        public bool MoveRight() { return Move(Vector2Int.right); }
        public bool MoveUp() { return Move(Vector2Int.up); }
        public bool MoveDown() { 
            
            bool moved = Move(Vector2Int.down);

            if (moved) return true;
            
            else {

                onPieceTouchdown?.Invoke(this);
                return false;
            }
        }

        private bool Move(Vector2Int direction) { //returns true if the move could be performed

            transform.position += (Vector3Int)direction;

            if (IsOnAvailablePosition()) return true;

            else {

                transform.position -= (Vector3Int)direction; //undo               

                return false;
            }
        }

        public bool Rotate() {

            transform.RotateAround(rotationPoint.position, -Vector3.forward, 90f);

            if (IsOnAvailablePosition()) return true;

            else {

                transform.RotateAround(rotationPoint.position, -Vector3.forward, -90f); //undo

                return false;
            }
        }

        public bool IsOnAvailablePosition() {

            foreach (Block block in blocks) {

                if (!block.IsOnAvailablePosition()) return false;
            }

            return true;
        }

        public SaveData GetSaveData() {

            SaveData saveData = new SaveData();
            saveData.PopulateSaveData(this);
            return saveData;
        }

        public void LoadSaveData(SaveData saveData) {

            Position = saveData.position.ToVector2Int();
            transform.rotation = saveData.rotation.ToQuaternion();
        }

        public class SaveData {

            public string type;
            public Vector2IntStruct position;
            public QuaternionStruct rotation;

            public void PopulateSaveData(Piece piece) {

                type = piece.typeIdentifier;
                position = new Vector2IntStruct(piece.Position);
                rotation = new QuaternionStruct(piece.transform.rotation);
            }
        }
        
    }
}