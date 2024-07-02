using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
public class AnonymousLogin : MonoBehaviour
{
    public static AnonymousLogin Instance;
    public GameObject loginUI, successUI, DashboardPanel,anonymousLoginPanel;
    public TMP_InputField guestName;
    public TextMeshProUGUI userName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public async void Login() {
        await AnonymousLoginBtn();
        PlayerPrefs.SetInt("player", 0);
        DashBoardScript.instance.db_display_name = guestName.text;
    }
    async Task AnonymousLoginBtn()
    { 
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        await auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            print("Login Success");

            AuthResult result = task.Result;
            //successUI.SetActive(true);
            //Debug.Log(result.User.UserId);
            StartCoroutine(ContinueToDashboard());
            
        });

       
    }
  
    IEnumerator ContinueToDashboard()
    {
        yield return new WaitForSeconds(2f);
        DashboardPanel.SetActive(true);
        //successUI.SetActive(false);
        anonymousLoginPanel.SetActive(false);
        userName.text = guestName.text;
    }

}