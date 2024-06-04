using Mu3Library;
using Mu3Library.Scene;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashController : SceneController {
    private IEnumerator splashActionCoroutine = null;



    private void Start() {
        if(splashActionCoroutine == null) {
            splashActionCoroutine = SplashActionCoroutine();
            StartCoroutine(splashActionCoroutine);
        }
    }

    private IEnumerator SplashActionCoroutine() {
        yield return new WaitForSeconds(1.0f);

        SceneLoader.Instance.LoadScene(SceneType.Main);

        splashActionCoroutine = null;
    }
}
