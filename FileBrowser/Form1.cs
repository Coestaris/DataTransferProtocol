using System;
using System.Collections.Generic;

using System.Windows.Forms;
using CWA.DTP;
using System.IO.Ports;

namespace FileBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static DtpSender sender = new DtpSender(DtpSenderType.SevenByteName, "Coestar");

        static SerialPort port = new SerialPort("COM5", 115200);

        static PacketHandler ph = new PacketHandler(sender, new DtpPacketListener(new SerialPacketReader(port, 4000), new SerialPacketWriter(port)));
        
        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.View =View.Details;
            listView1.FullRowSelect = true;

            listView1.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader() {Text = "Name" },
                new ColumnHeader() {Text = "Size" },
                new ColumnHeader(){Text = "Creation Date" },
                new ColumnHeader(){Text = "Flags" }
            } );

            List<string> files = new List<string>();
            List<string> dirs = new List<string>();

            if (!ph.DTP_GetDirectoriesAndFiles("/", out dirs,out files)) MessageBox.Show("Cant get root");

            foreach(var a in dirs)
            {
                var res = ph.DTP_GetFileInfo(a);
                ListViewItem item = new ListViewItem(new string[] { '[' + a + ']', "<folder>", res.CreationTime.ToString(), "____" }, 0);
                listView1.Items.Add(item);
            }

            foreach (var a in files)
            {
                var res = ph.DTP_GetFileInfo(a);
                ListViewItem item = new ListViewItem(new string[] { a, res.FileSize.ToString(), res.CreationTime.ToString(), "____" }, 0);
                listView1.Items.Add(item);
            }

        }
    }
}
