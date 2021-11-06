using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<CassetteTape> cassetteAnswer = new List<CassetteTape>();

    private List<CassetteTape> playerGuess = new List<CassetteTape>();

    public void AddGuess(CassetteTape tape)
    {
        playerGuess.Add(tape);
    }

    private void CheckWin()
    {
        
    }
}
