using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Nethereum.Metamask;
//using Nethereum.JsonRpc.UnityClient;
using System;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Hex.HexTypes;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Newtonsoft.Json.Linq;
using ERC721ContractLibrary.Contracts.ERC721PresetMinterPauserAutoId.ContractDefinition;

//Test external contract 0x345c2fa23160c63218dfaa25d37269f26c85ca47
//0x2002050e7084f5db6ac4e81d54fbb6b35c257592 address

public class CustomMetamaskController : MonoBehaviour
{
    private string _selectedAccountAddress; // = "0x12890D2cce102216644c59daE5baed380d84830c";
    private bool _isMetamaskInitialised = false;
    private BigInteger _currentChainId; //444444444500;
    private string _currentContractAddress; // = "0x32eb97b8ad202b072fd9066c03878892426320ed";
    private bool bMetamaskEnabled = false;

    [SerializeField]
    private DebugUIManager ui;

    void Start()
    {
#if UNITY_WEBGL
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            MetamaskInterop.EnableEthereum(gameObject.name, nameof(EthereumEnabled), nameof(DisplayError));
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
        }
#endif
    }



    public IEnumerator MintNFT()
    {
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            var x = new MetamaskTransactionUnityRequest(GetRpcUrl(), _selectedAccountAddress);

            yield return x.SendTransaction<MintFunction>(new MintFunction() { To = _selectedAccountAddress }, _currentContractAddress, gameObject.name, nameof(MintedResponse), nameof(DisplayError));

        }
    }

    public void MintedResponse(string rpcResponse)
    {
        var txnHash = MetamaskTransactionUnityRequest.DeserialiseTxnHashFromResponse(rpcResponse);
        print("mint txnhash: " + txnHash);
        ui.SetTransactionHash(txnHash);
    }

    public IEnumerator GetAllNFTImages()
    {
        //_lstViewNFTs.hierarchy.Clear();
        var nftsOfUser = new NFTsOfUserUnityRequest(GetRpcUrl(), _selectedAccountAddress);
        yield return nftsOfUser.GetAllMetadataUrls(_currentContractAddress, _selectedAccountAddress);

        if (nftsOfUser.Exception != null)
        {
            DisplayError(nftsOfUser.Exception.Message);
            yield break;
        }

        if (nftsOfUser.Result != null)
        {
            var metadataUnityRequest = new NftMetadataUnityRequest<NftMetadata>();
            yield return metadataUnityRequest.GetAllMetadata(nftsOfUser.Result);

            if (metadataUnityRequest.Exception != null)
            {
                DisplayError(metadataUnityRequest.Exception.Message);
                yield break;
            }
            if (metadataUnityRequest.Result != null)
            {
                foreach (var item in metadataUnityRequest.Result)
                {
                    var image = new Image();
                    // _lstViewNFTs.hierarchy.Add(image);
                    StartCoroutine(new ImageDownloaderTextureAssigner().DownloadAndSetImageTexture(item.Image, image));
                }
            }
        }
    }


    private void _btnDeployNFTContract_clicked()
    {
        StartCoroutine(DeploySmartContract());
    }
    private void _btnViewNFTs_clicked()
    {
        StartCoroutine(GetAllNFTImages());
    }
    public IEnumerator DeploySmartContract()
    {
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            var x = new MetamaskTransactionUnityRequest(GetRpcUrl(), _selectedAccountAddress);

            var erc721PresetMinter = new ERC721PresetMinterPauserAutoIdDeployment()
            {
                BaseURI = "https://my-json-server.typicode.com/juanfranblanco/samplenftdb/tokens/", //This is a simple example using a centralised server.. use ipfs etc for a proper decentralised inmutable
                Name = "NFTArt",
                Symbol = "NFA"
            };

            yield return x.SendDeploymentContractTransaction<ERC721PresetMinterPauserAutoIdDeployment>(erc721PresetMinter, gameObject.name, nameof(DeploySmartContractResponse), nameof(DisplayError));

        }
    }

    public void DeploySmartContractResponse(string rpcResponse)
    {
        print("deployment response:" + rpcResponse);
        var txnHash = MetamaskTransactionUnityRequest.DeserialiseTxnHashFromResponse(rpcResponse);
        StartCoroutine(GetDeploymentSmartContractAddressFromReceipt(txnHash));
    }

    private IEnumerator GetDeploymentSmartContractAddressFromReceipt(string transactionHash)
    {
        print(transactionHash);
        //create a poll to get the receipt when mined
        var transactionReceiptPolling = new TransactionReceiptPollingRequest(GetRpcUrl());

        //checking every 2 seconds for the receipt
        yield return transactionReceiptPolling.PollForReceipt(transactionHash, 2);

        var deploymentReceipt = transactionReceiptPolling.Result;
        _currentContractAddress = deploymentReceipt.ContractAddress;
        //  _txtSmartContractAddress.value = deploymentReceipt.ContractAddress;
        print(_currentContractAddress);

        //
        ui.SetDeployedSmartContractAddress(_currentContractAddress);
    }

    public void EthereumEnabled(string addressSelected)
    {
#if UNITY_WEBGL
        if (!_isMetamaskInitialised)
        {
            MetamaskInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            _isMetamaskInitialised = true;
            bMetamaskEnabled = true;
            ui.SetMetamaskEnabled();
            ui.SetConnectedWalletText(addressSelected);
        }
        NewAccountSelected(addressSelected);
#endif
    }

    public void ChainChanged(string chainId)
    {
        print(chainId);
        _currentChainId = new HexBigInteger(chainId).Value;
        try
        {
            //simple workaround to show suported configured chains
            print(_currentChainId.ToString());
            StartCoroutine(GetBlockNumber());
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
        }
    }

    public void NewAccountSelected(string accountAddress)
    {

        _selectedAccountAddress = accountAddress;
        Debug.Log("UNITY: account address: " + accountAddress);
        //  _lblAccountSelected.text = accountAddress;
        //  _lblAccountSelected.visible = true;
    }


    public void DisplayError(string errorMessage)
    {
        Debug.Log("UNITY: DisplayError: " + errorMessage);
        // _lblError.text = errorMessage;
        // _lblError.visible = true;
    }


    private IEnumerator GetBlockNumber()
    {
        string url = GetRpcUrl();
        var blockNumberRequest = new EthBlockNumberUnityRequest(url);
        yield return blockNumberRequest.SendRequest();
        print("UNITY: " + blockNumberRequest.Result.Value);
    }

    public string GetRpcUrl()
    {
        string infuraId = "206cfadcef274b49a3a15c45c285211c";
        switch ((long)_currentChainId)
        {
            case 0: //not configured go to mainnet
            case 1:
                return "https://mainnet.infura.io/v3/" + infuraId;
            case 3:
                return "https://ropsten.infura.io/v3/" + infuraId;
            case 4:
                return "https://rinkeby.infura.io/v3/" + infuraId;
            case 42:
                return "https://kovan.infura.io/v3/" + infuraId;
            case 444444444500:
                return "http://localhost:8545";
            default:
                {
                    DisplayError("Chain: " + _currentChainId + " not configured");
                    break;
                }

        }

        throw new Exception("Chain: " + _currentChainId + " not configured");

    }

    public bool IsMetamaskEnabled()
    {
        return bMetamaskEnabled;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
