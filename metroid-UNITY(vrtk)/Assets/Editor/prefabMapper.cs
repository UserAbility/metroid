using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]

public class PrefabMapper : EditorWindow
{

    [MenuItem("MetroidVR/Prefab Mapper")]
    static void Do()
    {

        EditorWindow.GetWindowWithRect(typeof(PrefabMapper), new Rect(0, 0, 600, 800));

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
    float scrollLastX = 10;
    float scrollLastY = 10;

    public static List<tileMapperList> tileMap = new List<tileMapperList>();

    bool firstGeneration = true;

    string tileByte;
    string imagePath;
    string areaName;
    string prefabName;
    XElement nesTileData;
    int timesGenereated = 0;
    int controlIndex = 0;

    public Vector2 scrollPosition = Vector2.zero;

    void OnInspectorUpdate()
    {
        Repaint();
    }



    void OnLoad()
    {

    }

    public bool HasPreviewGUI()
    {
        return true;
    }

    public void OnPreviewGUI(Rect r, GUIStyle background)
    {
        Debug.Log("HELLLLO?");
        Repaint();
    }

    void OnGUI()
    {




        if (firstGeneration)
        {
            // If the tile data XML file exists load it, otherwise create it and then load it
            if (File.Exists(@"assets\resources\nesTilesMap.xml") || "DebugON" == "DebugOFF")
            {
                nesTileData = XElement.Load(@"assets\resources\nesTilesMap.xml");
            }
            else
            {
                XMLContainer.createXML("00", @"Assets\Resources\Materials\10x_bush2-greenB-320x160.png", "brinstar");
                nesTileData = XElement.Load(@"assets\resources\nesTilesMap.xml");
            }

            firstGeneration = false;

            nesTileData = XElement.Load(@"assets\resources\nesTilesMap.xml");


            if (nesTileData != null)
            {

                IEnumerable<XElement> nesTiles = nesTileData.Elements();
                tileMap.Clear();
                int numberOfTile = 0;
                foreach (XElement nesTile in nesTiles)
                {
                    tileMap.Add(new tileMapperList() { tileByte = nesTile.Element("tileByte").Value, preFabName = nesTile.Element("prefabMappedName").Value, imagePath = nesTile.Element("imagePath").Value, areaName = "BRINSTAR", controlID = numberOfTile });
                    //Debug.Log("Added tileByte " + nesTile.Element("tileByte").Value + " with preFabMappedName " + nesTile.Element("prefabMappedName").Value + " and ImagePath " + nesTile.Element("imagePath").Value + " in areaname BRINSTAR and controlID of " + numberOfTile);
                    numberOfTile++;
                }


            }
            //  Debug.Log("Times generated??" + timesGenereated);
            timesGenereated++;

        }

        //if (scrollLastY != scrollPosition.y)
        //{
        scrollPosition = GUILayout.BeginScrollView(new Vector2(scrollLastX, scrollLastY), true, true, GUILayout.Width(600), GUILayout.Height(800));



        //GUI.Button(new Rect(0, 0, 100, 20), "Top-left");
        //GUI.Button(new Rect(120, 0, 100, 20), "Top-right");
        //GUI.Button(new Rect(0, 180, 100, 20), "Bottom-left");
        //GUI.Button(new Rect(120, 180, 100, 20), "Bottom-right");



        foreach (tileMapperList tile in tileMap)
        {
            if (!tile.imagePath.Contains("empty"))
            {
                string nesTileName = "BRINSTAR" + ": " + tile.tileByte;



                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();


                GUILayout.Label("NES tile = " + nesTileName);
                //GUILayout.FlexibleSpace();

                GUILayout.Box(LoadPNG(tile.imagePath));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Current Prefab Assigned = " + tile.preFabName);
                //GUILayout.FlexibleSpace();



                //if (!prefabTargetPreview)
                //prefabTargetPreview = Editor.CreateEditor(tile.gameObject);

                //prefabTargetPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
                //GUILayout.Box(AssetPreview.GetAssetPreview(targetPreFab));

                //targetObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(tile.preFabName), typeof(GameObject), true);
                if (GUILayout.Button("Prefeb To Replace") && replacementPrefabWindow != (100 + controlIndex))
                {
                    //create a window picker control ID
                    replacementPrefabWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + (100 + controlIndex);

                    // Debug.Log("{Init} Looking for tile matching tileByte " + tile.tileByte + " and controlID " + tile.controlID);
                    var tileToUpdate = tileMap.Find(d => d.tileByte == tile.tileByte && d.controlID == tile.controlID); ;
                    if (tileToUpdate != null)
                    {
                        tileToUpdate.controlID = replacementPrefabWindow;
                        //Debug.Log("Assigned tile " + tile.tileByte + " controlID to targetPrefabWindowID of " + replacementPrefabWindow);
                        controlIndex++;
                    };

                    //use the ID you just created
                    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "br-", replacementPrefabWindow);

                }

                if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == replacementPrefabWindow)
                {
                    targetPrefabName = EditorGUIUtility.GetObjectPickerObject().name;

                    // Debug.Log(" {Updater} Looking for tile matching tileByte " + tileMap.Find(d => d.controlID == replacementPrefabWindow).tileByte);
                    var tileToUpdate = tileMap.Find(d => d.tileByte == tileMap.Find(subFind => subFind.controlID == replacementPrefabWindow).tileByte);
                    if (tileToUpdate != null)
                    {
                        // Update the tileMap object table with the requested change
                        tileToUpdate.preFabName = targetPrefabName;
                        //Save the change to file 
                        XMLContainer.updateXML(tileToUpdate.tileByte, targetPrefabName);
                        // Debug.Log(targetPrefabName + " <- selected prefabname to update | Byte applied to -> " + tile.tileByte + " To targetPreFabWindow target -> " + replacementPrefabWindow);
                    };


                    //targetObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(targetPrefabName), typeof(GameObject), true);
                    // Debug.Log(targetPreFab.name + " target object loaded with name.. ");
                    replacementPrefabWindow = -1;



                }

                // Debug.Log(tile.preFabName + " <-- preview of the tile being loaded | Byte applied to -> " + tile.tileByte + " | Imagepath = " + tile.imagePath);
                targetObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(tile.preFabName), typeof(GameObject), true);
                //prefabTargetPreview = Editor.CreateEditor(targetObject);

                //prefabTargetPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
                GUILayout.Box(AssetPreview.GetAssetPreview(targetObject));

                //    Repaint();

                // Debug.Log(Event.current.commandName);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            }

        }

        scrollLastX = scrollPosition.x;
        scrollLastY = scrollPosition.y;

        GUILayout.EndScrollView();








        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

        // }


        //if (targetObject != null)
        //{
        //    if (prefabTargetPreview == null)
        //    {
        //        prefabTargetPreview = Editor.CreateEditor(targetObject);
        //    }
        //    else
        //    {
        //        prefabTargetPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
        //        Repaint();
        //    }
        //}


        //Repaint();
        // Debug.Log(tileMap.Count + " Tilemap count");





        //var monsterCollection = XMLContainer.Load(Path.Combine(Application.dataPath, @"\Assets\Resources\test.xml"));


        //var XMLCollection = XMLContainer.Load(Path.Combine(Application.dataPath, @"resources\nesTilesMap.xml"));

        //var xmlData = @"<XMLCollection><XMLs><XML name=""a""><Health>5</Health></XML></XMLs></XMLCollection>";
        //var XMLCollection = XMLContainer.LoadFromText(xmlData);

        //XMLCollection.Save((@"D:\UnityProjects\metroid-MetroidVR_Development\metroid-UNITY(vrtk)\Assets\Resources\Assets\Resources\test.xml"));

        //Debug.Log(@"D:\UnityProjects\metroid-MetroidVR_Development\metroid-UNITY(vrtk)\Assets\Resources\Assets\Resources\test.xml");

        //if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == targetPreFabWindow)
        //{
        //    targetPrefabName = EditorGUIUtility.GetObjectPickerObject().name;
        //    Debug.Log(targetPreFabWindow);
        //    targetObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(targetPrefabName), typeof(GameObject), true);
        //    targetPreFabWindow = -1;
        //    //Repaint();
        //}

        //if (targetObject != null)
        //{
        //    if (prefabTargetPreview == null)
        //    {
        //        prefabTargetPreview = Editor.CreateEditor(targetObject);
        //    }
        //    else
        //    {
        //        prefabTargetPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
        //        Repaint();
        //    }
        //}




        //if (GUILayout.Button("Prefeb To Replace") && replacementPrefabWindow != 100)
        //{
        //    //create a window picker control ID
        //    targetPreFabWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;

        //    //use the ID you just created
        //    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "br-", targetPreFabWindow);
        //}



        //if (GUILayout.Button("Prefeb To Replace") && replacementPrefabWindow != 100)
        //{
        //    //create a window picker control ID
        //    targetPreFabWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;

        //    //use the ID you just created
        //    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "br-", targetPreFabWindow);
        //}



        //if (targetObject != null)
        //{
        //    if (prefabTargetPreview == null)
        //    {
        //        prefabTargetPreview = Editor.CreateEditor(targetObject);
        //    }
        //    else
        //    {
        //        prefabTargetPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
        //        Repaint();
        //    }
        //}

        //GUILayout.EndVertical();

        //GUILayout.BeginVertical();
        //if (GUILayout.Button("Replacement Prefab") && replacementPrefabWindow != 101)
        //{
        //    //create a window picker control ID
        //    replacementPrefabWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 101;

        //    //use the ID you just created
        //    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "br-", replacementPrefabWindow);
        //}



        //if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == replacementPrefabWindow)
        //{
        //    replacementPrefabName = EditorGUIUtility.GetObjectPickerObject().name;
        //    //Debug.Log(replacementPreFab.name);
        //    Debug.Log(replacementPrefabWindow);
        //    replacementObject = (GameObject)EditorGUILayout.ObjectField(Resources.Load(replacementPrefabName), typeof(GameObject), true);
        //    replacementPrefabWindow = -1;
        //    // Repaint();
        //}

        //if (replacementObject != null)
        //{
        //    if (prefabReplacementPreview == null)
        //    {
        //        prefabReplacementPreview = Editor.CreateEditor(replacementObject);
        //    }
        //    else
        //    {
        //        prefabReplacementPreview.OnPreviewGUI(GUILayoutUtility.GetRect(250, 250), GUIStyle.none);
        //        Repaint();
        //    }

        //}

        //GUILayout.EndVertical();

        //GUILayout.EndHorizontal();
        //if (GUILayout.Button("Proceed with replacement?") && targetPrefabName != null && replacementPrefabName != null)
        //{
        //    //targetPrefabNameTB = GUI.TextField(new Rect(10, 100, 500, 200), targetPrefabName, 25);
        //    // replacementPrefabNameTB = GUI.TextField(new Rect(10, 130, 500, 200), replacementPrefabName, 25);



        //    foreach (Transform child in GameObject.Find("Plane").transform)
        //    {
        //        foreach (Transform subChild in child.transform)
        //        {
        //            Debug.Log(subChild.name.ToUpper().Replace("(CLONE)", "") + " current object -> " + targetObject.name.ToUpper().Replace("(CLONE)", ""));
        //            try
        //            {
        //                if (targetObject.name.ToUpper().Replace("(CLONE)", "") == subChild.name.ToUpper().Replace("(CLONE)", ""))
        //                {


        //                    GameObject newObject;
        //                    newObject = UnityEngine.Object.Instantiate(Resources.Load(replacementObject.name)) as GameObject;
        //                    newObject.transform.position = subChild.transform.position;
        //                    newObject.transform.rotation = subChild.transform.rotation;
        //                    newObject.transform.parent = subChild.transform.parent;
        //                    DestroyImmediate(subChild.gameObject, true);

        //                }
        //            }
        //            catch { }
        //        }
        //    }



        //    targetPrefabName = null;
        //    replacementPrefabName = null;
        //    DestroyImmediate(targetObject.gameObject, true);
        //    DestroyImmediate(replacementObject.gameObject, true);

        //}




        ////string commandName = Event.current.commandName;

        ////Debug.Log(commandName);

        ////if (commandName == "ObjectSelectorUpdated")
        ////{
        ////    targetPreFab = EditorGUIUtility.GetObjectPickerObject();



        ////}
        ////else if (commandName == "ObjectSelectorClosed")
        ////{
        ////    replacementPreFab = EditorGUIUtility.GetObjectPickerObject();
        ////}



        ////Repaint();


    }

    //public static void loadXMLGuiMapper(string tileByte, string imagePath, string areaName, string prefabName)
    //{
    //    string nesLabel = areaName + ": " + tileByte;
    //    prefabName = "PREFAB NAME HERE";

    //    //GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
    //    //GUILayout.BeginHorizontal();
    //    //GUILayout.BeginVertical();
    //    //GUILayout.Label(nesLabel);
    //    //GUILayout.FlexibleSpace();

    //    //GUILayout.Box(LoadPNG(imagePath));
    //    //GUILayout.EndVertical();

    //    //GUILayout.BeginVertical();
    //    //GUILayout.Label(prefabName);
    //    //GUILayout.FlexibleSpace();

    //    //GUILayout.Box(LoadPNG(imagePath));
    //    //GUILayout.EndVertical();
    //    //GUILayout.EndHorizontal();
    //    //GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });


    //    Debug.Log("ImagePath ->" + imagePath + " " + "nesLabel -> " + nesLabel + " prefabName ->" + prefabName);


    //}

    //public static void createXMLS()
    //{

    //}


    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }


}