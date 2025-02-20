using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

public class SteeringWheel : XRKnob
{
    [SerializeField]
    [Tooltip("The interactor for the second hand")]
    IXRSelectInteractor m_SecondInteractor;

    Vector3 m_InitialHandPosition;
    Quaternion m_InitialHandRotation;

    bool m_IsDualHanded = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Инициализируем второй интеректор
        selectEntered.AddListener(StartDualHandGrab);
        selectExited.AddListener(EndDualHandGrab);
    }

    protected override void OnDisable()
    {
        selectEntered.RemoveListener(StartDualHandGrab);
        selectExited.RemoveListener(EndDualHandGrab);
        base.OnDisable();
    }

    void StartDualHandGrab(SelectEnterEventArgs args)
    {
        if (m_Interactor == null)
        {
            m_Interactor = args.interactorObject;
            m_InitialHandPosition = m_Interactor.GetAttachTransform(this).position;
            m_InitialHandRotation = m_Interactor.GetAttachTransform(this).rotation;
            m_IsDualHanded = false;
        }
        else
        {
            m_SecondInteractor = args.interactorObject;
            m_IsDualHanded = true;
        }

        m_PositionAngles.Reset();
        m_UpVectorAngles.Reset();
        m_ForwardVectorAngles.Reset();

        UpdateBaseKnobRotation();
        UpdateRotation(true);
    }

    void EndDualHandGrab(SelectExitEventArgs args)
    {
        if (args.interactorObject == m_Interactor)
        {
            m_Interactor = null;
        }
        else if (args.interactorObject == m_SecondInteractor)
        {
            m_SecondInteractor = null;
        }

        m_IsDualHanded = m_Interactor != null && m_SecondInteractor != null;
    }

    protected override void UpdateRotation(bool freshCheck = false)
    {
        if (m_IsDualHanded)
        {
            var handPosition1 = m_Interactor.GetAttachTransform(this).position;
            var handPosition2 = m_SecondInteractor.GetAttachTransform(this).position;

            // Вычисляем разницу между двумя руками
            var handDistance = Vector3.Distance(handPosition1, handPosition2);
            var direction = (handPosition2 - handPosition1).normalized;

            // Если руки слишком далеко друг от друга, усиливаем движение руля
            var rotationFactor = Mathf.Clamp01(handDistance / 0.5f); // допустим, 0.5 - это максимальное расстояние

            // Обновляем углы с учетом различия в позициях
            m_PositionAngles.SetBaseFromVector(direction);

            // Добавляем влияние на поворот руля
            float knobRotation = m_BaseKnobRotation - (m_PositionAngles.totalOffset * rotationFactor);

            if (m_ClampedMotion)
                knobRotation = Mathf.Clamp(knobRotation, m_MinAngle, m_MaxAngle);

            SetKnobRotation(knobRotation);

            // Преобразуем угол в значение
            var knobValue = (knobRotation - m_MinAngle) / (m_MaxAngle - m_MinAngle);
            SetValue(knobValue);
        }
        else
        {
            base.UpdateRotation(freshCheck);
        }
    }
}
