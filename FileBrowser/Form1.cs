using CWA.DTP;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static Sender sender = new Sender(SenderType.SevenByteName, "Coestar");
        private static SerialPort port = new SerialPort("COM4", 115200);
        private static Bitmap folderImage = new Bitmap("folder.png");
        private static Bitmap backImage = new Bitmap("back.png");
        private static PacketHandler ph = new PacketHandler(sender, new PacketListener(new SerialPacketReader(port, 4000), new SerialPacketWriter(port)));
        private List<string> CurrentPath = new List<string>();

        private string ProccedSize(int size)
        {
            if (size < 1024) return size.ToString() + " B";
            else if (size < 1024 * 1024) return (size / 1024f).ToString() + " Kb";
            else if (size < 1024 * 1024 * 1024) return (size / 1024f / 1024f).ToString() + " Mb";
            else return (size / 1024f / 1024f / 1024f).ToString() + " Gb";
        }

        private void SetupFolder()
        {
            string Path = string.Join("", CurrentPath);
            label_path.Text = Path;
            listView1.Items.Clear();
            float totalWidth = Width;
            float[] WidthCoof =
            {
                .4f,
                .2f,
                .2f,
                .2f
            };
            for (int i = 0; i < listView1.Columns.Count; i++)
                listView1.Columns[i].Width = (int)(totalWidth * WidthCoof[i]);
            var result = ph.DTP_GetDirectoriesAndFiles(Path);
            if (result.Status != PacketHandler.FileDirHandleResult.OK)
            {
                System.Windows.Forms.MessageBox.Show("Cant get root");
                return;
            }
            if (Path != "/") listView1.Items.Add(new ListViewItem(new string[] { "...", "", "", "" }, result.ResultFiles.Count + 1));
            foreach (var a in result.ResultDirs)
            {
                var res = ph.DTP_GetFileInfo(Path == "/" ? a : Path + '/' + a);
                ListViewItem item = new ListViewItem(new string[] { '[' + a + ']', "<folder>", res.CreationTime.ToString(), "____" }, result.ResultFiles.Count);
                listView1.Items.Add(item);
            }
            ImageList il = new ImageList();
            foreach (var a in result.ResultFiles)
            {
                il.Images.Add(IconManager.FindIconForFilename(a, false));
                var res = ph.DTP_GetFileInfo(Path == "/" ? a : Path + '/' + a);
                ListViewItem item = new ListViewItem(new string[] { a, ProccedSize(res.FileSize), res.CreationTime.ToString(), "____" }, il.Images.Count - 1);
                listView1.Items.Add(item);
            }
            il.Images.Add(folderImage);
            il.Images.Add(backImage);
            listView1.SmallImageList = il;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader() {Text = "Name" },
                new ColumnHeader() {Text = "Size" },
                new ColumnHeader(){Text = "Creation Date" },
                new ColumnHeader(){Text = "Flags" }
            });
            CurrentPath.Add("/");
            SetupFolder();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].SubItems[1].Text != "<folder>")
                    if (listView1.SelectedIndices[0] == 0)
                    {
                        CurrentPath.RemoveAt(CurrentPath.Count - 1);
                        SetupFolder();
                    }
                    else System.Windows.Forms.MessageBox.Show("Its not FOLDER");
                else
                {
                    string path = listView1.SelectedItems[0].SubItems[0].Text.Trim('[', ']');
                    CurrentPath.Add(CurrentPath.Last().EndsWith("/") ? path : '/' + path);
                    SetupFolder();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new SendDialog(ph, "CWADTP.pdb", "/CWADTP.pdb").ShowDialog();
            SetupFolder();
        }
    }

    /// <summary>
    /// Internals are mostly from here: http://www.codeproject.com/Articles/2532/Obtaining-and-managing-file-and-folder-icons-using
    /// Caches all results.
    /// </summary>
    public static class IconManager
    {
        private static readonly Dictionary<string, Icon> _smallIconCache = new Dictionary<string, Icon>();
        private static readonly Dictionary<string, Icon> _largeIconCache = new Dictionary<string, Icon>();
        /// <summary>
        /// Get an icon for a given filename
        /// </summary>
        /// <param name="fileName">any filename</param>
        /// <param name="large">16x16 or 32x32 icon</param>
        /// <returns>null if path is null, otherwise - an icon</returns>
        public static Icon FindIconForFilename(string fileName, bool large)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
                return null;
            var cache = large ? _largeIconCache : _smallIconCache;
            Icon icon;
            if (cache.TryGetValue(extension, out icon))
                return icon;
            icon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false);
            cache.Add(extension, icon);
            return icon;
        }
        /// <summary>
        /// http://stackoverflow.com/a/6580799/1943849
        /// </summary>
        static ImageSource ToImageSource(this Icon icon)
        {
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }
        /// <summary>
        /// Provides static methods to read system icons for both folders and files.
        /// </summary>
        /// <example>
        /// <code>IconReader.GetFileIcon("c:\\general.xls");</code>
        /// </example>
        static class IconReader
        {
            /// <summary>
            /// Options to specify the size of icons to return.
            /// </summary>
            public enum IconSize
            {
                /// <summary>
                /// Specify large icon - 32 pixels by 32 pixels.
                /// </summary>
                Large = 0,
                /// <summary>
                /// Specify small icon - 16 pixels by 16 pixels.
                /// </summary>
                Small = 1
            }
            /// <summary>
            /// Returns an icon for a given file - indicated by the name parameter.
            /// </summary>
            /// <param name="name">Pathname for file.</param>
            /// <param name="size">Large or small</param>
            /// <param name="linkOverlay">Whether to include the link icon</param>
            /// <returns>System.Drawing.Icon</returns>
            public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
            {
                var shfi = new Shell32.Shfileinfo();
                var flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;
                if (linkOverlay) flags += Shell32.ShgfiLinkoverlay;
                /* Check the size specified for return. */
                if (IconSize.Small == size)
                    flags += Shell32.ShgfiSmallicon;
                else
                    flags += Shell32.ShgfiLargeicon;
                Shell32.SHGetFileInfo(name,
                    Shell32.FileAttributeNormal,
                    ref shfi,
                    (uint)Marshal.SizeOf(shfi),
                    flags);
                // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
                var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                User32.DestroyIcon(shfi.hIcon);     // Cleanup
                return icon;
            }
        }
        /// <summary>
        /// Wraps necessary Shell32.dll structures and functions required to retrieve Icon Handles using SHGetFileInfo. Code
        /// courtesy of MSDN Cold Rooster Consulting case study.
        /// </summary>
        static class Shell32
        {
            private const int MaxPath = 256;
            [StructLayout(LayoutKind.Sequential)]
            public struct Shfileinfo
            {
                private const int Namesize = 80;
                public readonly IntPtr hIcon;
                private readonly int iIcon;
                private readonly uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
                private readonly string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
                private readonly string szTypeName;
            };
            public const uint ShgfiIcon = 0x000000100;     // get icon
            public const uint ShgfiLinkoverlay = 0x000008000;     // put a link overlay on icon
            public const uint ShgfiLargeicon = 0x000000000;     // get large icon
            public const uint ShgfiSmallicon = 0x000000001;     // get small icon
            public const uint ShgfiUsefileattributes = 0x000000010;     // use passed dwFileAttribute
            public const uint FileAttributeNormal = 0x00000080;
            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref Shfileinfo psfi,
                uint cbFileInfo,
                uint uFlags
                );
        }
        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        static class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }
    }


}