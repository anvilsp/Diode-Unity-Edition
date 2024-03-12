using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] public static float defaultTime = 10f;
    [SerializeField] public GameObject level;
    [SerializeField] public Rigidbody2D rb;

    public float timeLeft = defaultTime;
    private int lastIntTime;
    private bool timeout = false;
    private bool timerStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        lastIntTime = (int) timeLeft;
    }

    // Update is called once per frame
    void Update()
    {
        if(timerStarted)
        {
            if (timeLeft <= 0.0f && !timeout)
                TimerEnded();
            else if (!timeout)
            {
                timeLeft -= Time.deltaTime;
                if ((int)timeLeft != lastIntTime)
                {
                    lastIntTime = (int)timeLeft;
                    print(lastIntTime);
                }
            }
        }

    }
    void TimerEnded()
    {
        timerStarted = false;
        timeLeft = defaultTime;
        lastIntTime = (int)timeLeft;
        LevelController lvlControl = level.GetComponent<LevelController>();
        transform.position = lvlControl.initKeyPos;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void StartTimer()
    {
        timerStarted = true;
    }



}
