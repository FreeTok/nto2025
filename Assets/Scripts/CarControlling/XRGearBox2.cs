using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable lever that snaps into an on or off position by a direct interactor
    /// </summary>
    public class XRGearBox2 : XRBaseInteractable
    {
        const float k_LeverDeadZone = 0.1f; // Prevents rapid switching between on and off states when right in the middle

        [SerializeField]
        [Tooltip("The object that is visually grabbed and manipulated")]
        Transform m_Handle = null;

        [SerializeField]
        [Tooltip("The value of the lever")]
        float m_Value;

        [SerializeField]
        [Tooltip("If enabled, the lever will snap to the value position when released")]
        bool m_LockToValue;

        [SerializeField]
        [Tooltip("Angle of the lever in the 'on' position")]
        [Range(-90.0f, 90.0f)]
        float m_MaxAngle = 90.0f;

        [SerializeField]
        [Tooltip("Angle of the lever in the 'off' position")]
        [Range(-90.0f, 90.0f)]
        float m_MinAngle = -90.0f;

        [SerializeField]
        [Tooltip("Events to trigger when the lever activates")]
        UnityEvent m_OnLeverActivate = new UnityEvent();

        [SerializeField]
        [Tooltip("Events to trigger when the lever deactivates")]
        UnityEvent m_OnLeverDeactivate = new UnityEvent();
        
        [SerializeField]
        [Tooltip("The current position of the lever (0 to 3)")]
        int m_Position = 0;

        IXRSelectInteractor m_Interactor;

        /// <summary>
        /// The object that is visually grabbed and manipulated
        /// </summary>
        public Transform handle
        {
            get => m_Handle;
            set => m_Handle = value;
        }

        /// <summary>
        /// If enabled, the lever will snap to the value position when released
        /// </summary>
        public bool lockToValue { get; set; }

        /// <summary>
        /// Angle of the lever in the 'on' position
        /// </summary>
        public float maxAngle
        {
            get => m_MaxAngle;
            set => m_MaxAngle = value;
        }

        /// <summary>
        /// Angle of the lever in the 'off' position
        /// </summary>
        public float minAngle
        {
            get => m_MinAngle;
            set => m_MinAngle = value;
        }

        /// <summary>
        /// Events to trigger when the lever activates
        /// </summary>
        public UnityEvent onLeverActivate => m_OnLeverActivate;

        /// <summary>
        /// Events to trigger when the lever deactivates
        /// </summary>
        public UnityEvent onLeverDeactivate => m_OnLeverDeactivate;
        
        [SerializeField]
        [Tooltip("Event that triggers when the lever position changes")]
        UnityEvent<int> m_OnPositionChanged = new UnityEvent<int>();

        /// <summary>
        /// Event that triggers when the lever position changes
        /// </summary>
        public UnityEvent<int> onPositionChanged => m_OnPositionChanged;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        void StartGrab(SelectEnterEventArgs args)
        {
            m_Interactor = args.interactorObject;
        }

        void EndGrab(SelectExitEventArgs args)
        {
            if (0.7 > m_Value && m_Position < 4)
            {
                m_Position += 1;
            }
            
            else if (m_Value < -0.7 && m_Position > -1)
            {
                m_Position -= 1;
            }
            
            SnapToNearestPosition();
            m_Interactor = null;

            OnPositionChanged();
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (isSelected)
                {
                    UpdateValue();
                }
            }
        }

        Vector3 GetLookDirection()
        {
            Vector3 direction = m_Interactor.GetAttachTransform(this).position - m_Handle.position;
            direction = transform.InverseTransformDirection(direction);
            direction.x = 0;

            return direction.normalized;
        }

        float m_CurrentAngle;

        void UpdateValue()
        {
            var lookDirection = GetLookDirection();
            var lookAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;

            // Плавно обновляем текущий угол рычага
            m_CurrentAngle = Mathf.Clamp(lookAngle, m_MinAngle, m_MaxAngle);
            SetHandleAngle(m_CurrentAngle);
            
            
        }
        
        void SnapToNearestPosition()
        {
            SetHandleAngle(0);
        }
        
        
        void OnPositionChanged()
        {
            // Здесь можно добавить логику, которая будет выполняться при изменении позиции рычага
            Debug.Log($"Lever position changed to {m_Position}");
            m_OnPositionChanged.Invoke(m_Position);
        }

        // void SetValue(float newValue, bool forceRotation = false)
        // {
        //     if (m_Position == position)
        //     {
        //         if (forceRotation)
        //             SetHandleAngle(m_MinAngle + position * ((m_MaxAngle - m_MinAngle) / 3));
        //         return;
        //     }
        //
        //     m_Position = position;
        //
        //     // Здесь можно добавить вызовы событий или другую логику, которая должна выполняться при изменении позиции
        //
        //     if (!isSelected && (m_LockToValue || forceRotation))
        //         SetHandleAngle(m_MinAngle + position * ((m_MaxAngle - m_MinAngle) / 3));
        // }

        void SetHandleAngle(float angle)
        {
            if (m_Handle != null)
                m_Handle.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
        }

        void OnDrawGizmosSelected()
        {
            var angleStartPoint = transform.position;

            if (m_Handle != null)
                angleStartPoint = m_Handle.position;

            const float k_AngleLength = 0.25f;

            var angleMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
            var angleMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MinAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(angleStartPoint, angleMaxPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(angleStartPoint, angleMinPoint);
        }

        void OnValidate()
        {
            SetHandleAngle(m_MinAngle + m_Position * ((m_MaxAngle - m_MinAngle) / 3));
        }
    }
}
