using UnityEngine;

public class tempPanelLogic : MonoBehaviour
{
    [SerializeField]
    GameObject contentContainer;

    GameObject currentActiveText = null;

    public void OpenPanel(GameObject initialTextPanel)
    {
        contentContainer.SetActive(true);
        initialTextPanel.SetActive(true);
        currentActiveText = initialTextPanel;
    }

    public void DisplayText(GameObject textContainer)
    {
        currentActiveText.SetActive(false);
        textContainer.SetActive(true);
        currentActiveText = textContainer;
    }

    public void Back()
    {
        currentActiveText.SetActive(false);
        currentActiveText = null;
        contentContainer.SetActive(false);
    }
}
