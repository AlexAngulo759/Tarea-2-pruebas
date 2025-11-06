using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Proyecto_Grafos.Services
{
    public class DrawingService
    {
        private const int HorizontalSpacing = 100;
        private const int VerticalSpacing = 120;
        private const int NodeWidth = 100;
        private const int NodeHeight = 100;

        public void DrawTree(Graphics g, List<Models.VisualNode> nodes, GraphService graphService)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var levels = CalculateLevels(nodes, graphService);
            CalculateNodePositions(nodes, levels);
            DrawConnections(g, nodes, graphService);
            DrawNodes(g, nodes);
        }


        private Dictionary<int, List<Models.VisualNode>> CalculateLevels(List<Models.VisualNode> nodes, GraphService graphService)
        {
            var depth = new Dictionary<string, int>();

            foreach (var n in nodes)
            {
                depth[n.Name] = 0;
            }

            bool changed;
            do
            {
                changed = false;

                foreach (var node in nodes)
                {
                    var parents = graphService.GetParents(node.Name);
                    if (parents.Count > 0)
                    {
                        int maxParentLevel = -1;
                        for (int i = 0; i < parents.Count; i++)
                        {
                            string parentName = parents.Get(i);
                            if (depth.ContainsKey(parentName) && depth[parentName] > maxParentLevel)
                            {
                                maxParentLevel = depth[parentName];
                            }
                        }

                        int requiredLevel = maxParentLevel + 1;
                        if (depth[node.Name] < requiredLevel)
                        {
                            depth[node.Name] = requiredLevel;
                            changed = true;
                        }
                    }
                }

            } while (changed);

            do
            {
                changed = false;

                foreach (var node in nodes)
                {
                    var parents = graphService.GetParents(node.Name);
                    if (parents.Count > 1)
                    {
                        int targetParentLevel = depth[node.Name] - 1;

                        for (int i = 0; i < parents.Count; i++)
                        {
                            string parentName = parents.Get(i);
                            if (depth.ContainsKey(parentName) && depth[parentName] != targetParentLevel)
                            {
                                depth[parentName] = targetParentLevel;
                                changed = true;
                            }
                        }
                    }
                }

            } while (changed);

            var levels = new Dictionary<int, List<Models.VisualNode>>();
            foreach (var n in nodes)
            {
                int lvl = depth[n.Name];
                if (!levels.ContainsKey(lvl))
                    levels[lvl] = new List<Models.VisualNode>();
                levels[lvl].Add(n);
            }

            return levels;
        }


        private void CalculateNodePositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels)
        {
            int startX = 400;
            int startY = 50;

            int maxNodesInLevel = 0;
            foreach (var level in levels.Values)
            {
                if (level.Count > maxNodesInLevel)
                    maxNodesInLevel = level.Count;
            }

            int maxWidth = maxNodesInLevel * (NodeWidth + HorizontalSpacing);

            foreach (var kvp in levels.OrderBy(k => k.Key))
            {
                int level = kvp.Key;
                List<Models.VisualNode> levelNodes = kvp.Value;
                int y = startY + level * (NodeHeight + VerticalSpacing);

                int totalWidth = levelNodes.Count * (NodeWidth + HorizontalSpacing) - HorizontalSpacing;
                int x = startX + (maxWidth - totalWidth) / 2; 

                foreach (var node in levelNodes)
                {
                    node.X = x;
                    node.Y = y;
                    node.Size = new Size(NodeWidth, NodeHeight);
                    x += NodeWidth + HorizontalSpacing;
                }
            }
        }

        private void DrawConnections(Graphics g, List<Models.VisualNode> nodes, GraphService graphService)
        {
            using (var pen = new Pen(Color.Black, 2))
            {
                foreach (var node in nodes)
                {
                    var childrenList = graphService.GetChildren(node.Name);
                    var childrenArray = childrenList.ToArray();

                    foreach (var childName in childrenArray)
                    {
                        var child = nodes.FirstOrDefault(n => n.Name == childName);
                        if (child == null) continue;

                        Point parentBottom = new Point(node.X + NodeWidth / 2, node.Y + NodeHeight);
                        Point childTop = new Point(child.X + NodeWidth / 2, child.Y);

                        g.DrawLine(pen, parentBottom, childTop);
                    }
                }
            }
        }

        private void DrawNodes(Graphics g, List<Models.VisualNode> nodes)
        {
            foreach (var node in nodes)
            {
                var rect = new Rectangle(node.X, node.Y, node.Size.Width, node.Size.Height);

                using (var brush = new SolidBrush(node.Color))
                {
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(Pens.Black, rect);
                }

                using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    var textSize = g.MeasureString(node.Name, font);
                    float textX = node.X + (NodeWidth - textSize.Width) / 2;
                    float textY = node.Y + (NodeHeight - textSize.Height) / 2;
                    g.DrawString(node.Name, font, Brushes.Black, textX, textY);
                }
            }
        }
    }
}
