using System;

namespace Proyecto_Grafos.Models
{
    public class Person
    {
        public string Name { get; set; }
        public string FatherOf { get; set; }
        public string ChildOf { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PhotoPath { get; set; }
        public string Cedula { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime? FechaFallecimiento { get; set; }
        public bool EstaVivo { get; set; }

        public Person(string name, double latitude, double longitude)
        {
            Name = name;
            FatherOf = string.Empty;
            ChildOf = string.Empty;
            Latitude = latitude;
            Longitude = longitude;
            PhotoPath = string.Empty;
            Cedula = string.Empty;
            FechaNacimiento = DateTime.Now;
            FechaFallecimiento = null;
            EstaVivo = true;
        }

        public int Edad
        {
            get
            {
                var fechaReferencia = EstaVivo ? DateTime.Now : FechaFallecimiento.Value;
                int edad = fechaReferencia.Year - FechaNacimiento.Year;

                if (FechaNacimiento.Date > fechaReferencia.AddYears(-edad))
                    edad--;

                return edad;
            }
        }
    }
}