using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUIManager : MonoBehaviour
{
    [SerializeField]
    private CustomMetamaskController MMController;

    [SerializeField]
    private TextMeshProUGUI MetamaskConnection;

    [SerializeField]
    private TextMeshProUGUI ConnectedWallet;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMetamaskEnabled()
    {
        MetamaskConnection.text = "Connected!";
    }

    public void SetConnectedWalletText(string walletAddress)
    {
        ConnectedWallet.text = walletAddress;
    }
}
