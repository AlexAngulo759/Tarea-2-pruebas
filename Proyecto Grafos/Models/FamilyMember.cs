namespace Proyecto_Grafos.Models
{
    public class FamilyMember
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }

        public FamilyMember(string name, double lat, double lng)
        {
            Name = name;
            Lat = lat;
            Lng = lng;
        }
    }
}
