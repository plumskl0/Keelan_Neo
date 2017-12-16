using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMessageObject {
    private string type;
    private string messageBody;

    public string Type
    {
        get
        {
            return type;
        }

        set
        {
            //TODO: Check if valid type
            type = value;
        }
    }

    public string MessageBody
    {
        get
        {
            return messageBody;
        }

        set
        {
            //TODO: Check if Structure is valid - considering the type chosen
            messageBody = value;
        }
    }

    public EventMessageObject (string _type, string _messageBody)
    {
        Type = _type;
        MessageBody = _messageBody;
    }

}
