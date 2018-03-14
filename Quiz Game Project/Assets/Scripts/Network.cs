using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class Network : MonoBehaviour
{
    static SocketIOComponent socket;
    private RoundData roundData;
    private JSONObject gameData;

    // Use this for initialization
    void Start()
    {
        socket = GetComponent<SocketIOComponent>();
        socket.On("open", OnConnected);
        socket.On("receive data", OnReceive);
    }

    //Get data from MongoDB on connection
    void OnConnected(SocketIOEvent e)
    {
        Debug.Log("Connected to server");

        socket.Emit("get data");
    }

    void OnReceive(SocketIOEvent e)
    {
        Debug.Log(e.data);

        gameData = e.data;
    }

    public RoundData GetRoundData()
    {
        return roundData;
    }

    float GetFloatFromJson(JSONObject data, string key)
    {
        return float.Parse(data[key].ToString().Replace("\"", ""));
    }
}
