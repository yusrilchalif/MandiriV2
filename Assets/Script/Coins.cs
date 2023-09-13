using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLocation;
using UnityEngine.Events;
using System;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

[Serializable]
public class Loc {
    public float latitude;
    public float longitude;

    public Loc (float latitude, float longitude) {
        this.latitude = latitude;
        this.longitude = longitude;
    }
}

[Serializable]
public class NewCoin {
    public long created_at;
    public bool is_available = true;
    public Loc location;
    public string type;
    public int value;

    public NewCoin(long created_at, bool is_available, Loc location, string type, int value) {
        this.created_at = created_at;
        this.is_available = is_available;
        this.location = location;
        this.type = type;
        this.value = value;
    }
}


public class Coins : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("Coin settings")]
    public NewCoin coin;
    public string ID;

    [Header("Configs")]
    public PlaceAtLocation placeAt;
    public MeshRenderer mesh;
    public GameObject coinVFX;
    public UnityAction<Coins> OnInteract;
    public UnityAction<string> OnFailedInteract;
    
    void Update() {
        RotateCoin(90f);
    }

    private void OnMouseDown() {
        Debug.Log("On mouse down detected!");
        //Coin Function
        // InteractCoin();
    }


    public void RotateCoin(float rotateSpeed) {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnEnable() {
        StartCoroutine(DelayInitCoin(coin, ID, OnInteract, OnFailedInteract));
    }

    public void InitCoin(NewCoin newCoin, string id, UnityAction<Coins> onInteract, UnityAction<string> onFailed) {
        StoreValue(newCoin, id, onInteract, onFailed);
        if(!gameObject.activeSelf) return;
        
        StartCoroutine(DelayInitCoin(newCoin, id, onInteract, onFailed));
    }

    IEnumerator DelayInitCoin(NewCoin newCoin, string id, UnityAction<Coins> onInteract, UnityAction<string> onFailed) {
        yield return new WaitForEndOfFrame();
        
        RestoreValue(newCoin, id, onInteract, onFailed);

        Debug.Log("On interact assigned!");
        Debug.Log("Distance from user is around : " + placeAt.RawGpsDistance);

        if(ColorUtility.TryParseHtmlString(coin.type, out Color newcolor)) {
            mesh.material.color = newcolor;
        }

        if(placeAt.RawGpsDistance > AuthController.Instance.globalUserSettings.coinDistance) {
            ShowCoins(false);
        }
        else ShowCoins(true);

    }
    void RestoreValue(NewCoin newCoin, string id, UnityAction<Coins> onInteract, UnityAction<string> onFailed) {
        
        placeAt = GetComponent<PlaceAtLocation>();
        Location coinLoc = new Location((double)newCoin.location.latitude, (double)newCoin.location.longitude);

        placeAt.Location = coinLoc;

        coin = newCoin;
        ID = id;
        OnInteract = onInteract;
        OnFailedInteract = onFailed;
    }
    
    void StoreValue(NewCoin newCoin, string id, UnityAction<Coins> onInteract, UnityAction<string> onFailed) {
        coin = newCoin;
        ID = id;
        OnInteract = onInteract;
        OnFailedInteract = onFailed;
    }


    public void InitCoin(NewCoin newCoin, string id, UnityAction<Coins> onInteract) {
        placeAt = gameObject.GetComponent<PlaceAtLocation>();

        coin = newCoin;
        ID = id;
        OnInteract = onInteract;

        Location coinLoc = new Location((double)newCoin.location.latitude, (double)newCoin.location.longitude);

        placeAt.Location = coinLoc;

        if(ColorUtility.TryParseHtmlString(coin.type, out Color newcolor)) {
            mesh.material.color = newcolor;
        }
    }

    public void ShowCoins(bool isShowing) {
        gameObject.SetActive(isShowing);
    }

    public double CoinDistance() {
        return placeAt.RawGpsDistance;
    }

    [ContextMenu("Interact")]
    public void InteractCoin() {

        Debug.Log("Start interact!");       
        UserData currentUser = UserAuth.Instance.CurrentUser;
        var currentUserMaxLimit = currentUser.currentLimit += coin.value;
        Debug.Log("Checking if user" + currentUser.userName + " is maxing their limit...");

        if(currentUserMaxLimit >= currentUser.maxLimit) {
            //Show that user already maxxed their coins for a week
            Debug.Log("User maxed their limits");
            OnFailedInteract?.Invoke("Anda telah mencapai batas maksimum selama seminggu!");
            return;
        }

        //Check if coins is already claimed on cloud
        CoinAuth.Instance.GetCoinStatus(ID,
            () => {
                PostCoinDB(currentUser);
            },
            (error) => {
                Debug.Log("Coin is not available");
                OnFailedInteract?.Invoke("Koin telah diklaim oleh pengguna lain!");
                DestroyCoin();
            }
        );
    }

    void PostCoinDB(UserData currentUser) {
        Debug.Log("Coin is available");
        //User Function
        currentUser.coinAmount += coin.value;
        currentUser.currentLimit += coin.value;

        Debug.Log("Posting coin to db...");
        CoinAuth.Instance.PostCoin(this, () => {
            //Succes
            Debug.Log("Posting coin success!");

            //Post function
            UserAuth.Instance.PostUser(currentUser, () => {
                    //Post Succes
                    Debug.Log("Updating current user...");
                    UserAuth.Instance.SetCurrentUser(currentUser);
                    
                    Debug.Log("Invoking on interact...");
                    //Coin Function
                    OnInteract?.Invoke(this);
                    DestroyCoin();
                }, (reason) => {
                    //Failed
                    Debug.Log("Failed because of : " + reason);
                    OnFailedInteract?.Invoke($"Gagal mengambil koin. Error Code : {reason}");
                    return;
                });
            },
            (rejected) => {
            //Failed
            print("Failed to post coin : " + rejected);
            OnFailedInteract?.Invoke($"Gagal mengambil koin. Error Code : {rejected}");
            }
        );
    }

    void DestroyCoin() {
        Debug.Log("Destroying coin and instantiate vfx");
        //VFX & Destroy
        Instantiate(coinVFX, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("On pointer click.");
        InteractCoin();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("On pointer down.");
    }
}
