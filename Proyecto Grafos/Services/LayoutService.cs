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
            var positions = new Dictionary<string, Point>();

            var levels = OrganizeByLevels(people, graphService);

            int currentY = MARGIN;
            foreach (var level in levels.ToArray())
            {
                int nodeCount = level.Count;
                int totalWidth = nodeCount * NODE_SPACING_X;
                int startX = (1000 - totalWidth) / 2;

                for (int i = 0; i < nodeCount; i++)
                {
                    string personName = level.Get(i);
                    int x = startX + i * NODE_SPACING_X;

                    var visualNode = new Models.VisualNode(personName, x, currentY);
                    visualNodes.Add(visualNode);
                }
                currentY += NODE_SPACING_Y;
            }

            return visualNodes;
        }

        private Models.LinkedList<Models.LinkedList<string>> OrganizeByLevels(Models.LinkedList<string> people, GraphService graphService)
        {
            var levels = new Models.LinkedList<Models.LinkedList<string>>();
            var visited = new Dictionary<string, bool>();
            var roots = new Models.LinkedList<string>();

            for (int i = 0; i < people.Count; i++)
            {
                string person = people.Get(i);
                var parent = GetParent(person, graphService);
                if (string.IsNullOrEmpty(parent))
                {
                    roots.Add(person);
                }
            }

            if (roots.Count == 0 && people.Count > 0)
            {
                roots.Add(people.Get(0));
            }

            var queue = new Models.LinkedList<(string person, int level)>();

            var rootsArray = roots.ToArray();
            foreach (var root in rootsArray)
            {
                queue.Add((root, 0));
                visited[root] = true;
            }

            while (queue.Count > 0)
            {
                var current = queue.Get(0);
                queue.RemoveAt(0);

                while (levels.Count <= current.level)
                {
                    levels.Add(new Models.LinkedList<string>());
                }
                levels.Get(current.level).Add(current.person);

                var children = graphService.GetChildren(current.person);
                for (int i = 0; i < children.Count; i++)
                {
                    string child = children.Get(i);
                    if (!visited.ContainsKey(child))
                    {
                        visited[child] = true;
                        queue.Add((child, current.level + 1));
                    }
                }
            }

            return levels;
        }

        private string GetParent(string person, GraphService graphService)
        {
            var personData = graphService.GetPersonData(person);
            return personData?.ChildOf ?? string.Empty;
        }
    }
}