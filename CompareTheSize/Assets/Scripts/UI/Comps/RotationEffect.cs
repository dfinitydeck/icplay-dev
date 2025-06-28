using UnityEngine;

namespace UI
{
    public class RotationEffect : MonoBehaviour
    {
        [SerializeField] private float m_RotSpeed = 1f;
        private float m_CurrZRot = 0;

        private void OnEnable()
        {
            m_CurrZRot = 0;
        }

        private void Update()
        {
            m_CurrZRot += Time.deltaTime * m_RotSpeed;
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, m_CurrZRot));
            if(m_CurrZRot >= 360)
                m_CurrZRot = 0;
        }
    }
}