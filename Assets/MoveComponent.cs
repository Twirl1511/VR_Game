using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MoveComponent : MonoBehaviour
{
    public XRNode leftHandNode = XRNode.LeftHand;  // ���������� ����� ����������
    private Vector2 leftThumbstickInput;           // ���������� ��� ���������� �������� �����

    void Update()
    {
        // ��������� ��� �������� ������ �����
        if (InputDevices.GetDeviceAtXRNode(leftHandNode).TryGetFeatureValue(CommonUsages.primary2DAxis, out leftThumbstickInput))
        {
            // ���������, ���� �� ��������
            if (leftThumbstickInput != Vector2.zero)
            {
                // ������: ������� �������� � �������
                Debug.Log("�������� ������ �����: " + leftThumbstickInput);

                // ����� ����� ������������ leftThumbstickInput ��� ����������� �������
                MoveCharacter(leftThumbstickInput);
            }
        }
    }

    // ����� ��� ����������� ������� � ����������� �� �������� �����
    private void MoveCharacter(Vector2 input)
    {
        Debug.Log(input);
        // ���������� �������� ������ VR-��������� �� ������ input
        // ��������, ����������� ������/����� ��� �����/������
    }
}
