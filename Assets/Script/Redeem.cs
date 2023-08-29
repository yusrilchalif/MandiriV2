using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Redeem : MonoBehaviour
{
    private Button currentButton;
    private int coinAmount;

    private void Start()
    {
        currentButton = GetComponent<Button>();
    }

    public void RedeemPrize(int cost) {

        coinAmount = UserAuth.Instance.CurrentUser.coinAmount;

        if (coinAmount < cost)
        {
            HomeSceneManager.Instance.UpdateRedeemCoin(coinAmount, "Koin kamu tidak cukup!\nJumlah koin kamu : ");
            return;
        }
        /*
        currentButton.interactable = false;
        */
        coinAmount -= cost;
        
        var currentUser = UserAuth.Instance.CurrentUser;
        currentUser.coinAmount = coinAmount;

        UserAuth.Instance.SetCurrentUser(currentUser);
        UserAuth.Instance.PostUser(currentUser, () => {}, (failed) => { Debug.Log(failed.Message); });

        HomeSceneManager.Instance.UpdateUserCoin(coinAmount);
        HomeSceneManager.Instance.UpdateRedeemCoin(coinAmount, "Jumlah koin kamu : ");
    }

}
