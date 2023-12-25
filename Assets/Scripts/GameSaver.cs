using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace Tetris {

    public struct Vector2IntStruct { //serializes less data than directly serializing a Vector2Int

        public int x;
        public int y;

        public Vector2IntStruct(Vector2Int vector2Int) {

            x = vector2Int.x;
            y = vector2Int.y;
        }

        public Vector2Int ToVector2Int() {
            
            return new Vector2Int(x, y);
        }
    }

    public struct QuaternionStruct {

        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionStruct(Quaternion quaternion) {

            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion ToQuaternion() {

            return new Quaternion(x, y, z, w);
        }
    }

    public struct ColorStruct {

        public float r;
        public float g;
        public float b;
        public float a;

        public ColorStruct(Color color) {

            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor() {

            return new Color(r, g, b, a);
        }
    }

    public static class GameSaver {

        public static string SavedGameDirectoryPath => Path.Combine(Application.persistentDataPath, "Saves");
        public static string SavedGameFileName => "savedGame.json";
        public static string SavedGameFilePath => Path.Combine(SavedGameDirectoryPath, SavedGameFileName);

        public static void SaveGame() {

            Initialize();

            SaveData saveData = new SaveData();
            saveData.PopulateSaveData();

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SavedGameFilePath, json);
        }

        public static bool LoadGame() {

            if (!SavedGameFound()) return false;
            
            try {

                string json = File.ReadAllText(SavedGameFilePath);
                SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);

                GameController.instance.LoadSaveData(saveData.game);
                Grid.instance.LoadSaveData(saveData.grid);

                return true;
            }

            catch (Exception e) {

                Debug.Log("Couldn't load saved game. Error message: " + e.Message);
                return false;
            }
        }

        public static void DeleteSavedGame() {

            if (!SavedGameFound()) return;

            File.Delete(SavedGameFilePath);
        }

        public static bool SavedGameFound() {

            return File.Exists(SavedGameFilePath);
        }

        public static void Initialize() {                     

            if (!Directory.Exists(SavedGameDirectoryPath)) Directory.CreateDirectory(SavedGameDirectoryPath);
        }

        public class SaveData {

            public GameController.SaveData game;
            public Grid.SaveData grid;

            public void PopulateSaveData() {

                game = GameController.instance.GetSaveData();
                grid = Grid.instance.GetSaveData();
            }
        }
    }
}