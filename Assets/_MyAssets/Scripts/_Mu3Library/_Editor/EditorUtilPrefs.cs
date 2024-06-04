#if UNITY_EDITOR
using Mu3Library;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtilPrefs {
    private static readonly string Key_UsePlayLoadScene = "UsePlayLoadScene";
    public static bool UsePlayLoadScene {
        get => usePlayLoadScene;
        set {
            EditorPrefs.SetBool(Key_UsePlayLoadScene, value);

            usePlayLoadScene = value;
        }
    }
    private static bool usePlayLoadScene = true;

    private static readonly string Key_PlayLoadScene = "PlayLoadScene";
    public static SceneType PlayLoadScene {
        get => playLoadScene;
        set {
            PlayLoadSceneName = value.ToString();

            EditorPrefs.SetString(Key_PlayLoadScene, PlayLoadSceneName);

            playLoadScene = value;
        }
    }
    private static SceneType playLoadScene = SceneType.Splash;
    public static string PlayLoadSceneName { get; private set; } = SceneType.Splash.ToString();



    [InitializeOnLoadMethod]
    private static void OnEditorLoaded() {
        string loadSceneName = EditorPrefs.GetString(Key_PlayLoadScene, playLoadScene.ToString());
        playLoadScene = UtilFunc.StringToEnum<SceneType>(loadSceneName);
        PlayLoadSceneName = playLoadScene.ToString();

        usePlayLoadScene = EditorPrefs.GetBool(Key_UsePlayLoadScene, usePlayLoadScene);
    }
}
#endif