using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JSONintentObject{
    double id;
    DateTime timeStamp;
    String resolvedQuery;
    String action;
    public override String ToString()
    {
        return " Timestamp: " + timeStamp + "| Resolved Query: " + resolvedQuery + "| Action: " + action;
    }
}
