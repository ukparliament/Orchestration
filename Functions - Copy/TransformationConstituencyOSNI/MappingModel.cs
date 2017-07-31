namespace Functions.TransformationConstituencyOSNI
{

    public class Rootobject
    {
        public string displayFieldName { get; set; }
        public Fieldaliases fieldAliases { get; set; }
        public string geometryType { get; set; }
        public Spatialreference spatialReference { get; set; }
        public Field[] fields { get; set; }
        public Feature[] features { get; set; }
    }

    public class Fieldaliases
    {
        public string PC_ID { get; set; }
        public string PC_NAME { get; set; }
    }

    public class Spatialreference
    {
        public int wkid { get; set; }
        public int latestWkid { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public string type { get; set; }
        public string alias { get; set; }
        public int length { get; set; }
    }

    public class Feature
    {
        public Attributes attributes { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Attributes
    {
        public string PC_ID { get; set; }
        public string PC_NAME { get; set; }
    }

    public class Geometry
    {
        public decimal[][][] rings { get; set; }
    }

}
