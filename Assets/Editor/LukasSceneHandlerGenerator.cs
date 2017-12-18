using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;


public class LukasSceneHandlerGenerator : MonoBehaviour {

	/// <summary>
	/// Generates a list of menuitems from an array of scenes
	/// </summary>
	[MenuItem("Assets/Lukas Scene Handler/Generate Scene Menu Items")]
	static void GenerateSceneMenuItems() {
		string pathFromAssets = "/Scene/Complete/";
		string alternativePath = "/Scene/";

		string scriptFile = Application.dataPath + "/Editor/LukasGeneratedSceneMenuItems.cs";

		string[] files;
		string[] unfinishedScenes;
		files = Directory.GetFiles (Application.dataPath + pathFromAssets, "*.unity");
		unfinishedScenes = Directory.GetFiles (Application.dataPath + alternativePath, "*.unity");

		// The class string
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("// This class is Auto-Generated");
		sb.AppendLine("using UnityEngine;");
		sb.AppendLine("using UnityEditor;");
		sb.AppendLine("using UnityEditor.SceneManagement;");
		sb.AppendLine("");
		sb.AppendLine("  public static class LukasGeneratedSceneMenuItems {");
		sb.AppendLine("");

		foreach (string s in files) {
			string fileName = s.Replace (Application.dataPath + "/Scene/Complete/", string.Empty).ToString ();
			Debug.Log ("path: " + Application.dataPath);
			Debug.Log (fileName);
			fileName = fileName.Replace (".unity", string.Empty).ToString ();

			sb.AppendLine("    [MenuItem(\"OpenScene/Complete/" + fileName + "\")]");
			sb.AppendLine("    private static void OpenScene" + fileName + "() {");
			sb.AppendLine("        Debug.Log(\"Selected item: " + fileName + "\");");
			sb.AppendLine("        OpenCompletedScene(\"" + fileName + "\");");
			sb.AppendLine("    }");
			sb.AppendLine("");
		}
		foreach (string s in unfinishedScenes) {
			string fileName = s.Replace (Application.dataPath + "/Scene/", string.Empty).ToString ();
			fileName = fileName.Replace ("." + "unity", string.Empty).ToString ();

			sb.AppendLine("    [MenuItem(\"OpenScene/" + fileName + "\")]");
			sb.AppendLine("    private static void OpenScene" + fileName + "() {");
			sb.AppendLine("        Debug.Log(\"Selected item: " + fileName + "\");");
			sb.AppendLine("        OpenUnfinishedScene(\"" + fileName + "\");");
			sb.AppendLine("    }");
			sb.AppendLine("");
		}

		sb.AppendLine ("static void OpenCompletedScene(string scene){");
		sb.AppendLine ("    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {");
		sb.AppendLine ("		EditorSceneManager.OpenScene(\"Assets/Scene/Complete/\" + scene + \".unity\");");
		sb.AppendLine("   }");
		sb.AppendLine("}");

		sb.AppendLine ("static void OpenUnfinishedScene(string scene){");
		sb.AppendLine ("    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {");
		sb.AppendLine ("		EditorSceneManager.OpenScene(\"Assets/Scene/\" + scene + \".unity\");");
		sb.AppendLine("   }");
		sb.AppendLine("}");
		sb.AppendLine("}");

		//Writes the class and imports it so it is visible in the Project window
		System.IO.File.Delete(scriptFile);
		System.IO.File.WriteAllText(scriptFile, sb.ToString(), System.Text.Encoding.UTF8);
		AssetDatabase.ImportAsset("Assets/Editor/LukasGeneratedSceneMenuItems.cs");
	}
}
