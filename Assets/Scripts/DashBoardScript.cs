using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBoardScript : MonoBehaviour
{
    public static DashBoardScript instance;
    public string db_display_name;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        if (PlayerPrefs.GetInt("player") == 0)
        {
            db_display_name = AnonymousLogin.Instance.guestName.text;
        }
        else if (PlayerPrefs.GetInt("player")==1)
        {
            db_display_name = AuthManager.instance.User.DisplayName;
        }
    }
}
