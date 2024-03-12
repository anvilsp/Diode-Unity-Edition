using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorButton : MonoBehaviour
{

    public enum SwitchColor { Red, Blue }

    [SerializeField] public GameObject level;
    [SerializeField] public BoxCollider2D bc;
    [SerializeField] public Animator animator;
    [SerializeField] public Transform playerCheck;
    [SerializeField] private LayerMask playerLayer;

    public SwitchColor switchColor = SwitchColor.Red;
    public bool disabled = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerPressed() && !disabled)
        {
            LevelController levelScript = level.GetComponent<LevelController>();
            switch (switchColor)
            {
                case SwitchColor.Red:
                    levelScript.ColorSwitch("Red");
                    break;
                case SwitchColor.Blue:
                    levelScript.ColorSwitch("Blue");
                    break;
            }
            disabled = true;
        }
    }

    public bool PlayerPressed()
    {
        return Physics2D.OverlapCircle(playerCheck.position, 0.2f, playerLayer);
    }

    public void Toggle(string col)
    {
        switch (col)
        {
            case "Red":
                if (switchColor == SwitchColor.Red)
                {
                    animator.Play("PressedRed");
                    disabled = true;
                }
                else
                {
                    animator.Play("UnpressedBlue");
                    disabled = false;
                }
                break;
            case "Blue":
                if (switchColor == SwitchColor.Blue)
                {
                    animator.Play("PressedBlue");
                    disabled = true;
                }
                else
                {
                    animator.Play("UnpressedRed");
                    disabled = false;
                }
                break;
        }

    }

}
