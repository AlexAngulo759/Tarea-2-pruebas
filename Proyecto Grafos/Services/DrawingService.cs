using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Models;
using SysCol = System.Collections.Generic;

namespace Proyecto_Grafos.Services
{
    public class DrawingService
    {
        private const int NodeWidth = 120;
        private const int NodeHeight = 140;
        private const int CoupleSpacing = 60;

        private readonly SysCol.Dictionary<string, string> _coupleRelationships = new SysCol.Dictionary<string, string>();
        private readonly SysCol.Dictionary<string, Image> _imageCache = new SysCol.Dictionary<string, Image>();

        public void DrawTree(Graphics g, List<VisualNode> nodes, GraphService graphService)
        {
            if (nodes == null || nodes.Count == 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            _coupleRelationships.Clear();
            DetectCouples(nodes, graphService);

            DrawConnections(g, nodes, graphService);
            DrawNodes(g, nodes, graphService);
        }

        private void DetectCouples(List<VisualNode> nodes, GraphService graphService)
        {
            foreach (var n in nodes)
            {
                var parents = graphService.GetParents(n.Name);
                if (parents.Count < 2) continue;
                var a = parents.Get(0);
                var b = parents.Get(1);
                if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) continue;
                if (!_coupleRelationships.ContainsKey(a)) _coupleRelationships.Add(a, b);
                if (!_coupleRelationships.ContainsKey(b)) _coupleRelationships.Add(b, a);
            }
        }

        private void DrawConnections(Graphics g, List<VisualNode> nodes, GraphService graphService)
        {
            using (var pen = new Pen(Color.Black, 2))
            using (var couplePen = new Pen(Color.White, 2))
            {
                DrawCoupleConnections(g, couplePen, nodes);
                DrawParentChildConnections(g, pen, nodes, graphService);
            }
        }

        private void DrawCoupleConnections(Graphics g, Pen pen, List<VisualNode> nodes)
        {
            var drawn = new SysCol.HashSet<string>();
            foreach (var n in nodes)
            {
                if (!_coupleRelationships.ContainsKey(n.Name)) continue;
                var pName = _coupleRelationships[n.Name];
                var p = nodes.Find(x => x.Name == pName);
                if (p == null) continue;

                var key = string.Compare(n.Name, pName, StringComparison.Ordinal) < 0
                    ? $"{n.Name}-{pName}" : $"{pName}-{n.Name}";

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
                var children = graphService.GetChildren(n.Name);
                if (children.Count == 0) continue;

                Point parentPoint;
                if (_coupleRelationships.ContainsKey(n.Name))
                {
                    var partnerName = _coupleRelationships[n.Name];
                    var partner = nodes.Find(x => x.Name == partnerName);
                    parentPoint = partner != null
                        ? new Point((n.X + partner.X + NodeWidth) / 2, n.Y + NodeHeight / 2)
                        : new Point(n.X + NodeWidth / 2, n.Y + NodeHeight / 2);
                }
                else
                {
                    parentPoint = new Point(n.X + NodeWidth / 2, n.Y + NodeHeight / 2);
                }

                for (int i = 0; i < children.Count; i++)
                {
                    var childName = children.Get(i);
                    var child = nodes.Find(x => x.Name == childName);
                    if (child == null) continue;

                    int childCenterX = child.X + NodeWidth / 2;
                    int childTopY = child.Y;
                    int midY = parentPoint.Y + (childTopY - parentPoint.Y) / 2;

                    g.DrawLine(pen, parentPoint, new Point(parentPoint.X, midY));
                    g.DrawLine(pen, new Point(parentPoint.X, midY), new Point(childCenterX, midY));
                    g.DrawLine(pen, new Point(childCenterX, midY), new Point(childCenterX, childTopY));
                }
            }
        }

        private void DrawNodes(Graphics g, List<VisualNode> nodes, GraphService graphService)
        {
            foreach (var n in nodes)
            {
                var person = graphService.GetPerson(n.Name); 
                DrawNode(g, n, person);
            }
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
                Image image;
                if (_imageCache.ContainsKey(person.PhotoPath))
                    image = _imageCache[person.PhotoPath];
                else
                {
                    image = Image.FromFile(person.PhotoPath);
                    _imageCache[person.PhotoPath] = image;
                }

                var circleDiameter = Math.Min(node.Size.Width - 10, node.Size.Height - 30);
                var circleRect = new Rectangle(node.X + (node.Size.Width - circleDiameter) / 2,
                                               node.Y + 5,
                                               circleDiameter,
                                               circleDiameter);

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(circleRect);
                    var state = g.Save();
                    g.SetClip(path);
                    var prev = g.InterpolationMode;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    if (image.Width > 0 && image.Height > 0)
                    {
                        var imgSize = CalculateImageSize(image.Size, circleRect.Size);
                        var imgX = circleRect.X + (circleRect.Width - imgSize.Width) / 2;
                        var imgY = circleRect.Y + (circleRect.Height - imgSize.Height) / 2;
                        g.DrawImage(image, imgX, imgY, imgSize.Width, imgSize.Height);
                    }

                    g.InterpolationMode = prev;
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
            var d = Math.Min(node.Size.Width - 10, node.Size.Height - 30);
            var rect = new Rectangle(node.X + (node.Size.Width - d) / 2, node.Y + 5, d, d);
            using (var brush = new SolidBrush(Color.LightGray))
                g.FillEllipse(brush, rect);
            g.DrawEllipse(Pens.DarkGray, rect);

            using (var font = new Font("Arial", d / 3, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.DarkGray))
            {
                var icon = "👤";
                var size = g.MeasureString(icon, font);
                var x = rect.X + (rect.Width - size.Width) / 2;
                var y = rect.Y + (rect.Height - size.Height) / 2;
                g.DrawString(icon, font, brush, x, y);
            }
        }

        private Size CalculateImageSize(Size original, Size max)
        {
            if (original.Width == 0 || original.Height == 0)
                return max;
            double rx = (double)max.Width / original.Width;
            double ry = (double)max.Height / original.Height;
            double r = Math.Min(rx, ry);
            return new Size(Math.Max(1, (int)(original.Width * r)),
                            Math.Max(1, (int)(original.Height * r)));
        }

        private void DrawNameBelowNode(Graphics g, VisualNode node, string name)
        {
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            using (var bg = new SolidBrush(Color.FromArgb(200, ColorTranslator.FromHtml("#404040"))))
            {
                var size = g.MeasureString(name, font);
                float x = node.X + (node.Size.Width - size.Width) / 2;
                float y = node.Y + node.Size.Height + 2;
                var bgRect = new RectangleF(x - 2, y - 1, size.Width + 4, size.Height + 2);
                g.FillRectangle(bg, bgRect);
                g.DrawRectangle(Pens.LightGray, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
                g.DrawString(name, font, brush, x, y);
            }
        }

        public void ClearImageCache()
        {
            foreach (var img in _imageCache.Values)
                img?.Dispose();
            _imageCache.Clear();
        }
    }
}