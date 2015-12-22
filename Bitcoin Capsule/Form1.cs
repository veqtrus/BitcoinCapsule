using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Bitcoin_Capsule
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtAddress.Text.StartsWith("1"))
                {
                    BitcoinAddress a = new BitcoinAddress(txtAddress.Text);
                    DateTimeOffset time = new DateTimeOffset(timeLock.Value, TimeSpan.Zero);
                    Script r;
                    BitcoinAddress na = TimeLock.ConvertAddress(a, time, out r);
                    txtNewAddress.Text = na.ToString();
                    txtRedeemScript.Text = r.ToHex();
                }
                else
                {
                    MessageBox.Show("Address must be P2PKH.", "Error - Bitcoin Capsule");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error - Bitcoin Capsule");
            }
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            try
            {
                Script r = new Script(Encoders.Hex.DecodeData(txtRedeemScriptVer.Text));
                BitcoinAddress a = r.GetScriptAddress(Network.Main);
                BitcoinAddress ea = TimeLock.EncapsulatedAddressFromRedeemScript(r);
                DateTimeOffset t = TimeLock.LockTimeFromRedeemScript(r).Date;
                txtAddressVer.Text = a.ToString();
                txtEncAddressVer.Text = ea.ToString();
                txtLockTimeVer.Text = t.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error - Bitcoin Capsule");
            }
        }

        private void btnAddInput_Click(object sender, EventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(txtHash.Text) || string.IsNullOrWhiteSpace(txtIndex.Text) || string.IsNullOrWhiteSpace(txtAmount.Text)))
            {
                listInputs.Items.Add(new ListViewItem(new string[] { txtHash.Text, txtIndex.Text, txtAmount.Text }));
                txtHash.Text = txtIndex.Text = txtAmount.Text = "";
            }
        }

        private void btnRemoveInput_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listInputs.SelectedItems)
            {
                listInputs.Items.Remove(item);
            }
        }

        private void btnSpend_Click(object sender, EventArgs e)
        {
            try
            {
                List<OutPoint> outs = new List<OutPoint>();
                Money sum = Money.Zero;
                foreach (ListViewItem item in listInputs.Items)
                {
                    uint256 h = new uint256(item.SubItems[0].Text);
                    int n = Convert.ToInt32(item.SubItems[1].Text);
                    Money m = new Money(Convert.ToDecimal(item.SubItems[2].Text), MoneyUnit.BTC);
                    outs.Add(new OutPoint(h, n));
                    sum += m;
                }
                Key priv = Key.Parse(txtPrivKey.Text);
                Script r = new Script(Encoders.Hex.DecodeData(txtRedeemScriptSpend.Text));
                BitcoinAddress a = new BitcoinAddress(txtAddressSpend.Text);
                Money fee = new Money(numericFee.Value, MoneyUnit.BTC);
                Transaction tx = TimeLock.Spend(a, fee, sum, priv, r, outs.ToArray());
                txtTx.Text = tx.ToHex();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error - Bitcoin Capsule");
            }
        }

        private void linkLockTimeInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Due to upcoming changes to the protocol it is suggested to subtract about one hour from the desired lock time.", "Bitcoin Capsule");
        }
    }
}
