using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class ModalPanel : MonoBehaviour {

    public Text question;
    public Button yesButton;
    public Button cancelButton;
    public GameObject modalPanelObject;

    private static ModalPanel modalPanel;

    public static ModalPanel Instance() {
        if (!modalPanel) {
            modalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if (!modalPanel)
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
        }

        return modalPanel;
    }

    // Yes/No/Cancel: A string, a Yes event, a No event and Cancel event
    public void Choice() {
        modalPanelObject.SetActive(true);
        question.text = modalPanelObject.gameObject.GetComponentInChildren<Text>().text;
        yesButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
    }

    public void ClosePanel() {
        modalPanelObject.SetActive(false);
    }
}