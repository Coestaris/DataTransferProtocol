using CWA.DTP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileBrowser
{
    public partial class SendDialog : Form
    {
        /*// private PacketHandler ph;
         private string OldName;
         private string NewName;
         private DateTime startTime;
         private long totalBytes;
         private FileSender fileSender;
         private int PacketSize = 1000;

         public SendDialog()
         {
             InitializeComponent();
         }

         public SendDialog(PacketHandler ph, string oldName, string newName)
         {
             this.ph = ph;
             OldName = oldName;
             NewName = newName;
             InitializeComponent();
             startTime = DateTime.Now;
             fileSender = new FileSender(ph, FileSender.FileSenderSecurityFlags.VerifyLengh)
             {
                PacketLength = PacketSize
             };
             fileSender.SendingProcessChanged += FileSender_SendingProcessChanged;
             fileSender.SendingError += FileSender_SendingError;
             fileSender.SendingEnd += FileSender_SendingEnd;
             totalBytes = new System.IO.FileInfo(oldName).Length;
             fileSender.SendFileAsync(oldName, newName);
             label_name.Text = string.Format("{0}  ->  {1}", oldName, newName);
         }


         private void FileSender_SendingEnd(FileSender.FileTransferEndArgs arg)
         {
             Console.WriteLine("Done in {0}! Speed {1:0.##} KBytes/s", arg.TimeSpend, totalBytes / arg.TimeSpend / 1024);
             FormCloseThread();
         }

         private void FileSender_SendingError(FileSender.FileSenderErrorArgs arg)
         {
             Console.Write("ERROR! Code {0}, IsCritical {1}", arg.Error.ToString(), arg.IsCritical);
             MessageBox.Show(string.Format("Проиошла ошибка. Код ошибки: {0}.\nВозможно продолжать отравку: {1}", arg.Error, arg.IsCritical ? "нет" : "да"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
             if(arg.IsCritical) FormCloseThread();
         }

         private delegate void FormCloseThreadFeedBack();
         private delegate void SendingProcessChangedThreadFeedBack(FileSender.FileTransferProcessArgs arg);

         private void FormCloseThread()
         {
             if (InvokeRequired)
             {
                 FormCloseThreadFeedBack d = new FormCloseThreadFeedBack(FormCloseThread);
                 Invoke(d, new object[] {  });
             }
             else
             {
                 Close();
             }
         }

         private void SendingProcessChangedThread(FileSender.FileTransferProcessArgs arg)
         {
             if (InvokeRequired)
             {
                 SendingProcessChangedThreadFeedBack d = new SendingProcessChangedThreadFeedBack(SendingProcessChangedThread);
                 Invoke(d, new object[] { arg });
             }
             else
             {
                 var total = arg.PacketSended + arg.PacketsLeft;
                 progressBar1.Value = (int)arg.PacketSended;
                 progressBar1.Maximum = (int)total;
                 label_percentage.Text = string.Format("Done {0:0.##}%", (double)arg.PacketSended / total * 100);
                 label_timeleft.Text = string.Format("Time Left: {0:0.#}%", arg.TimeLeft);
                 label_speed.Text = string.Format("Speed: {0:0.##} KBytes", arg.Speed);
             }
         }

         private void FileSender_SendingProcessChanged(FileSender.FileTransferProcessArgs arg)
         {
             var total = arg.PacketSended + arg.PacketsLeft;
             Console.WriteLine("[{2:0}%]. Packet#{0}/{1}. Time Left: {3:0.####} sec. Speed: {4:0.####}KBytes", arg.PacketSended, total, (double)arg.PacketSended / total * 100, arg.TimeLeft, arg.Speed);
             SendingProcessChangedThread(arg);
         }
     }*/
    }
}