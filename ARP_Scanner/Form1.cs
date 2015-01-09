using System;
using System.Drawing;
using System.Windows.Forms;
using ArpCs;
using System.Threading.Tasks;

namespace ARP_Scanner
{
    public partial class Form1 :Form
    {
        public Form1()
        {
            InitializeComponent();

            dgvResult.Columns.Add("IP Address", "IP Address");
            dgvResult.Columns.Add("Result", "Result");
            dgvResult.Columns.Add("MAC Address", "MAC Address");
            dgvResult.Columns[2].Width = 150;
            Text = ProductName.Replace('_', ' ') + " ver. " + ProductVersion;
        }

        /// <summary>
        /// テキストボックスに入力されてるIPアドレスを取得
        /// </summary>
        /// <param name="IPAddress1"></param>
        /// <param name="IPAddress2"></param>
        private void GetIPValue(out int IPAddress1, out int IPAddress2)
        {
            IPAddress1 = ARP.IPAddressStr2Num(txtIPAddressFrom.Text);
            IPAddress2 = ARP.IPAddressStr2Num(txtIPAddressTo.Text);

            // IPAddress1のほうが大きい場合は逆にする
            if (IPAddress1 > IPAddress2) {
                int tmp = IPAddress1;
                IPAddress1 = IPAddress2;
                IPAddress2 = tmp;
            }
        }

        /// <summary>
        /// 結果を格納する行を追加する
        /// </summary>
        /// <param name="IPAddress1"></param>
        /// <param name="IPAddress2"></param>
        /// <returns></returns>
        private int CreateGrid(int IPAddress1, int IPAddress2)
        {
            int pastRowsCount = dgvResult.Rows.Count;
            for (int i = IPAddress1; i <= IPAddress2; ++i) {
                dgvResult.Rows.Add();
                dgvResult[0, i - IPAddress1 + pastRowsCount].Value = ARP.IPAddressNum2Str(i);
            }
            return pastRowsCount;
        }

        private void SetBtnStartState(bool isActive)
        {
            btnClear.Enabled = !isActive;
            btnStart.Checked = isActive;
            btnStart.Text = isActive ? "Stop" : "Start";
        }

        /// <summary>
        /// ARPコマンドを送って結果を表に書き込む
        /// </summary>
        /// <param name="IPAddress1"></param>
        /// <param name="IPAddress2"></param>
        /// <param name="startRowIndex"></param>
        private void SendARP(int IPAddress1, int IPAddress2, int startRowIndex)
        {
            int count = 0;
            int maxCount = IPAddress2 - IPAddress1 + 1;
            Parallel.For(IPAddress1, IPAddress2 + 1, (ip, loopState) =>
                {
                    string mac;
                    string ipAddress = ARP.IPAddressNum2Str(ip);
                    bool result = ARP.SendARP(ipAddress, out mac);
                    int row = ip - IPAddress1 + startRowIndex;

                    if (loopState.ShouldExitCurrentIteration) {
                        return;
                    }

                    Invoke((MethodInvoker)(() =>
                    {
                        dgvResult[1, row].Value = result ? "FOUND" : "NOT FOUND";
                        dgvResult[2, row].Value = mac;
                        dgvResult.Rows[row].DefaultCellStyle.BackColor = result ? Color.Pink : Color.Empty;

                        ++count;
                        pgbARP.Value = count;
                        lblCount.Text = count.ToString() + " / " + maxCount.ToString();
                        if (isStop) {
                            loopState.Stop();
                            isStop = false;
                            SetBtnStartState(false);
                            lblCount.Text = "Stop";
                            return;
                        }
                        if (count == maxCount) {
                            SetBtnStartState(false);
                        }
                    }));
                });
        }

        bool isStop = false;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Checked) {
                isStop = false;
                SetBtnStartState(true);
                int IPAddress1;
                int IPAddress2;
                try {
                    GetIPValue(out IPAddress1, out IPAddress2);

                    pgbARP.Maximum = IPAddress2 - IPAddress1 + 1;
                    pgbARP.Value = 0;

                    // 表の行を追加してIPアドレスを書きこむ
                    int startRowIndex = CreateGrid(IPAddress1, IPAddress2);

                    // ARPコマンドを送信して表に結果を書き込む
                    Task.Factory.StartNew(() =>
                        {
                            SendARP(IPAddress1, IPAddress2, startRowIndex);
                        });
                } catch (Exception ex) {
                    isStop = true;
                    MessageBox.Show(ex.Message);
                    SetBtnStartState(false);
                }
            } else {
                isStop = true;
                SetBtnStartState(false);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvResult.Rows.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
