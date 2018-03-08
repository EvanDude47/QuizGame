using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using SocketIO;

public class DataEditor : EditorWindow
{
    string gameDataFilePath = "/StreamingAssets/data.json";

    public RoundData roundData;
    private GameObject server;
    private SocketIOComponent socket;

    [MenuItem("Window/Game Data Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(DataEditor)).Show();
    }

    void OnGUI()
    {
        if (roundData != null)
        {
            //Display data from json on client side
            SerializedObject serializedObj = new SerializedObject(this);
            SerializedProperty serializedProp = serializedObj.FindProperty("roundData");
            EditorGUILayout.PropertyField(serializedProp, true);
            serializedObj.ApplyModifiedProperties();

            if (GUILayout.Button("Save Data"))
            {
                SaveGameData();
            }

            if (GUILayout.Button("Send To Database"))
            {
                SendGameData();
            }
        }

        if (GUILayout.Button("Load Data"))
        {
            LoadGameData();
        }
    }

    //Load data from json
    void LoadGameData()
    {
        string filePath = Application.dataPath + gameDataFilePath;

        try
        {
            string gameData = File.ReadAllText(filePath);
            roundData = JsonUtility.FromJson<RoundData>(gameData);
        }
        catch
        {
            roundData = new RoundData();
        }
    }

    //Save data to json
    void SaveGameData()
    {
        string jsonObj = JsonUtility.ToJson(roundData);

        string filePath = Application.dataPath + gameDataFilePath;
        File.WriteAllText(filePath, jsonObj);
    }

    //Send data to MongoDB database for gameplay
    void SendGameData()
    {
        string jsonObj = JsonUtility.ToJson(roundData);

        server = GameObject.Find("Server");
        socket = server.GetComponent<SocketIOComponent>();

        socket.Emit("send data", new JSONObject(jsonObj));
    }
}
