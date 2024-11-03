using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MoveComponent : MonoBehaviour
{
    public XRNode leftHandNode = XRNode.LeftHand;  // Определяем левый контроллер
    private Vector2 leftThumbstickInput;           // Переменная для сохранения движения стика

    void Update()
    {
        // Считываем оси движения левого стика
        if (InputDevices.GetDeviceAtXRNode(leftHandNode).TryGetFeatureValue(CommonUsages.primary2DAxis, out leftThumbstickInput))
        {
            // Проверяем, есть ли движение
            if (leftThumbstickInput != Vector2.zero)
            {
                // Пример: выводим значение в консоль
                Debug.Log("Движение левого стика: " + leftThumbstickInput);

                // Здесь можно использовать leftThumbstickInput для перемещения объекта
                MoveCharacter(leftThumbstickInput);
            }
        }
    }

    // Метод для перемещения объекта в зависимости от значения стика
    private void MoveCharacter(Vector2 input)
    {
        Debug.Log(input);
        // Реализуйте движение вашего VR-персонажа на основе input
        // Например, перемещение вперед/назад или влево/вправо
    }
}
