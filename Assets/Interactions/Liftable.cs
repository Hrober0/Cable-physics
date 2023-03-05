using System.Collections.Generic;
using UnityEngine;

namespace HInteractions
{
    [RequireComponent(typeof(Rigidbody))]
    public class Liftable : Interactable
    {
        [field: SerializeField] public bool IsLift { get; private set; } = false;
        [field: SerializeField] public Vector3 LiftDirectionOffset { get; private set; } = Vector3.zero;

        public Rigidbody Rigidbody { get; protected set; }
        public IObjectHolder ObjectHolder { get; protected set; }

        private readonly List<(GameObject obj, int defaultLayer)> _defaultLayers = new();

        protected override void Awake()
        {
            base.Awake();
            Rigidbody = GetComponent<Rigidbody>();
        }

        public virtual void PickUp(IObjectHolder holder, int layer)
        {
            if (IsLift)
                return;

            ObjectHolder = holder;

            // save layers
            _defaultLayers.Clear();
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
                _defaultLayers.Add((col.gameObject, col.gameObject.layer));

            // set
            Rigidbody.useGravity = false;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            foreach ((GameObject obj, int defaultLayer) item in _defaultLayers)
                item.obj.layer = layer;

            IsLift = true;
        }
        public virtual void Drop()
        {
            if (!IsLift)
                return;

            ObjectHolder = null;

            Rigidbody.useGravity = true;
            Rigidbody.interpolation = RigidbodyInterpolation.None;
            foreach ((GameObject obj, int defaultLayer) item in _defaultLayers)
                item.obj.layer = item.defaultLayer;

            IsLift = false;
        }
    }
}