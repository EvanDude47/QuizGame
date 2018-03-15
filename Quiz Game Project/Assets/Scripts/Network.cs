using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class Network : MonoBehaviour
{
    static SocketIOComponent socket;
    private RoundData roundData;
    private JSONObject gameData;

    int questionSize;
    int answerSize;

    // Use this for initialization
    void Start()
    {
        socket = GetComponent<SocketIOComponent>();
        socket.On("open", OnConnected);
        socket.On("receive data", OnReceive);

        roundData = new RoundData();

        roundData.name = "Egg";
        roundData.timeLimitInSeconds = 10;
        roundData.pointsAddedForCorrectAnswer = 10;
        roundData.questions = new QuestionData[1];
        roundData.questions[0] = new QuestionData();
        roundData.questions[0].questionText = "Bees?";
        roundData.questions[0].answers = new AnswerData[2];
        roundData.questions[0].answers[0] = new AnswerData();
        roundData.questions[0].answers[0].answerText = "yes bees";
        roundData.questions[0].answers[0].isCorrect = true;
        roundData.questions[0].answers[1] = new AnswerData();
        roundData.questions[0].answers[1].answerText = "no bees";
        roundData.questions[0].answers[1].isCorrect = false;
    }

    //Get data from MongoDB on connection
    void OnConnected(SocketIOEvent e)
    {
        Debug.Log("Connected to server");

        socket.Emit("get data");
    }

    void OnReceive(SocketIOEvent e)
    {
        gameData = e.data;

        roundData = new RoundData();

        roundData.name = GetStringFromJson(gameData, "name");
        roundData.timeLimitInSeconds = GetIntFromJson(gameData, "timeLimitInSeconds");
        roundData.pointsAddedForCorrectAnswer = GetIntFromJson(gameData, "pointsAddedForCorrectAnswer");

        questionSize = gameData["questions"].Count;
        roundData.questions = new QuestionData[questionSize];

        for (int x = 0; x < questionSize; x++)
        {
            roundData.questions[x] = new QuestionData();

            roundData.questions[x].questionText = GetStringFromJson(gameData["questions"].list[x], "questionText");

            answerSize = gameData["questions"].list[x].GetField("answers").Count;

            roundData.questions[x].answers = new AnswerData[answerSize];

            for(int y = 0; y < answerSize; y++)
            {
                roundData.questions[x].answers[y] = new AnswerData();

                roundData.questions[x].answers[y].answerText = GetStringFromJson(gameData["questions"].list[x].GetField("answers").list[y], "answerText");
                roundData.questions[x].answers[y].isCorrect = GetBoolFromJson(gameData["questions"].list[x].GetField("answers").list[y], "isCorrect");
            }
        }
    }

    public RoundData GetRoundData()
    {
        return roundData;
    }

    string GetStringFromJson(JSONObject data, string key)
    {
        return data[key].ToString().Replace("\"", "");
    }

    bool GetBoolFromJson(JSONObject data, string key)
    {
        string boolString = data[key].ToString().Replace("\"", "");
        bool tempBool;

        if (boolString == "true")
            tempBool = true;
        else
            tempBool = false;

        return tempBool;
    }

    int GetIntFromJson(JSONObject data, string key)
    {
        return int.Parse(data[key].ToString().Replace("\"", ""));
    }
}
