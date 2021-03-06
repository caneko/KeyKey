// [AUTO_HEADER]

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace BaseIMEUI
{
    /// <remarks>
    /// The Candidate Window
    /// The Window to display the list of candidates and selection keys
    /// </remarks>
    public partial class BICandidateForm : BIForm
    {
        #region Members
        private string[] m_candidates = { };
        private string[] m_selectionKeys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        private string m_prompt = "";
        private bool m_inControl = true;
        private bool m_usePattern;

        private float m_fontSize = 12F;
        private int m_fontHeight = 0;
        private int m_windowWidth = 50;
        private int m_borderWidth = 2;
        private int m_pageCount = 0;
        private int m_currentPage = 0;
        private int m_highlightedItem = -1;
        private int m_itemsPerPage = 10;
        private float m_inputBufferHeightInPixel = 50;

        private Color m_borderColor;
        private Color m_backgroundColor;
        private Color m_patternColor;
        private Color m_foregroundColor;
        private Color m_hilightColor;
        private Color m_hilightEndColor;
        private Color m_hilightForegroundColor;
        private Color m_indicatorColor;
        private Color m_keyForgroundColor;
        private Color m_keyBackgroundColor;
        private Color m_keyHilightBackgroundColor;
        private Font m_indicatorFont;
        private Font m_promptFont;
        private Font m_selectionKeyFont;

        private Point m_targetPoint;
        private Size m_targetSize;
        #endregion

        public BICandidateForm()
        {
            CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();

            #region Initializing the basical variables.
            FontFamily fontFamily = Win32FontHelper.DefaultFontFamily();

            if (fontFamily.Name.Equals("\u5fae\u8edf\u6b63\u9ed1\u9ad4") ||
            	fontFamily.Name.Equals("Microsoft JhengHei"))
                this.Font = new Font(fontFamily, this.m_fontSize,
	 				FontStyle.Bold, GraphicsUnit.Point, ((byte)(136)));
            else
                this.Font = new Font(fontFamily, this.m_fontSize,
                	FontStyle.Regular, GraphicsUnit.Point, ((byte)(136)));
            fontFamily.Dispose();
            this.m_indicatorFont = new Font("Arial", 7, FontStyle.Bold,
 				GraphicsUnit.Point, ((byte)(136)));
            this.m_selectionKeyFont = new Font("Arial", 8, FontStyle.Bold,
 				GraphicsUnit.Point, ((byte)(136)));
            this.m_promptFont = new Font("Arial", 8, FontStyle.Regular,
 				GraphicsUnit.Point, ((byte)(136)));

            #region Color Schema
            this.m_borderColor = Color.FromArgb(149, 149, 149);
            this.m_backgroundColor = Color.Black;
            this.m_patternColor = Color.FromArgb(32, 32, 32);
            this.m_foregroundColor = Color.White;
            this.m_hilightColor = Color.FromArgb(140, 91, 156);
            this.m_hilightEndColor = Color.FromArgb(140, 91, 156);
            this.m_hilightForegroundColor = Color.White;
            this.m_indicatorColor = Color.FromArgb(183, 183, 183);
            this.m_keyBackgroundColor = Color.FromArgb(154, 154, 154);
            this.m_keyForgroundColor = Color.FromArgb(80, 80, 80);
            this.m_keyHilightBackgroundColor = Color.FromArgb(234, 209, 239);
            #endregion

            #endregion
        }

        private delegate void MethodInvoker();
        /// <summary>
        /// The shadowed Show()
        /// </summary>
        public new void Show()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(this.Show));
                return;
            }

            BIServerConnector callback = BIServerConnector.SharedInstance;
            if (callback != null)
            {
				// Get the color schema of the canidate window.
                if (callback.hasLoaderConfigKey("HighlightColor"))
                {
                    string colorString = callback.stringValueForLoaderConfigKey("HighlightColor");
                    if (colorString.Equals("Green"))
                    {
                        this.m_hilightColor = Color.Green;
                        this.m_hilightEndColor = Color.DarkGreen;
                    }
                    else if (colorString.Equals("Yellow"))
                    {
                        this.m_hilightColor = Color.Yellow;
                        this.m_hilightEndColor = Color.DarkOrange;
                    }
                    else if (colorString.Equals("Red"))
                    {
                        this.m_hilightColor = Color.Red;
                        this.m_hilightEndColor = Color.DarkRed;
                    }
                    else if (colorString.StartsWith("Color "))
                    {
                        string aString = colorString.Remove(0, 6);
                        Color aColor = Color.FromArgb(Int32.Parse(aString));
                        this.m_hilightColor = aColor;
                        this.m_hilightEndColor = aColor;
                    }
                    else
                    {
                        this.m_hilightColor = Color.FromArgb(140, 91, 156);
                        this.m_hilightEndColor = Color.FromArgb(140, 91, 156);
                    }
                }

                if (callback.hasLoaderConfigKey("BackgroundColor"))
                {
                    string colorString = callback.stringValueForLoaderConfigKey("BackgroundColor");
                    if (colorString.Equals("White"))
                    {
                        this.m_backgroundColor = Color.White;
                        this.m_patternColor = Color.LightGray;
                    }
                    else if (colorString.StartsWith("Color "))
                    {
                        string aString = colorString.Remove(0, 6);
                        Color aColor = Color.FromArgb(Int32.Parse(aString));
                        this.m_backgroundColor = aColor;
                        int r = aColor.R - 32;
                        if (r < 0) r = 0;
                        int g = aColor.G - 32;
                        if (g < 0) g = 0;
                        int b = aColor.B - 32;
                        if (b < 0) b = 0;
                        this.m_patternColor = Color.FromArgb(r, g, b);
                    }
                    else
                    {
                        this.m_backgroundColor = Color.Black;
                        this.m_patternColor = Color.FromArgb(32, 32, 32);
                    }
                }

                if (callback.hasLoaderConfigKey("TextColor"))
                {
                    string colorString = callback.stringValueForLoaderConfigKey("TextColor");
                    if (colorString.Equals("Black"))
                    {
                        this.m_foregroundColor = Color.Black;
                    }
                    else if (colorString.StartsWith("Color "))
                    {
                        string aString = colorString.Remove(0, 6);
                        Color aColor = Color.FromArgb(Int32.Parse(aString));
                        this.m_foregroundColor = aColor;
                    }
                    else
                    {
                        this.m_foregroundColor = Color.White;
                    }
                }

                if (callback.hasLoaderConfigKey("BackgroundPattern"))
                {
                    string patternString = callback.stringValueForLoaderConfigKey("BackgroundPattern");
                    if (patternString.Equals("true"))
                        this.m_usePattern = true;
                    else
                        this.m_usePattern = false;
                }
                else
                {
                    this.m_usePattern = false;
                }
            }

			// The Candidate Window should be always on top.
            Win32FunctionHelper.ShowWindowTopMost(base.Handle);
            base.Visible = true;
        }

        /// <summary>
        /// <para>To update the information about the height of the text of 
		/// the input buffer.</para>
        /// <para>The default position of the candidate window is at the 
		/// bottom of the input buffer, however, if the input buffer is near 
		/// the bottom of the screen, the candidate window should be at the
        /// top. To let the candidate window placed in a prooper location, we
 		/// need to know the height of the input buffer, and then the height 
		/// of current sceen minus the height of the input buffer and the
		/// candidate window is the correct position of the candidate 
		/// window.</para>
        /// </summary>
        /// <param name="newHeight">The new height of the input buffer in 
		/// pixel.</param>
        public void SetInputBufferHeightInPixel(long newHeight)
        {
            this.m_inputBufferHeightInPixel = (float)newHeight;
        }

        #region Window Location
        private delegate void SetLocationCallBack(int x, int y);
        /// <summary>
        /// To set the location of the candidate window.
        /// The SetLocation() method is inherited from the BIForm class,
 		/// however, the SetLocation() method of the candidate window do not
 		/// move the window directly, but set a target location temporary,
		/// the window will move to the target location while re-drawing the 
		/// window and the on-paint event is launched.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>        
        public new void SetLocation(int x, int y)
        {
            this.m_targetPoint = new Point(x, y);
        }

        public void SetLocationDirectly(int x, int y)
        {
            if (this.InvokeRequired)
            {
                SetLocationCallBack aSetLocationCall = new SetLocationCallBack(SetLocationDirectly);
                this.Invoke(aSetLocationCall, new object[] { x, y });
                return;
            }

            this.Location = new Point(x, y);
            this.Invalidate();
        }
        #endregion

        /// <summary>
        /// The current page index where the candidate window is in.
        /// </summary>
        public int CurrentPage
        {
            get { return this.m_currentPage; }
            set { this.m_currentPage = value; }
        }
        /// <summary>
        /// The amount of the pages in the candidate list.
        /// </summary>
        public int PageCount
        {
            get { return this.m_pageCount; }
            set { this.m_pageCount = value; }
        }

        /// <summary>
        /// Set the pgae count and the current page.
        /// </summary>
        /// <param name="CurrentPage">The current page index where the 
		/// candidate window is in.</param>
        /// <param name="PageCount">The amount of the pages in the candidate 
		/// list.</param>
        public void SetPage(int CurrentPage, int PageCount)
        {
            this.m_currentPage = CurrentPage;
            this.m_pageCount = PageCount;
        }

        #region Prompt
        /// <summary>
        /// The prompt of the candidate window.
        /// </summary>
        public string Prompt
        {
            get { return this.m_prompt; }
            set { this.SetPrompt(value); }
        }

        /// <summary>
        /// Set the prompt of the candidate window.
        /// </summary>
        /// <param name="newPrompt">The prompt as a string.</param>
        public void SetPrompt(string newPrompt)
        {
            this.m_prompt = newPrompt;
        }
        #endregion

        #region InControl
        /// <summary>
        /// If the Candidate Window is in control or not.
        /// </summary>
        public bool InControl
        {
            get { return this.m_inControl; }
            set { this.SetInControl(value); }
        }

        /// <summary>
        /// <para>Set if the candidate window is in control.</para>
        /// <para>If the candidate window is in control, there will be a
 		/// highlight bar to indicate which item in the candidate window is
 		/// selected, otherwise, this bar will not be
        /// shown.</para>
        /// </summary>
        /// <param name="newInControl">If the candidate window is in control
		/// </param>
        public void SetInControl(bool NewInControl)
        {
            this.m_inControl = NewInControl;
        }
        #endregion

        #region Candidates
        /// <summary>
        /// The list of the candidates.
        /// </summary>
        public string[] Candidates
        {
            get { return this.m_candidates; }
            set { this.SetCandidates(value); }
        }

        /// <summary>
        /// Passing a new string array to replace the items in the candidate
 		/// list.
        /// </summary>
        /// <param name="newCandidates"></param>
        public void SetCandidates(string[] NewCandidates)
        {
            this.m_candidates = NewCandidates;
            // Initialize the target width.
            this.m_windowWidth = 50;
            if (this.m_candidates.Length < 1)
                this.Hide();
        }
        #endregion

        #region SelectionKeys
        /// <summary>
        /// The selection keys.
        /// </summary>
        public string[] SelectionKeys
        {
            get { return m_selectionKeys; }
            set { this.SetSelectionKeys(value); }
        }

        /// <summary>
        /// Set the selection keys
        /// </summary>
        /// <param name="NewSelectionKeys">The new selection keys.</param>
        public void SetSelectionKeys(string[] NewSelectionKeys)
        {
            if (this.m_selectionKeys.Equals(NewSelectionKeys))
                return;
            this.m_selectionKeys = NewSelectionKeys;
            if (this.m_selectionKeys.Length == 0)
            {	
                string[] defaultSelectionKeys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                this.m_selectionKeys = defaultSelectionKeys;
            }
        }
        #endregion

        #region Items per Page
        public int ItemsPerPage
        {
            get { return this.m_itemsPerPage; }
            set { this.SetItemsPerPage(value); }
        }

        public void SetItemsPerPage(int newItemsPerPage)
        {
            this.m_itemsPerPage = newItemsPerPage;
            if (this.m_itemsPerPage == 0)
                this.m_itemsPerPage = 10;
        }
        #endregion

        #region HighlightedItem
        public int HighlightedItem
        {
            get { return this.m_highlightedItem; }
            set { this.SetHighlight(value); }
        }

        public void SetHighlight(int newHighlightedIndex)
        {
            this.m_highlightedItem = newHighlightedIndex;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// If the mouse cursor is over an item which is clickable, 
		/// we would like to change the style of cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BICandidateForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.m_fontHeight <= 0)
                return;

            if (this.m_inControl == false)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            if (e.Y > 20 && e.Y < (this.m_fontHeight * this.Candidates.Length + 20))
                this.Cursor = Cursors.Hand;
            else
                this.Cursor = Cursors.Default;
        }

        private void GotoPrevPage()
        {
            BIServerConnector callback = BIServerConnector.SharedInstance;
            if (callback != null)
            {
                callback.gotoPrevPage();
            }
        }

        private void GotoNextPage()
        {
            BIServerConnector callback = BIServerConnector.SharedInstance;
            if (callback != null)
            {
                callback.gotoNextPage();
            }
        }

        private void SendSelectionKey(char key)
        {
            BIServerConnector callback = BIServerConnector.SharedInstance;
            if (callback != null)
            {
                callback.sendChar(key);
            }
        }

        /// <summary>
        /// Handle the events of clicking on the candidate window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BICandidateForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.m_fontHeight <= 0)
                return;

            if (this.m_inControl == false)
                return;

            int selectedItem = (int)((float)(e.Y - 20) / (float)this.m_fontHeight);

            if (selectedItem > this.m_candidates.Length)
                return;
            if (e.Button.Equals(MouseButtons.Left))
            {
                if (e.Y < 20)
                {
                    this.GotoPrevPage();
                }
                else if (selectedItem == this.Candidates.Length)
                {
                    this.GotoNextPage();
                }
                else
                {
                    if (selectedItem >= this.m_candidates.Length || selectedItem >= this.m_selectionKeys.Length)
                        return;
                    this.m_highlightedItem = selectedItem;
                    string keyString = this.m_selectionKeys[selectedItem];
                    if (keyString == null)
                        return;
                    char key = keyString[0];
                    this.SendSelectionKey(key);

                }
            }
        }

        public new void Refresh()
        {
            base.Refresh();
            this.Invalidate();
        }

        protected void AdjustSize(Graphics g)
        {
            try
            {
                this.m_fontHeight = (int)g.MeasureString(m_candidates[0], this.Font).Height;

                if (this.m_prompt.Length > 0)
                    this.m_windowWidth = (int)g.MeasureString(this.m_prompt, this.m_promptFont).Width + 30;

                foreach (string CandidateItem in m_candidates)
                {
                    int Width = (int)g.MeasureString(CandidateItem, this.Font).Width + 50;
                    if (Width >= this.m_windowWidth)
                        this.m_windowWidth = (int)Width;
                }
                int lines;
                lines = m_itemsPerPage + 1;
                int newWindowHeight = (int)(m_fontHeight * lines) + 20;
                this.m_targetSize = new Size(m_windowWidth, newWindowHeight);
            }
            catch { }
        }

        protected void AdjustLocation()
        {
            #region Window Position

            try
            {
                Screen currentScreen;
                if (Screen.AllScreens.Length == 1)
                    currentScreen = Screen.PrimaryScreen;
                else
                    currentScreen = Screen.FromPoint(m_targetPoint);

                Rectangle tmpRect = new Rectangle(m_targetPoint, m_targetSize);

                if (tmpRect.Top < 0)
                    tmpRect.Y = (int)m_inputBufferHeightInPixel;
                if (tmpRect.Bottom > currentScreen.WorkingArea.Bottom)
                {
                    tmpRect.Y = tmpRect.Top - tmpRect.Height - (int)m_inputBufferHeightInPixel - 10;
                    if (tmpRect.Bottom > currentScreen.WorkingArea.Bottom)
                        tmpRect.Y = currentScreen.WorkingArea.Bottom - tmpRect.Height - (int)m_inputBufferHeightInPixel - 5;
                }
                if (tmpRect.Right > currentScreen.WorkingArea.Right)
                    tmpRect.X = currentScreen.WorkingArea.Right - tmpRect.Width;

                this.Size = tmpRect.Size;
                this.Location = tmpRect.Location;
            }
            catch { }
            #endregion
        }

        /// <summary>
        /// Draw the background, candidates and other elements on the 
		/// candidate window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // void BICandidateForm_Paint(object sender, PaintEventArgs e)
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.Visible)
            {
                base.OnPaint(e);
                return;
            }

            if (this.m_candidates.Length < 1 || this.m_selectionKeys.Length < 1)
            {
                this.Hide();
                base.OnPaint(e);
                return;
            }

            Graphics g = e.Graphics;

            try
            {
                // At first, we adjust the window size and location by the 
                // content of the canidate window.
                this.AdjustSize(g);
                this.AdjustLocation();

                Rectangle BackgroundRect = new Rectangle(0, 0, this.Width, this.Height);
                Rectangle BorderRect = new Rectangle(0, 0, this.Width - this.m_borderWidth, this.Height - this.m_borderWidth);

                // Then, fill color into the background
                SolidBrush backBrush = new SolidBrush(this.m_backgroundColor);
                g.FillRectangle(backBrush, BackgroundRect);
                backBrush.Dispose();

                if (this.m_usePattern == true)
                {
                    HatchBrush brush = new HatchBrush(HatchStyle.NarrowHorizontal, this.m_patternColor, this.m_backgroundColor);
                    g.FillRectangle(brush, BackgroundRect);
                    brush.Dispose();
                }

                // Draw the highlight bar if in need.
                if (this.m_highlightedItem > -1 && this.m_inControl)
                {
                    Rectangle highlightRect =
                        new Rectangle(
                            this.m_borderWidth + 3,
                            (int)(m_highlightedItem * this.m_fontHeight + 20),
                            this.Width - this.m_borderWidth * 2 - 10,
                            this.m_fontHeight);
                    LinearGradientBrush highlightBrush =
                        new LinearGradientBrush(
                            new Point(0, highlightRect.Top),
                            new Point(0, highlightRect.Bottom),
                            this.m_hilightColor, this.m_hilightEndColor);
                    g.FillRectangle(highlightBrush, highlightRect);
                    highlightBrush.Dispose();
                }

                SolidBrush borderBrush = new SolidBrush(m_borderColor);
                Pen borderPen = new Pen(borderBrush);
                g.DrawRectangle(borderPen, BorderRect);
                borderPen.Dispose();
                borderBrush.Dispose();

                using (Region windowRegion = new Region(BackgroundRect))
                {
                    this.Region = windowRegion;
                }

                if (this.m_prompt.Length > 0)
                {
                    int pos;
                    if (this.m_inControl == true)
                        pos = 18 + (int)(this.Width - 20 - g.MeasureString(this.m_prompt, this.m_promptFont).Width) / 2;
                    else
                        pos = (int)(this.Width - g.MeasureString(this.m_prompt, this.m_promptFont).Width) / 2;
                    SolidBrush promptBrush = new SolidBrush(m_indicatorColor);
                    g.DrawString(this.m_prompt, this.m_indicatorFont,
                        promptBrush, pos, 6);
                    promptBrush.Dispose();
                }

                int i = 0;
                foreach (string CandidateItem in this.m_candidates)
                {
                    string key = "";
                    if (i < this.m_selectionKeys.Length && this.m_selectionKeys[i].Length > 0)
                        key = this.m_selectionKeys[i];

                    GraphicsPath bgPath = Utilities.DrawBezelPath(10, (int)(i * m_fontHeight) + 25, 12, 12, 1);

                    if (i == this.m_highlightedItem)
                    {
                        SolidBrush stringBrush = new SolidBrush(this.m_hilightForegroundColor);
                        SolidBrush keyHilightBrush = new SolidBrush(this.m_keyHilightBackgroundColor);
                        g.DrawString(
                            CandidateItem, this.Font,
                            stringBrush, 30,
                            (int)(i * m_fontHeight) + 20);
                        g.FillPath(keyHilightBrush, bgPath);
                        stringBrush.Dispose();
                        keyHilightBrush.Dispose();
                    }
                    else
                    {
                        SolidBrush stringBrush = new SolidBrush(m_foregroundColor);
                        SolidBrush keyBackgroundBrush = new SolidBrush(this.m_keyBackgroundColor);
                        g.DrawString(
                            CandidateItem, this.Font,
                            stringBrush, 30,
                            (int)(i * m_fontHeight) + 20);
                        g.FillPath(keyBackgroundBrush, bgPath);
                        stringBrush.Dispose();
                        keyBackgroundBrush.Dispose();
                    }

                    bgPath.Dispose();

                    SolidBrush keyBrush = new SolidBrush(this.m_keyForgroundColor);
                    g.DrawString(
                        key, this.m_selectionKeyFont,
                        keyBrush, 11,
                        (int)(i * m_fontHeight) + 24);
                    keyBrush.Dispose();
                    i++;
                }
                #region Page number indicator
                {
                    string pageIndicator = "(" + this.m_currentPage + "/" + this.m_pageCount + ")";
                    int pos = (int)(this.Width - g.MeasureString(pageIndicator, this.m_indicatorFont).Width) - 5;
                    SolidBrush indicatorBrush = new SolidBrush(this.m_indicatorColor);
                    g.DrawString(
                        pageIndicator, this.m_indicatorFont,
                        indicatorBrush, pos,
                        (int)(m_itemsPerPage * m_fontHeight) + 20);
                    indicatorBrush.Dispose();
                }

                if (this.m_pageCount > 1 && m_inControl)
                {
                    SolidBrush pageBrush = new SolidBrush(this.m_indicatorColor);
                    GraphicsPath pageUpPath = Utilities.DrawPageUp(12, 6, 8);
                    g.FillPath(pageBrush, pageUpPath);
                    pageUpPath.Dispose();
                    GraphicsPath pageDownPath = Utilities.DrawPageDown(12, 
                        (int)(this.m_itemsPerPage * this.m_fontHeight) + 24, 8);
                    g.FillPath(pageBrush, pageDownPath);
                    pageDownPath.Dispose();
                    pageBrush.Dispose();
                }
                #endregion
            }
            catch { }
        }
        #endregion
    }
}
