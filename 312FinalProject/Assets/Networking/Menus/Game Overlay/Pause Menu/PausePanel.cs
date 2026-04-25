using UnityEngine;

public class PausePanel : MonoBehaviour
{
    [SerializeField]
    Transform panelObj;

    private void Awake()
    {
        panelObj.gameObject.SetActive(false);
    }

    public void TogglePausePanel(bool enablePanel) => panelObj.gameObject.SetActive(enablePanel);
}
