using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSystem : MonoBehaviour {
    public GameObject tradePanel;
    public Transform playerOfferSlot;
    public Transform otherOfferSlot;
    public Button acceptButton;
    public Button declineButton;
    
    private PlayerInventory localOffer;
    private PlayerInventory remoteOffer;
    
    void Start() {
        acceptButton.onClick.AddListener(AcceptTrade);
        declineButton.onClick.AddListener(CloseTrade);
    }
    
    public void OpenTrade(PlayerInventory other) {
        tradePanel.SetActive(true);
        remoteOffer = other;
    }
    
    void AcceptTrade() {
        // Exchange items
        CloseTrade();
    }
    
    void CloseTrade() { tradePanel.SetActive(false); }
}

public class PlayerInventory {
    public ResourceStack[] slots = new ResourceStack[36];
}
