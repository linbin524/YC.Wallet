using Mapster;
using Solnet.Metaplex.NFT.Library;
using Solnet.Programs;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Model;

namespace YC.SolanaSdkService
{
    public class TokenLockingService
    {
        internal static IRpcClient _rpcClient { get => BasicConfig.RpcClient; }


        /// <summary>
        /// 锁仓操作，交易手续费由 超级账户出，真正逻辑应该各自转入锁仓账户，交易费各自出
        /// </summary>
        /// <param name="parties">参与方列表，包含发起方A、参与方B、见证方C</param>
        /// <param name="lockAccount">锁仓账户</param>
        /// <param name="amount">锁仓数量</param>
        /// <returns>交易签名</returns>
        public static async Task<string> LockTokensAsync(Account payFeeAccount, List<Account> parties,  PublicKey lockAccount, int lockAccountCount,PublicKey tokenMint, ulong amount)
        {
            if (parties.Count != lockAccountCount)
            {
                throw new ArgumentException("必须提供指定的锁仓用户数。");
            }

            var transaction = new TransactionBuilder();
            foreach (var party in parties)
            {
                var sourceTokenAccount = await GetAssociatedTokenAccount(party, tokenMint);
                var transferInstruction = TokenProgram.Transfer(
                    sourceTokenAccount,
                    lockAccount,
                    amount,
                    party.PublicKey
                   );
                transaction = transaction.AddInstruction(transferInstruction);
            }

            var getLastestBlockHash = await BaseService.GetLatestBlockHashAsync();
            transaction = transaction.SetRecentBlockHash(getLastestBlockHash.Data)
                .SetFeePayer(payFeeAccount.PublicKey);
            var signers = new List<Account> { payFeeAccount };
            signers.AddRange(parties);
            var signedTransaction = transaction.Build(signers);
            var result = await _rpcClient.SendTransactionAsync(signedTransaction);
            return result.Result;
        }

        /// <summary>
        /// 解锁操作，
        /// 正常解锁逻辑，由锁仓账户分别给A、B、C等分发，但是交易字符串要得到两家以上的签名
        /// 
        /// 有漏洞，如果拿到锁仓的钱包，就可以直接转了，所以还是要在合约里限制
        /// </summary>
        /// <param name="parties">发起解锁的参与方列表</param>
        /// <param name="lockAccount">锁仓账户</param>
        /// <param name="amount">解锁数量</param>
        /// <returns>交易签名</returns>
        public static async Task<string> UnlockTokensAsync(Account payFeeAccount, List<Account> parties, PublicKey lockAccount, int unLockAccountCount, PublicKey tokenMint, ulong amount)
        {
            if (parties.Count <= unLockAccountCount)
            {
                throw new ArgumentException($"解锁需要至少{unLockAccountCount}参与方签名。");
            }

            var transaction = new TransactionBuilder();
            foreach (var party in parties)
            {
                var destinationTokenAccount = await GetAssociatedTokenAccount(party, tokenMint);
                var transferInstruction = TokenProgram.Transfer(
                    lockAccount,
                    destinationTokenAccount,
                    amount,
                    lockAccount
                    );
                transaction = transaction.AddInstruction(transferInstruction);
            }

            var getLastestBlockHash = await BaseService.GetLatestBlockHashAsync(); 
            transaction = transaction.SetRecentBlockHash(getLastestBlockHash.Data).SetFeePayer(payFeeAccount);
            var msgData= transaction.CompileMessage();///将三者分发的交易组合交易信息
           
            var msgStr= Convert.ToBase64String(msgData);///正常应该是将这个字符串分发给各自参与者
            Message msg = Message.Deserialize(msgStr);
            var signTrans = new List<byte[]>();
           var signers= parties.Take(unLockAccountCount);//获取至少解锁签名的参与者
            foreach (var party in signers) {
                signTrans.Add(party.Sign(msgData));//真实的业务，是要将这个交易字符串方法给多方参与者，又它们去签名后返回
            }
           Transaction tx = Transaction.Populate(msg, signTrans);//将交易信息分别签名

            var signedTransaction = tx.Build(payFeeAccount);
            var result = await _rpcClient.SendTransactionAsync(signedTransaction);
            return result.Result;
        }

        private static async Task<PublicKey> GetAssociatedTokenAccount(Account owner, PublicKey tokenMint)
        {
            var res=  await BaseService.GetAssociatedTokenAccountAsync(owner, tokenMint, owner);

            return res.Data;
        }
    }
}
