using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Bitcoin_Capsule
{
    static class TimeLock
    {
        public static BitcoinAddress ConvertAddress(BitcoinAddress address, DateTimeOffset timelock, out Script redeemScript)
        {
            if (address.Type == Base58Type.PUBKEY_ADDRESS)
            {
                redeemScript = new Script(Op.GetPushOp(Utils.DateTimeToUnixTime(timelock)).ToString() + " OP_CLTV OP_DROP " + address.ScriptPubKey.ToString());
                return new BitcoinScriptAddress(redeemScript.Hash, Network.Main);
            }
            throw new ArgumentException("Address must be P2PKH.", "address");
        }

        public static LockTime LockTimeFromRedeemScript(Script redeemScript)
        {
            Op[] op = redeemScript.ToOps().ToArray();
            return new LockTime(BitConverter.ToUInt32(op[0].PushData, 0));
        }

        public static BitcoinAddress EncapsulatedAddressFromRedeemScript(Script redeemScript)
        {
            Op[] op = redeemScript.ToOps().ToArray();
            Script script = new Script(op[3], op[4], op[5], op[6], op[7]);
            return script.GetDestinationAddress(Network.Main);
        }

        public static Transaction Spend(BitcoinAddress destination, Money fees, Money vin, Key privKey, Script redeemScript, params OutPoint[] outpoints)
        {
            Transaction tx = new Transaction() { LockTime = LockTimeFromRedeemScript(redeemScript) };
            tx.AddOutput(vin - fees, destination);
            foreach (OutPoint item in outpoints)
            {
                tx.AddInput(new TxIn(item) { Sequence = 0 });
            }
            //Script[] sigs = new Script[tx.Inputs.Count];
            for (int i = 0; i < tx.Inputs.Count; i++)
            {
                TransactionSignature sig = tx.SignInput(privKey, redeemScript, i);
                Script p2pkhProof = PayToPubkeyHashTemplate.Instance.GenerateScriptSig(sig, privKey.PubKey);
                tx.Inputs[i].ScriptSig = new Script(p2pkhProof.ToString() + ' ' + redeemScript.ToHex());
            }
            //for (int i = 0; i < tx.Inputs.Count; i++)
            //{
            //    tx.Inputs[i].ScriptSig = sigs[i];
            //}
            return tx;
        }
    }
}
