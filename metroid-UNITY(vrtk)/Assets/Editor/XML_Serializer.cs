using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;

[XmlRoot("XMLCollection")]
 public class XMLContainer
 {
 	[XmlArray("XMLs"),XmlArrayItem("XML")]
    public XMLContainer[] XMLs;
 
 	public void Save(string path)
 	{
 		var serializer = new XmlSerializer(typeof(XMLContainer));
 		using(var stream = new FileStream(path, FileMode.Create))
 		{
 			serializer.Serialize(stream, this);
 		}
 	}
 
 	public static XMLContainer Load(string path)
 	{
 		var serializer = new XmlSerializer(typeof(XMLContainer));
 		using(var stream = new FileStream(path, FileMode.Open))
 		{
 			return serializer.Deserialize(stream) as XMLContainer;
 		}
 	}
 
         //Loads the xml directly from the given string. Useful in combination with www.text.
         public static XMLContainer LoadFromText(string text) 
 	{
 		var serializer = new XmlSerializer(typeof(XMLContainer));
 		return serializer.Deserialize(new StringReader(text)) as XMLContainer;
 	}

    public static void updateXML(string tileByteToUpdate, string newPrefabName)
    {
        XmlTextReader reader = new XmlTextReader(@"assets\resources\nesTilesMap.xml");
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);
        reader.Close();
        XmlNode myNode;
        XmlElement root = doc.DocumentElement;
        myNode = root.SelectSingleNode("//NESTile[tileByte='"+ tileByteToUpdate + "']/prefabMappedName");

        myNode.InnerText = newPrefabName;

        doc.Save(@"assets\resources\nesTilesMap.xml");
    }

    public static void createXML(string nesTileByte, string nesImagePath,string areaName)
    {

        XElement NESTiles =
new XElement("NESTiles", new XElement("NESTile", new XAttribute("mappedData", "00"), new XElement("tileByte", "00"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_lava-base-orange-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-lava-orange-base")),
new XElement("NESTile", new XAttribute("mappedData", "01"), new XElement("tileByte", "01"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_lava-top-orange-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-lava-orange-top")),
new XElement("NESTile", new XAttribute("mappedData", "02"), new XElement("tileByte", "02"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_hall-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-hall-blue")),
new XElement("NESTile", new XAttribute("mappedData", "03"), new XElement("tileByte", "03"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_tubeHori-white-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-tubeHori-hole-white")),
new XElement("NESTile", new XAttribute("mappedData", "04"), new XElement("tileByte", "04"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Elevator cable left")),
new XElement("NESTile", new XAttribute("mappedData", "05"), new XElement("tileByte", "05"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Elevator cable right")),
new XElement("NESTile", new XAttribute("mappedData", "06"), new XElement("tileByte", "06"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_seal-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-seal-blue")),
new XElement("NESTile", new XAttribute("mappedData", "07"), new XElement("tileByte", "07"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_seal-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-seal-blue")),
new XElement("NESTile", new XAttribute("mappedData", "08"), new XElement("tileByte", "08"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_rock-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-rock-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "09"), new XElement("tileByte", "09"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_rockL-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-rockL-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "0A"), new XElement("tileByte", "0A"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_rockR-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-rockR-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "0B"), new XElement("tileByte", "0B"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_rock2-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-rock2-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "0C"), new XElement("tileByte", "0C"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_statue-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-statue-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "0D"), new XElement("tileByte", "0D"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_balls-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-balls-blue")),
new XElement("NESTile", new XAttribute("mappedData", "0E"), new XElement("tileByte", "0E"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "0F"), new XElement("tileByte", "0F"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "10"), new XElement("tileByte", "10"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "11"), new XElement("tileByte", "11"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "12"), new XElement("tileByte", "12"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "13"), new XElement("tileByte", "13"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "14"), new XElement("tileByte", "14"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "15"), new XElement("tileByte", "15"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_lava-top-orange-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-lava-orange-top")),
new XElement("NESTile", new XAttribute("mappedData", "16"), new XElement("tileByte", "16"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_lava-base-orange-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-lava-orange-base")),
new XElement("NESTile", new XAttribute("mappedData", "17"), new XElement("tileByte", "17"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_pillar-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-pillar-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "19"), new XElement("tileByte", "19"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "")),
new XElement("NESTile", new XAttribute("mappedData", "18"), new XElement("tileByte", "18"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "")),
new XElement("NESTile", new XAttribute("mappedData", "1A"), new XElement("tileByte", "1A"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_pillbrick-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-pillbrick2-blue")),
new XElement("NESTile", new XAttribute("mappedData", "1B"), new XElement("tileByte", "1B"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_tubeHori-white-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-tubeHori-white")),
new XElement("NESTile", new XAttribute("mappedData", "1C"), new XElement("tileByte", "1C"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_brush-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-brush-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "1D"), new XElement("tileByte", "1D"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_bush-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-bush-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "1E"), new XElement("tileByte", "1E"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_brick-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-brick-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "1F"), new XElement("tileByte", "1F"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_pot-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-pot-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "20"), new XElement("tileByte", "20"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_vent-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-vent-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "21"), new XElement("tileByte", "21"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Blank space")),
new XElement("NESTile", new XAttribute("mappedData", "22"), new XElement("tileByte", "22"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_ball-red-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-ball")),
new XElement("NESTile", new XAttribute("mappedData", "23"), new XElement("tileByte", "23"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_brick-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-brick-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "24"), new XElement("tileByte", "24"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_rock-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-rock-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "25"), new XElement("tileByte", "25"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "")),
new XElement("NESTile", new XAttribute("mappedData", "26"), new XElement("tileByte", "26"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "")),
new XElement("NESTile", new XAttribute("mappedData", "27"), new XElement("tileByte", "27"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Blank space")),
new XElement("NESTile", new XAttribute("mappedData", "28"), new XElement("tileByte", "28"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Blank space")),
new XElement("NESTile", new XAttribute("mappedData", "29"), new XElement("tileByte", "29"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_rock-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-rock-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "2A"), new XElement("tileByte", "2A"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_bush-blue-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-bush-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "2B"), new XElement("tileByte", "2B"), new XElement("imagePath", @"Assets\Resources\Materials\10x_tubeHori-white-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-tubeHori-white")),
new XElement("NESTile", new XAttribute("mappedData", "2C"), new XElement("tileByte", "2C"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_lava-top-orange-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-lava-orange-top")),
new XElement("NESTile", new XAttribute("mappedData", "2D"), new XElement("tileByte", "2D"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_lava-base-orange-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-lava-orange-base")),
new XElement("NESTile", new XAttribute("mappedData", "2E"), new XElement("tileByte", "2E"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_brick-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-brick-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "2F"), new XElement("tileByte", "2F"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_tubeHori-white-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-tube-aqua-hori")),
new XElement("NESTile", new XAttribute("mappedData", "30"), new XElement("tileByte", "30"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_bubble-lone-purple-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-bubble-lone-purple")),
new XElement("NESTile", new XAttribute("mappedData", "31"), new XElement("tileByte", "31"), new XElement("imagePath", @"Assets\Resources\Materials\10x_pipe-aquaA-320x80.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-pipe-aquaA")),
new XElement("NESTile", new XAttribute("mappedData", "32"), new XElement("tileByte", "32"), new XElement("imagePath", @"Assets\Resources\Materials\10x_pipe-aquaB-320x80.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-c-pipe-aquaB")),
new XElement("NESTile", new XAttribute("mappedData", "33"), new XElement("tileByte", "33"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_foam-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-foam-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "34"), new XElement("tileByte", "34"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_tube-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-tube-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "35"), new XElement("tileByte", "35"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_pillar-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-tube-aqua-hori")),
new XElement("NESTile", new XAttribute("mappedData", "36"), new XElement("tileByte", "36"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\10x_spiral-aqua-160x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-spiral-aqua")),
new XElement("NESTile", new XAttribute("mappedData", "37"), new XElement("tileByte", "37"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "38"), new XElement("tileByte", "38"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "39"), new XElement("tileByte", "39"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "3A"), new XElement("tileByte", "3A"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "3B"), new XElement("tileByte", "3B"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "3C"), new XElement("tileByte", "3C"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "3D"), new XElement("tileByte", "3D"), new XElement("imagePath", @"Assets\Resources\Materials\sprite-textures\empty.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "Monster statue part")),
new XElement("NESTile", new XAttribute("mappedData", "3E"), new XElement("tileByte", "3E"), new XElement("imagePath", @"Assets\Resources\Materials\10x_bush2-blueA-320x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-bush_blueA")),
new XElement("NESTile", new XAttribute("mappedData", "3F"), new XElement("tileByte", "3F"), new XElement("imagePath", @"Assets\Resources\Materials\10x_bush2-blueB-320x160.png"), new XElement("areaName", "BRINSTAR"), new XElement("prefabMappedName", "br-m-bush_blueB"))
);
        //new XElement("NESTile",
        //    new XAttribute("mappedData", nesTileByte),
        //    new XElement("tileByte", "01"),
        //    new XElement("imagePath", @"D:\UnityProjects\metroid-MetroidVR_Development\metroid-UNITY(vrtk)\Assets\Resources\Materials\10x_lava-orange-base-160x160.png")
        //    //new XElement("Price", "$44.95"),
        //    //new XElement("Publisher", "Microgold Publishing")
        //)
        ////,

        //new XElement("NESTile",
        //    new XAttribute("Name", "Scott Lysle"),
        //    new XElement("Book", "Custom Controls"),
        //    new XElement("Cost", "$39.95"),
        //    new XElement("Publisher", "C# Corner")
        //)


        NESTiles.Save(@"assets\resources\nesTilesMap.xml");
        }


 }
