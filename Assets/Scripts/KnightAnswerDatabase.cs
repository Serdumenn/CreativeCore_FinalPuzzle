using UnityEngine;

[CreateAssetMenu(fileName = "KnightAnswerDatabase", menuName = "Dialogue/Knight Answer Database")]
public class KnightAnswerDatabase : ScriptableObject
{
    [System.Serializable]
    public class KnightAnswer
    {
        public string answerText;
        public AudioClip answerClip;
    }

    public KnightAnswer[] answers = new KnightAnswer[3];
}