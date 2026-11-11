using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace VehicleServiceCenterManagementSystem.Helpers
{
    public static class GraphicHelper
    {
        private static Image _cachedBanner;

        public static Image GetBanner(int width = 720, int height = 400)
        {
            if (_cachedBanner != null) return _cachedBanner;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string[] possiblePaths = new[]
            {
                Path.Combine(baseDir, "Resources", "dashboard_banner.png"),
                Path.Combine(projectDir, "Resources", "dashboard_banner.png"),
                Path.Combine(projectDir, "VehicleServiceCenterManagementSystem", "Resources", "dashboard_banner.png")
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                {
                    try { _cachedBanner = Image.FromFile(p); return _cachedBanner; }
                    catch { }
                }
            }

            _cachedBanner = GenerateBanner(width, height);

            foreach (var p in possiblePaths)
            {
                try
                {
                    string dir = Path.GetDirectoryName(p);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    _cachedBanner.Save(p, ImageFormat.Png);
                    break;
                }
                catch { }
            }

            return _cachedBanner;
        }

        public static Bitmap GenerateBanner(int width = 720, int height = 400)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Deep automotive navy gradient background
            Rectangle rect = new Rectangle(0, 0, width, height);
            using (var bgBrush = new LinearGradientBrush(rect, Color.FromArgb(15, 23, 42), Color.FromArgb(30, 58, 95), 45f))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Tech diagnostic grid overlay
            using (var gridPen = new Pen(Color.FromArgb(18, 56, 189, 248), 1))
            {
                for (int x = 0; x < width; x += 40) g.DrawLine(gridPen, x, 0, x, height);
                for (int y = 0; y < height; y += 40) g.DrawLine(gridPen, 0, y, width, y);
            }

            // Tech HUD diagnostic circular arcs
            using (var arcPen1 = new Pen(Color.FromArgb(50, 14, 165, 233), 3))
            {
                g.DrawArc(arcPen1, width - 280, 20, 240, 240, 135, 270);
            }
            using (var arcPen2 = new Pen(Color.FromArgb(80, 56, 189, 248), 1.5f))
            {
                arcPen2.DashStyle = DashStyle.Dash;
                g.DrawArc(arcPen2, width - 260, 40, 200, 200, 45, 270);
            }

            // Workshop lift floor & platform
            int floorY = height - 100;
            using (var floorPen = new Pen(Color.FromArgb(71, 85, 105), 3))
            {
                g.DrawLine(floorPen, 30, floorY, width - 30, floorY);
            }

            // Hydraulic service lift pillars
            using (var pillarBrush = new SolidBrush(Color.FromArgb(51, 65, 85)))
            {
                g.FillRectangle(pillarBrush, 90, floorY - 50, 24, 52);
                g.FillRectangle(pillarBrush, width - 114, floorY - 50, 24, 52);
            }
            using (var padBrush = new SolidBrush(Color.FromArgb(14, 165, 233)))
            {
                g.FillRectangle(padBrush, 75, floorY - 55, 54, 7);
                g.FillRectangle(padBrush, width - 129, floorY - 55, 54, 7);
            }

            // Car soft ground shadow
            using (var shadowPath = new GraphicsPath())
            {
                shadowPath.AddEllipse(110, floorY - 20, width - 220, 30);
                using (var shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = Color.FromArgb(180, 0, 0, 0);
                    shadowBrush.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Car profile body
            using (var carPath = new GraphicsPath())
            {
                carPath.StartFigure();
                carPath.AddBezier(120, floorY - 45, 115, floorY - 60, 125, floorY - 75, 145, floorY - 80);
                carPath.AddBezier(145, floorY - 80, 190, floorY - 87, 230, floorY - 95, 265, floorY - 115);
                carPath.AddBezier(265, floorY - 115, 305, floorY - 155, 345, floorY - 180, 385, floorY - 180);
                carPath.AddBezier(385, floorY - 180, 435, floorY - 180, 475, floorY - 170, 500, floorY - 155);
                carPath.AddBezier(500, floorY - 155, 535, floorY - 130, 560, floorY - 100, 585, floorY - 95);
                carPath.AddBezier(585, floorY - 95, 598, floorY - 97, 605, floorY - 90, 600, floorY - 75);
                carPath.AddBezier(600, floorY - 75, 595, floorY - 60, 585, floorY - 45, 565, floorY - 43);
                carPath.AddArc(480, floorY - 80, 80, 80, 0, -180);
                carPath.AddLine(480, floorY - 40, 240, floorY - 40);
                carPath.AddArc(160, floorY - 80, 80, 80, 0, -180);
                carPath.AddLine(160, floorY - 40, 120, floorY - 45);
                carPath.CloseFigure();

                using (var carBrush = new LinearGradientBrush(new Rectangle(110, floorY - 185, 500, 150),
                    Color.FromArgb(56, 189, 248), Color.FromArgb(14, 116, 144), 55f))
                {
                    g.FillPath(carBrush, carPath);
                }

                using (var carPen = new Pen(Color.FromArgb(224, 242, 254), 2f))
                {
                    g.DrawPath(carPen, carPath);
                }
            }

            // Windows / Cabin
            using (var winPath = new GraphicsPath())
            {
                winPath.StartFigure();
                winPath.AddBezier(275, floorY - 117, 310, floorY - 150, 345, floorY - 170, 385, floorY - 172);
                winPath.AddLine(385, floorY - 172, 490, floorY - 167);
                winPath.AddBezier(490, floorY - 167, 520, floorY - 140, 545, floorY - 110, 555, floorY - 100);
                winPath.AddLine(555, floorY - 100, 275, floorY - 113);
                winPath.CloseFigure();

                using (var winBrush = new LinearGradientBrush(new Rectangle(270, floorY - 175, 290, 80),
                    Color.FromArgb(15, 23, 42), Color.FromArgb(30, 41, 59), 90f))
                {
                    g.FillPath(winBrush, winPath);
                }

                using (var pillarPen = new Pen(Color.FromArgb(56, 189, 248), 2.5f))
                {
                    g.DrawLine(pillarPen, 410, floorY - 171, 420, floorY - 107);
                }
            }

            // Headlight
            using (var hlBrush = new SolidBrush(Color.FromArgb(254, 240, 138)))
            {
                g.FillPolygon(hlBrush, new[] {
                    new Point(120, floorY - 73),
                    new Point(140, floorY - 75),
                    new Point(135, floorY - 67),
                    new Point(122, floorY - 65)
                });
            }

            // Taillight
            using (var tlBrush = new SolidBrush(Color.FromArgb(239, 68, 68)))
            {
                g.FillRectangle(tlBrush, 595, floorY - 87, 8, 12);
            }

            // Wheels
            DrawWheel(g, 200, floorY - 40, 36);
            DrawWheel(g, 520, floorY - 40, 36);

            // Title & Branding Typography
            using (var titleFont = new Font("Segoe UI", 22f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.DrawString("AutoCare-Pro", titleFont, titleBrush, 35, 30);
            }

            using (var subFont = new Font("Segoe UI", 10f, FontStyle.Regular))
            using (var subBrush = new SolidBrush(Color.FromArgb(56, 189, 248)))
            {
                g.DrawString("VEHICLE SERVICE CENTER & DIAGNOSTIC SYSTEM", subFont, subBrush, 37, 70);
            }

            // Bottom status pills
            DrawPill(g, 35, height - 70, 130, 45, "STATUS", "Active Workshop", Color.FromArgb(34, 197, 94));
            DrawPill(g, 175, height - 70, 140, 45, "DIAGNOSTICS", "Real-Time Health", Color.FromArgb(56, 189, 248));
            DrawPill(g, 325, height - 70, 130, 45, "INVENTORY", "Stock Tracked", Color.FromArgb(168, 85, 247));
            DrawPill(g, 465, height - 70, 150, 45, "APPOINTMENTS", "All Bays Operational", Color.FromArgb(251, 146, 60));

            return bmp;
        }

        private static void DrawWheel(Graphics g, int cx, int cy, int radius)
        {
            using (var tireBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.FillEllipse(tireBrush, cx - radius, cy - radius, radius * 2, radius * 2);
            }
            using (var tirePen = new Pen(Color.FromArgb(71, 85, 105), 3))
            {
                g.DrawEllipse(tirePen, cx - radius, cy - radius, radius * 2, radius * 2);
            }

            int rimRadius = (int)(radius * 0.65f);
            Rectangle rimRect = new Rectangle(cx - rimRadius, cy - rimRadius, rimRadius * 2, rimRadius * 2);
            using (var rimBrush = new LinearGradientBrush(rimRect, Color.FromArgb(226, 232, 240), Color.FromArgb(100, 116, 139), 45f))
            {
                g.FillEllipse(rimBrush, rimRect);
            }

            using (var caliperBrush = new SolidBrush(Color.FromArgb(239, 68, 68)))
            {
                g.FillPie(caliperBrush, cx - rimRadius + 4, cy - rimRadius + 4, (rimRadius - 4) * 2, (rimRadius - 4) * 2, 200, 50);
            }

            using (var hubBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                g.FillEllipse(hubBrush, cx - 6, cy - 6, 12, 12);
            }

            using (var spokePen = new Pen(Color.FromArgb(248, 250, 252), 2))
            {
                for (int a = 0; a < 360; a += 60)
                {
                    double rad = a * Math.PI / 180.0;
                    int x2 = cx + (int)(Math.Cos(rad) * (rimRadius - 2));
                    int y2 = cy + (int)(Math.Sin(rad) * (rimRadius - 2));
                    g.DrawLine(spokePen, cx, cy, x2, y2);
                }
            }
        }

        private static void DrawPill(Graphics g, int x, int y, int w, int h, string label, string val, Color accent)
        {
            Rectangle r = new Rectangle(x, y, w, h);
            using (var fillBrush = new SolidBrush(Color.FromArgb(200, 15, 23, 42)))
            {
                g.FillRectangle(fillBrush, r);
            }
            using (var borderPen = new Pen(Color.FromArgb(100, accent.R, accent.G, accent.B), 1))
            {
                g.DrawRectangle(borderPen, r);
            }
            using (var lblFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var lblBrush = new SolidBrush(accent))
            {
                g.DrawString(label, lblFont, lblBrush, x + 8, y + 5);
            }
            using (var valFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (var valBrush = new SolidBrush(Color.FromArgb(226, 232, 240)))
            {
                g.DrawString(val, valFont, valBrush, x + 8, y + 20);
            }
        }
    }
}
