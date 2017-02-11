
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CreatePrefabEditor
{
   
    public Transform player;
    public Transform floor_valid;
    public Transform floor_obstacle;
    public Transform floor_checkpoint;

    // Pre-fab children variable
    public GameObject childTileObject;

    // Map Data array
    public static string[][] mapStructure;

    public const string sfloor_valid = "0";
    public const string sfloor_obstacle = "1";
    public const string sfloor_checkpoint = "2";
    public const string sstart = "S";

    public static Transform lastChildPosition;
    public static float width;

    // Build a list to hold the multidimensional data mapped between NES tile byte assigments and Unity prefabs
    public static List<tileMapperList> tileMap = new List<tileMapperList>();
    public static List<roomDataList> roomDataList = new List<roomDataList>();



    //public const string br_c_platform_aqua = "";
    //public const string br_c_rock2_aqua = "";
    //public const string br_c_spiral_aqua = "";
    //public const string br_c_towers_aqua = "";

    public static string HexStr(byte[] p)
    {
        char[] c = new char[p.Length * 2 + 2];
        byte b;
        c[0] = '0'; c[1] = 'x';
        for (int y = 0, x = 2; y < p.Length; ++y, ++x)
        {
            b = ((byte)(p[y] >> 4));
            c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            b = ((byte)(p[y] & 0xF));
            c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
        }
        return new string(c);
    }

    public static int nextOffset = 0;
    public static string structureName = "";
    public static string pAreaName = "";
    public static int structNum = 0;

    public static void structureBuilder(string fileName, int offset, string areaName)
    {
        // Set the public variable name for this function call... do this differently later :P
        pAreaName = areaName;

        // Setup the binary stream reader to take read in the hex values
        Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Assign the virtual file to read the data stream into
        BinaryReader brFile = new BinaryReader(fileStream);

        // Set the initial position to read data from in the file using a hexadecimal offset (Where the data is in the file.
        // This is the first byte of the targetted structure)
        fileStream.Position = offset;

        // Read that first byte which says how many bytes are in that structures row (Likely values are 01-08)
        int numOfRowTiles = Convert.ToInt32(HexStr(brFile.ReadBytes(1)).Replace("0x", ""));

        // Set the offset to the macro data bytes of the first structure row to be read. This skips over the byte that contained
        // our numOfRowTiles value
        nextOffset = (Convert.ToInt32(offset.ToString("X"), 16) + 1);

        // Set a variable to monitor the datastream progression and stop if the data value returned is "FF". This value declares that
        // the structure is fully read into memory.                     
        string rowHeaderByte = "";

        // Create an array to store our structure row data in
        StringBuilder structure = new StringBuilder();

        // Look for more rows of the next structure until the byte F1 (brinstar, norfair) or A7 (tourian) or FF (ridley) is encountered
        while (rowHeaderByte != "F1" && rowHeaderByte != "A7" && rowHeaderByte != "FF")
        {
            // Look for more rows of this particular structure until the byte FF is encountered
            while (rowHeaderByte != "FF")
            {
                // Set the next offset position to read data from in the fileStream
                //fileStream.Position = nextOffset;


                //string lastOffset = nextOffset.ToString("X");
                string bytesRead = HexStr(brFile.ReadBytes(numOfRowTiles)).Replace("0x", "");

                // Append the current structure data row to the text file string builder
                if (numOfRowTiles > 0 && numOfRowTiles < 9)
                {
                    structure.AppendLine(Format(bytesRead, 2, " "));
                }
                else
                {
                    //  MessageBox.Show(numOfRowTiles + " <-- number to read, offset read --> " + lastOffset);
                    // break;
                }
                //MessageBox.Show("First Read Offset " + (Convert.ToInt32(fileStream.Position.ToString("X"), 16) - 1).ToString("X") + " , Bytes Read = " + numOfRowTiles+ " ,DATA = " + bytesRead);

                // Look at next immediate byte to see if it is FF or a value that indicates another row exists
                rowHeaderByte = HexStr(brFile.ReadBytes(1)).Replace("0x", "");


                if (rowHeaderByte != "FF")
                {
                    // Read that first byte of the next structure row which says how many bytes are in that structures row.
                    // (Likely values are 01-08)
                    if (Convert.ToInt32(rowHeaderByte) > 0 && Convert.ToInt32(rowHeaderByte) < 9)
                    {
                        numOfRowTiles = Convert.ToInt32(rowHeaderByte);
                    }
                    else
                    {
                        // MessageBox.Show("Inside FF check: " + rowHeaderByte + " <-- byte read, offset read --> " + fileStream.Position.ToString("X"));
                        //  break;
                    }
                }
            }

            //MessageBox.Show("OUTSIDE FF Offset " + (Convert.ToInt32(fileStream.Position.ToString("X"), 16) - 1).ToString("X") + " , Row Header Byte = " + rowHeaderByte);

            // Look for more rows of the next structure until the byte F1 (brinstar, norfair) or A7 (tourian) or FF (ridley) is encountered
            rowHeaderByte = HexStr(brFile.ReadBytes(1)).Replace("0x", "");
            if (rowHeaderByte != "F1" && rowHeaderByte != "A7" && rowHeaderByte != "FF")
            {
                numOfRowTiles = Convert.ToInt32(rowHeaderByte);
            }

            //MessageBox.Show("Outside FF next byte = " + numOfRowTiles.ToString() + " Offset = " + nextOffset.ToString("X"));

            // Write the structure data to a file on the system that reflects the actual structure byte value
            File.WriteAllText("Assets/Resources/struct/" + areaName + "/" + structNum.ToString("X2") + ".txt", structure.ToString());
            //MessageBox.Show("c:\\temp\\structure_" + structNum + "_" + areaName + ".txt <-- saving");

            // Increment the structure number count
            structNum++;

            // Reset the string builder object to clear out the previous structure
            structure.Length = 0;
        }
        Debug.Log("All structures read!");
    }


    public static void roomBuilder(string fileName, int offset, string areaName, int xMapOffset, int yMapOffset, string roomNumber)
    {
        // Setup the binary stream reader to take read in the hex values
        Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Assign the virtual file to read the data stream into
        BinaryReader brFile = new BinaryReader(fileStream);
        fileStream.Position = offset;

        // Variable to store the nes XY coords. They store them Y then X for some reason..            
        string nesYXcoords = "";

        // Variable to store the structure byte read from the NES file/structure
        string structureRead = "";

        // Variable to store the palette/attribute byte
        string paletteRead = "";

        while (nesYXcoords != "FD" && nesYXcoords != "FF")
        {
            nesYXcoords = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

            if (nesYXcoords != "FD" && nesYXcoords != "FF")
            {
                structureRead = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

                paletteRead = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

                //Debug.Log("XY positionRead = " + nesYXcoords + " structure # = " + structureRead + " paletteRead =" + paletteRead + " offset was " + offset.ToString("X"));
                Debug.Log("Assets/Resources/struct/" + areaName.Trim() + "/" + structureRead.Trim() + ".txt");

                structurePreFabBuilder(
                    "Assets/Resources/struct/" + areaName.Trim() + "/" + structureRead.Trim() + ".txt",
                    int.Parse(nesYXcoords.Substring(1, 1), System.Globalization.NumberStyles.HexNumber) + xMapOffset,
                    int.Parse(nesYXcoords.Substring(0, 1), System.Globalization.NumberStyles.HexNumber) + yMapOffset,
                    areaName,
                    structureRead,
                    roomNumber
                    );
            }
        }

        string thing1 = "";

        // Read past the structure data to get any item/enemy/door info
        while (thing1 != "FF")
        {
            // Sprite Slot/Item Type (01 = Slot 0/Enemy)
            thing1 = HexStr(brFile.ReadBytes(1)).Replace("0x", "");
            if (thing1 != "FF")
            {
                // Difficulty/Enemy Type (05 = easiest yellow crawler)
                string thing2 = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

                // YX screen placement for item
                string thing3 = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

                Debug.Log("thing1 = " + thing1 + " thing2 = " + thing2 + " thing3 =" + thing3 + " offset was " + fileStream.Position.ToString("X") + " Room # = " + roomNumber);
            }
        }
        Debug.Log("All room data read!");
    }

    public static void structurePreFabBuilder(string fileName, int nesX, int nesY, string areaName, string structureNumber, string roomNumber)
    {
        int scaleX = 2;
        int scaleY = 2;
        GameObject childTileObject = null;

        using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
        {

            // Declare a structure list to hold the bytes of our structure for indexing numerically later.
            List<structList> structure = new List<structList>();

            // Re-use the structure name from the file name. Later we may need other variables to say which 
            // CHR table data the structure is related to.
            string structName = Path.GetFileNameWithoutExtension(((FileStream)sr.BaseStream).Name);

            // Re-work the structname in case of duplicates
            structName = areaName + "_" + roomNumber + "_" + structureNumber + "_" + (FindGameObjectsWithSameName(structName).Length + 1).ToString();

            // Create default parent
            var currentStructure = new GameObject(structName);

            // Generate our structure at specific X,Y,Z coords to match NES placement
            currentStructure.transform.position += new Vector3(nesX, nesY, 0);

            //currentStructure.transform.localScale += new Vector3(0.1f, -0.1f, 0.1f);

            // Set the new parent object to the plane parent
            currentStructure.transform.SetParent(GameObject.Find("Plane").transform, true);

            string structureData = Regex.Replace(sr.ReadToEnd(), @"\t|\n|\r|", "");
            string[] structureBytes = structureData.Split(' ');
            foreach (string s in structureBytes)
            {
                if (s != "")
                {
                    structure.Add(new structList() { tileByte = s, structureName = structName });
                    //Debug.Log("Added TileByte = " + s + " StructureName = " + structName);
                }
            }

            string text = System.IO.File.ReadAllText(fileName);
            string[] lines = Regex.Split(text, "\r\n");
            int rows = lines.Length;
            int byteCount = 0;

            string[][] jagged = new string[rows][];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] stringsOfLine = Regex.Split(lines[i], " ");
                jagged[i] = stringsOfLine;
            }

            //Debug.Log("Line1 = " + (jagged.Length-1).ToString() + "," + (jagged[0].Length-1).ToString());
            //Debug.Log("Line2 = " + (jagged.Length-1).ToString() + "," + (jagged[1].Length-1).ToString());
            //Debug.Log("Line3 = " + (jagged.Length-1).ToString() + "," + (jagged[2].Length-1).ToString());
            //Debug.Log("Line4 = " + (jagged.Length-1).ToString() + "," + (jagged[3].Length-1).ToString());
            int x = 0;
            int y = 0;

            // create planes based on matrix
            for (y = 0; y < jagged.Length - 1; y++)
            {
                for (x = 0; x < jagged[y].Length - 1; x++)
                {
                    var prefabName = "";
                    // Create the prefab in the UI editor/canvas
                    if (byteCount < structure.Count)
                    {
                        // Use this to see where the last structure failed to load
                        Debug.Log("StructureCount  " + structure.Count);
                        Debug.Log("StructureTileByte  " + structure[byteCount].tileByte + " FileName > " + fileName);

                        prefabName = tileMap.Find(map => map.tileByte.Contains(structure[byteCount].tileByte)).preFabName;
                        byteCount++;

                        Debug.Log(prefabName.Replace("_", "-") + "," + structure.Count + "," + byteCount);

                        switch (prefabName)
                        {

                            case "blank":
                                break;


                            default:

                                if (prefabName.Replace("_", "-") != "")
                                {
                                    Debug.Log(structure.Count + "," + byteCount + "," + prefabName.Replace("_", "-") + "WTF");
                                    childTileObject = UnityEngine.Object.Instantiate(Resources.Load(prefabName), new Vector3(x, y, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
                                    foreach (Transform child in childTileObject.transform)
                                    {
                                        // child.transform.SetParent(childTileObject.transform, false);
                                        //child.transform.position = new Vector3(childTileObject.transform.position.x , childTileObject.transform.position.y -1, childTileObject.transform.position.z);

                                        child.transform.localScale = new Vector3(0.01f, -0.01f, 0.01f);

                                    }

                                    childTileObject.transform.localScale = new Vector3(0.01f, -0.01f, 0.01f);


                                    //BoxCollider _bc = (BoxCollider)childTileObject.gameObject.AddComponent(typeof(BoxCollider));
                                    //_bc.center = new Vector3(0, 50, 0);

                                    childTileObject.transform.SetParent(currentStructure.transform, false);


                                    // All Tiles get colliders
                                    BoxCollider _bc = (BoxCollider)childTileObject.gameObject.AddComponent(typeof(BoxCollider));
                                    // _bc.center = new Vector3(0, 50, 0);
                                    _bc.size = new Vector3(100, 100, 100);
                                }
                                break;


                        }

                    }
                }



            }

            currentStructure.transform.SetParent(GameObject.Find("Plane").transform, false);



            //currentStructure.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);

            // All Tiles get colliders per row
            //BoxCollider _bc = (BoxCollider)sceneParent.gameObject.AddComponent(typeof(BoxCollider));
            //Debug.Log(x + " <== X and Y ==>" + y);
            //_bc.center = new Vector3(1.5f, 1f, 0f);
            //_bc.size = new Vector3(x, y, 1);

            //BoxCollider _bc = (BoxCollider)sceneParent.gameObject.AddComponent(typeof(BoxCollider));
            //_bc.center = Vector3.zero;
        }



    }

    public static Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    public static string Format(string number, int batchSize, string separator)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i <= number.Length / batchSize; i++)
        {
            if (i > 0) sb.Append(separator);
            int currentIndex = i * batchSize;
            sb.Append(number.Substring(currentIndex,
                      Math.Min(batchSize, number.Length - currentIndex)));
        }
        return sb.ToString();
    }

    private static void loadRuntimeVariables()
    {

        // Add each related byte <-> prefab name mapping

        // Brinstar: Lava base
        tileMap.Add(new tileMapperList() { tileByte = "00", preFabName = "br-c-lava-orange-top" });
        tileMap.Add(new tileMapperList() { tileByte = "16", preFabName = "br-c-lava-orange-top" });
        tileMap.Add(new tileMapperList() { tileByte = "2D", preFabName = "br-c-lava-orange-top" });

        // Brinstar: Lava top 
        tileMap.Add(new tileMapperList() { tileByte = "01", preFabName = "br-c-lava-orange-base" });
        tileMap.Add(new tileMapperList() { tileByte = "15", preFabName = "br-c-lava-orange-base" });
        tileMap.Add(new tileMapperList() { tileByte = "2C", preFabName = "br-c-lava-orange-base" });

        // Brinstar: Hall blue ( NOT WORKING.. REPLACE WITH br-m-brick-aqua FOR NOW )
        tileMap.Add(new tileMapperList() { tileByte = "02", preFabName = "br-c-hall-blue" });
        //tileMap.Add(new tileMapperList() { tileByte = "02", preFabName = "br-m-brick-aqua" });

        // Brinstar: hollow tubes no window roll through???
        tileMap.Add(new tileMapperList() { tileByte = "03", preFabName = "br-m-tube-aqua-hori" });

        //Brinstar: Aqua Rock
        tileMap.Add(new tileMapperList() { tileByte = "08", preFabName = "br-m-rock-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "24", preFabName = "br-m-rock-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "29", preFabName = "br-m-rock-aqua" });


        tileMap.Add(new tileMapperList() { tileByte = "0A", preFabName = "br-m-rockR-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "09", preFabName = "br-m-rockL-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "0C", preFabName = "br-c-statue-aqua" });

        // Brinstar: Blue balls ( NOT WORKING.. REPLACE WITH POT AQUA FOR NOW )
        tileMap.Add(new tileMapperList() { tileByte = "0D", preFabName = "br-c-balls-purple" });
        tileMap.Add(new tileMapperList() { tileByte = "2F", preFabName = "br-c-balls-purple" });
        //tileMap.Add(new tileMapperList() { tileByte = "0D", preFabName = "br-c-pot-aqua" });
        //tileMap.Add(new tileMapperList() { tileByte = "2F", preFabName = "br-c-pot-aqua" });


        tileMap.Add(new tileMapperList() { tileByte = "17", preFabName = "br-m-pillar-aqua" });

        // Brinstar: Pillbrick  ( NOT WORKING REPLACE WITH POT AQUA FOR NOW )
        tileMap.Add(new tileMapperList() { tileByte = "1A", preFabName = "br-c-pillbrick-blue" });
        //tileMap.Add(new tileMapperList() { tileByte = "1A", preFabName = "br-c-pot-aqua" });



        tileMap.Add(new tileMapperList() { tileByte = "1C", preFabName = "br-c-brush-aqua" });


        

        // Brinstar: Bush Aqua (temp set to brush due to difficulties with the bush tile
        tileMap.Add(new tileMapperList() { tileByte = "1D", preFabName = "br-c-bush-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "2A", preFabName = "br-c-bush-aqua" });

        tileMap.Add(new tileMapperList() { tileByte = "1F", preFabName = "br-m-pot-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "20", preFabName = "br-m-vent-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "22", preFabName = "br-m-ball" });
        tileMap.Add(new tileMapperList() { tileByte = "23", preFabName = "br-c-brick-aqua" });

        tileMap.Add(new tileMapperList() { tileByte = "30", preFabName = "br-m-bubble-lone-purple" });

        //Brinstar: br-c-foam-aqua size appears to off. Is it 1:1 scale like the other tiles?
        tileMap.Add(new tileMapperList() { tileByte = "33", preFabName = "br-m-foam-aqua" });
        //tileMap.Add(new tileMapperList() { tileByte = "33", preFabName = "br-c-fence-purple" });


        tileMap.Add(new tileMapperList() { tileByte = "34", preFabName = "br-c-pillar-aqua" });



        //Brinstar: Need 1:1 of spiral-aqua using br-m-brick-white for now
        tileMap.Add(new tileMapperList() { tileByte = "36", preFabName = "br-c-spiral-blue" });
        //tileMap.Add(new tileMapperList() { tileByte = "36", preFabName = "br-m-brick-white" });


        //Brinstar: Brick Aqua ( NOT WORKING REPLACE WITH VENTS FOR NOW )
        tileMap.Add(new tileMapperList() { tileByte = "1E", preFabName = "br-c-brick-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "2E", preFabName = "br-c-brick-aqua" });


        //Brinstar: Need 1:1 of tube-hori-aqua using br-m-brick-aqua for now

        tileMap.Add(new tileMapperList() { tileByte = "1B", preFabName = "br-m-pillar-hori-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "2B", preFabName = "br-m-pillar-hori-aqua" });
        tileMap.Add(new tileMapperList() { tileByte = "35", preFabName = "br-m-pillar-hori-aqua" });
        //tileMap.Add(new tileMapperList() { tileByte = "1B", preFabName = "br-m-brick-aqua" });
        //tileMap.Add(new tileMapperList() { tileByte = "2B", preFabName = "br-m-brick-aqua" });
        //tileMap.Add(new tileMapperList() { tileByte = "35", preFabName = "br-m-brick-aqua" });


        tileMap.Add(new tileMapperList() { tileByte = "0B", preFabName = "br-m-rock2-aqua" });

        // Brinstar: door seal tiles set these to br-c-vent-aqua for now until the tiles are fixed
        tileMap.Add(new tileMapperList() { tileByte = "06", preFabName = "br-m-seal-blue" });
        tileMap.Add(new tileMapperList() { tileByte = "07", preFabName = "br-m-seal-blue" });
        //tileMap.Add(new tileMapperList() { tileByte = "06", preFabName = "br-c-vent-aqua" });
        //tileMap.Add(new tileMapperList() { tileByte = "07", preFabName = "br-c-vent-aqua" });

        // Temp blank/black tile assignments
        tileMap.Add(new tileMapperList() { tileByte = "04", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "05", preFabName = "blank" });

        tileMap.Add(new tileMapperList() { tileByte = "0E", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "0F", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "10", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "11", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "12", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "13", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "14", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "37", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "38", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "39", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "3A", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "3B", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "3C", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "3D", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "21", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "27", preFabName = "blank" });
        tileMap.Add(new tileMapperList() { tileByte = "28", preFabName = "blank" });

        // TEMP MAPPING FOR THE 2 piece BLUE BUSH
        tileMap.Add(new tileMapperList() { tileByte = "3E", preFabName = "br-c-bush-blueB" });
        tileMap.Add(new tileMapperList() { tileByte = "3F", preFabName = "br-c-bush-blue" });

        // TEMP MAPPING FOR THE 2 PIECE PIPE set to br-c-vent-aqua until tiles are made
        tileMap.Add(new tileMapperList() { tileByte = "31", preFabName = "br-c-pipe-aquaB" });
        tileMap.Add(new tileMapperList() { tileByte = "32", preFabName = "br-c-pipe-aquaA" });

    }

    public static GameObject[] FindGameObjectsWithSameName(string name)
    {
        GameObject[] allObjs = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> likeNames = new List<GameObject>();
        foreach (GameObject obj in allObjs)
        {
            if (obj.name.Contains(name))
            {
                likeNames.Add(obj);
            }
        }
        return likeNames.ToArray();
    }

    public static void buildRoomDataRef(string fileName, string areaName, string roomNumber,
           int topPtrByte, int bottomPtrByte, int xMapOffset, int yMapOffset, int nesBankOffset, int nesBaseAddress)
    {
        // Setup the binary stream reader to take read in the hex values
        Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Assign the virtual file to read the data stream into
        BinaryReader brFile = new BinaryReader(fileStream);

        fileStream.Position = topPtrByte;

        string roomPtr1stByte = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

        fileStream.Position = bottomPtrByte;

        string roomPtr2ndByte = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

        //Debug.Log(((Convert.ToInt32(("0x" + roomPtr1stByte + roomPtr2ndByte), 16)
        //    - Convert.ToInt32(("0x8000"), 16))
        //    + Convert.ToInt32(("0x4011"), 16)).ToString("X"));

        //Debug.Log(((Convert.ToInt32(("0x" + roomPtr1stByte + roomPtr2ndByte), 16)
        //    - nesBaseAddress)
        //    + nesBankOffset).ToString("X"));

        // Calculate the room data offset based on the room number pointer value
        int roomOffset = ((Convert.ToInt32(("0x" + roomPtr1stByte + roomPtr2ndByte), 16)
            - nesBaseAddress)
            + nesBankOffset);

        roomBuilder(fileName, roomOffset, areaName, xMapOffset, yMapOffset, roomNumber);

        brFile.Close();
    }


    private static void tileImportValidator(string fileName, int nesX, int nesY, string areaName, string structureNumber, string roomNumber)
    {

        GameObject childTileObject = null;

        using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
        {

            // Declare a structure list to hold the bytes of our structure for indexing numerically later.
            List<structList> structure = new List<structList>();

            // Re-use the structure name from the file name. Later we may need other variables to say which 
            // CHR table data the structure is related to.
            string structName = Path.GetFileNameWithoutExtension(((FileStream)sr.BaseStream).Name);

            // Re-work the structname in case of duplicates
            structName = areaName + "_" + roomNumber + "_" + structureNumber + "_" + (FindGameObjectsWithSameName(structName).Length + 1).ToString();

            // Make sure the plane created is set to 0,0,0 coords
            GameObject.Find("Plane").transform.position += new Vector3(0, 0, 0);


            // Create default parent
            var sceneParent = new GameObject(structName);


            // Generate our structure at specific X,Y,Z coords to match NES placement
            sceneParent.transform.position += new Vector3(nesX, nesY + 2, 0);

            // Set the new parent object to the plane parent
            sceneParent.transform.SetParent(GameObject.Find("Plane").transform, true);

            string structureData = Regex.Replace(sr.ReadToEnd(), @"\t|\n|\r|", "");
            string[] structureBytes = structureData.Split(' ');
            foreach (string s in structureBytes)
            {
                if (s != "")
                {
                    structure.Add(new structList() { tileByte = s, structureName = structName });
                    //Debug.Log("Added TileByte = " + s + " StructureName = " + structName);
                }
            }

            string text = System.IO.File.ReadAllText(fileName);
            string[] lines = Regex.Split(text, "\r\n");
            int rows = lines.Length;
            int byteCount = 0;

            string[][] jagged = new string[rows][];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] stringsOfLine = Regex.Split(lines[i], " ");
                jagged[i] = stringsOfLine;
            }

            //Debug.Log("Line1 = " + (jagged.Length-1).ToString() + "," + (jagged[0].Length-1).ToString());
            //Debug.Log("Line2 = " + (jagged.Length-1).ToString() + "," + (jagged[1].Length-1).ToString());
            //Debug.Log("Line3 = " + (jagged.Length-1).ToString() + "," + (jagged[2].Length-1).ToString());
            //Debug.Log("Line4 = " + (jagged.Length-1).ToString() + "," + (jagged[3].Length-1).ToString());

            // create planes based on matrix
            for (int y = 0; y < jagged.Length - 1; y++)
            {
                for (int x = 0; x < jagged[y].Length - 1; x++)
                {
                    var prefabName = "";
                    // Create the prefab in the UI editor/canvas
                    if (byteCount < structure.Count)
                    {
                        // Use this to see where the last structure failed to load
                        //Debug.Log("StructureCount  " + structure.Count);
                        //Debug.Log("StructureTileByte  " + structure[byteCount].tileByte + " FileName > " + fileName);

                        prefabName = tileMap.Find(map => map.tileByte.Contains(structure[byteCount].tileByte)).preFabName;
                        byteCount++;

                        //Debug.Log(prefabName.Replace("_", "-") + "," + structure.Count + "," + byteCount);

                        childTileObject = UnityEngine.Object.Instantiate(Resources.Load(prefabName.Replace("_", "-")), new Vector3(x * 2, y * 2, 0), Quaternion.Euler(0, 0, -180)) as GameObject;
                        childTileObject.transform.SetParent(GameObject.Find(structName).transform, false);

                    }

                }
            }
        }

    }

    private static void quickTester()
    {
        GameObject childTileObject = null;
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-ball"), new Vector3(0 + 0, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-balls-blue"), new Vector3(0 + 1, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-balls-green"), new Vector3(0 + 2, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-balls-purple"), new Vector3(0 + 3, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brick-aqua"), new Vector3(0 + 4, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brick-green"), new Vector3(0 + 5, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brick-orange"), new Vector3(0 + 6, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brick-white"), new Vector3(0 + 7, (-2 + 0), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brush-aqua"), new Vector3(0 + 0, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brush-orange"), new Vector3(0 + 1, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brush-purple"), new Vector3(0 + 2, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-brush-white"), new Vector3(0 + 3, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bubble-green"), new Vector3(0 + 4, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bubble-lone-purple"), new Vector3(0 + 5, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bubble-purple"), new Vector3(0 + 6, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bubble-quad-green"), new Vector3(0 + 7, (-2 + 1), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bubble-quad-purple"), new Vector3(0 + 0, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-aqua"), new Vector3(0 + 1, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-blue"), new Vector3(0 + 2, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-blueB"), new Vector3(0 + 3, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-green"), new Vector3(0 + 4, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-greenB"), new Vector3(0 + 5, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-orange"), new Vector3(0 + 6, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-bush-purple"), new Vector3(0 + 7, (-2 + 2), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-circle-blue"), new Vector3(0 + 0, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-circle-purple"), new Vector3(0 + 1, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cloud"), new Vector3(0 + 2, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cube-blue"), new Vector3(0 + 3, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cube-white"), new Vector3(0 + 4, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cubeplate-green"), new Vector3(0 + 5, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cubeplate-purple"), new Vector3(0 + 6, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cubeplate-white"), new Vector3(0 + 7, (-2 + 3), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-cubeQuad-white"), new Vector3(0 + 0, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-elevatorA"), new Vector3(0 + 1, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-elevatorB"), new Vector3(0 + 2, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-fangL-purple"), new Vector3(0 + 3, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-fangL-white"), new Vector3(0 + 4, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-fangR-purple"), new Vector3(0 + 5, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-fangR-white"), new Vector3(0 + 6, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-fence-green"), new Vector3(0 + 7, (-2 + 4), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-fence-purple"), new Vector3(0 + 0, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-foam-aqua"), new Vector3(0 + 1, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-foam-blue"), new Vector3(0 + 2, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-foam-green"), new Vector3(0 + 3, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-foam-orange"), new Vector3(0 + 4, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-hall-blue"), new Vector3(0 + 5, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-hall-purple"), new Vector3(0 + 6, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-orange-base"), new Vector3(0 + 7, (-2 + 5), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-orange-top"), new Vector3(0 + 0, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-pink-base"), new Vector3(0 + 1, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-pink-top"), new Vector3(0 + 2, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-red-base"), new Vector3(0 + 3, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-red-top"), new Vector3(0 + 4, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-yellow-base"), new Vector3(0 + 5, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-lava-yellow-top"), new Vector3(0 + 6, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-oliveB"), new Vector3(0 + 7, (-2 + 6), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-oliveS"), new Vector3(0 + 0, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pillar-aqua"), new Vector3(0 + 1, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pillar-orange"), new Vector3(0 + 2, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pillbrick-blue"), new Vector3(0 + 3, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pillbrick-white"), new Vector3(0 + 4, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pillbrick2-white"), new Vector3(0 + 5, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pipe_aquaA"), new Vector3(0 + 6, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pipe_aquaB"), new Vector3(0 + 7, (-2 + 7), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pipe_blueA"), new Vector3(0 + 0, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pipe_blueB"), new Vector3(0 + 1, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pipe_orangeA"), new Vector3(0 + 2, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pipe_orangeB"), new Vector3(0 + 3, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-port-floorA"), new Vector3(0 + 4, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-port-floorB"), new Vector3(0 + 5, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pot-aqua"), new Vector3(0 + 6, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pot-blue"), new Vector3(0 + 7, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pot-red"), new Vector3(0 + 0, (-2 + 8), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-pot-white"), new Vector3(0 + 1, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rock-aqua"), new Vector3(0 + 2, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rock-orange"), new Vector3(0 + 3, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rock-pink"), new Vector3(0 + 4, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rock-purple"), new Vector3(0 + 5, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rock2-aqua"), new Vector3(0 + 6, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rock2-orange"), new Vector3(0 + 7, (-2 + 9), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rockL-aqua"), new Vector3(0 + 0, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rockL-orange"), new Vector3(0 + 1, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rockR-blue"), new Vector3(0 + 2, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-rockR-orange"), new Vector3(0 + 3, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-seal-blue"), new Vector3(0 + 4, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-seal-green"), new Vector3(0 + 5, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-seal-orange"), new Vector3(0 + 6, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spikecube-blue"), new Vector3(0 + 7, (-2 + 10), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spikecube-white"), new Vector3(0 + 0, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spine-blue"), new Vector3(0 + 1, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spiral-blue"), new Vector3(0 + 2, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spiral-orange"), new Vector3(0 + 3, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spiral-purple"), new Vector3(0 + 4, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-spiral-white"), new Vector3(0 + 5, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-stagB-green"), new Vector3(0 + 6, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-stagB-purple"), new Vector3(0 + 7, (-2 + 11), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-stagS-green"), new Vector3(0 + 0, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-stagS-purple"), new Vector3(0 + 1, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-statue-aqua"), new Vector3(0 + 2, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-statue-norfair"), new Vector3(0 + 3, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tile-blue"), new Vector3(0 + 4, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tube-aqua-hori"), new Vector3(0 + 5, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tube-aqua"), new Vector3(0 + 6, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tube-blue"), new Vector3(0 + 7, (-2 + 12), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tube-green"), new Vector3(0 + 0, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tube-purple"), new Vector3(0 + 1, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tube-white"), new Vector3(0 + 2, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tubeHori-hole-white"), new Vector3(0 + 3, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-tubeHori-white"), new Vector3(0 + 4, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-vent-aqua"), new Vector3(0 + 5, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-vent-white"), new Vector3(0 + 6, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-weed-green"), new Vector3(0 + 7, (-2 + 13), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-weed-purple"), new Vector3(0 + 0, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-wires-blue"), new Vector3(0 + 1, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-wires-green"), new Vector3(0 + 2, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-wires-purple"), new Vector3(0 + 3, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-float-a1-1"), new Vector3(0 + 4, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-ball"), new Vector3(0 + 5, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-balls-blue"), new Vector3(0 + 6, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-balls-green"), new Vector3(0 + 7, (-2 + 14), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-balls-purple"), new Vector3(0 + 0, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brick-aqua"), new Vector3(0 + 1, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brick-green"), new Vector3(0 + 2, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brick-orange"), new Vector3(0 + 3, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brick-white"), new Vector3(0 + 4, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brush-aqua"), new Vector3(0 + 5, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brush-orange"), new Vector3(0 + 6, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brush-purple"), new Vector3(0 + 7, (-2 + 15), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-brush-white"), new Vector3(0 + 0, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bubble-green"), new Vector3(0 + 1, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bubble-lone-purple"), new Vector3(0 + 2, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bubble-purple"), new Vector3(0 + 3, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bubble-quad-green"), new Vector3(0 + 4, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bubble-quad-purple"), new Vector3(0 + 5, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bush-aqua"), new Vector3(0 + 6, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bush-orange"), new Vector3(0 + 7, (-2 + 16), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-bush-purple"), new Vector3(0 + 0, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cube-blue"), new Vector3(0 + 1, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cube-white"), new Vector3(0 + 2, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cubeBrick-white"), new Vector3(0 + 3, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cubeplate-green"), new Vector3(0 + 4, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cubeplate-purple"), new Vector3(0 + 5, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cubeplate-white"), new Vector3(0 + 6, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-cubeQuad-white"), new Vector3(0 + 7, (-2 + 17), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-door-blue"), new Vector3(0 + 0, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-fangL-purple"), new Vector3(0 + 1, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-fangL-white"), new Vector3(0 + 2, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-fangR-purple"), new Vector3(0 + 3, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-fangR-white"), new Vector3(0 + 4, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-foam-aqua"), new Vector3(0 + 5, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-foam-blue"), new Vector3(0 + 6, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-foam-green"), new Vector3(0 + 7, (-2 + 18), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-foam-orange"), new Vector3(0 + 0, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-foam-purple"), new Vector3(0 + 1, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-hall-blue"), new Vector3(0 + 2, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-hall-purple"), new Vector3(0 + 3, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-oliveB"), new Vector3(0 + 4, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-oliveS"), new Vector3(0 + 5, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillar-aqua"), new Vector3(0 + 6, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillar-hori-aqua"), new Vector3(0 + 7, (-2 + 19), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillar-hori-orange"), new Vector3(0 + 0, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillar-orange"), new Vector3(0 + 1, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillbrick-white"), new Vector3(0 + 2, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillbrick2-blue"), new Vector3(0 + 3, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pillbrick2-white"), new Vector3(0 + 4, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-platform-aqua"), new Vector3(0 + 5, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pot-aqua"), new Vector3(0 + 6, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pot-blue"), new Vector3(0 + 7, (-2 + 20), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pot-orange"), new Vector3(0 + 0, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-pot-white"), new Vector3(0 + 1, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rock-aqua"), new Vector3(0 + 2, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rock-orange"), new Vector3(0 + 3, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rock-pink"), new Vector3(0 + 4, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rock2-aqua"), new Vector3(0 + 5, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rock2-orange"), new Vector3(0 + 6, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rockL-aqua"), new Vector3(0 + 7, (-2 + 21), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rockL-orange"), new Vector3(0 + 0, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rockR-aqua"), new Vector3(0 + 1, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-rockR-orange"), new Vector3(0 + 2, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-seal-blue"), new Vector3(0 + 3, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-seal-green"), new Vector3(0 + 4, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-seal-red"), new Vector3(0 + 5, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-spikecube-aqua"), new Vector3(0 + 6, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-spikecube-white"), new Vector3(0 + 7, (-2 + 22), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-spiral-aqua"), new Vector3(0 + 0, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-spiral-orange"), new Vector3(0 + 1, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-spiral-purple"), new Vector3(0 + 2, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-spiral-white"), new Vector3(0 + 3, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tile-blue"), new Vector3(0 + 4, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-towers-aqua"), new Vector3(0 + 5, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tube-aqua-hori"), new Vector3(0 + 6, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tube-aqua"), new Vector3(0 + 7, (-2 + 23), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tube-blue"), new Vector3(0 + 0, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tube-green"), new Vector3(0 + 1, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tube-purple"), new Vector3(0 + 2, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tube-white"), new Vector3(0 + 3, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tubeHori-hole-white"), new Vector3(0 + 4, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-tubeHori-white"), new Vector3(0 + 5, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-vent-aqua"), new Vector3(0 + 6, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-vent-white"), new Vector3(0 + 7, (-2 + 24), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
        childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-m-vent-white"), new Vector3(0 + 0, (-2 + 25), 0), Quaternion.Euler(0, 0, -180)) as GameObject; childTileObject.transform.SetParent(GameObject.Find("Plane").transform, false);
    }

    public static void mapMaker(string fileName)
    {
        //254E

        // Setup the binary stream reader to take read in the hex values
        Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // Assign the virtual file to read the data stream into
        BinaryReader brFile = new BinaryReader(fileStream);

        // Set the initial position to read data from in the file using a hexadecimal offset (Where the data is in the file.
        // This is the first byte of the targetted structure)
        fileStream.Position = 0x254E;

        // Create an array to store our map structure
        mapStructure = new string[32][];
        for (int i = 0; i < mapStructure.Length; i++)
        {
            mapStructure[i] = new string[32];
        }

        for (int rows = 0; rows < 32; rows++)
        {
            for (int cols = 0; cols < 32; cols++)
            {
                mapStructure[rows][cols] = HexStr(brFile.ReadBytes(1)).Replace("0x", "");

                //int blankOffsetX = rows;
                //int blankOffsetY = cols;

                //if (mapStructure[rows][cols] != "FF")
                //{
                //    buildSingleRoom("BRINSTAR", mapStructure[rows][cols], rows * 16, cols * 16);
                //}
                //else
                //{
                //    buildSingleRoom("BRINSTAR", "08", rows * 16, cols * 16);
                //}

                
            }
        }

        //MessageBox.Show(mapStructure[14][2].ToString());
        //string mapData = HexStr(brFile.ReadBytes(1024)).Replace("0x", "");

        //dataBox.Text = mapData;


    }


    private static void buildRoomLists()
    {
        roomDataList.Add(new roomDataList() { nesRoomNum = "08", topByte = 0x6334, bottomByte = 0x6335, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });

        roomDataList.Add(new roomDataList() { nesRoomNum = "17", topByte = 0x6352, bottomByte = 0x6353, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "09", topByte = 0x6336, bottomByte = 0x6337, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0x634C, bottomByte = 0x634D, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0x634A, bottomByte = 0x634B, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "00", topByte = 0x6324, bottomByte = 0x6325, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "01", topByte = 0x6326, bottomByte = 0x6327, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "02", topByte = 0x6328, bottomByte = 0x6329, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "03", topByte = 0x632A, bottomByte = 0x632B, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "04", topByte = 0x632C, bottomByte = 0x632D, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "05", topByte = 0x632E, bottomByte = 0x632F, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "06", topByte = 0x6330, bottomByte = 0x6331, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "07", topByte = 0x6332, bottomByte = 0x6333, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0x6338, bottomByte = 0x6339, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0x633A, bottomByte = 0x633B, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0x633C, bottomByte = 0x633D, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0x633E, bottomByte = 0x633F, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0x6340, bottomByte = 0x6341, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0x6342, bottomByte = 0x6343, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0x6344, bottomByte = 0x6345, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0x6346, bottomByte = 0x6347, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0x6348, bottomByte = 0x6349, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "15", topByte = 0x634E, bottomByte = 0x634F, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "16", topByte = 0x6350, bottomByte = 0x6351, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "18", topByte = 0x6354, bottomByte = 0x6355, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "19", topByte = 0x6356, bottomByte = 0x6357, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1A", topByte = 0x6358, bottomByte = 0x6359, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1B", topByte = 0x635A, bottomByte = 0x635B, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1C", topByte = 0x635C, bottomByte = 0x635D, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1D", topByte = 0x635E, bottomByte = 0x635F, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1E", topByte = 0x6360, bottomByte = 0x6361, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1F", topByte = 0x6362, bottomByte = 0x6363, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "20", topByte = 0x6364, bottomByte = 0x6365, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "21", topByte = 0x6366, bottomByte = 0x6367, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "22", topByte = 0x6368, bottomByte = 0x6369, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "23", topByte = 0x636A, bottomByte = 0x636B, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "24", topByte = 0x636C, bottomByte = 0x636D, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "25", topByte = 0x636E, bottomByte = 0x636F, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "26", topByte = 0x6370, bottomByte = 0x6371, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "27", topByte = 0x6372, bottomByte = 0x6373, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "28", topByte = 0x6374, bottomByte = 0x6375, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "29", topByte = 0x6376, bottomByte = 0x6377, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2A", topByte = 0x6378, bottomByte = 0x6379, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2B", topByte = 0x637A, bottomByte = 0x637B, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2C", topByte = 0x637C, bottomByte = 0x637D, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2D", topByte = 0x637E, bottomByte = 0x637F, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2E", topByte = 0x6380, bottomByte = 0x6381, bankOffset = 0x4011, baseMemoryAddress = 0x8000, areaName = "BRINSTAR" });
        
        roomDataList.Add(new roomDataList() { nesRoomNum = "00", topByte = 0xA22B, bottomByte = 0xA22C, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "01", topByte = 0xA22D, bottomByte = 0xA22E, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "02", topByte = 0xA22F, bottomByte = 0xA230, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "03", topByte = 0xA231, bottomByte = 0xA232, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "04", topByte = 0xA233, bottomByte = 0xA234, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "05", topByte = 0xA235, bottomByte = 0xA236, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "06", topByte = 0xA237, bottomByte = 0xA238, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "07", topByte = 0xA239, bottomByte = 0xA23A, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "08", topByte = 0xA23B, bottomByte = 0xA23C, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "09", topByte = 0xA23D, bottomByte = 0xA23E, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0xA23F, bottomByte = 0xA240, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0xA241, bottomByte = 0xA242, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0xA243, bottomByte = 0xA244, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0xA245, bottomByte = 0xA246, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0xA247, bottomByte = 0xA248, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0xA249, bottomByte = 0xA24A, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0xA24B, bottomByte = 0xA24C, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0xA24D, bottomByte = 0xA24E, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0xA24F, bottomByte = 0xA250, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0xA251, bottomByte = 0xA252, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0xA253, bottomByte = 0xA254, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "15", topByte = 0xA255, bottomByte = 0xA256, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "16", topByte = 0xA257, bottomByte = 0xA258, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "17", topByte = 0xA259, bottomByte = 0xA25A, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "18", topByte = 0xA25B, bottomByte = 0xA25C, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "19", topByte = 0xA25D, bottomByte = 0xA25E, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1A", topByte = 0xA25F, bottomByte = 0xA260, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1B", topByte = 0xA261, bottomByte = 0xA262, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1C", topByte = 0xA263, bottomByte = 0xA264, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1D", topByte = 0xA265, bottomByte = 0xA266, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1E", topByte = 0xA267, bottomByte = 0xA268, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "1F", topByte = 0xA269, bottomByte = 0xA26A, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "20", topByte = 0xA26B, bottomByte = 0xA26C, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "21", topByte = 0xA26D, bottomByte = 0xA26E, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "22", topByte = 0xA26F, bottomByte = 0xA270, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "23", topByte = 0xA271, bottomByte = 0xA272, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "24", topByte = 0xA273, bottomByte = 0xA274, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "25", topByte = 0xA275, bottomByte = 0xA276, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "26", topByte = 0xA277, bottomByte = 0xA278, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "27", topByte = 0xA279, bottomByte = 0xA27A, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "28", topByte = 0xA27B, bottomByte = 0xA27C, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "29", topByte = 0xA27D, bottomByte = 0xA27E, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2A", topByte = 0xA27F, bottomByte = 0xA280, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2B", topByte = 0xA281, bottomByte = 0xA282, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2C", topByte = 0xA283, bottomByte = 0xA284, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "2D", topByte = 0xA285, bottomByte = 0xA286, bankOffset = 0x8011, baseMemoryAddress = 0x8000, areaName = "NORFAIR" });
        
        roomDataList.Add(new roomDataList() { nesRoomNum = "00", topByte = 0xE7E1, bottomByte = 0xE7E2, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "01", topByte = 0xE7E3, bottomByte = 0xE7E4, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "02", topByte = 0xE7E5, bottomByte = 0xE7E6, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "03", topByte = 0xE7E7, bottomByte = 0xE7E8, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "04", topByte = 0xE7E9, bottomByte = 0xE7EA, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "05", topByte = 0xE7EB, bottomByte = 0xE7EC, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "06", topByte = 0xE7ED, bottomByte = 0xE7EE, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "07", topByte = 0xE7EF, bottomByte = 0xE7F0, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "08", topByte = 0xE7F1, bottomByte = 0xE7F2, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "09", topByte = 0xE7F3, bottomByte = 0xE7F4, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0xE7F5, bottomByte = 0xE7F6, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0xE7F7, bottomByte = 0xE7F8, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0xE7F9, bottomByte = 0xE7FA, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0xE7FB, bottomByte = 0xE7FC, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0xE7FD, bottomByte = 0xE7FE, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0xE7FF, bottomByte = 0xE800, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0xE801, bottomByte = 0xE802, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0xE803, bottomByte = 0xE804, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0xE805, bottomByte = 0xE806, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0xE807, bottomByte = 0xE808, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0xE809, bottomByte = 0xE80A, bankOffset = 0xC011, baseMemoryAddress = 0x8000, areaName = "TOURIAN" });
        
        roomDataList.Add(new roomDataList() { nesRoomNum = "00", topByte = 0x121E5, bottomByte = 0x121E6, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "01", topByte = 0x121E7, bottomByte = 0x121E8, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "02", topByte = 0x121E9, bottomByte = 0x121EA, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "03", topByte = 0x121EB, bottomByte = 0x121EC, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "04", topByte = 0x121ED, bottomByte = 0x121EE, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "05", topByte = 0x121EF, bottomByte = 0x121F0, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "06", topByte = 0x121F1, bottomByte = 0x121F2, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "07", topByte = 0x121F3, bottomByte = 0x121F4, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "08", topByte = 0x121F5, bottomByte = 0x121F6, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "09", topByte = 0x121F7, bottomByte = 0x121F8, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0x121F9, bottomByte = 0x121FA, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0x121FB, bottomByte = 0x121FC, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0x121FD, bottomByte = 0x121FE, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0x121FF, bottomByte = 0x12200, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0x12201, bottomByte = 0x12202, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0x12203, bottomByte = 0x12204, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0x12205, bottomByte = 0x12206, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0x12207, bottomByte = 0x12208, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0x12209, bottomByte = 0x1220A, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0x1220B, bottomByte = 0x1220C, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0x1220D, bottomByte = 0x1220E, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0x1220F, bottomByte = 0x12210, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0x12211, bottomByte = 0x12212, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0x12213, bottomByte = 0x12214, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0x12215, bottomByte = 0x12216, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0x12217, bottomByte = 0x12218, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "15", topByte = 0x12219, bottomByte = 0x1221A, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "16", topByte = 0x1221B, bottomByte = 0x1221C, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "17", topByte = 0x1221D, bottomByte = 0x1221E, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "18", topByte = 0x1221F, bottomByte = 0x12220, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "19", topByte = 0x12221, bottomByte = 0x12222, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0x12223, bottomByte = 0x12224, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0x12225, bottomByte = 0x12226, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0x12227, bottomByte = 0x12228, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0x12229, bottomByte = 0x1222A, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0x1222B, bottomByte = 0x1222C, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0x1222D, bottomByte = 0x1222E, bankOffset = 0x10011, baseMemoryAddress = 0x8000, areaName = "KRAIDS" });
        
        roomDataList.Add(new roomDataList() { nesRoomNum = "00", topByte = 0x1618F, bottomByte = 0x16190, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "01", topByte = 0x16191, bottomByte = 0x16192, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "02", topByte = 0x16193, bottomByte = 0x16194, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "03", topByte = 0x16195, bottomByte = 0x16196, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "04", topByte = 0x16197, bottomByte = 0x16198, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "05", topByte = 0x16199, bottomByte = 0x1619A, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "06", topByte = 0x1619B, bottomByte = 0x1619C, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "07", topByte = 0x1619D, bottomByte = 0x1619E, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "08", topByte = 0x1619F, bottomByte = 0x161A0, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "09", topByte = 0x161A1, bottomByte = 0x161A2, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0x161A3, bottomByte = 0x161A4, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0x161A5, bottomByte = 0x161A6, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0x161A7, bottomByte = 0x161A8, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0x161A9, bottomByte = 0x161AA, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0x161AB, bottomByte = 0x161AC, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0x161AD, bottomByte = 0x161AE, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0x161AF, bottomByte = 0x161B0, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0x161B1, bottomByte = 0x161B2, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0x161B3, bottomByte = 0x161B4, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0x161B5, bottomByte = 0x161B6, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0x161B7, bottomByte = 0x161B8, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0x161B9, bottomByte = 0x161BA, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0x161BB, bottomByte = 0x161BC, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0x161BD, bottomByte = 0x161BE, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0x161BF, bottomByte = 0x161C0, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0x161C1, bottomByte = 0x161C2, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "15", topByte = 0x161C3, bottomByte = 0x161C4, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "16", topByte = 0x161C5, bottomByte = 0x161C6, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "17", topByte = 0x161C7, bottomByte = 0x161C8, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "18", topByte = 0x161C9, bottomByte = 0x161CA, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "19", topByte = 0x161CB, bottomByte = 0x161CC, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0A", topByte = 0x161CD, bottomByte = 0x161CE, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0B", topByte = 0x161CF, bottomByte = 0x161D0, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0C", topByte = 0x161D1, bottomByte = 0x161D2, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0D", topByte = 0x161D3, bottomByte = 0x161D4, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0E", topByte = 0x161D5, bottomByte = 0x161D6, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "0F", topByte = 0x161D7, bottomByte = 0x161D8, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "10", topByte = 0x161D9, bottomByte = 0x161DA, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "11", topByte = 0x161DB, bottomByte = 0x161DC, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "12", topByte = 0x161DD, bottomByte = 0x161DE, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "13", topByte = 0x161DF, bottomByte = 0x161E0, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
        roomDataList.Add(new roomDataList() { nesRoomNum = "14", topByte = 0x161E1, bottomByte = 0x161E2, bankOffset = 0x14011, baseMemoryAddress = 0x8000, areaName = "RIDLEYS" });
    }

    private static void buildSingleRoom(string areaName, string roomNumber, int xScenePositionStart, int yScenePositionStart)
    {
        Debug.Log(areaName + " , " + roomNumber + " , " + xScenePositionStart.ToString() + " , " + yScenePositionStart.ToString());

        var currentRoom = roomDataList.Find(room => room.areaName.Contains(areaName) && room.nesRoomNum.Contains(roomNumber));

        

        buildRoomDataRef("Assets/Resources/test.data", areaName, roomNumber, currentRoom.bottomByte, currentRoom.topByte, xScenePositionStart, yScenePositionStart, currentRoom.bankOffset, currentRoom.baseMemoryAddress);
    }

    [MenuItem("MetroidVR/Maya Export Test")]
    private static void testMaya()
    {
        // Load the static list of Prefab<->Structure mapping data
        loadRuntimeVariables();
        quickTester();
    }

    [MenuItem("MetroidVR/Create Scene")]
    private static void CreatePrefab()
    {

        // Create the plane automatically
        var sceneParent = new GameObject("Plane");

        // Make sure the plane created is set to 0,0,0 coords
        sceneParent.transform.position += new Vector3(0, 0, 0);


        // Load the static list of Prefab<->Structure mapping data
        loadRuntimeVariables();

        // Build the room lists
        buildRoomLists();

        // Generate the map based on the original data structure
        //mapMaker(@"C:\\temp\test.data");


        buildSingleRoom("BRINSTAR", "08", 0 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "17", 1 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "09", 2 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "14", 3 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "13", 4 * 16, 0 * 15);

        // 1st Cross section that goes up and down
        buildSingleRoom("BRINSTAR", "18", 5 * 16, 0 * 15);

        // Verticle shaft 1 mid secction
        buildSingleRoom("BRINSTAR", "06", 5 * 16, 1 * 15);
        buildSingleRoom("BRINSTAR", "06", 5 * 16, 2 * 15);
        buildSingleRoom("BRINSTAR", "06", 5 * 16, 3 * 15);
        buildSingleRoom("BRINSTAR", "03", 5 * 16, 4 * 15);

        // Verticle shaft 1 bottom secction
        buildSingleRoom("BRINSTAR", "08", 5 * 16, 5 * 15);
        
        // Verticle Shaft 1 (bottom horizontal outlet) - Elevator down..
        buildSingleRoom("BRINSTAR", "1C", 6 * 16, 4 * 15);

        // Past first coordidor until the second verticle cooridor
        buildSingleRoom("BRINSTAR", "12", 6 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "14", 7 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "19", 8 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "13", 9 * 16, 0 * 15);
        buildSingleRoom("BRINSTAR", "04", 10 * 16, 0 * 15);

        // Verticle Shaft 2 Bottom section
        buildSingleRoom("BRINSTAR", "08", 10 * 16, 1 * 15);

        // Verticle Shaft 2 mid section
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -1 * 15);
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -2 * 15);
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -4 * 15);
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -5 * 15);
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -6 * 15);

        // Veritcle Shaft 2 Mid section / right exit
        buildSingleRoom("BRINSTAR", "03", 10 * 16, -7 * 15);

        // Palette switch room for Brinstar
        buildSingleRoom("BRINSTAR", "00", 11 * 16, -7 * 15);

        // Verticle Shaft 2 mid section cont
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -8 * 15);

        // Veritcle Shaft 2 Mid section / Left exit
        buildSingleRoom("BRINSTAR", "04", 10 * 16, -9 * 15);

        // Mid horizontal section
        buildSingleRoom("BRINSTAR", "28", 9 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "29", 8 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "1A", 7 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "0A", 6 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "08", 5 * 16, -9 * 15);

        // Veritcle Shaft 2 Mid section / Above left mid exit
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -10 * 15);
        buildSingleRoom("BRINSTAR", "06", 10 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "04", 10 * 16, -12 * 15);

        // Verticle Shaft 2 top section
        buildSingleRoom("BRINSTAR", "08", 10 * 16, -13 * 15);

        // Top horizontal section 
        buildSingleRoom("BRINSTAR", "13", 9 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "14", 8 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "16", 7 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "15", 6 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "15", 5 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "27", 4 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "2B", 3 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "2C", 2 * 16, -12 * 15);


        // Verticle Shaft 3 
        buildSingleRoom("BRINSTAR", "08", 12 * 16, -2 * 15);

        buildSingleRoom("BRINSTAR", "03", 12 * 16, -11 * 15);

        // Verticle Shaft top door branch horizontal
        buildSingleRoom("BRINSTAR", "1F", 13 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "23", 14 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "25", 15 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "24", 16 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "26", 17 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "20", 18 * 16, -11 * 15);

        buildSingleRoom("BRINSTAR", "1E", 19 * 16, -11 * 15);
        //Verticle shaft diverge up/down
        buildSingleRoom("BRINSTAR", "08", 19 * 16, -13 * 15);
        buildSingleRoom("BRINSTAR", "2E", 19 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "28", 18 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "29", 17 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "29", 16 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "1A", 15 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "0A", 14 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "08", 13 * 16, -12 * 15);

        buildSingleRoom("BRINSTAR", "1F", 20 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "21", 21 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "21", 22 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "07", 23 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "22", 24 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "1D", 25 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "1B", 26 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "21", 27 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "20", 28 * 16, -11 * 15);
        buildSingleRoom("BRINSTAR", "04", 29 * 16, -11 * 15);
        // Verticle Shaft upper right branch hori/vert
        buildSingleRoom("BRINSTAR", "08", 29 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "02", 29 * 16, -10 * 15);

        buildSingleRoom("BRINSTAR", "04", 29 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "28", 28 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "29", 27 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "29", 26 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "1A", 25 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "0A", 24 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "08", 23 * 16, -9 * 15);

        buildSingleRoom("BRINSTAR", "06", 29 * 16, -8 * 15);
        buildSingleRoom("BRINSTAR", "08", 29 * 16, -6 * 15);

        buildSingleRoom("BRINSTAR", "06", 12 * 16, -4 * 15);
        buildSingleRoom("BRINSTAR", "06", 12 * 16, -5 * 15);
        buildSingleRoom("BRINSTAR", "06", 12 * 16, -6 * 15);
        buildSingleRoom("BRINSTAR", "05", 12 * 16, -7 * 15);

        // Verticle Shaft mid door branch horizontal
        buildSingleRoom("BRINSTAR", "0C", 13 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0E", 14 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0E", 15 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0D", 16 * 16, -7 * 15);

        buildSingleRoom("BRINSTAR", "10", 17 * 16, -7 * 15);
        // Verticle shaft diverge up/down
        buildSingleRoom("BRINSTAR", "08", 17 * 16, -8 * 15);
        buildSingleRoom("BRINSTAR", "11", 17 * 16, -6 * 15);
        buildSingleRoom("BRINSTAR", "11", 17 * 16, -5 * 15);
        buildSingleRoom("BRINSTAR", "08", 17 * 16, -4 * 15);

        buildSingleRoom("BRINSTAR", "0C", 18 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0F", 19 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0D", 20 * 16, -7 * 15);

        buildSingleRoom("BRINSTAR", "10", 21 * 16, -7 * 15);
        // Verticle shaft diverge up/down
        buildSingleRoom("BRINSTAR", "08", 21 * 16, -8 * 15);
        buildSingleRoom("BRINSTAR", "06", 21 * 16, -6 * 15);
        buildSingleRoom("BRINSTAR", "04", 21 * 16, -5 * 15);
        buildSingleRoom("BRINSTAR", "08", 21 * 16, -4 * 15);

        buildSingleRoom("BRINSTAR", "28", 20 * 16, -5 * 15);
        buildSingleRoom("BRINSTAR", "1A", 19 * 16, -5 * 15);
        buildSingleRoom("BRINSTAR", "0A", 18 * 16, -5 * 15);

        buildSingleRoom("BRINSTAR", "0C", 22 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0E", 23 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "1B", 24 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0F", 25 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0E", 26 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0F", 27 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "0D", 28 * 16, -7 * 15);
        buildSingleRoom("BRINSTAR", "04", 29 * 16, -7 * 15);

        buildSingleRoom("BRINSTAR", "06", 12 * 16, -8 * 15);
        buildSingleRoom("BRINSTAR", "06", 12 * 16, -9 * 15);
        buildSingleRoom("BRINSTAR", "06", 12 * 16, -10 * 15);
        buildSingleRoom("BRINSTAR", "03", 12 * 16, -11 * 15);

        // Verticle Shaft bottom door branch horizontal
        buildSingleRoom("BRINSTAR", "12", 13 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "14", 14 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "15", 15 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "14", 16 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "07", 17 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "16", 18 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "15", 19 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "13", 20 * 16, -3 * 15);
        buildSingleRoom("BRINSTAR", "0B", 21 * 16, -3 * 15);

        buildSingleRoom("BRINSTAR", "06", 12 * 16, -12 * 15);
        buildSingleRoom("BRINSTAR", "08", 12 * 16, -13 * 15);




        // Path to the nes rom for Metroid US
        //structureBuilder("Assets/Resources/test.data", 0x6C94, "Brinstar");
        //structureBuilder("Assets/Resources/test.data", 0xACC9, "Norfair");
        //structureBuilder("Assets/Resources/test.data", 0xEC26, "Tourian");
        //structureBuilder("Assets/Resources/test.data", 0x12A7B, "Kraid");
        //structureBuilder("Assets/Resources/test.data", 0x169CF, "Ridley");

        // Room 8 ( Farthest screen left in starting area )
        //roomBuilder("Assets/Resources/test.data", 0x6598, "Brinstar", 0);
        ////// Room 17 ( Screen with morph ball )
        //roomBuilder("Assets/Resources/test.data", 0x6802, "Brinstar", 16);
        ////// Room 9 (Starting Room)
        //roomBuilder("Assets/Resources/test.data", 0x65CA, "Brinstar", 32);
        ////// Room 14 ( 4th room from the right in starting area )
        //roomBuilder("Assets/Resources/test.data", 0x679c, "Brinstar", 48);
        ////// Room 13 ( Rightmost room with door in starting area )
        //roomBuilder("Assets/Resources/test.data", 0x6779, "Brinstar", 64);

        // Brinstar Area transition room with the door tiles
        //((A441-8000)+4011) = 6452

        // Brinstar Elevator Shaft 
        //((A454-8000)+ 4011) = 6465

        // Verticle chamber with no platforms 
        //((A45C-8000)+4011) =  646D

        //((A480-8000)+4011) = 6491

        //roomBuilder("Assets/Resources/test.data", 0x65D7, "Brinstar", 0);
        //tileImportValidator(@"c:\temp\allbrin.txt", 0, 0, "brinstar", "AllBrin", "TileLayout");

        // So here I am saying make the first 5 rooms that make up the first area.  It's technical in nature obviously, but its how the nes represents it all.
        // Brinstar starting area

        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "08", 0x6335, 0x6334, 0, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "17", 0x6353, 0x6352, 16, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "09", 0x6337, 0x6336, 32, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "14", 0x634D, 0x634C, 48, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "13", 0x634B, 0x634A, 64, 0, 0x4011, 0x8000);



        GameObject.Find("Plane").transform.rotation = Quaternion.Euler(180, 0, 0);
        GameObject.Find("Plane").transform.localScale = new Vector3(1f, -1f, 1f);

        //// The rest of Brinstar
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "00", 0x6325, 0x6324, 80, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "01", 0x6327, 0x6326, 96, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "02", 0x6329, 0x6328, 112, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "03", 0x632B, 0x632A, 128, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "04", 0x632D, 0x632C, 144, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "05", 0x632F, 0x632E, 160, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "06", 0x6331, 0x6330, 176, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "07", 0x6333, 0x6332, 192, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "0A", 0x6339, 0x6338, 208, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "0B", 0x633B, 0x633A, 224, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "0C", 0x633D, 0x633C, 240, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "0D", 0x633F, 0x633E, 256, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "0E", 0x6341, 0x6340, 272, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "0F", 0x6343, 0x6342, 288, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "10", 0x6345, 0x6344, 304, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "11", 0x6347, 0x6346, 320, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "12", 0x6349, 0x6348, 336, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "15", 0x634F, 0x634E, 352, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "16", 0x6351, 0x6350, 368, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "18", 0x6355, 0x6354, 384, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "19", 0x6357, 0x6356, 400, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "1A", 0x6359, 0x6358, 416, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "1B", 0x635B, 0x635A, 432, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "1C", 0x635D, 0x635C, 448, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "1D", 0x635F, 0x635E, 464, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "1E", 0x6361, 0x6360, 480, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "1F", 0x6363, 0x6362, 496, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "20", 0x6365, 0x6364, 512, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "21", 0x6367, 0x6366, 528, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "22", 0x6369, 0x6368, 544, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "23", 0x636B, 0x636A, 560, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "24", 0x636D, 0x636C, 576, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "25", 0x636F, 0x636E, 592, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "26", 0x6371, 0x6370, 608, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "27", 0x6373, 0x6372, 624, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "28", 0x6375, 0x6374, 640, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "29", 0x6377, 0x6376, 656, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "2A", 0x6379, 0x6378, 672, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "2B", 0x637B, 0x637A, 688, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "2C", 0x637D, 0x637C, 704, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "2D", 0x637F, 0x637E, 720, 0, 0x4011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "BRINSTAR", "2E", 0x6381, 0x6380, 736, 0, 0x4011, 0x8000);

        // Norfair
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "00", 0xA22C, 0xA22B, 0, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "01", 0xA22E, 0xA22D, 16, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "02", 0xA230, 0xA22F, 32, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "03", 0xA232, 0xA231, 48, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "04", 0xA234, 0xA233, 64, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "05", 0xA236, 0xA235, 80, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "06", 0xA238, 0xA237, 96, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "07", 0xA23A, 0xA239, 112, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "08", 0xA23C, 0xA23B, 128, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "09", 0xA23E, 0xA23D, 144, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "0A", 0xA240, 0xA23F, 160, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "0B", 0xA242, 0xA241, 176, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "0C", 0xA244, 0xA243, 192, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "0D", 0xA246, 0xA245, 208, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "0E", 0xA248, 0xA247, 224, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "0F", 0xA24A, 0xA249, 240, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "10", 0xA24C, 0xA24B, 256, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "11", 0xA24E, 0xA24D, 272, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "12", 0xA250, 0xA24F, 288, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "13", 0xA252, 0xA251, 304, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "14", 0xA254, 0xA253, 320, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "15", 0xA256, 0xA255, 336, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "16", 0xA258, 0xA257, 352, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "17", 0xA25A, 0xA259, 368, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "18", 0xA25C, 0xA25B, 384, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "19", 0xA25E, 0xA25D, 400, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "1A", 0xA260, 0xA25F, 416, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "1B", 0xA262, 0xA261, 432, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "1C", 0xA264, 0xA263, 448, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "1D", 0xA266, 0xA265, 464, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "1E", 0xA268, 0xA267, 480, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "1F", 0xA26A, 0xA269, 496, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "20", 0xA26C, 0xA26B, 512, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "21", 0xA26E, 0xA26D, 528, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "22", 0xA270, 0xA26F, 544, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "23", 0xA272, 0xA271, 560, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "24", 0xA274, 0xA273, 576, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "25", 0xA276, 0xA275, 592, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "26", 0xA278, 0xA277, 608, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "27", 0xA27A, 0xA279, 624, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "28", 0xA27C, 0xA27B, 640, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "29", 0xA27E, 0xA27D, 656, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "2A", 0xA280, 0xA27F, 672, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "2B", 0xA282, 0xA281, 688, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "2C", 0xA284, 0xA283, 704, 0, 0x8011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "NORFAIR", "2D", 0xA286, 0xA285, 720, 0, 0x8011, 0x8000);

        // Tourian
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "00", 0xE7E2, 0xE7E1, 0, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "01", 0xE7E4, 0xE7E3, 16, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "02", 0xE7E6, 0xE7E5, 32, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "03", 0xE7E8, 0xE7E7, 48, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "04", 0xE7EA, 0xE7E9, 64, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "05", 0xE7EC, 0xE7EB, 80, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "06", 0xE7EE, 0xE7ED, 96, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "07", 0xE7F0, 0xE7EF, 112, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "08", 0xE7F2, 0xE7F1, 128, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "09", 0xE7F4, 0xE7F3, 144, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "0A", 0xE7F6, 0xE7F5, 160, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "0B", 0xE7F8, 0xE7F7, 176, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "0C", 0xE7FA, 0xE7F9, 192, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "0D", 0xE7FC, 0xE7FB, 208, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "0E", 0xE7FE, 0xE7FD, 224, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "0F", 0xE800, 0xE7FF, 240, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "10", 0xE802, 0xE801, 256, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "11", 0xE804, 0xE803, 272, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "12", 0xE806, 0xE805, 288, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "13", 0xE808, 0xE807, 304, 0, 0xC011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "TOURIAN", "14", 0xE80A, 0xE809, 320, 0, 0xC011, 0x8000);

        // Kraids
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "00", 0x121E6, 0x121E5, 0, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "01", 0x121E8, 0x121E7, 16, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "02", 0x121EA, 0x121E9, 32, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "03", 0x121EC, 0x121EB, 48, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "04", 0x121EE, 0x121ED, 64, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "05", 0x121F0, 0x121EF, 80, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "06", 0x121F2, 0x121F1, 96, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "07", 0x121F4, 0x121F3, 112, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "08", 0x121F6, 0x121F5, 128, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "09", 0x121F8, 0x121F7, 144, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0A", 0x121FA, 0x121F9, 160, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0B", 0x121FC, 0x121FB, 176, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0C", 0x121FE, 0x121FD, 192, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0D", 0x12200, 0x121FF, 208, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0E", 0x12202, 0x12201, 224, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0F", 0x12204, 0x12203, 240, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "10", 0x12206, 0x12205, 256, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "11", 0x12208, 0x12207, 272, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "12", 0x1220A, 0x12209, 288, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "13", 0x1220C, 0x1220B, 304, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "14", 0x1220E, 0x1220D, 320, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "10", 0x12210, 0x1220F, 336, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "11", 0x12212, 0x12211, 352, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "12", 0x12214, 0x12213, 368, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "13", 0x12216, 0x12215, 384, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "14", 0x12218, 0x12217, 400, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "15", 0x1221A, 0x12219, 416, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "16", 0x1221C, 0x1221B, 432, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "17", 0x1221E, 0x1221D, 448, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "18", 0x12220, 0x1221F, 464, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "19", 0x12222, 0x12221, 480, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0A", 0x12224, 0x12223, 496, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0B", 0x12226, 0x12225, 512, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0C", 0x12228, 0x12227, 528, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0D", 0x1222A, 0x12229, 544, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0E", 0x1222C, 0x1222B, 560, 0, 0x10011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "KRAIDS", "0F", 0x1222E, 0x1222D, 576, 0, 0x10011, 0x8000);

        //////Ridleys
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "00", 0x16190, 0x1618F, 0, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "01", 0x16192, 0x16191, 16, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "02", 0x16194, 0x16193, 32, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "03", 0x16196, 0x16195, 48, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "04", 0x16198, 0x16197, 64, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "05", 0x1619A, 0x16199, 80, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "06", 0x1619C, 0x1619B, 96, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "07", 0x1619E, 0x1619D, 112, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "08", 0x161A0, 0x1619F, 128, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "09", 0x161A2, 0x161A1, 144, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0A", 0x161A4, 0x161A3, 160, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0B", 0x161A6, 0x161A5, 176, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0C", 0x161A8, 0x161A7, 192, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0D", 0x161AA, 0x161A9, 208, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0E", 0x161AC, 0x161AB, 224, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0F", 0x161AE, 0x161AD, 240, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "10", 0x161B0, 0x161AF, 256, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "11", 0x161B2, 0x161B1, 272, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "12", 0x161B4, 0x161B3, 288, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "13", 0x161B6, 0x161B5, 304, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "14", 0x161B8, 0x161B7, 320, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "10", 0x161BA, 0x161B9, 336, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "11", 0x161BC, 0x161BB, 352, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "12", 0x161BE, 0x161BD, 368, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "13", 0x161C0, 0x161BF, 384, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "14", 0x161C2, 0x161C1, 400, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "15", 0x161C4, 0x161C3, 416, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "16", 0x161C6, 0x161C5, 432, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "17", 0x161C8, 0x161C7, 448, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "18", 0x161CA, 0x161C9, 464, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "19", 0x161CC, 0x161CB, 480, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0A", 0x161CE, 0x161CD, 496, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0B", 0x161D0, 0x161CF, 512, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0C", 0x161D2, 0x161D1, 528, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0D", 0x161D4, 0x161D3, 544, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0E", 0x161D6, 0x161D5, 560, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "0F", 0x161D8, 0x161D7, 576, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "10", 0x161DA, 0x161D9, 592, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "11", 0x161DC, 0x161DB, 608, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "12", 0x161DE, 0x161DD, 624, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "13", 0x161E0, 0x161DF, 640, 0, 0x14011, 0x8000);
        //buildRoomDataRef("Assets/Resources/test.data", "RIDLEYS", "14", 0x161E2, 0x161E1, 656, 0, 0x14011, 0x8000);

        GameObject.Find("Plane").transform.Rotate(180, 0, 0);

    }



    [MenuItem("MetroidVR/Create Scene", true)]
    bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null;
    }

    // Create Empty Prefab and then Replace 
    static void CreateNew(GameObject obj, string localPath)
    {
        //UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);

        // Will ultimately create a game object mapping equal to the count of tiles in the structure
        GameObject childTileObject = UnityEngine.Object.Instantiate(Resources.Load("br-c-vent-aqua"), new Vector3(1000, 800, 1200), Quaternion.Euler(0, -180, 0)) as GameObject;

        //childTileObject.transform.SetParent(GameObject.Find("p-home").transform, false);

        //PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }

}