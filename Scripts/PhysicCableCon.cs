using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Connector))]
public class PhysicCableCon : Liftable
{
    private Connector _connector;

    protected override void Awake()
    {
        base.Awake();

        _connector = gameObject.GetComponent<Connector>();
    }

    public override void Drop()
    {
        base.Drop();
        
        Interactable selecredObject = FindObjectOfType<PlayerInteractions>().SelectedObject;
        if (selecredObject && selecredObject.TryGetComponent(out Connector secondConnector))
        {
            if (_connector.CanConnect(secondConnector))
                secondConnector.Connect(_connector, true);
            else if (!secondConnector.IsConnected)
            {
                transform.rotation = secondConnector.ConnectionRotation * _connector.RotationOffset;
                transform.position = (secondConnector.ConnectionPosition + secondConnector.ConnectedOutOffset * 0.2f) - (_connector.ConnectionPosition - _connector.transform.position);
            }
        }
    }
    public override void PickUp(int layer)
    {
        base.PickUp(layer);

        if (_connector.ConnectedTo)
            _connector.Disconnect(true);



        StartCoroutine(UpdatePositionOfPreviouesCable());
    }

    private IEnumerator UpdatePositionOfPreviouesCable()
    {
        PhysicCable cable = GetComponentInParent<PhysicCable>();
        Transform previousPoint = null;
        if (_connector == cable.StartConnector)
            previousPoint = cable.Points[1];
        else if (_connector == cable.EndConnector)
            previousPoint = cable.Points[cable.Points.Count - 2];

        if (previousPoint == null)
        {
            Debug.LogWarning($"{name}: Cant find previous point!");
            yield break;
        }

        Rigidbody thisRB = GetComponent<Rigidbody>();
        Rigidbody previousRB = previousPoint.GetComponent<Rigidbody>();
        previousRB.isKinematic = true;

        SpringJoint spring = null;
        foreach (var s in previousPoint.GetComponents<SpringJoint>())
            if (s.connectedBody == thisRB)
                spring = s;
        if (spring != null)
            Destroy(spring);

        while (IsLift)
        {
            if (Vector3.Distance(transform.position, previousPoint.position) > 0)
                previousPoint.position += (transform.position - previousPoint.position) * Mathf.Clamp01(Time.deltaTime * 100);
            yield return null;
        }

        previousRB.isKinematic = false;
        spring = previousPoint.gameObject.AddComponent<SpringJoint>();
        cable.SetSpirng(spring, thisRB);
    }
}
