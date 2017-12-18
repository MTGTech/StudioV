// This class is Auto-Generated
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

  public static class LukasGeneratedSceneMenuItems {

    [MenuItem("OpenScene/Complete/StudioVExampleDay")]
    private static void OpenSceneStudioVExampleDay() {
        Debug.Log("Selected item: StudioVExampleDay");
        OpenCompletedScene("StudioVExampleDay");
    }

    [MenuItem("OpenScene/Complete/StudioVExampleNight")]
    private static void OpenSceneStudioVExampleNight() {
        Debug.Log("Selected item: StudioVExampleNight");
        OpenCompletedScene("StudioVExampleNight");
    }

    [MenuItem("OpenScene/Launcher")]
    private static void OpenSceneLauncher() {
        Debug.Log("Selected item: Launcher");
        OpenUnfinishedScene("Launcher");
    }

static void OpenCompletedScene(string scene){
    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
		EditorSceneManager.OpenScene("Assets/Scene/Complete/" + scene + ".unity");
   }
}
static void OpenUnfinishedScene(string scene){
    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
		EditorSceneManager.OpenScene("Assets/Scene/" + scene + ".unity");
   }
}
}
