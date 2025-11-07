using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Proyecto_Grafos.Services
{
    public class DrawingService
    {
        private const int BaseHorizontalSpacing = 120;
        private const int VerticalSpacing = 120;
        private const int NodeWidth = 100;
        private const int NodeHeight = 100;
        private const int MinimumSpacing = 80;
        private const int CoupleSpacing = 60;


        private Dictionary<string, string> _coupleRelationships = new Dictionary<string, string>();

        public void DrawTree(Graphics g, List<Models.VisualNode> nodes, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            _coupleRelationships.Clear();

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
            var processed = new HashSet<string>();

            foreach (var n in nodes)
            {
                depth[n.Name] = 0;
            }

            var rootNodes = nodes.Where(n => graphService.GetParents(n.Name).Count == 0).ToList();

            foreach (var root in rootNodes)
            {
                AssignLevelsRecursively(root, nodes, graphService, depth, processed, 0);
            }

            foreach (var node in nodes.Where(n => !processed.Contains(n.Name)))
            {
                if (graphService.GetParents(node.Name).Count > 0)
                {
                    depth[node.Name] = 1;
                }
            }

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

        private void AssignLevelsRecursively(Models.VisualNode node, List<Models.VisualNode> nodes,
                                           GraphService graphService, Dictionary<string, int> depth,
                                           HashSet<string> processed, int currentLevel)
        {
            if (processed.Contains(node.Name)) return;

            depth[node.Name] = currentLevel;
            processed.Add(node.Name);

            var children = graphService.GetChildren(node.Name);
            for (int i = 0; i < children.Count; i++)
            {
                var childName = children.Get(i);
                var child = nodes.FirstOrDefault(n => n.Name == childName);
                if (child != null && !processed.Contains(childName))
                {
                    AssignLevelsRecursively(child, nodes, graphService, depth, processed, currentLevel + 1);
                }
            }

            var parents = graphService.GetParents(node.Name);
            for (int i = 0; i < parents.Count; i++)
            {
                var parentName = parents.Get(i);
                var parent = nodes.FirstOrDefault(n => n.Name == parentName);
                if (parent != null && !processed.Contains(parentName))
                {
                    AssignLevelsRecursively(parent, nodes, graphService, depth, processed, currentLevel - 1);
                }
            }
        }

        private void CalculateNodePositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;
            if (levels == null || levels.Count == 0) return;

            int treeDepth = levels.Keys.Max() + 1;
            int dynamicHorizontalSpacing = CalculateDynamicSpacing(treeDepth);

            int startX = 400;
            int startY = 50;

            var sortedLevels = levels.OrderBy(kvp => kvp.Key).ToList();

            foreach (var kvp in sortedLevels)
            {
                DetectCouplesInLevel(kvp.Value, nodes, graphService);
            }

            CalculateFinalPositions(nodes, levels, startX, startY, dynamicHorizontalSpacing);
        }

        private void DetectCouplesInLevel(List<Models.VisualNode> levelNodes, List<Models.VisualNode> allNodes, GraphService graphService)
        {
            var processed = new HashSet<string>();

            foreach (var node in levelNodes)
            {
                if (processed.Contains(node.Name)) continue;


                var partner = FindPartner(node, levelNodes, allNodes, graphService);

                if (partner != null && !processed.Contains(partner.Name))
                {                    
                    _coupleRelationships[node.Name] = partner.Name;
                    _coupleRelationships[partner.Name] = node.Name;
                    processed.Add(node.Name);
                    processed.Add(partner.Name);
                }
                processed.Add(node.Name);
            }
        }

        private Models.VisualNode FindPartner(Models.VisualNode node, List<Models.VisualNode> levelNodes,
                                            List<Models.VisualNode> allNodes, GraphService graphService)
        {
            var children = graphService.GetChildren(node.Name);
            if (children.Count == 0) return null;

            foreach (var potentialPartner in levelNodes)
            {
                if (potentialPartner.Name == node.Name) continue;

                var partnerChildren = graphService.GetChildren(potentialPartner.Name);

                for (int i = 0; i < children.Count; i++)
                {
                    if (partnerChildren.Contains(children.Get(i)))
                    {
                        return potentialPartner;
                    }
                }
            }

            return null;
        }

        private void CalculateFinalPositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels,
                                           int startX, int startY, int horizontalSpacing)
        {
            var sortedLevels = levels.OrderBy(kvp => kvp.Key).ToList();

            var levelWidths = new Dictionary<int, int>();
            foreach (var kvp in sortedLevels)
            {
                int level = kvp.Key;
                var levelNodes = kvp.Value;

                int coupleCount = levelNodes.Count(n =>
                    _coupleRelationships.ContainsKey(n.Name) &&
                    string.Compare(n.Name, _coupleRelationships[n.Name]) < 0);
                int individualCount = levelNodes.Count(n => !_coupleRelationships.ContainsKey(n.Name));

                int totalWidth = (coupleCount * (NodeWidth * 2 + CoupleSpacing)) +
                               (individualCount * (NodeWidth + horizontalSpacing));

                levelWidths[level] = totalWidth;
            }

            int maxLevelWidth = levelWidths.Values.Max();

            foreach (var kvp in sortedLevels)
            {
                int level = kvp.Key;
                var levelNodes = kvp.Value;
                int y = startY + level * (NodeHeight + VerticalSpacing);

                int levelWidth = levelWidths[level];
                int startLevelX = startX + (maxLevelWidth - levelWidth) / 2;

                OrganizeLevelPositions(levelNodes, startLevelX, y, horizontalSpacing);
            }
        }

        private void OrganizeLevelPositions(List<Models.VisualNode> levelNodes, int startX, int y, int spacing)
        {
            int currentX = startX;
            var processed = new HashSet<string>();

            foreach (var node in levelNodes.Where(n => _coupleRelationships.ContainsKey(n.Name) && !processed.Contains(n.Name)))
            {
                var partnerName = _coupleRelationships[node.Name];
                var partner = levelNodes.FirstOrDefault(n => n.Name == partnerName);

                if (partner != null && !processed.Contains(partnerName))
                {
                    node.X = currentX;
                    node.Y = y;
                    node.Size = new Size(NodeWidth, NodeHeight);

                    partner.X = currentX + NodeWidth + CoupleSpacing;
                    partner.Y = y;
                    partner.Size = new Size(NodeWidth, NodeHeight);

                    currentX += NodeWidth * 2 + CoupleSpacing + spacing;
                    processed.Add(node.Name);
                    processed.Add(partner.Name);
                }
            }

            foreach (var node in levelNodes.Where(n => !_coupleRelationships.ContainsKey(n.Name) && !processed.Contains(n.Name)))
            {
                node.X = currentX;
                node.Y = y;
                node.Size = new Size(NodeWidth, NodeHeight);
                currentX += NodeWidth + spacing;
                processed.Add(node.Name);
            }
        }

        private int CalculateDynamicSpacing(int treeDepth)
        {
            int baseSpacing = BaseHorizontalSpacing;
            int depthMultiplier = Math.Max(1, (int)Math.Log(treeDepth + 1, 2));

            int dynamicSpacing = baseSpacing * depthMultiplier;
            return Math.Max(dynamicSpacing, MinimumSpacing);
        }

        private void DrawConnections(Graphics g, List<Models.VisualNode> nodes, GraphService graphService)
        {
            using (var pen = new Pen(Color.Black, 2))
            using (var couplePen = new Pen(Color.DarkBlue, 2))
            {
                DrawCoupleConnections(g, couplePen, nodes);

                DrawParentChildConnections(g, pen, nodes, graphService);
            }
        }

        private void DrawCoupleConnections(Graphics g, Pen pen, List<Models.VisualNode> nodes)
        {
            var drawnCouples = new HashSet<string>();

            foreach (var node in nodes.Where(n => _coupleRelationships.ContainsKey(n.Name)))
            {
                var partnerName = _coupleRelationships[node.Name];
                var partner = nodes.FirstOrDefault(n => n.Name == partnerName);

                if (partner != null)
                {
                    var coupleKey = string.Compare(node.Name, partnerName) < 0
                        ? $"{node.Name}-{partnerName}"
                        : $"{partnerName}-{node.Name}";

                    if (!drawnCouples.Contains(coupleKey))
                    {
                        Point leftPoint = new Point(node.X + NodeWidth, node.Y + NodeHeight / 2);
                        Point rightPoint = new Point(partner.X, partner.Y + NodeHeight / 2);

                        g.DrawLine(pen, leftPoint, rightPoint);
                        drawnCouples.Add(coupleKey);
                    }
                }
            }
        }

        private void DrawParentChildConnections(Graphics g, Pen pen, List<Models.VisualNode> nodes, GraphService graphService)
        {
            foreach (var node in nodes)
            {
                var children = graphService.GetChildren(node.Name);

                if (children.Count > 0)
                {
                    Point parentConnectionPoint;

                    if (_coupleRelationships.ContainsKey(node.Name))
                    {
                        var partnerName = _coupleRelationships[node.Name];
                        var partner = nodes.FirstOrDefault(n => n.Name == partnerName);
                        if (partner != null)
                        {
                            int coupleCenterX = (node.X + partner.X + NodeWidth) / 2;
                            parentConnectionPoint = new Point(coupleCenterX, node.Y + NodeHeight);
                        }
                        else
                        {
                            parentConnectionPoint = new Point(node.X + NodeWidth / 2, node.Y + NodeHeight);
                        }
                    }
                    else
                    {
                        parentConnectionPoint = new Point(node.X + NodeWidth / 2, node.Y + NodeHeight);
                    }

                    for (int i = 0; i < children.Count; i++)
                    {
                        var childName = children.Get(i);
                        var child = nodes.FirstOrDefault(n => n.Name == childName);

                        if (child != null)
                        {
                            Point childTop = new Point(child.X + NodeWidth / 2, child.Y);

                            Point verticalEnd = new Point(parentConnectionPoint.X, childTop.Y - 10);
                            g.DrawLine(pen, parentConnectionPoint, verticalEnd);

                            g.DrawLine(pen, verticalEnd, childTop);

                            g.FillEllipse(Brushes.Red, verticalEnd.X - 3, verticalEnd.Y - 3, 6, 6);
                        }
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