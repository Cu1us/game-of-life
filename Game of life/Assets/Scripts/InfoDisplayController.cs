using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoDisplayController : MonoBehaviour
{
    [SerializeField]
    KeyCode KeyToShowInfo;

    [SerializeField]
    GameObject PressButtonToShowInfoLabel;
    [SerializeField]
    GameObject FullInfoLabel;

    void Update()
    {
        bool showInfo = Input.GetKey(KeyToShowInfo);
        if (PressButtonToShowInfoLabel.activeSelf == showInfo)
            PressButtonToShowInfoLabel.SetActive(!showInfo);
        if (FullInfoLabel.activeSelf != showInfo)
            FullInfoLabel.SetActive(showInfo);
    }
}