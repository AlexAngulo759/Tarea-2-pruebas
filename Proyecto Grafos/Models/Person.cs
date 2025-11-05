namespace Proyecto_Grafos.Models
{
    public class Person
    {
        public string Name { get; set; }
        public string FatherOf { get; set; }
        public string ChildOf { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Person(string name, double latitude, double longitude)
        {
            Name = name;
            FatherOf = string.Empty;
            ChildOf = string.Empty;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}