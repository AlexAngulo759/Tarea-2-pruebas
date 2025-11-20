using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace Proyecto_Grafos.Markers
{
    public class PhotoMarker : GMapMarker
    {
        private readonly Bitmap _image;
        private readonly string _initial;
        private readonly Color _backColor;
        private readonly Pen _borderPen = new Pen(Color.White, 3);
        private readonly Pen _outerPen = new Pen(Color.FromArgb(120, Color.Black), 2);

        public PhotoMarker(PointLatLng position, Bitmap image, string name)
            : base(position)
        {
            Size = new Size(64, 64);

            Offset = new Point(-Size.Width / 2, -Size.Height / 2); 
            _image = image;
            _initial = string.IsNullOrWhiteSpace(name) ? "?" : name.Trim()[0].ToString().ToUpperInvariant();
            _backColor = Color.FromArgb(90, 140, 200); 
            ToolTipMode = MarkerTooltipMode.Always;
        }

        public override void OnRender(Graphics g)
        {
            var rect = new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var shadowRect = new Rectangle(rect.X + 2, rect.Y + 4, rect.Width, rect.Height);
            using (var shadowPath = new GraphicsPath())
            {
                shadowPath.AddEllipse(shadowRect);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    g.FillPath(shadowBrush, shadowPath);
            }

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (var clip = new Region(path))
                {
                    var state = g.Save();
                    g.SetClip(clip, CombineMode.Replace);

                    if (_image != null)
                    {
                        g.DrawImage(_image, rect);
                    }
                    else
                    {
                        using (var bg = new SolidBrush(_backColor))
                            g.FillEllipse(bg, rect);
                        using (var font = new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel))
                        using (var textBrush = new SolidBrush(Color.White))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(_initial, font, textBrush, rect, sf);
                        }
                    }

                    g.Restore(state);
                }

                g.DrawEllipse(_borderPen, rect);
                g.DrawEllipse(_outerPen, rect);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _image?.Dispose();
            _borderPen.Dispose();
            _outerPen.Dispose();
        }
    }
}