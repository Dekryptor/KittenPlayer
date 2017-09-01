﻿using System;
using System.Reflection;
//using System.Object;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace KittehPlayer
{

    /// <summary>
    /// Main window of the program.
    /// </summary>

    public partial class MainWindow : Form
    {

        MusicPlayer musicPlayer = MusicPlayer.NewMusicPlayer();
        LocalData localData = LocalData.NewLocalData();


        public MainWindow()
        {
            InitializeComponent();
            this.KeyPress += MainTabs_KeyPress;
            this.KeyPreview = true;

            ListBox playlistBox = new PlaylistBox() as ListBox;

            CopyList(ref MusicList, ref playlistBox);

            MusicList.Hide();
            MainTab.Controls.Remove(MusicList);
            MusicList = playlistBox;
            MainTab.Controls.Add(MusicList);

            LoadAllPlaylists();
        }

        private void MainTab_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void MainTab_Click(object sender, EventArgs e)
        {
            
        }
        

        private void MusicList_Click(object sender, EventArgs e)
        {
            
        }
        

        ///<summary>
        ///<para>Method for adding files to list.</para>
        ///</summary>

        private void MusicList_DragDrop(object sender, DragEventArgs e)
        {
            if (sender is PlaylistBox)
            {
                PlaylistBox playlist = sender as PlaylistBox;

                string[] FileList = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                foreach (string filePath in FileList)
                {
                    playlist.AddNewTrack(filePath);
                }

                SaveAllPlaylists();
            }
        }
        
        const String Directory = "./";

        private void SavePlaylist(Control PlaylistTab, int Index)
        {
            String Name = Directory + Index;

            PlaylistBox playlistBox = PlaylistTab.Controls[0] as PlaylistBox;
            playlistBox.SaveToFile(Name, PlaylistTab.Text);
        }

        private void SaveAllPlaylists()
        {
            for(int i = 0; i < MainTabs.Controls.Count; i++)
            {
                var playlistTab = MainTabs.Controls[i];
                SavePlaylist(playlistTab, i);
            }
        }

        private void LoadAllPlaylists()
        {
            //MainTabs.Controls.Clear();
            int Index = 0;
            String Name = Directory + Index;
            while (File.Exists(Name))
            {
                LoadPlaylist(Name);
                Index++;
                Name = Directory + Index;
            }
        }

        private void LoadPlaylist(String Name)
        {
            var tabPage = AddNewTab("");
            PlaylistBox playlist = tabPage.Controls[0] as PlaylistBox;
            String Title;
            playlist.LoadFromFile(Name, out Title);
            tabPage.Text = Title;
        }

        private void MusicList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;

            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void MusicList_DoubleClick(object sender, EventArgs Event)
        {
            if (sender is PlaylistBox)
            {
                PlaylistBox playlist = sender as PlaylistBox;
                MouseEventArgs mouseEvent = Event as MouseEventArgs;

                int index = playlist.IndexFromPoint(mouseEvent.Location);
                if (index != System.Windows.Forms.ListBox.NoMatches)
                {
                    musicPlayer.Play(playlist.GetDirectory(index));
                }
            }
        }


        private TabPage AddNewTab(String Name)
        {

            TabPage tabPage = new TabPage();
            MainTabs.Controls.Add(tabPage);
            tabPage.Text = Name;
            tabPage.UseVisualStyleBackColor = true;

            ListBox listBox = new PlaylistBox();
            CopyList(ref MusicList, ref listBox);

            tabPage.Controls.Add(listBox);

            return tabPage;
        }

        private void DeleteTab(Control Tab)
        {
            MainTabs.Controls.Remove(Tab);
        }

        private void CopyList(ref ListBox In, ref ListBox Out)
        {

            Out.AllowDrop = In.AllowDrop;
            Out.BorderStyle = In.BorderStyle;
            Out.CausesValidation = In.CausesValidation;
            Out.FormattingEnabled = In.FormattingEnabled;
            Out.Location = In.Location;
            Out.Name = In.Name;
            Out.Size = In.Size;
            Out.TabIndex = In.TabIndex;
            Out.Click += new System.EventHandler(this.MusicList_Click);
            Out.TabIndexChanged += new System.EventHandler(this.MusicList_TabIndexChanged);
            Out.DragDrop += new System.Windows.Forms.DragEventHandler(this.MusicList_DragDrop);
            Out.DragEnter += new System.Windows.Forms.DragEventHandler(this.MusicList_DragEnter);
            Out.DoubleClick += new System.EventHandler(this.MusicList_DoubleClick);
            Out.Anchor = In.Anchor;
        }


        private void MainTabs_DoubleClick(object sender, EventArgs e)
        {
            if(sender is TabControl)
            {
                TabControl Tab = (TabControl)sender;
                Debug.WriteLine(Tab.Name);
            }
            
        }


        private void MainTabs_Selected(object sender, TabControlEventArgs e)
        {
            Debug.WriteLine(e.TabPage.Text);
        }

        private void MusicList_TabIndexChanged(object sender, EventArgs e)
        {

        }
        
        /// <summary>
        /// Right Click on Tab invoker. Right click automatically selects clicked tab.
        /// </summary>

        private void MainTabs_Click(object sender, EventArgs Event)
        {
            if (Event is MouseEventArgs && sender is TabControl)
            {
                MouseEventArgs mouseEvent = (MouseEventArgs)Event;
                if (mouseEvent.Button == MouseButtons.Right)
                {

                    TabControl tabControl = (TabControl)sender;

                    Rectangle mouseRect = new Rectangle(mouseEvent.X, mouseEvent.Y, 1, 1);
                    for (int i = 0; i < tabControl.TabCount; i++)
                    {
                        if (tabControl.GetTabRect(i).IntersectsWith(mouseRect))
                        {
                            tabControl.SelectedIndex = i;
                            break;
                        }
                    }
                    
                    ContextTab.Show(MainTabs, mouseEvent.Location);
                }
            }
        }

        private void MainWindow_Click(object sender, EventArgs e)
        {

        }

        private void MainWindow_DoubleClick(object sender, EventArgs e)
        {
            AddNewTab("NewTab");
        }

        private void ContextTab_Opening(object sender, CancelEventArgs e)
        {

        }

        TextBox RenameBox = null;
        
        /// <summary>
        /// On renaming action TextBox appears in exact place of original playlist name.
        /// </summary>

        private void RenameTab()
        {
            int TabNum = MainTabs.SelectedIndex;
            
            Rectangle rect = MainTabs.GetTabRect(TabNum);
            Point point = MainTabs.Location;

            rect = MainTabs.RectangleToScreen(rect);
            rect = MainTabs.Parent.RectangleToClient(rect);

            RenameBox = new TextBox();
            MainTabs.GetControl(TabNum).Controls.Add(RenameBox);
            MainTabs.Parent.Controls.Add(RenameBox);
            RenameBox.TextAlign = HorizontalAlignment.Center;
            RenameBox.Font = MainTabs.GetControl(TabNum).Font;
            RenameBox.SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            RenameBox.Text = MainTabs.Controls[TabNum].Text;
            RenameBox.BringToFront();
            RenameBox.KeyPress += textBox1_KeyPress;
            RenameBox.Focus();
            RenameBox.Show();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameTab();
        }

        private void TabHover(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Auxiliary method for killing a textbox.
        /// </summary>

        private void KillTextBox()
        {
            RenameBox.Hide();
            RenameBox.Parent.Controls.Remove(RenameBox);
            RenameBox = null; //if it doesn't kill it i don't know what does
            MainTabs.Select();
        }

        /// <summary>
        /// Method for handling the textbox. For now Enter means to update the name and Esc to back to state before.
        /// </summary>

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (RenameBox == null) return;

            if(e.KeyChar == (char)Keys.Enter)
            {
                MainTabs.SelectedTab.Text = RenameBox.Text;
                KillTextBox();
            }

            else if(e.KeyChar == (char)Keys.Escape){
                KillTextBox();
            }

        }

        private void MainTabs_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void MainTabs_KeyPress(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                RenameTab();
            }

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "mp3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach(String s in openFileDialog.FileNames)
                {
                    Debug.WriteLine(s);

                    //here i need to add the adding files handling, i wait for playlist to be fully implemented


                }
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void addNewPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewTab("New Tab");
        }

        private void deletePlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int TabNum = MainTabs.SelectedIndex;
            MainTabs.Controls.RemoveAt(MainTabs.SelectedIndex);
        }

        private void MainTabs_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Mouse down - start hovering");
        }
    }
}