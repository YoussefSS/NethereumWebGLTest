using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private CustomMetamaskController MMController;

    [SerializeField]
    private bool bNFT = true;
    [SerializeField]
    private bool bSmartContract = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Collider>().gameObject.CompareTag("Player"))
        {
            if(bNFT)
            {
                StartCoroutine(MMController.MintNFT());
            }
            else if(bSmartContract)
            {
                StartCoroutine(MMController.DeploySmartContract());
            }
            
        }
    }
}
