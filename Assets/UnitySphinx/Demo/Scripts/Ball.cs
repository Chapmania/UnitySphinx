using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySphinx.Demo {

    public class Ball : MonoBehaviour {

        public PttDemo ptt;

        private Vector3 _target = new Vector3(0f, 0.5f, 0f);

        private Vector3 _velocity = Vector3.zero;

        void Start() {
            ptt.OnHypothesis += this.OnHypothesis;
        }

        void OnHypothesis(string text, float score, bool isComplete) {
            if (!isComplete) return;
            
            var tokens = text.Split(' ');
            if (tokens.Length == 1 && "stop".Equals(tokens[0])) {
                _target = transform.position;
            } else if (tokens.Length == 4 && "go".Equals(tokens[0])) {
                switch (tokens[2]) {
                    case "alpha":   _target.x = -4.5f; break;
                    case "bravo":   _target.x = -3.5f; break;
                    case "charlie": _target.x = -2.5f; break;
                    case "delta":   _target.x = -1.5f; break;
                    case "echo":    _target.x = -0.5f; break;
                    case "foxtrot": _target.x =  0.5f; break;
                    case "golf":    _target.x =  1.5f; break;
                    case "hotel":   _target.x =  2.5f; break;
                    case "indigo":  _target.x =  3.5f; break;
                    case "juliet":  _target.x =  4.5f; break;
                }
                switch (tokens[3]) {
                    case "zero":  _target.z = -4.5f; break;
                    case "one":   _target.z = -3.5f; break;
                    case "two":   _target.z = -2.5f; break;
                    case "three": _target.z = -1.5f; break;
                    case "four":  _target.z = -0.5f; break;
                    case "five":  _target.z = 0.5f; break;
                    case "six":   _target.z = 1.5f; break;
                    case "seven": _target.z = 2.5f; break;
                    case "eight": _target.z = 3.5f; break;
                    case "nine":  _target.z = 4.5f; break;
                }
            }
        }

        // Update is called once per frame
        void FixedUpdate() {
            transform.position = Vector3.SmoothDamp(transform.position, _target, ref _velocity, 1.0f, 1.0f);
            transform.RotateAround(Vector3.Cross(Vector3.up, _velocity.normalized), Mathf.PI * _velocity.magnitude * Time.fixedDeltaTime);
        }
    }
}