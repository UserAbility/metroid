using System.Collections.Generic;
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


    private List<GameObject> toDelete = new List<GameObject>();


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

        // Find and replace the prefabs selected 
        if (GUILayout.Button("Proceed with replacement?") && targetPrefabName != null && replacementPrefabName != null)
        {
            foreach (Transform child in GameObject.Find("Plane").transform)
            {
                //Debug.Log("Count of plane child objects = " + GameObject.Find("Plane").transform.childCount);

                //Debug.Log(child.name.ToUpper().Replace("(CLONE)", "") + " current object -> " + targetObject.name.ToUpper().Replace("(CLONE)", ""));

                if (targetObject.name.ToUpper().Replace("(CLONE)", "") == child.name.ToUpper().Replace("(CLONE)", ""))
                {
                    GameObject newObject;
                    newObject = UnityEngine.Object.Instantiate(Resources.Load(replacementObject.name)) as GameObject;

                    if (child.transform.parent.name == GameObject.Find("Plane").transform.name)
                    {
                        newObject.transform.parent = GameObject.Find("Plane").transform;
                    }
                    else
                    {
                        newObject.transform.parent = child.transform.parent;
                    }

                    newObject.transform.localPosition = child.transform.localPosition;
                    newObject.transform.localRotation = child.transform.localRotation;
                    newObject.transform.localScale = child.transform.localScale;

                    BoxCollider _bc = (BoxCollider)newObject.gameObject.AddComponent(typeof(BoxCollider));
                    // _bc.center = new Vector3(0, 50, 0);
                    _bc.size = new Vector3(100, 100, 100);


                    //Debug.Log(string.Format("[ReplaceGameObjects] {0} in {1}", child.transform.localPosition, newObject.transform.localPosition));
                    toDelete.Add(child.gameObject);



                }

                foreach (Transform subChild in child.transform)
                {
                    //Debug.Log("Count of plane child objects = " + GameObject.Find("Plane").transform.childCount);

                    //Debug.Log(child.name.ToUpper().Replace("(CLONE)", "") + " current object -> " + targetObject.name.ToUpper().Replace("(CLONE)", ""));

                    if (targetObject.name.ToUpper().Replace("(CLONE)", "") == subChild.name.ToUpper().Replace("(CLONE)", ""))
                    {
                        GameObject newObject;
                        newObject = UnityEngine.Object.Instantiate(Resources.Load(replacementObject.name)) as GameObject;

                        if (subChild.transform.parent.name == GameObject.Find("Plane").transform.name)
                        {
                            newObject.transform.parent = GameObject.Find("Plane").transform;
                        }
                        else
                        {
                            newObject.transform.parent = subChild.transform.parent;
                        }

                        newObject.transform.localPosition = subChild.transform.localPosition;
                        newObject.transform.localRotation = subChild.transform.localRotation;
                        newObject.transform.localScale = subChild.transform.localScale;

                        BoxCollider _bc = (BoxCollider)newObject.gameObject.AddComponent(typeof(BoxCollider));
                        // _bc.center = new Vector3(0, 50, 0);
                        _bc.size = new Vector3(100, 100, 100);


                        //Debug.Log(string.Format("[ReplaceGameObjects] {0} in {1}", child.transform.localPosition, newObject.transform.localPosition));
                        toDelete.Add(subChild.gameObject);



                    }
                }
            }

            


            //Delete the original prefabs
            foreach (var del in toDelete)
            {
                DestroyImmediate(del);
            }
        }
    }
}

