using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LetterPanel : MonoBehaviour
{
    public void OnCloseButton() {
        this.gameObject.SetActive(false);
    }
}