﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Tibia.Objects;
using Tibia;
using Tibia.Packets;
using Tibia.Util;
using System.Diagnostics;

namespace BynaCam_Recorder
{
    public partial class Form1 : Form
    {
        StreamWriter file;
        Client c;
        Stopwatch w = new Stopwatch();
        TimeSpan time;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClientChooserOptions op = new ClientChooserOptions();
            op.ShowOTOption = false;

            c = ClientChooser.ShowBox();
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "BynaCam files|*.byn";

            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                    MessageBox.Show(null, "You must choose file to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    notifyIcon1.Visible = false;
                    Process.GetCurrentProcess().Kill();
                    try { c.Process.Kill(); }
                    catch { }
            }

            file = new StreamWriter(File.Create(dialog.FileName));

            if (c != null)
            {
                notifyIcon1.ShowBalloonTip(5000, "BynaCam", "BynaCam is waiting for login...", ToolTipIcon.Info);
                c.Exited += new EventHandler(c_Exited);
                c.StartProxy();
                c.Proxy.IncomingSplitPacket += new SocketBase.SplitPacket(Proxy_IncomingSplitPacket);
                c.Proxy.ReceivedPlayerSpeechOutgoingPacket += new SocketBase.OutgoingPacketListener(Proxy_ReceivedPlayerSpeechOutgoingPacket);
            }
            else
            {
                MessageBox.Show("Choose your client first!!");
                notifyIcon1.Visible = false;
                Process.GetCurrentProcess().Kill();
            }
         }

        private bool Proxy_ReceivedPlayerSpeechOutgoingPacket(OutgoingPacket packet)
        {
            Tibia.Packets.Outgoing.PlayerSpeechPacket p = (Tibia.Packets.Outgoing.PlayerSpeechPacket)packet;

            if (p.SpeechType == SpeechType.Private)
            {
                Invoke(new Action(delegate()
                {
                    //(c, p.Receiver, 0, ">> " + p.Message, SpeechType.Private, p.ChannelId);

                    Tibia.Packets.Incoming.CreatureSpeechPacket pack = new Tibia.Packets.Incoming.CreatureSpeechPacket(c);
                    pack.SenderName = p.Receiver;
                    pack.SenderLevel = 0;
                    pack.Message = ">> " + p.Message;
                    pack.SpeechType = p.SpeechType;
                    pack.ChannelId = p.ChannelId;
                    pack.Position = Tibia.Objects.Location.Invalid;
                    pack.Time = 0;

                    file.WriteLine(w.Elapsed.ToString()+"\r\n");
                    file.WriteLine(pack.ToByteArray().ToHexString() + "\r\n");
                    file.Flush();
                }));
            }

            return true;
        }

        private void Proxy_IncomingSplitPacket(byte type, byte[] data)
        {
            file.Flush();

            if (!w.IsRunning)
            {
                BeginInvoke(new Action(delegate() { this.Hide(); }));
                notifyIcon1.ShowBalloonTip(5000, "BynaCam", "BynaCam is recording...", ToolTipIcon.Info);
                w.Start();
                time = w.Elapsed;
            }

            LogPacket(data, w.Elapsed - time);
            time = w.Elapsed;
        }

        private void LogPacket(byte[] data, TimeSpan time)
        {
            BeginInvoke(new Action(delegate()
                {
                    file.WriteLine(time);
                    file.WriteLine(data.ToHexString());
                    file.Flush();
                }));
        }

        private void saveAndExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file.Flush();
            notifyIcon1.Visible = false;
            c.Process.Kill();
            Process.GetCurrentProcess().Kill();
        }

        private void c_Exited(object sender, EventArgs e)
        {
            file.Flush();
            notifyIcon1.Visible = false;
            Process.GetCurrentProcess().Kill();
        }
    }
}
