using UnityEngine;
using System.Collections;
using SDK;

public class IAPManager : MonoBehaviour {
    static IAPManager _Inst;
    public static IAPManager Inst
    { 
        get
        {
            if (_Inst != null || Application.platform != RuntimePlatform.IPhonePlayer)
            {
                return _Inst;
            }
            GameObject go = new GameObject("IAPManager");
            go.AddComponent<EasyStoreKit>();
            _Inst = go.AddComponent<IAPManager>();
            Inst.ConfigureStoreKitEvents();
            return _Inst;
        }
    }
    public string[] productIdentifiers = new string[1] { Application.identifier };

    enum GUIState {
	    Loading,
	    MainUI,
    };

    private GUIState guiState = GUIState.MainUI;

    private StoreKitProduct[] products;

    //message to be displayed on UI.
    private string message;
    private string restoredIdentifiers;

    //void Start () {
	//    ConfigureStoreKitEvents();
	    //0. Assign identifiers
	   // EasyStoreKit.AssignIdentifiers(productIdentifiers);
        //EasyStoreKit.LoadProducts();
   // }
    //EasyStoreKit event handlers
    private void ConfigureStoreKitEvents() {
	    EasyStoreKit.productsLoadedEvent += ProductsLoaded;
	    EasyStoreKit.transactionPurchasedEvent += TransactionPurchased;
	    EasyStoreKit.transactionFailedEvent += TransactionFailed;
	    EasyStoreKit.transactionRestoredEvent += TransactionRestored;
	    EasyStoreKit.transactionCancelledEvent += TransactionCancelled;
	    EasyStoreKit.restoreCompletedEvent += RestoreCompleted;
	    EasyStoreKit.restoreFailedEvent += RestoreFailed;
    }
    private int buyNumber = 1;
    //private double buyPrice = 0;
    public void Payment(SDK.PayInfo info)  /// <param name="payDes">服务器透传值</param>
    {
        Debug.Log("内购aProductId：" + info.aProductId);
        //EasyStoreKit.BuyProductWithIdentifier(aProductId, aNumber);

        buyNumber = info.aNumber;
        //buyPrice = price;
        string[] tmpProducts = { info.aProductId };
        //tmpProductId = aProductId;

        //0. Assign identifiers
        EasyStoreKit.AssignIdentifiers(tmpProducts);

        //1. Check for internet reachability
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {

            //2. Check if payments can be made
            if (EasyStoreKit.CanMakePayments())
            {
                //nullify previously loaded products
                this.products = null;

                //3. Load products
                EasyStoreKit.LoadProducts();

            }
            else
            {
                Debug.Log("Application is not allowed to make payments!");
            }
        }
        else
        {
            Debug.Log("No internet connection available!");
        }
    }

    void ProductsLoaded(StoreKitProduct[] products) {
	    this.products = products;
        Debug.Log("内购商品数量：" + products.Length);
        if(products.Length == 0)
        {
            SDK.QGameSDK.Instance.OnPayCall("products==0", "", QGameSDK.PayResult.Failed);
            return;
        }
        for (int i = 0; i < this.products.Length; i++)
        {
            Debug.Log(products[i].ToString());
            //buy product
            if (EasyStoreKit.BuyProductWithIdentifier(this.products[i].identifier, buyNumber))
            {
                //valid product identifier. Do nothing, the event will be called once processing is complete
            }
            else
            {

            }
        }
            //change ui state
            guiState = GUIState.MainUI;
    }

    void TransactionPurchased(string productIdentifier) {
	    //Unlock feature based on the identifier
	    //We are only updating the UI!
	    message = "Successfully purchased: " + productIdentifier;
        //Debug.Log(message);
	    guiState = GUIState.MainUI;
        SDK.QGameSDK.Instance.OnPayCall(productIdentifier, "", QGameSDK.PayResult.Success);
    }

    void TransactionFailed(string productIdentifier, string errorMessage) {
	    message = "Transaction failed for : " + productIdentifier + " :" + errorMessage;
        //Debug.Log(message);
	    guiState = GUIState.MainUI;
        SDK.QGameSDK.Instance.OnPayCall(errorMessage, "", QGameSDK.PayResult.Failed);
    }

    void TransactionRestored(string productIdentifier) {
	    //Unlock feature based on the identifier restored.
	    //We are only updating the UI!
	    restoredIdentifiers += productIdentifier + " ";
    }

    void TransactionCancelled(string productIdentifier) {
	    //Remove any activity indicators as the user has cancelled the transaction
	    guiState = GUIState.MainUI;
        SDK.QGameSDK.Instance.OnPayCall(productIdentifier, "", QGameSDK.PayResult.Cancel);
    }

    void RestoreCompleted() {
	    //change ui state
	    guiState = GUIState.MainUI;
	
	    //set message
	    message = "Restored " + restoredIdentifiers;
    }

    void RestoreFailed(string errorMessage) {
	    //change ui state
	    guiState = GUIState.MainUI;
	
	    //set message
	    message = "Restore failed: " + errorMessage;
    }
}
