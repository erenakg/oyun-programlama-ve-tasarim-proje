using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class RopeVerlet : MonoBehaviour
{
    [Header("Rope")]
    [SerializeField] private int _numofRopeSegments = 50;
    [SerializeField] private float _ropeSegmentLength = 0.225f;

    [Header("Physics")]
    [SerializeField] private Vector2 _gravityForce = new Vector2(0f, -2f);
    [SerializeField] private float _dampingFactor = 0.98f;
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.1f;
    [SerializeField] private float _bounceFactor = 0.1f;
    [SerializeField] private float _correctionClampAmount = 0.1f;

    [Header("Constraints")]
    [SerializeField] private int _numOFConstraintRuns = 50;

    [Header("Optimization")]
    [SerializeField] private int _collisionSegmentInterval = 2;

    [Header("Character Attachment")]
    [SerializeField] private Transform _leftCharacter;
    [SerializeField] private Transform _rightCharacter;
    [SerializeField] private Vector2 _leftCharacterOffset = Vector2.zero;
    [SerializeField] private Vector2 _rightCharacterOffset = Vector2.zero;

    [SerializeField] private LineRenderer _lineRenderer;
    private readonly List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    private Vector3 _ropeStartPoint;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _numofRopeSegments;

        _ropeStartPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        for (int i = 0; i < _numofRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(_ropeStartPoint));
            _ropeStartPoint.y -= _ropeSegmentLength;
        }
    }

    private void Update()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        Simulate();

        for (int i = 0; i < _numOFConstraintRuns; i++)
        {
            ApplyConstraints();

            if (i % _collisionSegmentInterval == 0)
            {
                HandleCollisions();
            }
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[_numofRopeSegments];
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            ropePositions[i] = _ropeSegments[i].CurrentPosition;
        }
        _lineRenderer.SetPositions(ropePositions);
    }

    private void Simulate()
    {
        int startIndex = _leftCharacter != null ? 1 : 0;
        int endIndex = _rightCharacter != null ? _ropeSegments.Count - 2 : _ropeSegments.Count - 1;

        for (int i = startIndex; i <= endIndex; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = (segment.CurrentPosition - segment.OldPosition) * _dampingFactor;

            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += velocity;
            segment.CurrentPosition += _gravityForce * Time.fixedDeltaTime;

            _ropeSegments[i] = segment;
        }
    }

    private void ApplyConstraints()
    {
        if (_leftCharacter != null)
        {
            RopeSegment firstSegment = _ropeSegments[0];
            firstSegment.CurrentPosition = (Vector2)_leftCharacter.position + _leftCharacterOffset;
            firstSegment.OldPosition = firstSegment.CurrentPosition;
            _ropeSegments[0] = firstSegment;
        }

        if (_rightCharacter != null)
        {
            RopeSegment lastSegment = _ropeSegments[_ropeSegments.Count - 1];
            lastSegment.CurrentPosition = (Vector2)_rightCharacter.position + _rightCharacterOffset;
            lastSegment.OldPosition = lastSegment.CurrentPosition;
            _ropeSegments[_ropeSegments.Count - 1] = lastSegment;
        }

        for (int i = 0; i < _ropeSegments.Count - 1; i++)
        {
            RopeSegment currentSeg = _ropeSegments[i];
            RopeSegment nextSeg = _ropeSegments[i + 1];

            float dist = Vector2.Distance(currentSeg.CurrentPosition, nextSeg.CurrentPosition);
            if (dist == 0f)
                continue;

            float difference = dist - _ropeSegmentLength;
            Vector2 changeDir = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).normalized;
            Vector2 changeVector = changeDir * difference;

            bool currentFixed = i == 0 && _leftCharacter != null;
            bool nextFixed = i == _ropeSegments.Count - 2 && _rightCharacter != null;

            if (currentFixed && nextFixed)
            {
            }
            else if (currentFixed)
            {
                nextSeg.CurrentPosition += changeVector;
            }
            else if (nextFixed)
            {
                currentSeg.CurrentPosition -= changeVector;
            }
            else
            {
                currentSeg.CurrentPosition -= changeVector * 0.5f;
                nextSeg.CurrentPosition += changeVector * 0.5f;
            }

            _ropeSegments[i] = currentSeg;
            _ropeSegments[i + 1] = nextSeg;
        }
    }

    private void HandleCollisions()
    {
        int startIndex = _leftCharacter != null ? 1 : 0;
        int endIndex = _rightCharacter != null ? _ropeSegments.Count - 2 : _ropeSegments.Count - 1;

        for (int i = startIndex; i <= endIndex; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = segment.CurrentPosition - segment.OldPosition;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(segment.CurrentPosition, _collisionRadius, _collisionMask);

            foreach (Collider2D collider in colliders)
            {
                Vector2 closestPoint = collider.ClosestPoint(segment.CurrentPosition);
                float distance = Vector2.Distance(segment.CurrentPosition, closestPoint);

                if (distance < _collisionRadius)
                {
                    Vector2 normal = (segment.CurrentPosition - closestPoint).normalized;
                    if (normal == Vector2.zero)
                    {
                        normal = (segment.CurrentPosition - (Vector2)collider.transform.position).normalized;
                    }

                    float depth = _collisionRadius - distance;
                    segment.CurrentPosition += normal * depth;
                    velocity = Vector2.Reflect(velocity, normal) * _bounceFactor;
                }
            }

            segment.OldPosition = segment.CurrentPosition - velocity;
            _ropeSegments[i] = segment;
        }
    }

    public struct RopeSegment
    {
        public Vector2 CurrentPosition;
        public Vector2 OldPosition;

        public RopeSegment(Vector2 pos)
        {
            CurrentPosition = pos;
            OldPosition = pos;
        }
    }
}