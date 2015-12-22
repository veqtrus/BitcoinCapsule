# Bitcoin Capsule

## Creating time locked addresses

1. Generate a new P2PKH address using your wallet software or a tool like vanitygen.
2. Choose a lock time in the UTC timezone. Due to upcoming changes to the protocol it is suggested to subtract about one hour from the desired lock time.
3. Click *Create*.
4. Save the redeem script and address.

## Spending funds received to a time locked address

1. Identify the hashes of the funding transactions, the indices of the outputs (starting from zero) and their amounts.
2. Input the private key of the encapsulated address, the redeem script and the destination address.
3. Adjust the fee.
4. Click *Spend*.
5. Copy the hexadecimal representation of the transaction and broadcast it.
