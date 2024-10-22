using UnityEngine;

[System.Serializable]
public class Leg
{
    public Transform predictPosition;
    public Transform currentPosition;
    public Transform center;

    public Vector3 NewPosition { get; private set; }
    public float Time { get; private set; }
    public float Speed { get; private set; }
    public bool CanMove;
    public bool IsMoving;
    public int index;

    public void SetSpeed(float speed)
    {
        Speed = speed;
    }

    public void SetTime(float time)
    {
        Time = time;
    }

    public void SetNewPosition(Vector3 newPosition)
    {
        NewPosition = newPosition;
    }

    public void SetCanMove(bool value)
    {
        CanMove = value;
    }

    public void SetIsMoving(bool value)
    {
        IsMoving = value;
    }
}
