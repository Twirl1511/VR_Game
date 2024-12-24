using UnityEngine;


namespace Utils
{
    public static class IKUtils
    {
        public static Vector3[] SolvePointsFB(Vector3[] _joints, float[] _distances, Vector3 _target, float[] _jointsUplift, int _iterations)
        {
            Vector3 origin = _joints[0];
            bool isFlipped = false;

            for (int i = 0; i < _iterations; i++)
            {
                //flip
                isFlipped = !isFlipped;

                //reassign nodes
                _joints[0] = origin;
                _joints[_joints.Length - 1] = _target;

                if (isFlipped)
                {
                    for (int t = 1; t < _joints.Length; t++)
                    {
                        Vector3 direction = (_joints[t] - _joints[t - 1]).normalized + Vector3.up * _jointsUplift[t];
                        _joints[t] = _joints[t - 1] + direction * _distances[t - 1];
                    }
                }
                else
                {
                    for (int t = _joints.Length - 2; t >= 0; t--)
                    {
                        Vector3 direction = (_joints[t] - _joints[t + 1]).normalized + Vector3.up * _jointsUplift[t];
                        _joints[t] = _joints[t + 1] + direction * _distances[t];
                    }
                }
            }

            return _joints;
        }


        public static Quaternion QuaternionSmoothDamp(Quaternion _rot, Quaternion _target, ref Quaternion _rotRef, float _time)
        {
            if (Time.deltaTime < Mathf.Epsilon) return _rot;

            var Dot = Quaternion.Dot(_rot, _target);
            var Multi = Dot > 0f ? 1f : -1f;
            _target.x *= Multi;
            _target.y *= Multi;
            _target.z *= Multi;
            _target.w *= Multi;

            var Result = new Vector4(Mathf.SmoothDamp(_rot.x, _target.x, ref _rotRef.x, _time), Mathf.SmoothDamp(_rot.y, _target.y, ref _rotRef.y, _time), Mathf.SmoothDamp(_rot.z, _target.z, ref _rotRef.z, _time), Mathf.SmoothDamp(_rot.w, _target.w, ref _rotRef.w, _time)).normalized;
            var _rotRefError = Vector4.Project(new Vector4(_rotRef.x, _rotRef.y, _rotRef.z, _rotRef.w), Result);

            _rotRef.x -= _rotRefError.x;
            _rotRef.y -= _rotRefError.y;
            _rotRef.z -= _rotRefError.z;
            _rotRef.w -= _rotRefError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }


        public static Vector3 LockInRangeVector(Vector3 _vector, Vector2 _rangeX, Vector2 _rangeY)
        {
            if (_vector.x < _rangeX.x) _vector.x = _rangeX.x;
            else if (_vector.x > _rangeX.y) _vector.x = _rangeX.y;
            if (_vector.z < _rangeY.x) _vector.z = _rangeY.x;
            else if (_vector.z > _rangeY.y) _vector.z = _rangeY.y;

            return _vector;
        }
    }
}