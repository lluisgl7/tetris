using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris {

    public class PiecesManager : MonoBehaviour {

        public static PiecesManager instance;

        [SerializeField] private List<Piece> piecesPrefabs = new List<Piece>();
        [SerializeField] private Block blockPrefab;

        private void Awake() {

            SingletonCheck();

            void SingletonCheck() {

                if (!instance) instance = this;
                else if (instance != this) Destroy(gameObject);
            }
        }

        public Piece GetRandomPiecePrefab() {

            return piecesPrefabs[Random.Range(0, piecesPrefabs.Count)];
        }

        public Piece GetPiecePrefab(string typeIdentifier) {

            foreach (Piece piecePrefab in piecesPrefabs)
                if (piecePrefab.TypeIdentifier == typeIdentifier) return piecePrefab;            

            return null;
        }

        public Block GetBlockPrefab() {

            return blockPrefab;
        }
    }
}