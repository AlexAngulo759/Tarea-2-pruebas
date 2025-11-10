using GMap.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Proyecto_Grafos
{
    public static class CalculateStatistics
    {
       
        public static (string member1, string member2, double distance) FindFurthestPair(DataTable familyMembers)
        {
            if (familyMembers.Rows.Count < 2)
                return ("", "", 0);

            string member1 = "";
            string member2 = "";
            double maxDistance = 0;

            for (int i = 0; i < familyMembers.Rows.Count; i++)
            {
                for (int j = i + 1; j < familyMembers.Rows.Count; j++)
                {
                    double lat1 = Convert.ToDouble(familyMembers.Rows[i]["Lat"]);
                    double lng1 = Convert.ToDouble(familyMembers.Rows[i]["Long"]);
                    double lat2 = Convert.ToDouble(familyMembers.Rows[j]["Lat"]);
                    double lng2 = Convert.ToDouble(familyMembers.Rows[j]["Long"]);

                    string name1 = familyMembers.Rows[i]["Nombre"].ToString();
                    string name2 = familyMembers.Rows[j]["Nombre"].ToString();

                    double distance = DistanceCalculation.CalculateDistance(
                        new PointLatLng(lat1, lng1),
                        new PointLatLng(lat2, lng2));

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        member1 = name1;
                        member2 = name2;
                    }
                }
            }

            return (member1, member2, maxDistance);
        }

        public static (string member1, string member2, double distance) FindClosestPair(DataTable familyMembers)
        {
            if (familyMembers.Rows.Count < 2)
                return ("", "", 0);

            string member1 = "";
            string member2 = "";
            double minDistance = double.MaxValue;

            for (int i = 0; i < familyMembers.Rows.Count; i++)
            {
                for (int j = i + 1; j < familyMembers.Rows.Count; j++)
                {
                    double lat1 = Convert.ToDouble(familyMembers.Rows[i]["Lat"]);
                    double lng1 = Convert.ToDouble(familyMembers.Rows[i]["Long"]);
                    double lat2 = Convert.ToDouble(familyMembers.Rows[j]["Lat"]);
                    double lng2 = Convert.ToDouble(familyMembers.Rows[j]["Long"]);

                    string name1 = familyMembers.Rows[i]["Nombre"].ToString();
                    string name2 = familyMembers.Rows[j]["Nombre"].ToString();

                    double distance = DistanceCalculation.CalculateDistance(
                        new PointLatLng(lat1, lng1),
                        new PointLatLng(lat2, lng2));

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        member1 = name1;
                        member2 = name2;
                    }
                }
            }

            return (member1, member2, minDistance);
        }

        public static double CalculateAverageDistance(DataTable familyMembers)
        {
            if (familyMembers.Rows.Count < 2)
                return 0;

            double totalDistance = 0;
            int pairCount = 0;

            for (int i = 0; i < familyMembers.Rows.Count; i++)
            {
                for (int j = i + 1; j < familyMembers.Rows.Count; j++)
                {
                    double lat1 = Convert.ToDouble(familyMembers.Rows[i]["Lat"]);
                    double lng1 = Convert.ToDouble(familyMembers.Rows[i]["Long"]);
                    double lat2 = Convert.ToDouble(familyMembers.Rows[j]["Lat"]);
                    double lng2 = Convert.ToDouble(familyMembers.Rows[j]["Long"]);

                    double distance = DistanceCalculation.CalculateDistance(
                        new PointLatLng(lat1, lng1),
                        new PointLatLng(lat2, lng2));

                    totalDistance += distance;
                    pairCount++;
                }
            }

            return pairCount > 0 ? totalDistance / pairCount : 0;
        }

        public static Dictionary<string, double> CalculateDistancesFromMember(string memberName, DataTable familyMembers)
        {
            var distances = new Dictionary<string, double>();

            DataRow originRow = familyMembers.Rows.Cast<DataRow>()
                .FirstOrDefault(row => row["Nombre"].ToString() == memberName);

            if (originRow == null) return distances;

            double originLat = Convert.ToDouble(originRow["Lat"]);
            double originLng = Convert.ToDouble(originRow["Long"]);

            foreach (DataRow row in familyMembers.Rows)
            {
                string targetName = row["Nombre"].ToString();
                if (targetName != memberName)
                {
                    double targetLat = Convert.ToDouble(row["Lat"]);
                    double targetLng = Convert.ToDouble(row["Long"]);

                    double distance = DistanceCalculation.CalculateDistance(
                        new PointLatLng(originLat, originLng),
                        new PointLatLng(targetLat, targetLng));

                    distances.Add(targetName, distance);
                }
            }

            return distances;
        }
    }
}