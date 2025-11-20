using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Models;

namespace Proyecto_Grafos.Services
{
    public class DrawingService
    {
        private const int BaseHorizontalSpacing = 120;
        private const int VerticalSpacing = 120;
        private const int NodeWidth = 120;
        private const int NodeHeight = 140;
        private const int MinimumSpacing = 80;
        private const int CoupleSpacing = 60;

        private System.Collections.Generic.Dictionary<string, string> _coupleRelationships = new System.Collections.Generic.Dictionary<string, string>();
        private System.Collections.Generic.Dictionary<string, string> _familySide = new System.Collections.Generic.Dictionary<string, string>();
        private System.Collections.Generic.Dictionary<string, Image> _imageCache = new System.Collections.Generic.Dictionary<string, Image>();

        private class Block
        {
            public List<VisualNode> Nodes { get; set; }
            public int Width { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public void DrawTree(Graphics g, List<VisualNode> nodes, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            _coupleRelationships.Clear();
            _familySide.Clear();
            var levels = CalculateLevels(nodes, graphService);
            if (levels == null || levels.Count == 0) return;
            DetectFamilySides(nodes, graphService);
            CalculateNodePositions(nodes, levels, graphService);
            DrawConnections(g, nodes, graphService);
            DrawNodes(g, nodes, graphService);
        }

        private System.Collections.Generic.Dictionary<int, List<VisualNode>> CalculateLevels(List<VisualNode> nodes, GraphService graphService)
        {
            var depth = new System.Collections.Generic.Dictionary<string, int>();
            var processed = new System.Collections.Generic.HashSet<string>();
            foreach (var n in nodes) depth[n.Name] = 0;
            var roots = nodes.Where(n => graphService.GetParents(n.Name).Count() == 0).ToList();
            foreach (var r in roots) AssignLevelsRecursively(r, nodes, graphService, depth, processed, 0);
            foreach (var n in nodes.Where(n => !processed.Contains(n.Name)))
                if (graphService.GetParents(n.Name).Count() > 0) depth[n.Name] = 1;
            var levels = new System.Collections.Generic.Dictionary<int, List<VisualNode>>();
            foreach (var n in nodes)
            {
                int lvl = depth[n.Name];
                if (!levels.ContainsKey(lvl)) levels[lvl] = new List<VisualNode>();
                levels[lvl].Add(n);
            }
            return levels;
        }

        private void AssignLevelsRecursively(VisualNode node, List<VisualNode> nodes, GraphService graphService,
            System.Collections.Generic.Dictionary<string, int> depth, System.Collections.Generic.HashSet<string> processed, int currentLevel)
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

            private void CalculateFinalPositions(List<VisualNode> nodes, System.Collections.Generic.Dictionary<int, List<VisualNode>> levels, int startX, int startY, int spacing, GraphService graphService)
            {
                var sorted = levels.OrderBy(kvp => kvp.Key).ToList();
                int maxWidth = 0;
                var widths = new System.Collections.Generic.Dictionary<int, int>();
                foreach (var kvp in sorted)
                {
                    int couples = kvp.Value.Count(n => _coupleRelationships.ContainsKey(n.Name) && string.Compare(n.Name, _coupleRelationships[n.Name]) < 0);
                    int singles = kvp.Value.Count(n => !_coupleRelationships.ContainsKey(n.Name));
                    widths[kvp.Key] = (couples * (NodeWidth * 2 + CoupleSpacing)) + (singles * (NodeWidth + spacing));
                }
                if (widths.Count > 0) maxWidth = widths.Values.Max();

                // Block-based layout: treat couples as indivisible blocks to avoid splits and cascading shifts
                var nodeByName = nodes.ToDictionary(n => n.Name);
                var blocksByLevel = new System.Collections.Generic.Dictionary<int, List<Block>>();

                foreach (var kvp in sorted)
                {
                    int level = kvp.Key;
                    int y = startY + level * (NodeHeight + VerticalSpacing);
                    var levelNodes = kvp.Value;

                    var processed = new System.Collections.Generic.HashSet<string>();
                    var blocks = new List<Block>();

                    foreach (var n in levelNodes)
                    {
                        if (processed.Contains(n.Name)) continue;

                        if (_coupleRelationships.ContainsKey(n.Name))
                        {
                            var partnerName = _coupleRelationships[n.Name];
                            var partner = levelNodes.FirstOrDefault(x => x.Name == partnerName);
                            if (partner != null && !processed.Contains(partnerName))
                            {
                                // decide left/right by family side heuristics
                                var leftNode = n;
                                var rightNode = partner;
                                string sideNode = _familySide.ContainsKey(n.Name) ? _familySide[n.Name] : "none";
                                string sidePartner = _familySide.ContainsKey(partner.Name) ? _familySide[partner.Name] : "none";
                                if (sidePartner == "paterno" && sideNode != "paterno")
                                {
                                    leftNode = partner; rightNode = n;
                                }
                                else if (sideNode == "paterno" && sidePartner != "paterno")
                                {
                                    leftNode = n; rightNode = partner;
                                }

                                var b = new Block { Nodes = new List<VisualNode> { leftNode, rightNode }, Width = NodeWidth * 2 + CoupleSpacing };
                                blocks.Add(b);
                                processed.Add(leftNode.Name);
                                processed.Add(rightNode.Name);
                                continue;
                            }
                        }

                        // single node block
                        var singleBlock = new Block { Nodes = new List<VisualNode> { n }, Width = NodeWidth };
                        blocks.Add(singleBlock);
                        processed.Add(n.Name);
                    }

                    // compute total width and start offset
                    int totalWidth = blocks.Sum(b => b.Width) + Math.Max(0, blocks.Count - 1) * spacing;
                    int startLeft = startX + (maxWidth - totalWidth) / 2;

                    // assign positions for blocks and inner nodes
                    foreach (var b in blocks)
                    {
                        b.X = startLeft;
                        b.Y = y;
                        if (b.Nodes.Count == 2)
                        {
                            var left = b.Nodes[0];
                            var right = b.Nodes[1];
                            left.X = b.X;
                            left.Y = y;
                            left.Size = new Size(NodeWidth, NodeHeight);
                            right.X = b.X + NodeWidth + CoupleSpacing;
                            right.Y = y;
                            right.Size = new Size(NodeWidth, NodeHeight);
                        }
                        else
                        {
                            var node = b.Nodes[0];
                            node.X = b.X;
                            node.Y = y;
                            node.Size = new Size(NodeWidth, NodeHeight);
                        }

                        startLeft += b.Width + spacing;
                    }

                    blocksByLevel[level] = blocks;
                }

                // Post-process: align parent blocks above their children's centers (bottom-up)
                var levelKeysDesc = sorted.Select(k => k.Key).OrderByDescending(k => k).ToList();
                foreach (var lvl in levelKeysDesc)
                {
                    if (!blocksByLevel.ContainsKey(lvl)) continue;
                    var blocks = blocksByLevel[lvl];
                    foreach (var b in blocks)
                    {
                        // gather children centers for all nodes in this block
                        var childCenters = new System.Collections.Generic.List<int>();
                        foreach (var node in b.Nodes)
                        {
                            var children = graphService.GetChildren(node.Name).ToList();
                            foreach (var cn in children)
                            {
                                if (!nodeByName.ContainsKey(cn)) continue;
                                var ch = nodeByName[cn];
                                childCenters.Add(ch.X + ch.Size.Width / 2);
                            }
                        }

                        if (childCenters.Count == 0) continue;

                        int avgCenter = (int)childCenters.Average();
                        int newLeft = avgCenter - b.Width / 2;
                        // move block and update internal node positions
                        b.X = newLeft;
                        if (b.Nodes.Count == 2)
                        {
                            var left = b.Nodes[0];
                            var right = b.Nodes[1];
                            left.X = b.X;
                            right.X = b.X + NodeWidth + CoupleSpacing;
                        }
                        else
                        {
                            var node = b.Nodes[0];
                            node.X = b.X;
                        }
                    }
                }

                // Resolve overlaps per level at block granularity
                foreach (var kvp2 in sorted)
                {
                    int lvl = kvp2.Key;
                    if (!blocksByLevel.ContainsKey(lvl)) continue;
                    var levelBlocks = blocksByLevel[lvl].OrderBy(b => b.X).ToList();
                    int prevRight = int.MinValue;
                    foreach (var b in levelBlocks)
                    {
                        int minLeft = (prevRight == int.MinValue) ? b.X : Math.Max(b.X, prevRight + spacing);
                        int shift = minLeft - b.X;
                        if (shift > 0)
                        {
                            b.X += shift;
                        }

                        // update node positions
                        if (b.Nodes.Count == 2)
                        {
                            var left = b.Nodes[0];
                            var right = b.Nodes[1];
                            left.X = b.X;
                            right.X = b.X + NodeWidth + CoupleSpacing;
                        }
                        else
                        {
                            var node = b.Nodes[0];
                            node.X = b.X;
                        }

                        prevRight = Math.Max(prevRight, b.X + b.Width);
                    }
                }
            }
        

        private void OrganizeLevelPositions(List<VisualNode> levelNodes, int startX, int y, int spacing, int level)
        {
            int currentX = startX;
            var processed = new System.Collections.Generic.HashSet<string>();
            var couples = levelNodes.Where(n => _coupleRelationships.ContainsKey(n.Name) && !processed.Contains(n.Name)).ToList();

            couples = couples.OrderBy(n =>
            {
                string sideA = _familySide.ContainsKey(n.Name) ? _familySide[n.Name] : "none";
                string partnerName = _coupleRelationships.ContainsKey(n.Name) ? _coupleRelationships[n.Name] : null;
                string sideB = (partnerName != null && _familySide.ContainsKey(partnerName)) ? _familySide[partnerName] : "none";
                // If either is paternal prefer left (0), if either is materno prefer right (2), otherwise middle (1)
                if (sideA == "paterno" || sideB == "paterno") return 0;
                if (sideA == "materno" || sideB == "materno") return 2;
                return 1;
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
                    // Decide left/right by family side when available: paternal left, maternal right
                    string sideNode = _familySide.ContainsKey(node.Name) ? _familySide[node.Name] : "none";
                    string sidePartner = _familySide.ContainsKey(partner.Name) ? _familySide[partner.Name] : "none";
                    if (sidePartner == "paterno" && sideNode != "paterno")
                    {
                        leftNode = partner;
                        rightNode = node;
                    }
                    else if (sideNode == "paterno" && sidePartner != "paterno")
                    {
                        leftNode = node;
                        rightNode = partner;
                    }
                    else if (sidePartner == "materno" && sideNode != "materno")
                    {
                        leftNode = node;
                        rightNode = partner;
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
            // Prevent spacing from growing too large with depth which can push the whole tree to the right.
            int mult = Math.Max(1, (int)Math.Log(treeDepth + 1, 2));
            mult = Math.Min(mult, 2); // cap multiplier to avoid excessive spacing on deep trees
            int dynamic = baseSpacing * mult;
            return Math.Max(dynamic, MinimumSpacing);
        }

        private void DrawConnections(Graphics g, List<VisualNode> nodes, GraphService graphService)
        {
            using (var pen = new Pen(Color.Black, 2))
            using (var couplePen = new Pen(Color.DarkBlue, 2))
            {
                DrawCoupleConnections(g, couplePen, nodes);
                DrawParentChildConnections(g, pen, nodes, graphService);
            }
        }

        private void DrawCoupleConnections(Graphics g, Pen pen, List<VisualNode> nodes)
        {
            var drawn = new System.Collections.Generic.HashSet<string>();
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

        private void DrawParentChildConnections(Graphics g, Pen pen, List<VisualNode> nodes, GraphService graphService)
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
                        parentPoint = new Point(center, n.Y + NodeHeight / 2);
                    }
                    else parentPoint = new Point(n.X + NodeWidth / 2, n.Y + NodeHeight / 2);
                }
                else parentPoint = new Point(n.X + NodeWidth / 2, n.Y + NodeHeight / 2);

                foreach (var cName in children)
                {
                    var child = nodes.FirstOrDefault(x => x.Name == cName);
                    if (child == null) continue;

                    int childCenterX = child.X + NodeWidth / 2;
                    int childTopY = child.Y;
                    int parentBottomY = parentPoint.Y;

                    // compute a middle Y between parent bottom and child top to route connectors smoothly
                    int midY = parentBottomY + (childTopY - parentBottomY) / 2;

                    // vertical from parent bottom to midY
                    g.DrawLine(pen, new Point(parentPoint.X, parentBottomY), new Point(parentPoint.X, midY));
                    // horizontal from parent X to child center X at midY
                    g.DrawLine(pen, new Point(parentPoint.X, midY), new Point(childCenterX, midY));
                    // vertical down to child top
                    g.DrawLine(pen, new Point(childCenterX, midY), new Point(childCenterX, childTopY));
                }
            }
        }

        private void DrawNodes(Graphics g, List<VisualNode> nodes, GraphService graphService)
        {
            foreach (var n in nodes)
            {
                var person = graphService.GetPersonData(n.Name);
                DrawNode(g, n, person);
            }
        }

        private void DetectFamilySides(List<VisualNode> nodes, GraphService graphService)
        {
            // Simple heuristic: detect parent pairs (couples) from shared children
            // and register couple relationships. Family side ('paterno'/'materno')
            // is left as 'none' unless further rules are added.
            foreach (var n in nodes)
            {
                var parents = graphService.GetParents(n.Name)?.ToList();
                if (parents == null || parents.Count < 2) continue;
                // take first two parents as the couple
                var a = parents[0];
                var b = parents[1];
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) continue;
                if (!_coupleRelationships.ContainsKey(a)) _coupleRelationships[a] = b;
                if (!_coupleRelationships.ContainsKey(b)) _coupleRelationships[b] = a;
            }
        }

        private void CalculateNodePositions(List<VisualNode> nodes, System.Collections.Generic.Dictionary<int, List<VisualNode>> levels, GraphService graphService)
        {
            if (levels == null || levels.Count == 0) return;
            int minLevel = levels.Keys.Min();
            int maxLevel = levels.Keys.Max();
            int treeDepth = Math.Max(1, maxLevel - minLevel + 1);

            int spacing = CalculateDynamicSpacing(treeDepth);
            int startX = 20;
            int startY = 20;

            CalculateFinalPositions(nodes, levels, startX, startY, spacing, graphService);
        }

        private void DrawNode(Graphics g, VisualNode node, Person person)
        {
            var rect = new Rectangle(node.X, node.Y, node.Size.Width, node.Size.Height);

            using (var brush = new SolidBrush(node.Color))
            {
                g.FillEllipse(brush, rect);
                g.DrawEllipse(Pens.Black, rect);
            }

            DrawCircularImage(g, node, person);
            DrawNameBelowNode(g, node, person?.Name ?? node.Name);
        }

        private void DrawCircularImage(Graphics g, VisualNode node, Person person)
        {
            if (person == null || string.IsNullOrEmpty(person.PhotoPath))
            {
                DrawCircularPlaceholder(g, node);
                return;
            }

            try
            {
                Image image = null;

                if (_imageCache.ContainsKey(person.PhotoPath))
                {
                    image = _imageCache[person.PhotoPath];
                }
                else
                {
                    image = Image.FromFile(person.PhotoPath);
                    _imageCache[person.PhotoPath] = image;
                }

                var circleDiameter = Math.Min(node.Size.Width - 10, node.Size.Height - 30);
                var circleRect = new Rectangle(
                    node.X + (node.Size.Width - circleDiameter) / 2,
                    node.Y + 5,
                    circleDiameter,
                    circleDiameter
                );

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(circleRect);
                    var state = g.Save();
                    g.SetClip(path);

                    var prevInterpolation = g.InterpolationMode;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    if (image.Width > 0 && image.Height > 0)
                    {
                        var imageSize = CalculateImageSize(image.Size, circleRect.Size);
                        var imageX = circleRect.X + (circleRect.Width - imageSize.Width) / 2;
                        var imageY = circleRect.Y + (circleRect.Height - imageSize.Height) / 2;

                        g.DrawImage(image, imageX, imageY, imageSize.Width, imageSize.Height);
                    }

                    g.InterpolationMode = prevInterpolation;
                    g.Restore(state);
                }

                g.DrawEllipse(Pens.DarkGray, circleRect);
            }
            catch (Exception ex)
            {
                DrawCircularPlaceholder(g, node);
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");

                if (_imageCache.ContainsKey(person.PhotoPath))
                    _imageCache.Remove(person.PhotoPath);
            }
        }

        private void DrawCircularPlaceholder(Graphics g, VisualNode node)
        {
            var circleDiameter = Math.Min(node.Size.Width - 10, node.Size.Height - 30);
            var circleRect = new Rectangle(
                node.X + (node.Size.Width - circleDiameter) / 2,
                node.Y + 5,
                circleDiameter,
                circleDiameter
            );

            using (var brush = new SolidBrush(Color.LightGray))
            {
                g.FillEllipse(brush, circleRect);
            }
            g.DrawEllipse(Pens.DarkGray, circleRect);

            using (var font = new Font("Arial", circleDiameter / 3, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.DarkGray))
            {
                var personIcon = "👤";
                var size = g.MeasureString(personIcon, font);
                var x = circleRect.X + (circleRect.Width - size.Width) / 2;
                var y = circleRect.Y + (circleRect.Height - size.Height) / 2;
                g.DrawString(personIcon, font, brush, x, y);
            }
        }

        private Size CalculateImageSize(Size originalSize, Size maxSize)
        {
            if (originalSize.Width == 0 || originalSize.Height == 0)
                return maxSize;

            double ratioX = (double)maxSize.Width / originalSize.Width;
            double ratioY = (double)maxSize.Height / originalSize.Height;
            double ratio = Math.Min(ratioX, ratioY);

            return new Size(
                Math.Max(1, (int)(originalSize.Width * ratio)),
                Math.Max(1, (int)(originalSize.Height * ratio))
            );
        }

        private void DrawNameBelowNode(Graphics g, VisualNode node, string name)
        {
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            using (var backgroundBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
            {
                var nameSize = g.MeasureString(name, font);

                float nameX = node.X + (node.Size.Width - nameSize.Width) / 2;
                float nameY = node.Y + node.Size.Height + 2;

                var backgroundRect = new RectangleF(
                    nameX - 2,
                    nameY - 1,
                    nameSize.Width + 4,
                    nameSize.Height + 2
                );

                g.FillRectangle(backgroundBrush, backgroundRect);
                g.DrawRectangle(Pens.LightGray, backgroundRect.X, backgroundRect.Y, backgroundRect.Width, backgroundRect.Height);

                g.DrawString(name, font, brush, nameX, nameY);
            }
        }

        public void ClearImageCache()
        {
            foreach (var image in _imageCache.Values)
            {
                image?.Dispose();
            }
            _imageCache.Clear();
        }
    }
}