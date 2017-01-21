
using System.Xml.Serialization;

namespace Game1
{
    public class GrasslandsConfig
    {
        [XmlElement("GrassFrequency")]
        public float GrassFrequency { get; set; }

        [XmlElement("GroundNoiseOctaves")]
        public int GroundNoiseOctaves { get; set; }

        [XmlElement("GroundNoiseFrequency")]
        public double GroundNoiseFrequency { get; set; }

        [XmlElement("GroundNoisePersistence")]
        public double GroundNoisePersistence { get; set; }

        [XmlElement("GroundNoiseLacunarity")]
        public double GroundNoiseLacunarity { get; set; }
    }
}
