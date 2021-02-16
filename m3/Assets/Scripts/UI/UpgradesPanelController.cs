using GameData;
using UnityEngine;

public class UpgradesPanelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in GamePersistentData.Instance.ConfigData.Catalog)
        {

        }
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }
}
