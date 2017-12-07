using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.UI;
using System.Text;

public class DatabaseManager : MonoBehaviour {

    public Text highScoreText;
    private String connectionString;
    
	// Use this for initialization
	void Start () {
        connectionString = "URI=file:" + Application.dataPath + "/Database/Highscore.sqlite";
        CreateTable();
        GetScores(10);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void GetScores(int scoresNeeded)
    {
        StringBuilder first10Scores = new StringBuilder();
        //nutze using -> auto shutdown für Idispoasable
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                String sqlQuery = "SELECT * FROM Highscore ORDER BY Time DESC";
                dbCmd.CommandText = sqlQuery;
                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    int readCount = 0;
                    while (reader.Read() && readCount < scoresNeeded)
                    {
                        float score = reader.GetFloat(2);
                        //first10Scores.AppendLine(score.ToString());
                        first10Scores.AppendLine(getTimerText(score));
                        readCount++;
                    }
                    dbConnection.Close();
                    reader.Close();
                    highScoreText.text = first10Scores.ToString();
                }
            }
        }
    }
    public string getTimerText(float time)
    {
        var minutes = time / 60; //Divide the guiTime by sixty to get the minutes.
        var seconds = time % 60;//Use the euclidean division for the seconds.
        var fraction = (time * 100) % 100;

        return string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);
    }

    public void InsertNewScore(float timeNeeded)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                String sqlQuery = String.Format("INSERT INTO Highscore (Time) VALUES(" +timeNeeded+")");
                dbCmd.CommandText = sqlQuery;
                int affectedRows = dbCmd.ExecuteNonQuery();
                Debug.Log("WroteNewTimeTO DB " + affectedRows);
                dbConnection.Close();
            }
        }
    }

    private void CreateTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                String sqlQuery =String.Format("CREATE TABLE if not exists Highscore (ID INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , Name TEXT DEFAULT unknown, Time REAL, Date DATETIME DEFAULT CURRENT_TIMESTAMP)");
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteScalar();
                dbConnection.Close();
            }
        }
    }
}
