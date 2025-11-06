using System.Collections.Generic;
using System.Drawing;

namespace Proyecto_Grafos.Services
{
    public class LayoutService
    {
        private const int NODE_SPACING_X = 120;
        private const int NODE_SPACING_Y = 100;
        private const int MARGIN = 50;

        public List<Models.VisualNode> CalculateLayout(Models.LinkedList<string> people, GraphService graphService)
        {
            var visualNodes = new List<Models.VisualNode>();

            var peopleList = new List<string>();
            for (int i = 0; i < people.Count; i++)
            {
                peopleList.Add(people.Get(i));
            }

            var levels = CalculateLevels(peopleList, graphService);

            int currentY = MARGIN;
            int maxNodesInLevel = 0;

            foreach (var level in levels.Values)
            {
                if (level.Count > maxNodesInLevel)
                    maxNodesInLevel = level.Count;
            }

            int maxWidth = maxNodesInLevel * NODE_SPACING_X;

            foreach (var kvp in levels)
            {
                int level = kvp.Key;
                var levelNodes = kvp.Value;

                int totalWidth = levelNodes.Count * NODE_SPACING_X;
                int startX = (1000 - totalWidth) / 2;

                foreach (var personName in levelNodes)
                {
                    int x = startX;
                    var visualNode = new Models.VisualNode(personName, x, currentY);
                    visualNodes.Add(visualNode);
                    startX += NODE_SPACING_X;
                }

                currentY += NODE_SPACING_Y;
            }

            return visualNodes;
        }

        private Dictionary<int, List<string>> CalculateLevels(List<string> people, GraphService graphService)
        {
            var depth = new Dictionary<string, int>();
            var visited = new Dictionary<string, bool>();

            foreach (var person in people)
            {
                depth[person] = -1;
                visited[person] = false;
            }

            var roots = new List<string>();
            foreach (var person in people)
            {
                var parents = graphService.GetParents(person);
                if (parents.Count == 0)
                {
                    depth[person] = 0;
                    roots.Add(person);
                    visited[person] = true;
                }
            }

            var queue = new Queue<string>();
            foreach (var root in roots)
            {
                queue.Enqueue(root);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int currentLevel = depth[current];

                var children = graphService.GetChildren(current);
                for (int i = 0; i < children.Count; i++)
                {
                    string child = children.Get(i);
                    if (!visited[child])
                    {
                        depth[child] = currentLevel + 1;
                        visited[child] = true;
                        queue.Enqueue(child);
                    }
                }
            }

            bool changed;
            do
            {
                changed = false;
                foreach (var person in people)
                {
                    var parents = graphService.GetParents(person);
                    if (parents.Count > 0)
                    {
                        int maxParentLevel = -1;
                        for (int i = 0; i < parents.Count; i++)
                        {
                            string parent = parents.Get(i);
                            if (depth.ContainsKey(parent) && depth[parent] > maxParentLevel)
                            {
                                maxParentLevel = depth[parent];
                            }
                        }

                        if (maxParentLevel >= 0 && depth[person] < maxParentLevel + 1)
                        {
                            depth[person] = maxParentLevel + 1;
                            changed = true;
                        }
                    }
                }
            } while (changed);

            var levels = new Dictionary<int, List<string>>();
            foreach (var person in people)
            {
                int lvl = depth[person];
                if (!levels.ContainsKey(lvl))
                    levels[lvl] = new List<string>();
                levels[lvl].Add(person);
            }

            return levels;
        }
    }
}