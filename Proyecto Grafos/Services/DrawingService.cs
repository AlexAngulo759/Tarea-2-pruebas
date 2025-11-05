using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Proyecto_Grafos.Services
{
    public class DrawingService
    {
        public void DrawTree(Graphics graphics, List<Models.VisualNode> nodes, GraphService graphService)
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawRelationships(graphics, nodes, graphService);

            DrawNodes(graphics, nodes);
        }

        private void DrawRelationships(Graphics graphics, List<Models.VisualNode> nodes, GraphService graphService)
        {
            using (Pen relationPen = new Pen(Color.Black, 2))
            {
                foreach (var node in nodes)
                {
                    var children = graphService.GetChildren(node.Name);

                    for (int i = 0; i < children.Count; i++)
                    {
                        string childName = children.Get(i);
                        var childNode = nodes.FirstOrDefault(n => n.Name == childName);

                        if (childNode != null)
                        {
                            Point start = node.GetCenter();
                            Point end = childNode.GetCenter();
                            graphics.DrawLine(relationPen, start.X, start.Y, end.X, end.Y);
                        }
                    }
                }
            }
        }

        private void DrawNodes(Graphics graphics, List<Models.VisualNode> nodes)
        {
            foreach (var node in nodes)
            {
                graphics.FillEllipse(new SolidBrush(node.Color),
                    node.X, node.Y, node.Size.Width, node.Size.Height);
                graphics.DrawEllipse(Pens.Black, node.X, node.Y, node.Size.Width, node.Size.Height);

                using (Font font = new Font("Arial", 8))
                {
                    SizeF textSize = graphics.MeasureString(node.Name, font);
                    float textX = node.X + node.Size.Width / 2 - textSize.Width / 2;
                    float textY = node.Y + node.Size.Height / 2 - textSize.Height / 2;
                    graphics.DrawString(node.Name, font, Brushes.Black, textX, textY);
                }
            }
        }
    }
}