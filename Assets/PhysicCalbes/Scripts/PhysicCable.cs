using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace HPhysic
{
    public class PhysicCable : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField, Min(1)] private int numberOfPoints = 3;
        [SerializeField, Min(0.01f)] private float space = 0.3f;
        [SerializeField, Min(0.01f)] private float size = 0.3f;

        [Header("Bahaviour")]
        [SerializeField, Min(1f)] private float springForce = 200;
        [SerializeField, Min(1f)] private float brakeLengthMultiplier = 2f;
        [SerializeField, Min(0.1f)] private float minBrakeTime = 1f;
        private float brakeLength;
        private float timeToBrake = 1f;

        [Header("Object to set")]
        [SerializeField, Required] private GameObject start;
        [SerializeField, Required] private GameObject end;
        [SerializeField, Required] private GameObject connector0;
        [SerializeField, Required] private GameObject point0;

        private List<Transform> points;
        private List<Transform> connectors;

        private const string cloneText = "Part";

        private Connector startConnector;
        private Connector endConnector;

        [Button("Reset points")]
        private void UpdatePoints()
        {
            if (!start || !end || !point0 || !connector0)
            {
                Debug.LogWarning("Can't update because one of objects to set is null!");
                return;
            }

            // delete old
            int length = transform.childCount;
            for (int i = 0; i < length; i++)
                if (transform.GetChild(i).name.StartsWith(cloneText))
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                    length--;
                    i--;
                }

            // set new
            Vector3 lastPos = start.transform.position;
            Rigidbody lasBody = start.GetComponent<Rigidbody>();
            for (int i = 0; i < numberOfPoints; i++)
            {
                GameObject cConnector = i == 0 ? connector0 : CreateNewCon(i);
                GameObject cPoint = i == 0 ? point0 : CreateNewPoint(i);

                Vector3 newPos = CountNewPointPos(lastPos);
                cPoint.transform.position = newPos;
                cPoint.transform.localScale = Vector3.one * size;
                cPoint.transform.rotation = transform.rotation;

                SetSpirng(cPoint.GetComponent<SpringJoint>(), lasBody);

                lasBody = cPoint.GetComponent<Rigidbody>();

                cConnector.transform.position = CountConPos(lastPos, newPos);
                cConnector.transform.localScale = CountSizeOfCon(lastPos, newPos);
                cConnector.transform.rotation = CountRoationOfCon(lastPos, newPos);
                lastPos = newPos;
            }

            Vector3 endPos = CountNewPointPos(lastPos);
            end.transform.position = endPos;
            SetSpirng(lasBody.gameObject.AddComponent<SpringJoint>(), end.GetComponent<Rigidbody>());

            GameObject endConnector = CreateNewCon(numberOfPoints);
            endConnector.transform.position = CountConPos(lastPos, endPos);
            endConnector.transform.rotation = CountRoationOfCon(lastPos, endPos);


            Vector3 CountNewPointPos(Vector3 lastPos) => lastPos + transform.forward * space;
        }

        [Button("Add point")]
        private void AddPoint()
        {
            Transform lastprevPoint = GetPoint(numberOfPoints - 1);
            if (lastprevPoint == null)
            {
                Debug.LogWarning("Dont found point number " + (numberOfPoints - 1));
                return;
            }

            Rigidbody endRB = end.GetComponent<Rigidbody>();
            foreach (var spring in lastprevPoint.GetComponents<SpringJoint>())
                if (spring.connectedBody == endRB)
                    DestroyImmediate(spring);

            GameObject cPoint = CreateNewPoint(numberOfPoints);
            GameObject cConnector = CreateNewCon(numberOfPoints + 1);

            cPoint.transform.position = end.transform.position;
            cPoint.transform.rotation = end.transform.rotation;
            cPoint.transform.localScale = Vector3.one * size;

            SetSpirng(cPoint.GetComponent<SpringJoint>(), lastprevPoint.GetComponent<Rigidbody>());
            SetSpirng(cPoint.AddComponent<SpringJoint>(), endRB);

            // fix end
            end.transform.position += end.transform.forward * space;

            cConnector.transform.position = CountConPos(cPoint.transform.position, end.transform.position);
            cConnector.transform.localScale = CountSizeOfCon(cPoint.transform.position, end.transform.position);
            cConnector.transform.rotation = CountRoationOfCon(cPoint.transform.position, end.transform.position);

            numberOfPoints++;
        }

        [Button("Remove point")]
        private void RemovePoint()
        {
            if (numberOfPoints < 2)
            {
                Debug.LogWarning("Cable can't be shorter then 1");
                return;
            }

            Transform lastprevPoint = GetPoint(numberOfPoints - 1);
            if (lastprevPoint == null)
            {
                Debug.LogWarning("Dont found point number " + (numberOfPoints - 1));
                return;
            }

            Transform lastprevCon = GetConnector(numberOfPoints);
            if (lastprevCon == null)
            {
                Debug.LogWarning("Dont found connector number " + (numberOfPoints));
                return;
            }

            Transform lastlastprevPoint = GetPoint(numberOfPoints - 2);
            if (lastlastprevPoint == null)
            {
                Debug.LogWarning("Dont found point number " + (numberOfPoints - 2));
                return;
            }


            Rigidbody endRB = end.GetComponent<Rigidbody>();
            SetSpirng(lastlastprevPoint.gameObject.AddComponent<SpringJoint>(), endRB);

            end.transform.position = lastprevPoint.position;
            end.transform.rotation = lastprevPoint.rotation;

            DestroyImmediate(lastprevPoint.gameObject);
            DestroyImmediate(lastprevCon.gameObject);

            numberOfPoints--;
        }


        private void Start()
        {
            startConnector = start.GetComponent<Connector>();
            endConnector = end.GetComponent<Connector>();

            brakeLength = space * numberOfPoints * brakeLengthMultiplier + 2f;

            points = new List<Transform>();
            connectors = new List<Transform>();

            points.Add(start.transform);
            points.Add(point0.transform);

            connectors.Add(connector0.transform);

            for (int i = 1; i < numberOfPoints; i++)
            {
                Transform conn = GetConnector(i);
                if (conn == null)
                    Debug.LogWarning("Dont found connector number " + i);
                else
                    connectors.Add(conn);

                Transform point = GetPoint(i);
                if (conn == null)
                    Debug.LogWarning("Dont found point number " + i);
                else
                    points.Add(point);
            }

            Transform endConn = GetConnector(numberOfPoints);
            if (endConn == null)
                Debug.LogWarning("Dont found connector number " + numberOfPoints);
            else
                connectors.Add(endConn);

            points.Add(end.transform);
        }

        private void Update()
        {
            float cableLength = 0f;
            bool isConnected = startConnector.IsConnected || endConnector.IsConnected;

            int numOfParts = connectors.Count;
            Transform lastPoint = points[0];
            for (int i = 0; i < numOfParts; i++)
            {
                Transform nextPoint = points[i + 1];
                Transform connector = connectors[i].transform;
                connector.position = CountConPos(lastPoint.position, nextPoint.position);
                if (lastPoint.position == nextPoint.position || nextPoint.position == connector.position)
                {
                    connector.localScale = Vector3.zero;
                }
                else
                {
                    connector.rotation = Quaternion.LookRotation(nextPoint.position - connector.position);
                    connector.localScale = CountSizeOfCon(lastPoint.position, nextPoint.position);
                }

                if (isConnected)
                    cableLength += (lastPoint.position - nextPoint.position).magnitude;

                lastPoint = nextPoint;
            }

            if (isConnected)
            {
                if (cableLength > brakeLength)
                {
                    timeToBrake -= Time.deltaTime;
                    if (timeToBrake < 0f)
                    {
                        startConnector.Disconnect();
                        endConnector.Disconnect();
                        timeToBrake = minBrakeTime;
                    }
                }
                else
                {
                    timeToBrake = minBrakeTime;
                }
            }
        }

        private Vector3 CountConPos(Vector3 start, Vector3 end) => (start + end) / 2f;
        private Vector3 CountSizeOfCon(Vector3 start, Vector3 end) => new Vector3(size, size, (start - end).magnitude / 2f);
        private Quaternion CountRoationOfCon(Vector3 start, Vector3 end) => Quaternion.LookRotation(end - start, Vector3.right);
        private string ConnectorName(int index) => $"{cloneText}_{index}_Conn";
        private string PointName(int index) => $"{cloneText}_{index}_Point";
        private Transform GetConnector(int index) => index > 0 ? transform.Find(ConnectorName(index)) : connector0.transform;
        private Transform GetPoint(int index) => index > 0 ? transform.Find(PointName(index)) : point0.transform;


        public void SetSpirng(SpringJoint spring, Rigidbody connectedBody)
        {
            spring.connectedBody = connectedBody;
            spring.spring = springForce;
            spring.damper = 0.2f;
            spring.autoConfigureConnectedAnchor = false;
            spring.anchor = Vector3.zero;
            spring.connectedAnchor = Vector3.zero;
            spring.minDistance = space;
            spring.maxDistance = space;
        }
        private GameObject CreateNewPoint(int index)
        {
            GameObject temp = Instantiate(point0);
            temp.name = PointName(index);
            temp.transform.parent = transform;
            return temp;
        }
        private GameObject CreateNewCon(int index)
        {
            GameObject temp = Instantiate(connector0);
            temp.name = ConnectorName(index);
            temp.transform.parent = transform;
            return temp;
        }


        public Connector StartConnector => startConnector;
        public Connector EndConnector => endConnector;
        public IReadOnlyList<Transform> Points => points;
    }
}