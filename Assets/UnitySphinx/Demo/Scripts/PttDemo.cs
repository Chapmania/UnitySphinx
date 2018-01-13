using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pocketsphinx;

namespace UnitySphinx.Demo {

    public delegate void OnHypothesis(string text, float score, bool isComplete);

    public class PttDemo : MonoBehaviour {
        private PsDecoder _decoder;

        private IEnumerator _captureLoop;

        private string _mic;

        public event OnHypothesis OnHypothesis;

        public void Awake() {
            var config = new CmdLineConfig();
            config.Hmm = Application.dataPath + "\\UnitySphinx\\Models\\en-us\\en-us";
            config.Dict = Application.dataPath + "\\UnitySphinx\\Models\\en-us\\cmudict-en-us.dict";
            config.Jsgf = Application.dataPath + "\\UnitySphinx\\Demo\\Grammars\\demo.gram";
            config.Logfn = Application.dataPath + "\\sphinxlog.txt";

            _decoder = new PsDecoder(config);
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.P)) {
                TryStart();
            } else if (Input.GetKeyUp(KeyCode.P)) {
                TryStop();
            }
        }

        public void TryStart() {
            if (_captureLoop == null) {
                StartUtterance();
            } else {
                Debug.LogWarning("PTT already started");
            }
        }

        public void TryStop() {
            if (_captureLoop != null) {
                EndUtterance();
            } else {
                Debug.LogWarning("PTT already stopped");
            }
        }

        void StartUtterance() {
            _decoder.StartUtt(System.Guid.NewGuid().ToString());

            _mic = Microphone.devices[0];
            StartCoroutine(_captureLoop = CaptureUtterance());
        }

        void EndUtterance() {
            // Stop capturing mic data
            StopCoroutine(_captureLoop);
            _captureLoop = null;
            Microphone.End(_mic);

            _decoder.EndUtt();

            float score;
            string uttid;
            var hypothesis = _decoder.GetHypothesis(out score, out uttid);
            if (OnHypothesis != null && hypothesis != null) {
                OnHypothesis(hypothesis, score, true);
            }
            
            Debug.Log("hypothesis: " + hypothesis);
        }

        IEnumerator CaptureUtterance() {

            // Start capturing mic data into an audio clip
            var clip = Microphone.Start(_mic, true, 10, 16000);

            int readHead = 0; // position in the clip we're reading from
            int writeHead;    // position in the clip last written too by mic
            int nFloatsToGet; // number of new samples available in the clip
            float[] buffer;   // buffer to copy clip data into

            // Keeps track of last hyothesis we got (to avoid sending events if it hasn't changed)
            string prevHypothesis = null;
            
            while (true) {
                writeHead = Microphone.GetPosition(_mic);

                nFloatsToGet = (clip.samples + writeHead - readHead) % clip.samples;

                if (nFloatsToGet < 512) {
                    // Avoid processing small blocks of data, try again next cycle
                    yield return null;
                    continue;
                }

                // copy mic data into our own buffer
                buffer = new float[nFloatsToGet];
                clip.GetData(buffer, readHead);

                readHead = (readHead + nFloatsToGet) % clip.samples;

                _decoder.ProcessRaw(buffer, 0, nFloatsToGet);

                float score;
                string uttid;
                string hypothesis = _decoder.GetHypothesis(out score, out uttid);
                if (hypothesis != null && !hypothesis.Equals(prevHypothesis)) {
                    if (OnHypothesis != null) {
                        OnHypothesis(hypothesis, score, false);
                    }
                    prevHypothesis = hypothesis;
                    Debug.Log("partial: " + hypothesis);
                }
                
                yield return null;
            }
        }
    }
}