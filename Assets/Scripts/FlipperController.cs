using UnityEngine;

public class FlipperController : MonoBehaviour
{
    public enum FlipperSide { Left, Right }
    public FlipperSide side;

    public float restAngle = 0f;
    public float activeAngle = -45f;
    public float speed = 15f;

    private float targetAngle;
    private KeyCode inputKey;

    void Start()
    {
        targetAngle = restAngle;
        inputKey = (side == FlipperSide.Left) ? KeyCode.LeftArrow : KeyCode.RightArrow;
    }

    void Update()
    {
        if (Input.GetKey(inputKey))
        {
            targetAngle = (side == FlipperSide.Left) ? activeAngle : -activeAngle;
        }
        else
        {
            targetAngle = restAngle;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, speed * 200f * Time.deltaTime);
    }
}