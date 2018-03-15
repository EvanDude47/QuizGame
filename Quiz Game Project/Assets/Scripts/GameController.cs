using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SocketIO;

public class GameController : MonoBehaviour
{
    public Text questionText;
    public Text scoreText;
    public Text timeText;
    private int playerScore;
    private float timeRemaining;

    public Transform answerParent;

    public GameObject questionDisplay;
    public GameObject endGameDisplay;

    public BasicObjectPool answerButtonPool;
    private Network network;
    private RoundData roundData;
    private GameObject server;
    private SocketIOComponent socket;
    private QuestionData[] questionPool;
    private bool isRoundActive;
    private int questionIndex;

    private List<GameObject> answerButtonObjects = new List<GameObject>();

    struct submissionData
    {
        public string name;
        public int score;
        public int time;

        public submissionData(string name, int score, int time)
        {
            this.name = name;
            this.score = score;
            this.time = time;
        }
    }

	// Use this for initialization
	void Start()
    {
        network = FindObjectOfType<Network>();
        roundData = network.GetRoundData();
        questionPool = roundData.questions;

        playerScore = 0;
        timeRemaining = roundData.timeLimitInSeconds;
        isRoundActive = true;

        questionIndex = 0;

        ShowQuestions();
	}

    private void ShowQuestions()
    {
        //Clear answer buttons
        RemoveAnswerButtons();

        //Set question text
        QuestionData questionData = questionPool[questionIndex];
        questionText.text = questionData.questionText;

        //Add answer buttons
        for (int i = 0; i < questionData.answers.Length; i++)
        {
            GameObject answerButtonObject = answerButtonPool.GetObject();
            answerButtonObject.transform.SetParent(answerParent);
            answerButtonObjects.Add(answerButtonObject);

            AnswerButton answerButton = answerButtonObject.GetComponent<AnswerButton>();
            answerButton.SetUp(questionData.answers[i]);
        }
    }

    private void RemoveAnswerButtons()
    {
        while(answerButtonObjects.Count > 0)
        {
            answerButtonPool.ReturnObject(answerButtonObjects[0]);
            answerButtonObjects.RemoveAt(0);
        }
    }

    public void AnswerClicked(bool isCorrect)
    {
        if(isCorrect)
        {
            //Add to player score
            playerScore += roundData.pointsAddedForCorrectAnswer;
            scoreText.text = "Score:" + playerScore.ToString();
        }

        //Advance to next question
        if(questionPool.Length > questionIndex + 1)
        {
            questionIndex++;
            ShowQuestions();
        }
        else
        {
            EndRound();
        }
    }

    public void EndRound()
    {
        isRoundActive = false;

        questionDisplay.SetActive(false);
        endGameDisplay.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene("MenuScreen");
    }

    private void UpdateTime()
    {
        timeText.text = "Time:" + Mathf.Round(timeRemaining).ToString();
    }

    //Send player score to database
    public void SubmitScore()
    {
        string playerName = GameObject.Find("NameField").GetComponent<InputField>().text;

        submissionData submission = new submissionData(playerName, playerScore, (int)timeRemaining);

        string jsonObj = JsonUtility.ToJson(submission);

        server = GameObject.Find("Server");
        socket = server.GetComponent<SocketIOComponent>();

        socket.Emit("push score", new JSONObject(jsonObj));

        SceneManager.LoadScene("MenuScreen");
    }
	
	// Update is called once per frame
	void Update()
    {
		if(isRoundActive)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTime();

            if(timeRemaining <= 0)
                EndRound();
        }
	}
}
