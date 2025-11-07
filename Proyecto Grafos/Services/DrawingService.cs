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
        private Dictionary<string, string> _familySide = new Dictionary<string, string>();

        public void DrawTree(Graphics g, List<Models.VisualNode> nodes, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            _coupleRelationships.Clear();
            _familySide.Clear();
            var levels = CalculateLevels(nodes, graphService);
            if (levels == null || levels.Count == 0) return;
            DetectFamilySides(nodes, graphService);
            CalculateNodePositions(nodes, levels, graphService);
            DrawConnections(g, nodes, graphService);
            DrawNodes(g, nodes);
        }

        private Dictionary<int, List<Models.VisualNode>> CalculateLevels(List<Models.VisualNode> nodes, GraphService graphService)
        {
            var depth = new Dictionary<string, int>();
            var processed = new HashSet<string>();
            foreach (var n in nodes) depth[n.Name] = 0;
            var roots = nodes.Where(n => graphService.GetParents(n.Name).Count == 0).ToList();
            foreach (var r in roots) AssignLevelsRecursively(r, nodes, graphService, depth, processed, 0);
            foreach (var n in nodes.Where(n => !processed.Contains(n.Name)))
                if (graphService.GetParents(n.Name).Count > 0) depth[n.Name] = 1;
            var levels = new Dictionary<int, List<Models.VisualNode>>();
            foreach (var n in nodes)
            {
                int lvl = depth[n.Name];
                if (!levels.ContainsKey(lvl)) levels[lvl] = new List<Models.VisualNode>();
                levels[lvl].Add(n);
            }
            return levels;
        }

        private void AssignLevelsRecursively(Models.VisualNode node, List<Models.VisualNode> nodes, GraphService graphService,
            Dictionary<string, int> depth, HashSet<string> processed, int currentLevel)
        {
            if (processed.Contains(node.Name)) return;
            depth[node.Name] = currentLevel;
            processed.Add(node.Name);

            var children = graphService.GetChildren(node.Name).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var c = nodes.FirstOrDefault(n => n.Name == children[i]);
                if (c != null) AssignLevelsRecursively(c, nodes, graphService, depth, processed, currentLevel + 1);
            }

            var parents = graphService.GetParents(node.Name).ToList();
            for (int i = 0; i < parents.Count; i++)
            {
                var p = nodes.FirstOrDefault(n => n.Name == parents[i]);
                if (p != null) AssignLevelsRecursively(p, nodes, graphService, depth, processed, currentLevel - 1);
            }
        }

        private void DetectFamilySides(List<Models.VisualNode> nodes, GraphService graphService)
        {
            foreach (var node in nodes)
            {
                if (node.Name.IndexOf("padre", StringComparison.OrdinalIgnoreCase) >= 0)
                    _familySide[node.Name] = "paterno";
                else if (node.Name.IndexOf("madre", StringComparison.OrdinalIgnoreCase) >= 0)
                    _familySide[node.Name] = "materno";
            }

            foreach (var node in nodes)
            {
                var children = graphService.GetChildren(node.Name).ToList();
                foreach (var childName in children)
                {
                    if (_familySide.ContainsKey(node.Name))
                        _familySide[childName] = _familySide[node.Name];
                }
            }
        }

        private void CalculateNodePositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels, GraphService graphService)
        {
            int treeDepth = levels.Keys.Max() + 1;
            int spacing = CalculateDynamicSpacing(treeDepth);
            int startX = 400;
            int startY = 50;
            var sorted = levels.OrderBy(kvp => kvp.Key).ToList();
            foreach (var kvp in sorted) DetectCouplesInLevel(kvp.Value, nodes, graphService);
            CalculateFinalPositions(nodes, levels, startX, startY, spacing);
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

        private Models.VisualNode FindPartner(Models.VisualNode node, List<Models.VisualNode> levelNodes, List<Models.VisualNode> allNodes, GraphService graphService)
        {
            var children = graphService.GetChildren(node.Name).ToList();
            if (children.Count == 0) return null;
            foreach (var p in levelNodes)
            {
                if (p.Name == node.Name) continue;
                var c2 = graphService.GetChildren(p.Name).ToList();
                for (int i = 0; i < children.Count; i++)
                    if (c2.Contains(children[i])) return p;
            }
            return null;
        }

        private void CalculateFinalPositions(List<Models.VisualNode> nodes, Dictionary<int, List<Models.VisualNode>> levels, int startX, int startY, int spacing)
        {
            var sorted = levels.OrderBy(kvp => kvp.Key).ToList();
            var widths = new Dictionary<int, int>();
            foreach (var kvp in sorted)
            {
                int couples = kvp.Value.Count(n => _coupleRelationships.ContainsKey(n.Name) && string.Compare(n.Name, _coupleRelationships[n.Name]) < 0);
                int singles = kvp.Value.Count(n => !_coupleRelationships.ContainsKey(n.Name));
                widths[kvp.Key] = (couples * (NodeWidth * 2 + CoupleSpacing)) + (singles * (NodeWidth + spacing));
            }
            int maxWidth = widths.Values.Max();
            foreach (var kvp in sorted)
            {
                int level = kvp.Key;
                int y = startY + level * (NodeHeight + VerticalSpacing);
                int levelWidth = widths[level];
                int center = startX + (maxWidth - levelWidth) / 2;
                OrganizeLevelPositions(kvp.Value, center, y, spacing, level);
            }
        }

        private void OrganizeLevelPositions(List<Models.VisualNode> levelNodes, int startX, int y, int spacing, int level)
        {
            int currentX = startX;
            var processed = new HashSet<string>();
            var couples = levelNodes.Where(n => _coupleRelationships.ContainsKey(n.Name) && !processed.Contains(n.Name)).ToList();

            couples = couples.OrderBy(n =>
            {
                string side = _familySide.ContainsKey(n.Name) ? _familySide[n.Name] : "none";
                return side == "paterno" ? 0 : (side == "materno" ? 2 : 1);
            }).ToList();

            foreach (var node in couples)
            {
                if (processed.Contains(node.Name)) continue;
                var partnerName = _coupleRelationships[node.Name];
                var partner = levelNodes.FirstOrDefault(n => n.Name == partnerName);
                if (partner != null && !processed.Contains(partnerName))
                {
                    var leftNode = node;
                    var rightNode = partner;
                    if (_familySide.ContainsKey(partner.Name) && _familySide[partner.Name] == "paterno")
                    {
                        leftNode = partner;
                        rightNode = node;
                    }
                    leftNode.X = currentX;
                    leftNode.Y = y;
                    leftNode.Size = new Size(NodeWidth, NodeHeight);
                    rightNode.X = currentX + NodeWidth + CoupleSpacing;
                    rightNode.Y = y;
                    rightNode.Size = new Size(NodeWidth, NodeHeight);
                    currentX += NodeWidth * 2 + CoupleSpacing + spacing;
                    processed.Add(leftNode.Name);
                    processed.Add(rightNode.Name);
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
            int mult = Math.Max(1, (int)Math.Log(treeDepth + 1, 2));
            int dynamic = baseSpacing * mult;
            return Math.Max(dynamic, MinimumSpacing);
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
            var drawn = new HashSet<string>();
            foreach (var n in nodes.Where(n => _coupleRelationships.ContainsKey(n.Name)))
            {
                var pName = _coupleRelationships[n.Name];
                var p = nodes.FirstOrDefault(x => x.Name == pName);
                if (p == null) continue;
                var key = string.Compare(n.Name, pName) < 0 ? $"{n.Name}-{pName}" : $"{pName}-{n.Name}";
                if (drawn.Contains(key)) continue;
                Point l = new Point(n.X + NodeWidth, n.Y + NodeHeight / 2);
                Point r = new Point(p.X, p.Y + NodeHeight / 2);
                g.DrawLine(pen, l, r);
                drawn.Add(key);
            }
        }

        private void DrawParentChildConnections(Graphics g, Pen pen, List<Models.VisualNode> nodes, GraphService graphService)
        {
            foreach (var n in nodes)
            {
                var children = graphService.GetChildren(n.Name).ToList();
                if (children.Count == 0) continue;
                Point parentPoint;
                if (_coupleRelationships.ContainsKey(n.Name))
                {
                    var partnerName = _coupleRelationships[n.Name];
                    var partner = nodes.FirstOrDefault(x => x.Name == partnerName);
                    if (partner != null)
                    {
                        int center = (n.X + partner.X + NodeWidth) / 2;
                        parentPoint = new Point(center, n.Y + NodeHeight);
                    }
                    else parentPoint = new Point(n.X + NodeWidth / 2, n.Y + NodeHeight);
                }
                else parentPoint = new Point(n.X + NodeWidth / 2, n.Y + NodeHeight);

                foreach (var cName in children)
                {
                    var child = nodes.FirstOrDefault(x => x.Name == cName);
                    if (child == null) continue;
                    Point top = new Point(child.X + NodeWidth / 2, child.Y);
                    Point end = new Point(parentPoint.X, top.Y - 10);
                    g.DrawLine(pen, parentPoint, end);
                    g.DrawLine(pen, end, top);
                    g.FillEllipse(Brushes.Red, end.X - 3, end.Y - 3, 6, 6);
                }
            }
        }

        private void DrawNodes(Graphics g, List<Models.VisualNode> nodes)
        {
            foreach (var n in nodes)
            {
                var rect = new Rectangle(n.X, n.Y, n.Size.Width, n.Size.Height);
                using (var brush = new SolidBrush(n.Color))
                {
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(Pens.Black, rect);
                }
                using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    var size = g.MeasureString(n.Name, font);
                    float tx = n.X + (NodeWidth - size.Width) / 2;
                    float ty = n.Y + (NodeHeight - size.Height) / 2;
                    g.DrawString(n.Name, font, Brushes.Black, tx, ty);
                }
            }
        }
    }
}
