using System;
using System.Linq;
using GMap.NET;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Models;

namespace Proyecto_Grafos.Services
{
    public static class GraphPathfinder
    {
        public static (Proyecto_Grafos.Models.Dictionary<string, double> distances, Proyecto_Grafos.Models.Dictionary<string, string> previous)
            Dijkstra(GraphService graphService, string source, bool treatAsUndirected = true, bool useCompleteGraph = false)
        {
            if (graphService == null) throw new ArgumentNullException(nameof(graphService));
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            var nodes = graphService.GetPeople();

            var distCustom = new Proyecto_Grafos.Models.Dictionary<string, double>();
            var prevCustom = new Proyecto_Grafos.Models.Dictionary<string, string>();
            foreach (var n in nodes)
                distCustom.Add(n, double.PositiveInfinity);

            if (!distCustom.ContainsKey(source))
            {
                return (new Proyecto_Grafos.Models.Dictionary<string, double>(),
                        new Proyecto_Grafos.Models.Dictionary<string, string>());
            }
            if (distCustom.ContainsKey(source))
                distCustom.Remove(source);
            distCustom.Add(source, 0.0);
            var pq = new Proyecto_Grafos.Models.Dictionary<double, Proyecto_Grafos.Models.LinkedList<string>>();
            Enqueue(pq, 0.0, source);

            while (TryDequeueMin(pq, out var d, out var u))
            {
                var distU = distCustom.Get(u);
                if (d > distU) continue;

                Proyecto_Grafos.Models.LinkedList<string> neighboursList = null;
                if (useCompleteGraph)
                {
                    neighboursList = new Proyecto_Grafos.Models.LinkedList<string>();
                    foreach (var n in nodes.Where(n => !string.Equals(n, u, StringComparison.OrdinalIgnoreCase)))
                        neighboursList.Add(n);
                }
                else
                {
                    neighboursList = new Proyecto_Grafos.Models.LinkedList<string>();
                    foreach (var c in graphService.GetChildren(u))
                        if (!ContainsIgnoreCase(neighboursList, c)) neighboursList.Add(c);
                    if (treatAsUndirected)
                        foreach (var p in graphService.GetParents(u))
                            if (!ContainsIgnoreCase(neighboursList, p)) neighboursList.Add(p);
                }

                var pu = graphService.GetPerson(u);
                if (pu == null) continue;

                foreach (var v in neighboursList)
                {
                    var pv = graphService.GetPerson(v);
                    if (pv == null) continue;

                    double w = DistanceCalculation.CalculateDistance(
                        new PointLatLng(pu.Latitude, pu.Longitude),
                        new PointLatLng(pv.Latitude, pv.Longitude));

                    if (double.IsNaN(w) || double.IsInfinity(w)) continue;

                    var alt = distCustom.Get(u) + w;

                    if (!distCustom.ContainsKey(v))
                        distCustom.Add(v, double.PositiveInfinity);

                    if (alt < distCustom.Get(v))
                    {
                        if (distCustom.ContainsKey(v)) distCustom.Remove(v);
                        distCustom.Add(v, alt);

                        if (prevCustom.ContainsKey(v)) prevCustom.Remove(v);
                        prevCustom.Add(v, u);

                        Enqueue(pq, alt, v);
                    }
                }
            }

            return (distCustom, prevCustom);
        }
        private static void Enqueue(Proyecto_Grafos.Models.Dictionary<double, Proyecto_Grafos.Models.LinkedList<string>> pq, double key, string node)
        {
            if (!pq.ContainsKey(key))
            {
                pq.Add(key, new Proyecto_Grafos.Models.LinkedList<string>());
            }
            var q = pq.Get(key);
            q.Add(node);
        }
        private static bool TryDequeueMin(Proyecto_Grafos.Models.Dictionary<double, Proyecto_Grafos.Models.LinkedList<string>> pq, out double minKey, out string node)
        {
            minKey = default(double);
            node = null;

            var keys = pq.Keys();
            bool any = false;
            double min = 0.0;
            foreach (var k in keys)
            {
                if (!any || k < min)
                {
                    min = k;
                    any = true;
                }
            }

            if (!any) return false;

            var q = pq.Get(min);
            node = q.Get(0);
            q.RemoveAt(0);
            if (q.Count == 0) pq.Remove(min);

            minKey = min;
            return true;
        }
        private static bool ContainsIgnoreCase(Proyecto_Grafos.Models.LinkedList<string> list, string value)
        {
            foreach (var item in list)
            {
                if (string.Equals(item, value, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}