﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Media;

namespace WSJTX_Controller
{
    public partial class SetupDlg : Form
    {
        public SetupDlg()
        {
            InitializeComponent();
        }


        public WsjtxClient wsjtxClient;
        public Controller ctrl;
        public int pct = 0;
        private int pctIdx = 0;

        private List<int> pcts = new List<int>() { 0, 25, 50 };

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            bool multicast = multicastcheckBox.Checked;
            bool overrideUdp = overrideCheckBox.Checked;
            UInt16 port;
            IPAddress ipAddress;


            if (!UInt16.TryParse(portTextBox.Text, out port))
            {
                MessageBox.Show("A port number must be between 0 and 65535.\n\nExample: 2237", wsjtxClient.pgmName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (addrTextBox.Text.Split('.').Length != 4)
            {
                string ex = multicast ? "239.255.0.0" : "127.0.0.1";
                MessageBox.Show($"An IP address must be 4 numbers between 0 and 255, each separated by a period.\n\nExample: {ex}", wsjtxClient.pgmName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                ipAddress = IPAddress.Parse(addrTextBox.Text);
            }
            catch (Exception err)
            {
                err.ToString();
                string ex = multicast ? "239.255.0.0" : "127.0.0.1";
                MessageBox.Show($"An IP address must be 4 numbers between 0 and 255, each separated by a period.\n\nExample: {ex}", wsjtxClient.pgmName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //if no change, no need to notify/exit
            if (wsjtxClient.ipAddress.ToString() == ipAddress.ToString() && wsjtxClient.port == port && wsjtxClient.multicast == multicast && overrideUdp)
            {
                SaveMisc();
                Close();
                return;
            }

            SaveMisc();
            wsjtxClient.UpdateAddrPortMulti(ipAddress, port, multicast, overrideUdp);
            Close();
        }

        private void SaveMisc()
        {
            ctrl.alwaysOnTop = onTopCheckBox.Checked;
            wsjtxClient.LogModeChanged(diagLogCheckBox.Checked);
            if (pcts[pctIdx] != pct) ctrl.ResizeForm(pcts[pctIdx]);
        }

        private void SetupDlg_Load(object sender, EventArgs e)
        {
            overrideCheckBox.Checked = wsjtxClient.overrideUdpDetect;
            addrTextBox.Text = wsjtxClient.ipAddress.ToString();
            portTextBox.Text = wsjtxClient.port.ToString();
            multicastcheckBox.Checked = wsjtxClient.multicast;
            multicastcheckBox_CheckedChanged(null, null);
            onTopCheckBox.Checked = ctrl.alwaysOnTop;
            diagLogCheckBox.Checked = wsjtxClient.diagLog;
            diagLogCheckBox.Visible = true;
            overrideCheckBox_CheckedChanged(null, null);

            pctIdx = pcts.IndexOf(pct);
            if (pctIdx < 0) pctIdx = 1;
            UpdatePctLabel();
        }

        private void multicastcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (multicastcheckBox.Checked)
            {
                addrLabel.Text = "(Standard: 239.255.0.0)";
            }
            else
            {
                addrLabel.Text = "(Standard: 127.0.0.1)";
            }
        }

        private void udpHelpLabel_Click(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(delegate
            {
                MessageBox.Show
                (
                  $"This information allows communication with WSJT-X.{Environment.NewLine}{Environment.NewLine}- In the WSJT-X program, select File | Settings | Reporting.{Environment.NewLine}{Environment.NewLine}- Enter the UDP server address (xxx.xxx.xxx.xxx) and port number shown there.{Environment.NewLine}{Environment.NewLine}- Make sure 'Accept UDP requests' is enabled in WSJT-X.{Environment.NewLine}{Environment.NewLine}Note: Select 'Multicast' here (entering the standard UDP address and port number) if other WSJT-X helper programs (loggers, maps, etc.) will be used.",
                  wsjtxClient.pgmName,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information
                );
            })).Start();
        }

        private void overrideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            addrTextBox.Enabled = portTextBox.Enabled = multicastcheckBox.Enabled = overrideCheckBox.Checked;
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (pctIdx < pcts.Count - 1) pctIdx++;
            UpdatePctLabel();
        }

        private void downButton_Click(object sender, EventArgs e)
        {

            if (pctIdx > 0) pctIdx--;
            UpdatePctLabel();
        }

        private void UpdatePctLabel()
        {
            pctLabel.Text = $"{(pcts[pctIdx] + 100).ToString()}%";
            upButton.Enabled = pctIdx < pcts.Count - 1;
            downButton.Enabled = pctIdx > 0;
        }

        private void SetupDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            ctrl.SetupDlgClosed();
        }
    }
}
