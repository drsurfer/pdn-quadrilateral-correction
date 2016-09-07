﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace QuadControl
{
    internal partial class QuadControl : UserControl
    {
        internal QuadControl()
        {
            InitializeComponent();
        }

        private void QuadControl1_Load(object sender, EventArgs e)
        {
            using (var memoryStream = new MemoryStream(QuadrilateralCorrectionEffect.Resources.HandOpen))
            {
                handOpen = new Cursor(memoryStream);
            }
            using (var memoryStream = new MemoryStream(QuadrilateralCorrectionEffect.Resources.HandGrab))
            {
                handGrab = new Cursor(memoryStream);
            }
        }

        #region Variables
        Rectangle uiImgBounds;
        Bitmap gBmp;// = new Bitmap(500, 500); // fills the entire control surface.
        bool MouseIsDown = false; // True if mouse button is down
        Point MouseFromNub = new Point();
        int Radius = 3; // nb Radius * 2 + 1 = size
        Cursor handOpen;
        Cursor handGrab;
        const int DeadZone = 30;
        #endregion

        #region Properties
        // four Nubs to store coordinates and activation states
        Nub nubTL, nubTR, nubBR, nubBL;

        // four publicly accessible get/sets which map the internal location variables
        public Point NubTL
        {
            get { return nubTL.Location; }
            set
            {
                nubTL.Location = value;
                OnValueChanged();
                pictureBox1.Refresh();
            }
        }
        public Point NubTR
        {
            get { return nubTR.Location; }
            set
            {
                nubTR.Location = value;
                OnValueChanged();
                pictureBox1.Refresh();
            }
        }
        public Point NubBR
        {
            get { return nubBR.Location; }
            set
            {
                nubBR.Location = value;
                OnValueChanged();
                pictureBox1.Refresh();
            }
        }
        public Point NubBL
        {
            get { return nubBL.Location; }
            set
            {
                nubBL.Location = value;
                OnValueChanged();
                pictureBox1.Refresh();
            }
        }

        public Bitmap UiImage
        {
            set
            {
                pictureBox1.BackgroundImage = value;

                float divisor = Math.Max(value.Width, value.Height) / 500f;
                uiImgBounds.Width = (int)(value.Width / divisor);
                uiImgBounds.Height = (int)(value.Height / divisor);

                gBmp = new Bitmap(uiImgBounds.Width, uiImgBounds.Height);
            }
        }

        public byte SelectedNub
        {
            get
            {
                byte nub = 0;

                if (nubTL.Selected)
                    nub = 1;
                else if (nubTR.Selected)
                    nub = 2;
                else if (nubBR.Selected)
                    nub = 3;
                else if (nubBL.Selected)
                    nub = 4;

                return nub;
            }
        }
        #endregion

        #region Event handler
        // delegate event handler
        public delegate void ValueChangedEventHandler(object sender);
        public event ValueChangedEventHandler ValueChanged;

        protected void OnValueChanged()
        {
            if (this.ValueChanged != null) this.ValueChanged(this);
        }
        #endregion

        #region Graphics
        private void DrawGraphics()
        {
            // Draw the Quadrilateral and four control Nubs.
            using (Graphics g = Graphics.FromImage(gBmp))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;

                // clear bitmap
                g.Clear(Color.Transparent);

                // Draw quadrilateral
                using (Pen outlinePen = new Pen(Color.Black))
                {
                    outlinePen.Color = Color.White;
                    outlinePen.DashStyle = DashStyle.Dash;
                    g.DrawLine(outlinePen, nubTL.Location, nubTR.Location);
                    g.DrawLine(outlinePen, nubTR.Location, nubBR.Location);
                    g.DrawLine(outlinePen, nubBR.Location, nubBL.Location);
                    g.DrawLine(outlinePen, nubBL.Location, nubTL.Location);

                    outlinePen.Color = Color.Black;
                    outlinePen.DashStyle = DashStyle.Dot;
                    g.DrawLine(outlinePen, nubTL.Location, nubTR.Location);
                    g.DrawLine(outlinePen, nubTR.Location, nubBR.Location);
                    g.DrawLine(outlinePen, nubBR.Location, nubBL.Location);
                    g.DrawLine(outlinePen, nubBL.Location, nubTL.Location);
                }

                // Draw Nubs
                using (Pen nubPen = new Pen(Color.White, 4))
                using (Pen nubStatePen = new Pen(Color.Black, 1.6f))
                {
                    // Top Left control nub
                    Radius = (nubTL.Hovered || nubTL.Selected) ? 5 : 3;
                    g.DrawEllipse(nubPen, nubTL.X - Radius, nubTL.Y - Radius, Radius * 2 + 1, Radius * 2 + 1);
                    nubStatePen.Color = (nubTL.Selected) ? Color.DodgerBlue : Color.Black;
                    g.DrawEllipse(nubStatePen, nubTL.X - Radius, nubTL.Y - Radius, Radius * 2 + 1, Radius * 2 + 1);

                    // Top Right control nub
                    Radius = (nubTR.Hovered || nubTR.Selected) ? 5 : 3;
                    g.DrawEllipse(nubPen, nubTR.X - Radius - 1, nubTR.Y - Radius, Radius * 2 + 1, Radius * 2 + 1);
                    nubStatePen.Color = (nubTR.Selected) ? Color.DodgerBlue : Color.Black;
                    g.DrawEllipse(nubStatePen, nubTR.X - Radius - 1, nubTR.Y - Radius, Radius * 2 + 1, Radius * 2 + 1);

                    // Bottom Right control nub
                    Radius = (nubBR.Hovered || nubBR.Selected) ? 5 : 3;
                    g.DrawEllipse(nubPen, nubBR.X - Radius - 1, nubBR.Y - Radius - 1, Radius * 2 + 1, Radius * 2 + 1);
                    nubStatePen.Color = (nubBR.Selected) ? Color.DodgerBlue : Color.Black;
                    g.DrawEllipse(nubStatePen, nubBR.X - Radius - 1, nubBR.Y - Radius - 1, Radius * 2 + 1, Radius * 2 + 1);

                    // Bottom Left control nub
                    Radius = (nubBL.Hovered || nubBL.Selected) ? 5 : 3;
                    g.DrawEllipse(nubPen, nubBL.X - Radius, nubBL.Y - Radius - 1, Radius * 2 + 1, Radius * 2 + 1);
                    nubStatePen.Color = (nubBL.Selected) ? Color.DodgerBlue : Color.Black;
                    g.DrawEllipse(nubStatePen, nubBL.X - Radius, nubBL.Y - Radius - 1, Radius * 2 + 1, Radius * 2 + 1);
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawGraphics();
            e.Graphics.DrawImage(gBmp, 0, 0);
        }
        #endregion

        #region Mouse events
        private void pictureBox1_MouseDown_1(object sender, MouseEventArgs e)
        {
            MouseIsDown = true; // because the mouse button is down
            //MouseDownStart = e.Location; // has the location of the mouse pointer when the button is pressed

            // find which control nub is being activated (if any)
            if (NearNub(e.Location, nubTL))
            {
                if (e.Button == MouseButtons.Right)
                {
                    SelectNub(nubTL);
                }
                else
                {
                    GrabNub(nubTL, e.Location);
                }
            }
            else if (NearNub(e.Location, nubTR))
            {
                if (e.Button == MouseButtons.Right)
                {
                    SelectNub(nubTR);
                }
                else
                {
                    GrabNub(nubTR , e.Location);
                }
            }
            else if (NearNub(e.Location, nubBR))
            {
                if (e.Button == MouseButtons.Right)
                {
                    SelectNub(nubBR);
                }
                else
                {
                    GrabNub(nubBR, e.Location);
                }
            }
            else if (NearNub(e.Location, nubBL))
            {
                if (e.Button == MouseButtons.Right)
                {
                    SelectNub(nubBL);
                }
                else
                {
                    GrabNub(nubBL, e.Location);
                }
            }

            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseUp_1(object sender, MouseEventArgs e)
        {
            nubTL.Grabbed = false;
            nubTR.Grabbed = false;
            nubBR.Grabbed = false;
            nubBL.Grabbed = false;
            MouseIsDown = false;

            pictureBox1.Refresh();
            OnValueChanged();
        }

        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (!MouseIsDown)
            {
                if (NearNub(e.Location, nubTL))
                {
                    HoverNub(nubTL);
                }
                else if (NearNub(e.Location, nubTR))
                {
                    HoverNub(nubTR);
                }
                else if (NearNub(e.Location, nubBR))
                {
                    HoverNub(nubBR);
                }
                else if (NearNub(e.Location, nubBL))
                {
                    HoverNub(nubBL);
                }
                else
                {
                    UnHoverNubs();
                }

                pictureBox1.Refresh();
                return;
            }


            if (nubTL.Grabbed)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    if (e.X <= nubTL.X - DeadZone)
                        nubTL.X = ClipWidth(e.X + DeadZone);
                    else if (e.X >= nubTL.X + DeadZone)
                        nubTL.X = ClipWidth(e.X - DeadZone);

                    if (e.Y <= nubTL.Y - DeadZone)
                        nubTL.Y = ClipHeight(e.Y + DeadZone);
                    else if (e.Y >= nubTL.Y + DeadZone)
                        nubTL.Y = ClipHeight(e.Y - DeadZone);
                }
                else
                {
                    nubTL.X = ClipWidth(e.X - MouseFromNub.X);
                    nubTL.Y = ClipHeight(e.Y - MouseFromNub.Y);
                }
            }
            else if (nubTR.Grabbed)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    if (e.X <= nubTR.X - DeadZone)
                        nubTR.X = ClipWidth(e.X + DeadZone);
                    else if (e.X >= nubTR.X + DeadZone)
                        nubTR.X = ClipWidth(e.X - DeadZone);

                    if (e.Y <= nubTR.Y - DeadZone)
                        nubTR.Y = ClipHeight(e.Y + DeadZone);
                    else if (e.Y >= nubTR.Y + DeadZone)
                        nubTR.Y = ClipHeight(e.Y - DeadZone);
                }
                else
                {
                    nubTR.X = ClipWidth(e.X - MouseFromNub.X);
                    nubTR.Y = ClipHeight(e.Y - MouseFromNub.Y);
                }
            }
            else if (nubBR.Grabbed)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    if (e.X <= nubBR.X - DeadZone)
                        nubBR.X = ClipWidth(e.X + DeadZone);
                    else if (e.X >= nubBR.X + DeadZone)
                        nubBR.X = ClipWidth(e.X - DeadZone);

                    if (e.Y <= nubBR.Y - DeadZone)
                        nubBR.Y = ClipHeight(e.Y + DeadZone);
                    else if (e.Y >= nubBR.Y + DeadZone)
                        nubBR.Y = ClipHeight(e.Y - DeadZone);
                }
                else
                {
                    nubBR.X = ClipWidth(e.X - MouseFromNub.X);
                    nubBR.Y = ClipHeight(e.Y - MouseFromNub.Y);
                }
            }
            else if (nubBL.Grabbed)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    if (e.X <= nubBL.X - DeadZone)
                        nubBL.X = ClipWidth(e.X + DeadZone);
                    else if (e.X >= nubBL.X + DeadZone)
                        nubBL.X = ClipWidth(e.X - DeadZone);

                    if (e.Y <= nubBL.Y - DeadZone)
                        nubBL.Y = ClipHeight(e.Y + DeadZone);
                    else if (e.Y >= nubBL.Y + DeadZone)
                        nubBL.Y = ClipHeight(e.Y - DeadZone);
                }
                else
                {
                    nubBL.X = ClipWidth(e.X - MouseFromNub.X);
                    nubBL.Y = ClipHeight(e.Y - MouseFromNub.Y);
                }
            }
            pictureBox1.Refresh();
            OnValueChanged();
        }
        #endregion

        #region Nub functions
        private void SelectNub(Nub nub)
        {
            nubTL.Selected = false;
            nubTR.Selected = false;
            nubBR.Selected = false;
            nubBL.Selected = false;

            if (nub.Location == nubTL.Location)
            {
                nubTL.Selected = !nub.Selected;
            }
            else if (nub.Location == nubTR.Location)
            {
                nubTR.Selected = !nub.Selected;
            }
            else if (nub.Location == nubBR.Location)
            {
                nubBR.Selected = !nub.Selected;
            }
            else if (nub.Location == nubBL.Location)
            {
                nubBL.Selected = !nub.Selected;
            }

            nubTL.Grabbed = false;
            nubTR.Grabbed = false;
            nubBR.Grabbed = false;
            nubBL.Grabbed = false;

            this.Cursor = Cursors.Default;
        }

        private void GrabNub(Nub nub, Point mouseLocation)
        {
            nubTL.Grabbed = false;
            nubTR.Grabbed = false;
            nubBR.Grabbed = false;
            nubBL.Grabbed = false;

            nubTL.Hovered = false;
            nubTR.Hovered = false;
            nubBR.Hovered = false;
            nubBL.Hovered = false;

            if (nub.Location == nubTL.Location)
            {
                nubTL.Grabbed = true;
                nubTL.Hovered = true;
            }
            else if (nub.Location == nubTR.Location)
            {
                nubTR.Grabbed = true;
                nubTR.Hovered = true;
            }
            else if (nub.Location == nubBR.Location)
            {
                nubBR.Grabbed = true;
                nubBR.Hovered = true;
            }
            else if (nub.Location == nubBL.Location)
            {
                nubBL.Grabbed = true;
                nubBL.Hovered = true;
            }

            nubTL.Selected = false;
            nubTR.Selected = false;
            nubBR.Selected = false;
            nubBL.Selected = false;

            MouseFromNub.X = mouseLocation.X - nub.X;
            MouseFromNub.Y = mouseLocation.Y - nub.Y;

            this.Cursor = handGrab;
        }

        private void UnHoverNubs()
        {
            nubTL.Hovered = false;
            nubTR.Hovered = false;
            nubBR.Hovered = false;
            nubBL.Hovered = false;

            this.Cursor = Cursors.Default;
        }

        private void HoverNub(Nub nub)
        {
            nubTL.Hovered = false;
            nubTR.Hovered = false;
            nubBR.Hovered = false;
            nubBL.Hovered = false;

            if (nub.Location == nubTL.Location)
            {
                nubTL.Hovered = true;
            }
            else if (nub.Location == nubTR.Location)
            {
                nubTR.Hovered = true;
            }
            else if (nub.Location == nubBR.Location)
            {
                nubBR.Hovered = true;
            }
            else if (nub.Location == nubBL.Location)
            {
                nubBL.Hovered = true;
            }

            this.Cursor = handOpen;
        }

        private bool NearNub(Point mouseLocation, Nub nub)
        {
            return ((Math.Abs(mouseLocation.X - nub.X) <= Radius + 10) && (Math.Abs(mouseLocation.Y - nub.Y) <= Radius + 10));
        }
        #endregion

        #region Utility routines
        private int ClipWidth(int x)
        {
            int y = (x < 0) ? 0 : (x > uiImgBounds.Width - 1) ? uiImgBounds.Width - 1 : x;
            return y;
        }

        private int ClipHeight(int x)
        {
            int y = (x < 0) ? 0 : (x > uiImgBounds.Height - 1) ? uiImgBounds.Height - 1 : x;
            return y;
        }
        #endregion

        private struct Nub
        {
            private Point location;
            internal Point Location
            {
                get
                {
                    return location;
                }
                set
                {
                    location = value;
                }
            }
            internal int X
            {
                get
                {
                    return location.X;
                }
                set
                {
                    location.X = value;
                }
            }
            internal int Y
            {
                get
                {
                    return location.Y;
                }
                set
                {
                    location.Y = value;
                }
            }
            internal bool Grabbed { get; set; }
            internal bool Hovered { get; set; }
            internal bool Selected { get; set; }
        }
    }
}