using UnityEngine;

public class TailController : MonoBehaviour
{
    [SerializeField] private Transform _centerPoint;
    [SerializeField] private Transform _tailTarget;
    [SerializeField] private float _radius = 5f;   
    [SerializeField] private float _interval = 2f;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _moveSquareDistanceDistance = 0.3f;

    private float _timer;
    private Vector3 _newPosition;
    private Vector3 _previousAlienPosition;

    void Start()
    {
        GenerateRandomPoint();
        _previousAlienPosition = _centerPoint.position;
    }

    void FixedUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0f;
            GenerateRandomPoint();
        }

        _tailTarget.position = Vector3.Lerp(
                _tailTarget.position,
                _newPosition,
                Time.deltaTime * _speed);



        float distance = (_centerPoint.position - _previousAlienPosition).sqrMagnitude;

        if (distance > _moveSquareDistanceDistance)
        {
            print(1);
            GenerateRandomPoint();
            _previousAlienPosition = _centerPoint.position;
        }
    }

    void GenerateRandomPoint()
    {
        Vector2 randomCirclePoint = Random.insideUnitCircle * _radius;
        _newPosition = new Vector3(_centerPoint.position.x + randomCirclePoint.x, _centerPoint.position.y, _centerPoint.position.z + randomCirclePoint.y);
    }
}
