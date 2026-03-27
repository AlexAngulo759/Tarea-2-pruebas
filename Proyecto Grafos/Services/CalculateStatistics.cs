using GMap.NET;
using Proyecto_Grafos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Proyecto_Grafos
{
    public static class CalculateStatistics
    {
        public static (string member1, string member2, double distance) FindFurthestPair(List<FamilyMember> members)
        {
            if (members.Count < 2) return ("", "", 0);

            string m1 = "", m2 = "";
            double maxDistance = 0;

            for (int i = 0; i < members.Count; i++)
            {
                for (int j = i + 1; j < members.Count; j++)
                {
                    double distance = DistanceCalculation.CalculateDistance(
                        new PointLatLng(members[i].Lat, members[i].Lng),
                        new PointLatLng(members[j].Lat, members[j].Lng));

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        m1 = members[i].Name;
                        m2 = members[j].Name;
                    }
                }
            }

            return (m1, m2, maxDistance);
        }

        public static (string member1, string member2, double distance) FindClosestPair(List<FamilyMember> members)
        {
            if (members.Count < 2) return ("", "", 0);

            string m1 = "", m2 = "";
            double minDistance = double.MaxValue;

            for (int i = 0; i < members.Count; i++)
            {
                for (int j = i + 1; j < members.Count; j++)
                {
                    double distance = DistanceCalculation.CalculateDistance(
                        new PointLatLng(members[i].Lat, members[i].Lng),
                        new PointLatLng(members[j].Lat, members[j].Lng));

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        m1 = members[i].Name;
                        m2 = members[j].Name;
                    }
                }
            }

            return (m1, m2, minDistance);
        }

        public static double CalculateAverageDistance(List<FamilyMember> members)
        {
            if (members.Count < 2) return 0;

            double total = 0;
            int count = 0;

            for (int i = 0; i < members.Count; i++)
            {
                for (int j = i + 1; j < members.Count; j++)
                {
                    total += DistanceCalculation.CalculateDistance(
                        new PointLatLng(members[i].Lat, members[i].Lng),
                        new PointLatLng(members[j].Lat, members[j].Lng));
                    count++;
                }
            }

            return total / count;
        }
    }
}
