using Solnet.Extensions.TokenMint;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Common.ShareUtils;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;

namespace SolanaTest
{
    public class ServiceTest
    {
        public Wallet _ownsWallet { get; set; }//测试使用的钱包

        public Wallet _secondWallet { get; set; }//测试钱包2
        public IRpcClient _rpcClient { get; set; }//测试请求的PRC

        public ServiceTest()
        {

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo.txt";
            string walletInfo = "";
            FileUtils.ReadFile(filePath, out walletInfo);
            Assert.True(!string.IsNullOrEmpty(walletInfo));

            string filePath2 = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo2.txt";
            string walletInfo2 = "";
            FileUtils.ReadFile(filePath2, out walletInfo2);
            Assert.True(!string.IsNullOrEmpty(walletInfo2));


            var tempWalletObj = walletInfo.ToObject<WalletDto>();//不能直接转为wallet 对象
            var tempWalletObj2 = walletInfo2.ToObject<WalletDto>();//不能直接转为wallet 对象
                                                                   // To initialize a wallet and have access to the same keys generated in sollet (the default)
            _ownsWallet = new Wallet(string.Join(" ", tempWalletObj.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);
            _secondWallet = new Wallet(string.Join(" ", tempWalletObj2.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);

            _rpcClient = ClientFactory.GetClient(Cluster.DevNet);//定义全局RPC

        }

        /// <summary>
        /// 查询各自钱包各类代币的余额
        /// </summary>
        [Fact]
        public async void GetWalletTokenBalancesTest() {
            var mintAccount = _ownsWallet.GetAccount(322);
            var testToken = new TokenDef(mintAccount.PublicKey, "CTK", "CTK", 2);
            var tokenDefList = new List<TokenDef>() { testToken };
            var ownsTokenBalances = await BaseService.GetWalletTokenBalanceAsync(_ownsWallet, tokenDefList);
            var secondTokenBalances = await BaseService.GetWalletTokenBalanceAsync(_secondWallet, tokenDefList);
        }
        /// <summary>
        /// 冻结账户测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FreezeTokenAccountTest()
        {
            // 代币的 mint 地址
            Solnet.Wallet.PublicKey mintAccount = new Solnet.Wallet.PublicKey("AhxWF63HAXZbFaCP6HAEVE8X5EYCBhbQVovBFtVqiQBi");
            // 代币账户地址(_secondWallet 里面的)
            Solnet.Wallet.PublicKey tokenAccount = new Solnet.Wallet.PublicKey("3DoNg4xg4TU6qisYvMysDCzCt8JH4AdnfMxfaqbg5h7e");

            // 代币程序的 ID
            Solnet.Wallet.PublicKey tokenProgramId = TokenProgram.ProgramIdKey;
            // 签名者列表，如果冻结权限是多签名则需要提供，这里假设不是多签名，传入 null
            IEnumerable<Solnet.Wallet.PublicKey> signers = null;
            var result =await BaseService.FreezeTokenAccountAsync(_ownsWallet.Account, mintAccount, tokenAccount, tokenProgramId, signers);
        }
        /// <summary>
        /// 查看代币账户状态
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAccountStatusAsyncTest() {
            // 代币的 mint 地址
            Solnet.Wallet.PublicKey mintAccount = new Solnet.Wallet.PublicKey("AhxWF63HAXZbFaCP6HAEVE8X5EYCBhbQVovBFtVqiQBi");
            // 代币账户地址(_secondWallet 里面的)
            Solnet.Wallet.PublicKey tokenAccount = new Solnet.Wallet.PublicKey("3DoNg4xg4TU6qisYvMysDCzCt8JH4AdnfMxfaqbg5h7e");

            // 代币程序的 ID
            Solnet.Wallet.PublicKey tokenProgramId = TokenProgram.ProgramIdKey;
            var result = await BaseService.CheckAccountStatusAsync(tokenAccount);
            if (result.Data == Solnet.Programs.Models.TokenProgram.TokenAccount.AccountState.Frozen) {
                IEnumerable<Solnet.Wallet.PublicKey> signers = null;
               var thawResult= await BaseService.ThawTokenAccountAsync(_ownsWallet.Account, mintAccount, tokenAccount, tokenProgramId, signers);
            }
            var statusResult = await BaseService.CheckAccountStatusAsync(tokenAccount);
        }

        /// <summary>
        /// 交易测试
        /// </summary>
        [Fact]
        public async Task TransferTokenTest() {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            Console.WriteLine($"张三 管理账户OwnerAccount: {ownerAccount}");
            var zhangsanAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_ownsWallet.Account.PublicKey, mintAccount.PublicKey);
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
            Console.WriteLine($"张三 关联自定义Token的InitialAccount: {initalAccount}");

            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);

            var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
            #region 处理关联账户
            if (getLisiAtaInfo.Result.Value == null)
            {
                // 如果李四的关联代币账户不存在，创建它
                var createAtaInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                     _secondWallet.Account.PublicKey,
                     _secondWallet.Account.PublicKey,
                    mintAccount.PublicKey
                );
            }
            #endregion 
            ulong amount = 1000000;
            try
            {
                var resut =await SPLTokenService.TransferSPLTokenAsync(mintAccount, ownerAccount, initalAccount, lisiAssociatedTokenAccount, amount, null);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        
    } 
}
