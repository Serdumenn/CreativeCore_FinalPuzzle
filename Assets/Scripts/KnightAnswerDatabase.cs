using System;
using UnityEngine;

[CreateAssetMenu(fileName = "KnightAnswerDatabase", menuName = "Dialogue/Knight Answer Database")]
public class KnightAnswerDatabase : ScriptableObject
{
    public KnightAnswer[] answers;
}

[Serializable]
public struct KnightAnswer
{
    [TextArea] public string question;
    [TextArea] public string answer;
}