using System;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Feedback
{
    public abstract class AbstractFeedback : MonoBehaviour
    {
        public abstract void CreateFeedback();
        public abstract void FinishFeedback();

        private void OnDisable()
        {
            FinishFeedback();
        }
    }
}