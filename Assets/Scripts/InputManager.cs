using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris {

    public class InputManager : MonoBehaviour {

        public static InputManager instance;

        [Header("Soft Drop Parameters")]
        [SerializeField] private float repeatDownMovementInterval = 0.05f;

        [Header("Lateral Movement Parameters")]
        [SerializeField] private float repeatLateralMovementInitialDelay = 0.2f;
        [SerializeField] private float repeatLateralMovementInterval = 0.05f;

        private Piece ActivePiece => Grid.instance.ActivePiece;

        private void Awake() {

            SingletonCheck();

            void SingletonCheck() {

                if (!instance) instance = this;
                else if (instance != this) Destroy(gameObject);
            }
        }

        private void Update() {

            ListenForInput();
        }       

        private void ListenForInput() {

            if (!ActivePiece) return;

            if (Input.GetKeyDown(KeyCode.DownArrow)) { StartCoroutine(SoftDropCoroutine()); }

            if (Input.GetKeyDown(KeyCode.LeftArrow)) { StartCoroutine(LateralMovementCoroutine(KeyCode.LeftArrow, delegate () { ActivePiece.MoveLeft(); })); }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { StartCoroutine(LateralMovementCoroutine(KeyCode.RightArrow, delegate () { ActivePiece.MoveRight(); })); }

            if (Input.GetKeyDown(KeyCode.UpArrow)) { ActivePiece.Rotate(); }
        }

        private IEnumerator SoftDropCoroutine() {
            
            while (true) {

                if (!ActivePiece) yield break;

                ActivePiece.MoveDown();

                yield return new WaitForSeconds(repeatDownMovementInterval);

                if (!Input.GetKey(KeyCode.DownArrow)) break;
            }
        }

        private IEnumerator LateralMovementCoroutine(KeyCode key, Action moveAction) {

            moveAction();

            float nextTimeToRepeat = Time.time + repeatLateralMovementInitialDelay;

            while (Time.time < nextTimeToRepeat) {

                if (!Input.GetKey(key)) yield break;  //if key not pressed, stop the coroutine.
                else yield return null;
            }

            while (true) {

                if (!Input.GetKey(key)) break;

                else if (Time.time > nextTimeToRepeat) {

                    if (!ActivePiece) yield break;

                    moveAction();
                    nextTimeToRepeat = Time.time + repeatLateralMovementInterval;
                }

                yield return null;
            }         
        }       
    }
}