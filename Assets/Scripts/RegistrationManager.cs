using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistrationManager : MonoBehaviour
{
    public GameObject anonymousPanel;
    public GameObject signUpPanel;
    public GameObject signInPanel;
    public GameObject userInterfacePanel;
    public GameObject forgotPasswordPanel;

    public void AnonymousLogin()
    {
        userInterfacePanel.SetActive(false);
        anonymousPanel.SetActive(true);
    }
    public void SignUp()
    {
        userInterfacePanel.SetActive(false);
        signUpPanel.SetActive(true);
    }
    public void SignIn()
    {
        userInterfacePanel.SetActive(false);
        signInPanel.SetActive(true);
    }
    public void ForgotPassButton()
    {
        signInPanel.SetActive(false) ;
        forgotPasswordPanel.SetActive(true) ;
    }
}
