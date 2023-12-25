using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris {

    public class Block : MonoBehaviour {

        public Vector2Int Position {
         
            get { return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)); }
            private set { transform.position = (Vector3Int)value; }
        }

        public Color Color {

            get { return GetComponent<SpriteRenderer>().color; }
            private set { GetComponent<SpriteRenderer>().color = value; }
        }

        public bool IsOnAvailablePosition() {

            return Grid.instance.PositionIsAvailable(Position);
        }

        public SaveData GetSaveData() {

            SaveData saveData = new SaveData();
            saveData.PopulateSaveData(this);
            return saveData;
        }

        public void LoadSaveData(SaveData saveData) {

            Position = saveData.position.ToVector2Int();
            Color = saveData.color.ToColor();
        }

        public class SaveData {

            public Vector2IntStruct position;
            public ColorStruct color;

            public void PopulateSaveData(Block block) {

                position = new Vector2IntStruct(block.Position);
                color = new ColorStruct(block.Color);
            }
        }
    }
}