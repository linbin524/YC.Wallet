using Solnet.Metaplex.NFT;
using Solnet.Metaplex.NFT.Library;
using Solnet.Rpc;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Common.ShareUtils;
using YC.SolanaSdkService.DTO;

namespace SolanaTest
{
    public class NFTTest
    {
        public Wallet _ownsWallet { get; set; }//测试使用的钱包

        public Wallet _secondWallet { get; set; }//测试钱包2
        public IRpcClient _rpcClient { get; set; }//测试请求的PRC

        public NFTTest()
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

        [Fact]
        public async void CreateNFTTest() {

            var ownerAccount = _secondWallet.Account;
            var mintAccount = _secondWallet.GetAccount(712);
            //Create the creator list
            List<Creator> creatorList = new List<Creator>
            {
                new Creator(ownerAccount.PublicKey, 100, true)
            };
            Metadata tokenMetadata = new Metadata
            {
                name = "碳币-NFT",
                symbol = "CTK-NFT",
                sellerFeeBasisPoints = 500,
                uri = "arweave link",
                creators = creatorList,

                //If your NFT has a parent collection NFT. You can specify it here
                //collection = new Collection(collectionAddress),

                //uses = new Uses(UseMethod.Single, 5, 5),

                //If your NFT is programmable and has a ruleset then specify it here
                //programmableConfig = new ProgrammableConfig(rulesetAddress)
            };
            
            //Easily create any type of metadata token. Any nullable parameters can be overrided to provide the data needed to create complex metadata tokens or use legacy instructions
            MetadataClient metaplexClient = new MetadataClient(_rpcClient);
            //如果设置TokenStandard.Fungible等原版的，那么isMasterEdition必须要是true，如过 TokenStandard.Fungible,那么isMasterEdition必须要是false
            var tx = await metaplexClient.CreateNFT(ownerAccount, mintAccount, TokenStandard.Fungible, tokenMetadata, true, true);
            Console.WriteLine(tx.RawRpcResponse);
        }
    }
}
