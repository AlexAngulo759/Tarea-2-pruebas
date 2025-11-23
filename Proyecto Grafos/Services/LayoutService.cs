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
            public List<VisualNode> Nodes { get; set; } = new List<VisualNode>();
            public int Width { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int TargetX { get; set; }
        }

        private SysCol.Dictionary<int, List<VisualNode>> CalculateLevels(List<VisualNode> nodes, GraphService graphService)
        {
            var depth = new SysCol.Dictionary<string, int>();
            var processed = new SysCol.HashSet<string>();
            foreach (var n in nodes) depth[n.Name] = 0;

            var roots = nodes.Where(n => graphService.GetParents(n.Name).Count == 0).ToList();
            foreach (var r in roots)
                AssignLevelsRecursively(r, nodes, graphService, depth, processed, 0);

            foreach (var n in nodes.Where(n => !processed.Contains(n.Name) && graphService.GetParents(n.Name).Count > 0))
                depth[n.Name] = 1;

            var levels = new SysCol.Dictionary<int, List<VisualNode>>();
            foreach (var n in nodes)
            {
                int lvl = depth.ContainsKey(n.Name) ? depth[n.Name] : 0;
                if (!levels.ContainsKey(lvl)) levels[lvl] = new List<VisualNode>();
                levels[lvl].Add(n);
            }
            return levels;
        }

        private void AssignLevelsRecursively(VisualNode node, List<VisualNode> nodes, GraphService graphService, SysCol.Dictionary<string, int> depth, SysCol.HashSet<string> processed, int currentLevel)
        {
            if (node == null || processed.Contains(node.Name)) return;
            depth[node.Name] = currentLevel;
            processed.Add(node.Name);

            var children = graphService.GetChildren(node.Name);
            for (int i = 0; i < children.Count; i++)
            {
                var c = nodes.FirstOrDefault(n => n.Name == children.Get(i));
                AssignLevelsRecursively(c, nodes, graphService, depth, processed, currentLevel + 1);
            }

            var parents = graphService.GetParents(node.Name);
            for (int i = 0; i < parents.Count; i++)
            {
                var p = nodes.FirstOrDefault(n => n.Name == parents.Get(i));
                AssignLevelsRecursively(p, nodes, graphService, depth, processed, currentLevel - 1);
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

        private void CalculateNodePositions(List<VisualNode> nodes, SysCol.Dictionary<int, List<VisualNode>> levels, GraphService graphService, SysCol.Dictionary<string, string> couples)
        {
            if (levels.Count == 0) return;

            int spacing = CalculateDynamicSpacing(levels.Keys.Max() - levels.Keys.Min() + 1);
            int startX = 20;
            int startY = 20;
            var nodeByName = nodes.ToDictionary(n => n.Name);

            var allBlocksByLevel = new SysCol.Dictionary<int, List<Block>>();
            int maxWidth = 0;
            var sortedLevels = levels.Keys.OrderByDescending(k => k).ToList();
            SysCol.Dictionary<string, VisualNode> positionedNodes = new SysCol.Dictionary<string, VisualNode>();

            foreach (var level in sortedLevels)
            {
                var levelNodes = levels[level];
                var blocks = CreateFamilyBlocks(levelNodes, graphService, couples, spacing);
                if (allBlocksByLevel.Count > 0) 
                {
                    blocks = blocks.OrderBy(b => GetBlockChildrenCenterX(b, graphService, positionedNodes)).ToList();
                }

                allBlocksByLevel[level] = blocks;
                int currentWidth = blocks.Sum(b => b.Width) + (blocks.Count > 1 ? (blocks.Count - 1) * spacing : 0);
                if (currentWidth > maxWidth)
                {
                    maxWidth = currentWidth;
                }
                int totalWidth = blocks.Sum(b => b.Width) + (blocks.Count > 1 ? (blocks.Count - 1) * spacing : 0);
                int currentX = startX + (maxWidth - totalWidth) / 2;
                int y = startY + level * (NodeHeight + VerticalSpacing);

                foreach (var block in blocks)
                {
                    PositionBlock(block, currentX, y, couples, spacing, nodeByName);
                    currentX += block.Width + spacing;
                    foreach (var node in block.Nodes)
                    {
                        if (!positionedNodes.ContainsKey(node.Name))
                        {
                            positionedNodes.Add(node.Name, node);
                        }
                    }
                }
            }
            foreach (var level in levels.Keys.OrderBy(k => k))
            {
                var blocks = allBlocksByLevel[level];
                int totalWidth = blocks.Sum(b => b.Width) + (blocks.Count > 1 ? (blocks.Count - 1) * spacing : 0);
                int currentX = startX + (maxWidth - totalWidth) / 2;
                int y = startY + level * (NodeHeight + VerticalSpacing);

                foreach (var block in blocks)
                {
                    PositionBlock(block, currentX, y, couples, spacing, nodeByName);
                    currentX += block.Width + spacing;
                }
            }

            AdjustLayoutForCentering(allBlocksByLevel, graphService, nodeByName, spacing, couples);
        }

        private int GetBlockChildrenCenterX(Block block, GraphService graphService, SysCol.Dictionary<string, VisualNode> positionedNodes)
        {
            var childrenXPositions = new List<int>();
            foreach (var node in block.Nodes)
            {
                var children = graphService.GetChildren(node.Name);
                for (int i = 0; i < children.Count; i++)
                {
                    var childName = children.Get(i);
                    if (positionedNodes.ContainsKey(childName))
                    {
                        childrenXPositions.Add(positionedNodes[childName].X);
                    }
                }
            }

            if (childrenXPositions.Any())
            {
                return (int)childrenXPositions.Average();
            }
            return block.X;
        }

        private void PositionBlock(Block block, int x, int y, SysCol.Dictionary<string, string> couples, int spacing, SysCol.Dictionary<string, VisualNode> nodeByName)
        {
            block.X = x;
            block.Y = y;
            int innerX = x;

            for (int i = 0; i < block.Nodes.Count; i++)
            {
                var node = block.Nodes[i];
                node.X = innerX;
                node.Y = y;
                node.Size = new Size(NodeWidth, NodeHeight);

                innerX += NodeWidth;
                if (i < block.Nodes.Count - 1)
                {
                    var nextNode = block.Nodes[i + 1];
                    bool areCouple = couples.ContainsKey(node.Name) && couples[node.Name] == nextNode.Name;
                    innerX += areCouple ? CoupleSpacing : spacing;
                }
            }
        }

        private void AdjustLayoutForCentering(SysCol.Dictionary<int, List<Block>> allBlocksByLevel, GraphService graphService, SysCol.Dictionary<string, VisualNode> nodeByName, int spacing, SysCol.Dictionary<string, string> couples)
        {
            var sortedLevels = allBlocksByLevel.Keys.OrderByDescending(k => k).ToList();

            for (int i = 0; i < sortedLevels.Count - 1; i++)
            {
                var childLevelKey = sortedLevels[i];
                var parentLevelKey = sortedLevels[i + 1];

                if (!allBlocksByLevel.ContainsKey(parentLevelKey)) continue;

                var parentBlocks = allBlocksByLevel[parentLevelKey];
                var childBlocks = allBlocksByLevel[childLevelKey];
                var orderedParentBlocks = parentBlocks.OrderBy(p =>
                {
                    var childrenOfBlock = GetChildrenOfBlock(p, graphService, nodeByName);
                    if (!childrenOfBlock.Any()) return p.X;
                    var relevantChildBlocks = childBlocks.Where(c => c.Nodes.Any(n => childrenOfBlock.Contains(n)));
                    if (!relevantChildBlocks.Any()) return p.X;
                    return relevantChildBlocks.Min(c => c.X);
                }).ToList();

                for (int j = 0; j < orderedParentBlocks.Count; j++)
                {
                    var parentBlock = orderedParentBlocks[j];
                    var childrenOfBlock = GetChildrenOfBlock(parentBlock, graphService, nodeByName);

                    if (childrenOfBlock.Any())
                    {
                        var relevantChildBlocks = childBlocks.Where(b => b.Nodes.Any(n => childrenOfBlock.Contains(n))).Distinct().ToList();
                        if (relevantChildBlocks.Any())
                        {
                            int minChildX = relevantChildBlocks.Min(b => b.X);
                            int maxChildX = relevantChildBlocks.Max(b => b.X + b.Width);
                            int childrenCenterX = minChildX + (maxChildX - minChildX) / 2;
                            parentBlock.X = childrenCenterX - parentBlock.Width / 2;
                        }
                    }
                    if (j > 0)
                    {
                        var leftBlock = orderedParentBlocks[j - 1];
                        int desiredX = leftBlock.X + leftBlock.Width + spacing;
                        if (parentBlock.X < desiredX)
                        {
                            parentBlock.X = desiredX;
                        }
                    }
                }
                foreach (var block in orderedParentBlocks)
                {
                    PositionBlock(block, block.X, block.Y, couples, spacing, nodeByName);
                }
            }
        }

        private List<VisualNode> GetChildrenOfBlock(Block block, GraphService graphService, SysCol.Dictionary<string, VisualNode> nodeByName)
        {
            var children = new List<VisualNode>();
            foreach (var node in block.Nodes)
            {
                var childrenNames = graphService.GetChildren(node.Name);
                for (int i = 0; i < childrenNames.Count; i++)
                {
                    if (nodeByName.ContainsKey(childrenNames.Get(i)))
                    {
                        children.Add(nodeByName[childrenNames.Get(i)]);
                    }
                }
            }
            return children.Distinct().ToList();
        }

        private List<Block> CreateFamilyBlocks(List<VisualNode> levelNodes, GraphService graphService, SysCol.Dictionary<string, string> couples, int spacing)
        {
            var blocks = new List<Block>();
            var processed = new HashSet<string>();
            foreach (var node in levelNodes)
            {
                if (processed.Contains(node.Name)) continue;

                if (couples.ContainsKey(node.Name))
                {
                    var partner = levelNodes.FirstOrDefault(n => n.Name == couples[node.Name]);
                    if (partner != null && !processed.Contains(partner.Name))
                    {
                        var block = new Block();
                        var familyUnit = new List<VisualNode>();

                        var leftSpouse = (string.Compare(node.Name, partner.Name) < 0) ? node : partner;
                        var rightSpouse = (leftSpouse == node) ? partner : node;

                        var leftSiblings = GetSiblings(leftSpouse, levelNodes, graphService).Except(new[] { rightSpouse }).ToList();
                        var rightSiblings = GetSiblings(rightSpouse, levelNodes, graphService).Except(new[] { leftSpouse }).ToList();

                        familyUnit.AddRange(leftSiblings.OrderBy(n => n.Name));
                        familyUnit.Add(leftSpouse);
                        familyUnit.Add(rightSpouse);
                        familyUnit.AddRange(rightSiblings.OrderBy(n => n.Name));

                        block.Nodes.AddRange(familyUnit.Distinct());
                        blocks.Add(block);

                        familyUnit.ForEach(n => processed.Add(n.Name));
                    }
                }
            }
            foreach (var node in levelNodes)
            {
                if (processed.Contains(node.Name)) continue;

                var block = new Block();
                var siblings = GetSiblings(node, levelNodes, graphService);
                var familyUnit = new List<VisualNode> { node };
                familyUnit.AddRange(siblings);

                block.Nodes.AddRange(familyUnit.Distinct().OrderBy(n => n.Name));
                blocks.Add(block);

                familyUnit.ForEach(n => processed.Add(n.Name));
            }
            foreach (var block in blocks)
            {
                int width = 0;
                for (int i = 0; i < block.Nodes.Count; i++)
                {
                    width += NodeWidth;
                    if (i < block.Nodes.Count - 1)
                    {
                        var currentNode = block.Nodes[i];
                        var nextNode = block.Nodes[i + 1];
                        bool areCouple = couples.ContainsKey(currentNode.Name) && couples[currentNode.Name] == nextNode.Name;
                        width += areCouple ? CoupleSpacing : spacing;
                    }
                }
                block.Width = width;
            }

            return blocks;
        }

        private IEnumerable<VisualNode> GetSiblings(VisualNode node, List<VisualNode> levelNodes, GraphService graphService)
        {
            var parents = graphService.GetParents(node.Name);
            if (parents.Count == 0) return Enumerable.Empty<VisualNode>();

            var parentNames = new List<string>();
            for (int i = 0; i < parents.Count; i++)
            {
                parentNames.Add(parents.Get(i));
            }

            return levelNodes.Where(n =>
            {
                if (n == node) return false;
                var otherParents = graphService.GetParents(n.Name);
                if (otherParents.Count == 0) return false;

                var otherParentNames = new List<string>();
                for (int i = 0; i < otherParents.Count; i++)
                {
                    otherParentNames.Add(otherParents.Get(i));
                }

                return parentNames.Any(p => otherParentNames.Contains(p));
            });
        }

        private int CalculateDynamicSpacing(int treeDepth)
        {
            int baseSpacing = BaseHorizontalSpacing;
            if (treeDepth <= 0) treeDepth = 1;
            int mult = System.Math.Max(1, (int)System.Math.Log(treeDepth + 1, 2));
            mult = System.Math.Min(mult, 2);
            int dynamic = baseSpacing * mult;
            return System.Math.Max(dynamic, MinimumSpacing);
        }
    }
}