using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Proyecto_Grafos.Services
{
    public class DrawingService
    {
        private const int BaseHorizontalSpacing = 80; // Espaciado base
        private const int VerticalSpacing = 120;
        private const int NodeWidth = 100;
        private const int NodeHeight = 100;
        private const int MinimumSpacing = 50; // Separación mínima absoluta

        public void DrawTree(Graphics g, List<Models.VisualNode> nodes, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var levels = CalculateLevels(nodes, graphService);

            if (levels == null || levels.Count == 0) return;

            CalculateNodePositions(nodes, levels, graphService);
            DrawConnections(g, nodes, graphService);
            DrawNodes(g, nodes);
        }

        private Dictionary<int, List<Models.VisualNode>> CalculateLevels(List<Models.VisualNode> nodes, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0)
                return new Dictionary<int, List<Models.VisualNode>>();

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

        private void CalculateNodePositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;
            if (levels == null || levels.Count == 0) return;

            int treeDepth = levels.Keys.Max() + 1;
            int dynamicHorizontalSpacing = CalculateDynamicSpacing(treeDepth);

            int startX = 400;
            int startY = 50;

            int maxNodesInLevel = 0;
            foreach (var level in levels.Values)
            {
                if (level.Count > maxNodesInLevel)
                    maxNodesInLevel = level.Count;
            }

            int maxWidth = maxNodesInLevel * (NodeWidth + dynamicHorizontalSpacing);

            var mainPerson = FindMainPerson(nodes, levels);
            if (mainPerson == null)
            {
                CalculateSimplePositions(nodes, levels, startX, startY, maxWidth, dynamicHorizontalSpacing);
                return;
            }

            OrganizeTreeFromRoot(mainPerson, nodes, levels, graphService, startX, startY, dynamicHorizontalSpacing, treeDepth);
        }

        private int CalculateDynamicSpacing(int treeDepth)
        {
            int baseSpacing = BaseHorizontalSpacing;
            int depthMultiplier = (int)Math.Pow(1.5, treeDepth - 1); 

            int dynamicSpacing = baseSpacing * depthMultiplier;

            return Math.Max(dynamicSpacing, MinimumSpacing);
        }

        private Models.VisualNode FindMainPerson(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels)
        {
            if (levels == null || levels.Count == 0) return null;

            try
            {
                int maxLevel = levels.Keys.Max();
                if (levels.ContainsKey(maxLevel) && levels[maxLevel].Count > 0)
                {
                    return levels[maxLevel][0];
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return null;
        }

        private void CalculateSimplePositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels, int startX, int startY, int maxWidth, int horizontalSpacing)
        {
            foreach (var kvp in levels.OrderBy(k => k.Key))
            {
                int level = kvp.Key;
                List<Models.VisualNode> levelNodes = kvp.Value;
                int y = startY + level * (NodeHeight + VerticalSpacing);

                int totalWidth = levelNodes.Count * (NodeWidth + horizontalSpacing) - horizontalSpacing;
                int x = startX + (maxWidth - totalWidth) / 2;

                foreach (var node in levelNodes)
                {
                    node.X = x;
                    node.Y = y;
                    node.Size = new Size(NodeWidth, NodeHeight);
                    x += NodeWidth + horizontalSpacing;
                }
            }
        }

        private void OrganizeTreeFromRoot(Models.VisualNode root, List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels, GraphService graphService, int startX, int startY, int horizontalSpacing, int treeDepth)
        {
            foreach (var node in nodes)
            {
                node.X = 0;
                node.Y = 0;
            }

            foreach (var kvp in levels)
            {
                int level = kvp.Key;
                int y = startY + level * (NodeHeight + VerticalSpacing);
                foreach (var node in kvp.Value)
                {
                    node.Y = y;
                    node.Size = new Size(NodeWidth, NodeHeight);
                }
            }

            int dynamicBoundary = CalculateDynamicBoundary(treeDepth, horizontalSpacing);

            root.X = startX;

            OrganizeSubtree(root, nodes, graphService, levels, 0, startX - dynamicBoundary, startX + dynamicBoundary, horizontalSpacing);
        }

        private int CalculateDynamicBoundary(int treeDepth, int horizontalSpacing)
        {
            int baseBoundary = 300;
            int depthMultiplier = (int)Math.Pow(2, treeDepth - 1);

            return baseBoundary * depthMultiplier + (horizontalSpacing * treeDepth);
        }

        private void OrganizeSubtree(Models.VisualNode node, List<Models.VisualNode> nodes, GraphService graphService, Dictionary<int, List<Models.VisualNode>> levels, int currentLevel, int leftBoundary, int rightBoundary, int horizontalSpacing)
        {
            var parents = graphService.GetParents(node.Name);
            if (parents.Count == 0) return;

            int levelSpacing = horizontalSpacing + (currentLevel * 20);

            if (parents.Count == 1)
            {
                var parent = nodes.FirstOrDefault(n => n.Name == parents.Get(0));
                if (parent != null)
                {
                    parent.X = node.X;
                    OrganizeSubtree(parent, nodes, graphService, levels, currentLevel - 1, leftBoundary, rightBoundary, horizontalSpacing);
                }
            }
            else if (parents.Count == 2)
            {
                var parent1 = nodes.FirstOrDefault(n => n.Name == parents.Get(0));
                var parent2 = nodes.FirstOrDefault(n => n.Name == parents.Get(1));

                if (parent1 != null && parent2 != null)
                {
                    int availableWidth = rightBoundary - leftBoundary;
                    int centerX = node.X;

                    int minParentSeparation = NodeWidth + levelSpacing;

                    int leftCenter = Math.Max(leftBoundary + minParentSeparation, centerX - minParentSeparation);
                    parent1.X = leftCenter;

                    int rightCenter = Math.Min(rightBoundary - minParentSeparation, centerX + minParentSeparation);
                    parent2.X = rightCenter;

                    if (Math.Abs(parent1.X - parent2.X) < minParentSeparation)
                    {
                        parent1.X = centerX - minParentSeparation;
                        parent2.X = centerX + minParentSeparation;
                    }

                    int newLeftBoundary = leftBoundary - levelSpacing;
                    int newRightBoundary = rightBoundary + levelSpacing;

                    OrganizeSubtree(parent1, nodes, graphService, levels, currentLevel - 1, newLeftBoundary, centerX, horizontalSpacing);
                    OrganizeSubtree(parent2, nodes, graphService, levels, currentLevel - 1, centerX, newRightBoundary, horizontalSpacing);
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