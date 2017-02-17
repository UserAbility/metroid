using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PrefabReplacement : EditorWindow
{

    [MenuItem("MetroidVR/Prefab Replacement")]
    static void Do()
    {

        EditorWindow.GetWindowWithRect(typeof(PrefabReplacement), new Rect(0, 0, 600, 350));
    }

    Object targetPreFab = null;
    Object replacementPreFab = null;
    int targetPreFabWindow;
    int replacementPrefabWindow;
    public string targetPrefabNameTB = "";
    public string replacementPrefabNameTB = "";
    public string targetPrefabName = "";
    public string replacementPrefabName = "";
    Editor prefabTargetPreview = null;
    Editor prefabReplacementPreview = null;
    GameObject targetObject = null;
    GameObject replacementObject = null;

    void OnInspectorUpdate()
    {
        // Repaint();
    }

    public bool HasPreviewGUI()
    {
        return true;
    }

    public void OnPreviewGUI(Rect r, GUIStyle background)
    {
        Debug.Log("Test");
    }

    void OnGUI()
    {

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Prefeb To Replace") && replacementPrefabWindow != 100)
        {
            //create a window picker control ID
            targetPreFabWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;

            //use the ID you just created
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "br-", targetPreFabWindow);
        }

        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == targetPreFabWindow)
        {
            targetPrefabName = EditorGUIUtility.GetObjectPickerObject().name;
            Debug.Log(targetPreFabWindow);
            targetObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(targetPrefabName), typeof(GameObject), true);
            targetPreFabWindow = -1;
            //Repaint();
        }

        if (targetObject != null)
        {
            if (prefabTargetPreview == null)
            {
                prefabTargetPreview = Editor.CreateEditor(targetObject);
            }
            else
            {
                prefabTargetPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
                Repaint();
            }
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Replacement Prefab") && replacementPrefabWindow != 101)
        {
            //create a window picker control ID
            replacementPrefabWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 101;

            //use the ID you just created
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "br-", replacementPrefabWindow);
        }



        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == replacementPrefabWindow)
        {
            replacementPrefabName = EditorGUIUtility.GetObjectPickerObject().name;
            //Debug.Log(replacementPreFab.name);
            Debug.Log(replacementPrefabWindow);
            replacementObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(replacementPrefabName), typeof(GameObject), true);
            replacementPrefabWindow = -1;
            // Repaint();
        }

        if (replacementObject != null)
        {
            if (prefabReplacementPreview == null)
            {
                prefabReplacementPreview = Editor.CreateEditor(replacementObject);
            }
            else
            {
                prefabReplacementPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
                Repaint();
            }

        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        if (GUILayout.Button("Proceed with replacement?") && targetPrefabName != null && replacementPrefabName != null)
        {
            //targetPrefabNameTB = GUI.TextField(new Rect(10, 100, 500, 200), targetPrefabName, 25);
           // replacementPrefabNameTB = GUI.TextField(new Rect(10, 130, 500, 200), replacementPrefabName, 25);



            foreach (Transform child in GameObject.Find("Plane").transform)
            {
                foreach (Transform subChild in child.transform)
                {
                    Debug.Log(subChild.name.ToUpper().Replace("(CLONE)", "") + " current object -> " + targetObject.name.ToUpper().Replace("(CLONE)", ""));
                    try
                    {
                        if (targetObject.name.ToUpper().Replace("(CLONE)", "") == subChild.name.ToUpper().Replace("(CLONE)", ""))
                        {


                            GameObject newObject;
                            newObject = UnityEngine.Object.Instantiate(Resources.Load(replacementObject.name)) as GameObject;
                            newObject.transform.position = subChild.transform.position;
                            newObject.transform.rotation = subChild.transform.rotation;
                            newObject.transform.parent = subChild.transform.parent;
                            DestroyImmediate(subChild.gameObject, true);

                        }
                    }
                    catch { }
                }
            }



            targetPrefabName = null;
            replacementPrefabName = null;
            DestroyImmediate(targetObject.gameObject, true);
            DestroyImmediate(replacementObject.gameObject, true);

        }

        


        //string commandName = Event.current.commandName;

        //Debug.Log(commandName);

        //if (commandName == "ObjectSelectorUpdated")
        //{
        //    targetPreFab = EditorGUIUtility.GetObjectPickerObject();



        //}
        //else if (commandName == "ObjectSelectorClosed")
        //{
        //    replacementPreFab = EditorGUIUtility.GetObjectPickerObject();
        //}



        //Repaint();


    }


}