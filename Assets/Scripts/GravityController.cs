using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    public float speed = 5f;              // Скорость движения
    public float arcAngle = 270f;         // Угол дуги
    public int arcResolution = 6;       // Разрешение дуги (количество сегментов)
    [SerializeField] private float _gravityForce = 9.81f;
    [SerializeField] private Transform _model;
    public LayerMask groundLayer;
    public Vector3 direction;

    public Vector3 GravityDirection { get; private set; }
    private List<Vector3> _arcPoints = new List<Vector3>();
    private bool _isChanged;
    private float _timer;
    private Vector3 _previousHitNormal;

    private void Start()
    {
        GravityDirection = Vector3.down * _gravityForce;
    }

    private void FixedUpdate()
    {
        // Радиус дуги зависит от скорости и deltaTime
        float arcRadius = speed * Time.deltaTime;

        if (_isChanged)
        {
            if(_timer < 2f)
            {
                _timer += Time.deltaTime;
            }
            else
            {
                _timer = 0;
                _isChanged = false;
            }

            return;
        }
        ArcCast(_model.position, _model.rotation,
                                     arcAngle, arcRadius, (int)arcResolution,
                                     groundLayer, out RaycastHit hit);

        //// Проверяем дуговой луч, чтобы узнать, куда двигаться
        //if (ArcCast(_model.position, _model.rotation,
        //                             arcAngle, arcRadius, (int)arcResolution,
        //                             groundLayer, out RaycastHit hit))
        //{
        //    // Перемещаем объект на точку попадания
        //    //transform.position = hit.point;
        //    GravityDirection = -hit.normal * _gravityForce;
            
        //    // Обновляем ориентацию объекта в зависимости от нормали поверхности
        //    transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        //    if(_previousHitNormal != hit.normal)
        //        _isChanged = true;

        //    _previousHitNormal = hit.normal;
        //}
    }

    private bool ArcCast(Vector3 center, Quaternion rotation,
                           float angle, float radius, int resolution,
                           LayerMask layer, out RaycastHit hit)
    {
        rotation *= Quaternion.Euler(-angle / 2, 0, 0);
        _arcPoints.Clear();
        for (int i = 0; i < resolution; i++)
        {
            Vector3 A = center + rotation * direction * radius;
            rotation *= Quaternion.Euler(angle / resolution, 0, 0);
            Vector3 B = center + rotation * direction * radius;
            Vector3 AB = B - A;

            _arcPoints.Add(A);
            _arcPoints.Add(B);

            if (Physics.Raycast(A, AB, out hit, AB.magnitude * 1.001f, layer))
                return true;
        }

        hit = new RaycastHit();
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < _arcPoints.Count - 1; i += 2)
            Gizmos.DrawLine(_arcPoints[i], _arcPoints[i + 1]);
    }
}
