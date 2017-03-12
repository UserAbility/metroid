using UnityEditor;
using UnityEngine;

public class tileMapperList
{
    public string tileByte { get; set; }
    public string preFabName { get; set; }
    public string areaName { get; set; }
    public string imagePath { get; set; }
    public GameObject gameObject { get; set; }
    public int controlID { get; set; }
}


public class structList
{
    public string structureName { get; set; }
    public string tileByte { get; set; }


}

public class enemyDataList
{
    public string enemyNumber { get; set; }
    public string enemyName { get; set; }
    public string enemyColor { get; set; }
    public string enemySpriteFileName { get; set; }
}

public class roomDataList
{
    public string nesRoomNum { get; set; }
    public int topByte { get; set; }
    public int bottomByte { get; set; }
    public int bankOffset { get; set; }
    public int baseMemoryAddress { get; set; }
    public string areaName { get; set; }

}

