using UnityEngine;
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private float platformSpeed;
    [SerializeField] private Vector3 start;
    [SerializeField] private Vector3 end;
    public Vector3 velocity;

    public Vector3 Velocity { get => velocity; set => velocity = value; }

    void FixedUpdate()
    {
        if (!isActive)
        {
            Velocity = Vector3.zero;
            return;
        }

        float pingPong = Mathf.PingPong(Time.fixedTime * this.platformSpeed,
       1.0f);
        var newPosition = Vector3.Lerp(this.start, this.end, pingPong);
        Velocity = newPosition - this.transform.localPosition;
        Velocity = Velocity / Time.fixedDeltaTime;
        this.transform.localPosition = newPosition;
    }

    public Vector3 GetVelocity()
    {
        return Velocity;
    }

    public void setActive(bool isActive)
    {
        this.isActive = isActive;
    }
}