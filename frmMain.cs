using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace OpenPGPExplorer
{
    public partial class frmMain : Form
    {
        
        public frmMain()
        {
            InitializeComponent();

            OpenPGP.StatusUpdate += StatusUpdateCallback;
            Worker.StatusUpdate += StatusUpdateCallback;
            Worker.AddComplete += AddComplete;
            Worker.ProcessComplete += ProcessComplete;
            Worker.Error += WorkerError;
        }

        private void ResetStatus()
        {
            pbStatus.Value = 0;
            pbStatus.Visible = false;
            lblStatus.Text = "";
        }

        private void ProcessComplete(ByteBlock BB, TreeNode tn)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ProcessCompleteDelegate(ProcessComplete), new object[] { BB, tn });
                return;
            }

            ResetStatus();
            if (tn.Nodes.Count == 0)
            {
                CreateBlockTree(tn, BB.ChildBlock);
                RefreshNodes(tn.Nodes);
            }
        }

        private void StatusUpdateCallback(string Message, int Percent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StatusUpdates(StatusUpdateCallback), new object[] { Message, Percent });
                return;
            }

            lblStatus.Text = Message;

            if (Percent >= 0 && Percent <=100)
            {
                pbStatus.Visible = true;
                pbStatus.Value = Percent;

            }

        }      

        private void AddComplete(string FileName, ByteBlock StartBlock)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new AddCompleteDelegate(AddComplete), new object[] { FileName, StartBlock });
                return;
            }

            ResetStatus();

            var tn = treeView1.Nodes.Add(FileName);
            tn.ImageKey = "file";

            tn.NodeFont = new Font(treeView1.Font, FontStyle.Bold);
            tn.Text = FileName;
            CreateBlockTree(tn, StartBlock);

            RefreshNodes(treeView1.Nodes);
        }

        private void WorkerError(string Message, bool Reset)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ErrorDelegate(WorkerError), new object[] { Message, Reset });
                return;
            }

            if (Reset)
                ResetStatus();
            MessageBox.Show(this, Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void CreateBlockTree(TreeNode tn, ByteBlock bt)
        {
            while (bt != null)
            {
                var newNode = tn.Nodes.Add(bt.Label + (string.IsNullOrEmpty(bt.Description) ? "" : ": " + bt.Description));                
                newNode.Tag = bt;
                if (bt.ChildBlock != null)
                    CreateBlockTree(newNode, bt.ChildBlock);
                bt = bt.NextBlock;
            }
        }

        private void RefreshNodes(TreeNodeCollection col)
        {
            if (col == null)
                return;

            foreach(TreeNode n in col)
            {
                if (n.Tag is ByteBlock bt)
                {
                    if (bt.Type == ByteBlockType.Calculated)
                        n.ForeColor = Color.Blue;
                    else if (bt.Type == ByteBlockType.CalculatedError)
                        n.ForeColor = Color.Red;
                    else if (bt.Type == ByteBlockType.CalculatedSuccess)
                        n.ForeColor = Color.Green;

                    if (bt is PacketBlock pb)
                    {
                        n.ToolTipText = pb.Message;

                        if (pb.PGPPacket is PublicKeyPacket)
                            n.ImageKey = "key";
                        else if (pb.PGPPacket is UserIDPacket)
                            n.ImageKey = "user";
                        else if (pb.PGPPacket is SignaturePacket)
                            if (pb.Type == ByteBlockType.CalculatedSuccess)
                                n.ImageKey = "tick";
                            else
                                n.ImageKey = "cross";
                        else
                            n.ImageKey = "packet";
                    }
                }
                RefreshNodes(n.Nodes);
            }
        }



        private void Clear()
        {
            treeView1.Nodes.Clear();
            txtValue.Text = "";
            txtValueText.Text = "";
            OpenPGP.Reset();
        }

       

        private void DoNodeAction(TreeNode tn)
        {
            if (tn.Tag != null && tn.Nodes.Count == 0)
            {
                var BB = (ByteBlock)tn.Tag;
                txtValue.Text = BitConverter.ToString(BB.RawBytes);
                txtValueText.Text = Encoding.UTF8.GetString(BB.RawBytes);

                if (BB.ProcessBlock != null)
                    Worker.ProcessBlock(BB, tn);
            }
        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            
            FileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog(this) == DialogResult.OK)
            {                

                Worker.AddFile(fd.FileName);                
            }
            

        }


        private void cmdClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            DoNodeAction(e.Node);
        }
    }
}
