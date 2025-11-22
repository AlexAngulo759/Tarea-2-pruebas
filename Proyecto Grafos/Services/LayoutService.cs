using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Proyecto_Grafos.Core.Models;
using SysCol = System.Collections.Generic;

namespace Proyecto_Grafos.Services
{
    public class LayoutService
    {
        private const int BaseHorizontalSpacing = 120;
        private const int VerticalSpacing = 120;
        private const int NodeWidth = 120;
        private const int NodeHeight = 140;
        private const int MinimumSpacing = 80;
        private const int CoupleSpacing = 60;
        public List<VisualNode> CalculateLayout(List<string> people, GraphService graphService) =>
            BuildLayout(people, graphService);

        public List<VisualNode> BuildLayout(List<string> people, GraphService graphService)
        {
            var nodes = people.Select(p => new VisualNode(p, 0, 0)).ToList();
            var levels = CalculateLevels(nodes, graphService);
            DetectCouples(nodes, graphService, out var couples);
            CalculateNodePositions(nodes, levels, graphService, couples);
            return nodes;
        }

        private class Block
        {
            public List<VisualNode> Nodes { get; set; }
            public int Width { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        private SysCol.Dictionary<int, List<VisualNode>> CalculateLevels(List<VisualNode> nodes, GraphService graphService)
        {
            var depth = new SysCol.Dictionary<string, int>();
            var processed = new SysCol.HashSet<string>();
            foreach (var n in nodes) depth[n.Name] = 0;

            var roots = nodes.Where(n => graphService.GetParents(n.Name).Count == 0).ToList();
            foreach (var r in roots)
                AssignLevelsRecursively(r, nodes, graphService, depth, processed, 0);

            foreach (var n in nodes.Where(n => !processed.Contains(n.Name)))
                if (graphService.GetParents(n.Name).Count > 0)
                    depth[n.Name] = 1;

            var levels = new SysCol.Dictionary<int, List<VisualNode>>();
            foreach (var n in nodes)
            {
                int lvl = depth[n.Name];
                if (!levels.ContainsKey(lvl)) levels[lvl] = new List<VisualNode>();
                levels[lvl].Add(n);
            }
            return levels;
        }

        private void AssignLevelsRecursively(VisualNode node,
                                             List<VisualNode> nodes,
                                             GraphService graphService,
                                             SysCol.Dictionary<string, int> depth,
                                             SysCol.HashSet<string> processed,
                                             int currentLevel)
        {
            if (processed.Contains(node.Name)) return;
            depth[node.Name] = currentLevel;
            processed.Add(node.Name);

            var children = graphService.GetChildren(node.Name);
            for (int i = 0; i < children.Count; i++)
            {
                var c = nodes.FirstOrDefault(n => n.Name == children.Get(i));
                if (c != null) AssignLevelsRecursively(c, nodes, graphService, depth, processed, currentLevel + 1);
            }

            var parents = graphService.GetParents(node.Name);
            for (int i = 0; i < parents.Count; i++)
            {
                var p = nodes.FirstOrDefault(n => n.Name == parents.Get(i));
                if (p != null) AssignLevelsRecursively(p, nodes, graphService, depth, processed, currentLevel - 1);
            }
        }

        private void DetectCouples(List<VisualNode> nodes, GraphService graphService, out SysCol.Dictionary<string, string> couples)
        {
            couples = new SysCol.Dictionary<string, string>();
            foreach (var n in nodes)
            {
                var parents = graphService.GetParents(n.Name);
                if (parents.Count < 2) continue;
                var a = parents.Get(0);
                var b = parents.Get(1);
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) continue;
                if (!couples.ContainsKey(a)) couples.Add(a, b);
                if (!couples.ContainsKey(b)) couples.Add(b, a);
            }
        }

        private void CalculateNodePositions(List<VisualNode> nodes,
                                            SysCol.Dictionary<int, List<VisualNode>> levels,
                                            GraphService graphService,
                                            SysCol.Dictionary<string, string> couples)
        {
            if (levels.Count == 0) return;

            int minLevel = levels.Keys.Min();
            int maxLevel = levels.Keys.Max();
            int treeDepth = maxLevel - minLevel + 1;
            int spacing = CalculateDynamicSpacing(treeDepth);
            int startX = 20;
            int startY = 20;

            var sorted = levels.OrderBy(kvp => kvp.Key).ToList();
            var widths = new SysCol.Dictionary<int, int>();
            foreach (var kvp in sorted)
            {
                int coupleCount = kvp.Value.Count(n => couples.ContainsKey(n.Name) && string.Compare(n.Name, couples[n.Name]) < 0);
                int singles = kvp.Value.Count(n => !couples.ContainsKey(n.Name));
                widths[kvp.Key] = (coupleCount * (NodeWidth * 2 + CoupleSpacing)) + (singles * (NodeWidth + spacing));
            }
            int maxWidth = widths.Values.Max();

            var nodeByName = nodes.ToDictionary(n => n.Name);
            var blocksByLevel = new SysCol.Dictionary<int, List<Block>>();

            foreach (var kvp in sorted)
            {
                int level = kvp.Key;
                int y = startY + level * (NodeHeight + VerticalSpacing);
                var levelNodes = kvp.Value;

                var processed = new SysCol.HashSet<string>();
                var blocks = new List<Block>();

                foreach (var n in levelNodes)
                {
                    if (processed.Contains(n.Name)) continue;

                    if (couples.ContainsKey(n.Name))
                    {
                        var partnerName = couples[n.Name];
                        var partner = levelNodes.FirstOrDefault(x => x.Name == partnerName);
                        if (partner != null && !processed.Contains(partnerName))
                        {
                            VisualNode leftNode, rightNode;
                            if (string.Compare(n.Name, partner.Name) <= 0)
                            {
                                leftNode = n; rightNode = partner;
                            }
                            else
                            {
                                leftNode = partner; rightNode = n;
                            }
                            blocks.Add(new Block
                            {
                                Nodes = new List<VisualNode> { leftNode, rightNode },
                                Width = NodeWidth * 2 + CoupleSpacing,
                                Y = y
                            });
                            processed.Add(leftNode.Name);
                            processed.Add(rightNode.Name);
                            continue;
                        }
                    }

                    blocks.Add(new Block
                    {
                        Nodes = new List<VisualNode> { n },
                        Width = NodeWidth,
                        Y = y
                    });
                    processed.Add(n.Name);
                }

                int totalWidth = blocks.Sum(b => b.Width) + (blocks.Count - 1) * spacing;
                int startLeft = startX + (maxWidth - totalWidth) / 2;

                foreach (var b in blocks)
                {
                    b.X = startLeft;
                    if (b.Nodes.Count == 2)
                    {
                        b.Nodes[0].X = b.X;
                        b.Nodes[1].X = b.X + NodeWidth + CoupleSpacing;
                    }
                    else
                    {
                        b.Nodes[0].X = b.X;
                    }
                    foreach (var nn in b.Nodes)
                    {
                        nn.Y = y;
                        nn.Size = new Size(NodeWidth, NodeHeight);
                    }
                    startLeft += b.Width + spacing;
                }

                blocksByLevel[level] = blocks;
            }

            var descending = sorted.Select(k => k.Key).OrderByDescending(k => k).ToList();
            foreach (var lvl in descending)
            {
                var blocks = blocksByLevel[lvl];
                foreach (var b in blocks)
                {
                    var centers = new List<int>();
                    foreach (var node in b.Nodes)
                    {
                        var children = graphService.GetChildren(node.Name);
                        for (int i = 0; i < children.Count; i++)
                        {
                            var cname = children.Get(i);
                            if (!nodeByName.ContainsKey(cname)) continue;
                            centers.Add(nodeByName[cname].X + NodeWidth / 2);
                        }
                    }
                    if (centers.Count == 0) continue;
                    int avg = (int)centers.Average();
                    int newLeft = avg - b.Width / 2;
                    int shift = newLeft - b.X;
                    if (shift != 0)
                    {
                        foreach (var node in b.Nodes) node.X += shift;
                        b.X = newLeft;
                    }
                }
            }

            foreach (var lvl in sorted.Select(k => k.Key))
            {
                var blocks = blocksByLevel[lvl].OrderBy(b => b.X).ToList();
                int prevRight = int.MinValue;
                foreach (var b in blocks)
                {
                    int desiredLeft = prevRight == int.MinValue ? b.X : prevRight + spacing;
                    int shift = desiredLeft - b.X;
                    if (shift > 0)
                    {
                        foreach (var node in b.Nodes) node.X += shift;
                        b.X += shift;
                    }
                    prevRight = b.X + b.Width;
                }
            }
        }

        private int CalculateDynamicSpacing(int treeDepth)
        {
            int baseSpacing = BaseHorizontalSpacing;
            int mult = System.Math.Max(1, (int)System.Math.Log(treeDepth + 1, 2));
            mult = System.Math.Min(mult, 2);
            int dynamic = baseSpacing * mult;
            return System.Math.Max(dynamic, MinimumSpacing);
        }
    }
}