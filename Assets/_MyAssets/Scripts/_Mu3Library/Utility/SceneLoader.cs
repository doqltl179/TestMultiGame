
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mu3Library.Utility {
    public class SceneLoader : GenericSingleton<SceneLoader> {
        public object[] Param { get; private set; } = null;

        public bool IsLoading { get { return loadSceneCoroutine != null; } }
        public SceneType CurrentLoadedScene {
            get => currentLoadedScene;
            set {
                if(currentLoadedScene != value) {
                    currentLoadedScene = value;

                    OnSceneChanged?.Invoke(value);
                }
            }
        }
        private SceneType currentLoadedScene = SceneType.None;
        public Action<SceneType> OnSceneChanged;

        public float ProgressNum { get; private set; } = -1;

        private IEnumerator loadSceneCoroutine = null;



        #region Utility
        public void LoadScene(SceneType scene, Action sceneLoadStarted = null, Action sceneLoadEnded = null, object[] param = null) {
            if(loadSceneCoroutine == null) {
                Param = param;

                loadSceneCoroutine = LoadSceneCoroutine(scene, sceneLoadStarted, sceneLoadEnded);
                StartCoroutine(loadSceneCoroutine);
            }
        }

        /// <summary>
        /// 컴포넌트에 선언된 "CurrentLoadedScene"의 값만 바꾸고 실제로 Scene을 이동하지는 않음
        /// </summary>
        public void ChangeCurrentLoadedSceneImmediately(SceneType type) {
            currentLoadedScene = type;
        }
        #endregion

        private IEnumerator LoadSceneCoroutine(SceneType scene, Action sceneLoadStarted = null, Action sceneLoadEnded = null) {
            ProgressNum = 0.0f;

            // 다른 작업에 먼저 하기 위함.
            yield return null;

            sceneLoadStarted?.Invoke();

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.ToString());
            asyncLoad.allowSceneActivation = false;
            while(asyncLoad.progress < 0.9f) {
                ProgressNum = asyncLoad.progress;

                yield return null;
            }

            CurrentLoadedScene = scene;
            asyncLoad.allowSceneActivation = true;

            //Fake Wait
            const float fakeWaitTime = 1.0f;
            float timer = 0.0f;
            while(timer < fakeWaitTime) {
                timer += Time.deltaTime;

                ProgressNum = Mathf.Lerp(0.9f, 1.0f, timer / fakeWaitTime);

                yield return null;
            }
            ProgressNum = 1.0f;

            yield return new WaitForSeconds(0.5f);

            while(!asyncLoad.isDone) {
                yield return null;
            }

            sceneLoadEnded?.Invoke();

            loadSceneCoroutine = null;
        }
    }
}