using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBlock : MonoBehaviour
{

    public enum SwitchColor { Red, Blue }

    [SerializeField] public GameObject level;
    [SerializeField] public BoxCollider2D bc;
    [SerializeField] public Animator animator;
    public SwitchColor switchColor = SwitchColor.Red;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Toggle(string col)
    {
        print(col);

        switch (col)
        {
            case "Red":
                if (switchColor == SwitchColor.Red)
                {
                    animator.Play("SolidRed");
                    bc.isTrigger = false;
                }
                else
                {
                    animator.Play("GhostBlue");
                    bc.isTrigger = true;
                }
                break;
            case "Blue":
                if (switchColor == SwitchColor.Blue)
                {
                    animator.Play("SolidBlue");
                    bc.isTrigger = false;
                }
                else
                {
                    animator.Play("GhostRed");
                    bc.isTrigger = true;
                }
                break;
        }

    }

}
