using System.Collections;
using UnityEngine;
using HInteractions;

namespace HPhysic
{
    [RequireComponent(typeof(Connector))]
    public class PhysicCableCon : Liftable
    {
        private Connector _connector;

        protected override void Awake()
        {
            base.Awake();

            _connector = gameObject.GetComponent<Connector>();
        }

        public override void PickUp(IObjectHolder holder, int layer)
        {
            base.PickUp(holder, layer);

            if (_connector.ConnectedTo)
                _connector.Disconnect();
        }

        public override void Drop()
        {
            if (ObjectHolder.SelectedObject && ObjectHolder.SelectedObject.TryGetComponent(out Connector secondConnector))
            {
                if (_connector.CanConnect(secondConnector))
                    secondConnector.Connect(_connector);
                else if (!secondConnector.IsConnected)
                {
                    transform.rotation = secondConnector.ConnectionRotation * _connector.RotationOffset;
                    transform.position = (secondConnector.ConnectionPosition + secondConnector.ConnectedOutOffset * 0.2f) - (_connector.ConnectionPosition - _connector.transform.position);
                }
            }

            base.Drop();
        }
    }
}