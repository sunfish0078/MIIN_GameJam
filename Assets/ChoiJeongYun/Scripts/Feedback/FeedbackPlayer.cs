using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Feedback
{
    public class FeedbackPlayer : MonoBehaviour
    {
        private List<AbstractFeedback> _feedbacks;

        private void Awake()
        {
            _feedbacks = GetComponents<AbstractFeedback>().ToList();
        }

        public void PlayFeedback()
        {
            _feedbacks.ForEach(feedback => feedback.CreateFeedback());
        }

        public void StopFeedback()
        {
            _feedbacks.ForEach(feedback => feedback.FinishFeedback());
        }
    }
}