using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscountObject : MonoBehaviour
{

    [SerializeField] private int discountAmount;

    [SerializeField] private string webLink = "https://www.mandirikartukredit.com/";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown() {
        Application.OpenURL("https://www.mandirikartukredit.com/");
    }

}
