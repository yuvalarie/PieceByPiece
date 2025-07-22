using System;
using System.Collections.Generic;
using Gameplay.Objects.TargetObjects;
using UnityEngine;

namespace Core.Managers
{
    
    [Serializable]
    public struct DistanceScoreEntry
    {
        public float minDistance;
        public float score;
        public float maxDistance;
    }
    
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private DistanceScoreEntry scoreEntries;
        [SerializeField] private int brokenJointPenalty;

        private double _score;
        
        public static ScoreManager Instance {get; private set;}
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventManager.GameOver += CalculateScore;
        }
        
        private void OnDisable()
        {
            EventManager.GameOver -= CalculateScore;
        }

        private void CalculateScore()
        {
            _score = Math.Ceiling(_score);
        }
        
        public void AddToScore(float distance)
        {
            _score += DisToScore(distance);
            Debug.Log($"Score added: {_score} for distance: {distance}");
        }
        
        private float DisToScore(float distance)
        {
            if (distance < scoreEntries.minDistance)
                return scoreEntries.score;
            if (distance > scoreEntries.maxDistance)
                return 0;
            return (distance - scoreEntries.minDistance) * (scoreEntries.score / (scoreEntries.maxDistance - scoreEntries.minDistance));
        }
        
        public void BrokenJointPenalty()
        {
            _score -= brokenJointPenalty;
            Debug.Log($"Broken joint penalty applied: {_score} remaining score.");
        }
    }
}