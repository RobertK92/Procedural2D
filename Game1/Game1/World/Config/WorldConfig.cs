using System.Xml.Serialization;

namespace Game1
{
    [XmlRoot("World")]
    public class WorldConfig
    {
        [XmlElement("ShowWholeWorld")]
        public bool ShowWholeWorld { get; set; }

        [XmlElement("ChunkWidth")]
        public int ChunkWidth { get; set; }

        [XmlElement("ChunkHeight")]
        public int ChunkHeight { get; set; }

        [XmlElement("RenderRadiusInChunks")]
        public int RenderRadiusInChunks { get; set; }        
        
        [XmlElement("WorldObjectFadingSpeed")]
        public float WorldObjectFadeSpeed { get; set; }

        [XmlElement("WorldObjectFadingOpacity")]
        public float WorldObjectFadedOpacity { get; set; }
        
        [XmlElement("WorldObjectFadingEnabled")]
        public bool WorldObjectFadingEnabled { get; set; }

        [XmlElement("UpdatePlayerChunkEveryXSeconds")]
        public float UpdatePlayerChunkEveryXSeconds { get; set; }

        [XmlElement("Grasslands")]
        public GrasslandsConfig Grasslands { get; set; }



    }
}
