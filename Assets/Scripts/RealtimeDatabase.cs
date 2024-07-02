using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;

public class RealtimeDatabase : MonoBehaviour
{
    public dataToSave dts;
    public string userId;
    public DatabaseReference db_Reference;

    private void Awake()
    {
        db_Reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(dts);
        db_Reference.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    public void LoadData()
    {
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        var serverData = db_Reference.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        Debug.Log("process is completed");
        DataSnapshot dataSnapshot = serverData.Result;
        string jsonData = dataSnapshot.GetRawJsonValue();

        if(jsonData != null )
        {
            Debug.Log("data found");
            dts = JsonUtility.FromJson<dataToSave>(jsonData);
        }
        else
        {
            Debug.Log("No data found");
        }
    }

}


[Serializable]
public class dataToSave
{
    public string name;
    public int scores;
    public int diamonds;
}