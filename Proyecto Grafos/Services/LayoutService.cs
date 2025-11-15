using System.Collections.Generic;
using System.Drawing;
using Proyecto_Grafos.Core.Models;

namespace Proyecto_Grafos.Services
{
    public class LayoutService
    {
        private const int NODE_SPACING_X = 120;
        private const int NODE_SPACING_Y = 100;
        private const int MARGIN_X = 50;
        private const int MARGIN_Y = 50;

        public List<VisualNode> CalculateLayout(List<string> people, GraphService graphService)
        {
            var visualNodes = new List<VisualNode>();

            var roots = new List<string>();
            foreach (var p in people)
            {
                var parents = graphService.GetParents(p);
                if (parents.Count == 0)
                    roots.Add(p);
            }

            float currentX = MARGIN_X;
            foreach (var root in roots)
            {
                LayoutSubtree(graphService, root, currentX, MARGIN_Y, visualNodes);
                currentX += GetSubtreeWidth(graphService, root) + NODE_SPACING_X;
            }

            return visualNodes;
        }

        private float LayoutSubtree(GraphService graphService, string person, float x, float y, List<VisualNode> visualNodes)
        {
            if (visualNodes.Exists(v => v.Name == person))
                return x;

            visualNodes.Add(new VisualNode(person, (int)x, (int)y));

            var parents = graphService.GetParents(person);
            var children = graphService.GetChildren(person);

            if (parents.Count > 0)
            {
                float parentX = x - ((parents.Count - 1) * NODE_SPACING_X) / 2f;
                for (int i = 0; i < parents.Count; i++)
                {
                    string parent = parents.Get(i);
                    LayoutSubtree(graphService, parent, parentX, y - NODE_SPACING_Y, visualNodes);
                    parentX += NODE_SPACING_X;
                }
            }

            if (children.Count > 0)
            {
                float childX = x - ((children.Count - 1) * NODE_SPACING_X) / 2f;
                for (int i = 0; i < children.Count; i++)
                {
                    string child = children.Get(i);
                    LayoutSubtree(graphService, child, childX, y + NODE_SPACING_Y, visualNodes);
                    childX += NODE_SPACING_X;
                }
            }

            return x;
        }

        private float GetSubtreeWidth(GraphService graphService, string person)
        {
            var children = graphService.GetChildren(person);
            if (children.Count == 0)
                return NODE_SPACING_X;

            float total = 0;
            for (int i = 0; i < children.Count; i++)
                total += GetSubtreeWidth(graphService, children.Get(i));

            return total;
        }
    }
}