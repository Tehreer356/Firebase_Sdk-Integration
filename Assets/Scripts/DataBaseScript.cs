using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataBaseScript : MonoBehaviour
{
    public DataToSave dataToSave;
    string userName;
    public DatabaseReference db_Ref;
    public TMP_InputField scoreField, diamondField;
    public GameObject userInterfacePanel, dashboardPanel;
    [SerializeField] GameObject element;
    [SerializeField] Transform holder;
    public GameObject scoreBoardPanel;

    private void Awake()
    {
        db_Ref = FirebaseDatabase.DefaultInstance.RootReference;
        //if (PlayerPrefs.GetInt("player") == 0)
        //{
        //    userName = AnonymousLogin.Instance.guestName.text;
        //}
        //else 
        //{
        //    userName = AuthManager.instance.User.DisplayName;
        //}
    }

    public void SaveData()
    {
        userName = DashBoardScript.instance.db_display_name;
        Debug.Log(userName);

        // Check if user exists
        StartCoroutine(CheckAndSaveUserData());
    }

    private IEnumerator CheckAndSaveUserData()
    {
        var checkTask = db_Ref.Child("users").Child(userName).GetValueAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        if (checkTask.Exception != null)
        {
            Debug.LogError($"Failed to check user data: {checkTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = checkTask.Result;
            bool userExists = snapshot.Exists;

            dataToSave.name = userName;
            dataToSave.scores = int.Parse(scoreField.text);
            dataToSave.diamonds = int.Parse(diamondField.text);

            string json = JsonUtility.ToJson(dataToSave);

            if (userExists)
            {
                // User exists, update data
                db_Ref.Child("users").Child(userName).SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("User data updated successfully.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to update user data: {task.Exception}");
                    }
                });
            }
            else
            {
                // User does not exist, create new data
                db_Ref.Child("users").Child(userName).SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("User data created successfully.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to create user data: {task.Exception}");
                    }
                });
            }
        }
    }


    public void LoadScoreBoard()
    {
        StartCoroutine(LoadData());
        scoreBoardPanel.SetActive(true);
        dashboardPanel.SetActive(false);
    }

    IEnumerator LoadData()
    {
        var db_task = db_Ref.Child("users").OrderByChild("scores").GetValueAsync();
        yield return new WaitUntil(predicate: () => db_task.IsCompleted);

        if (db_task.Exception != null)
        {
            Debug.LogWarning(message: "Failed to load with " + db_task.Exception.Message);
        }
        else
        {
            DataSnapshot snapshot = db_task.Result;

            foreach (Transform child in holder)
            {
                Destroy(child.gameObject);
            }

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                Debug.Log(childSnapshot.ToString());
                string name = childSnapshot.Child("name").Value.ToString();
                int scores= int.Parse(childSnapshot.Child("scores").Value.ToString());
                //string json = childSnapshot.GetRawJsonValue();
                //DataToSave data = JsonUtility.FromJson<DataToSave>(json);

                GameObject entryObject = Instantiate(element, holder);
                Text[] texts = entryObject.GetComponentsInChildren<Text>();
                texts[0].text = name.ToString();
                texts[1].text = scores.ToString();
            }
        }
    }

    public void SignOut()
    {
        if(PlayerPrefs.GetInt("player")==1)
        {
            AuthManager.instance.auth.SignOut();
        }
        userInterfacePanel.SetActive(true);
        dashboardPanel.SetActive(false);
        AuthManager.instance.ClearAllFields();
        PlayerPrefs.DeleteKey("player");
        DashBoardScript.instance.db_display_name = " ";
    }
    public void HideLeaderBoard()
    {
        scoreBoardPanel.SetActive(false);
        dashboardPanel.SetActive(true);
    }
}

[Serializable]
public class DataToSave
{
    public string name;
    public int scores;
    public int diamonds;
}
