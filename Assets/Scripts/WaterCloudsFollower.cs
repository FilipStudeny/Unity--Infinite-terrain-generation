using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCloudsFollower : MonoBehaviour
{
    [SerializeField] GameObject waterObject;
    [SerializeField] GameObject cloudsObject;

    bool renderWater;
    bool renderClouds;

    Vector3 cameraPosition;

    private void Start()
    {
        waterObject.transform.position = new Vector3(transform.position.x, 7, transform.position.z);
        cloudsObject.transform.position = new Vector3(transform.position.x, 500, transform.position.z);


        renderWater = true;
        renderClouds = true;
    }

    void Update()
    {
        cameraPosition = new Vector3(transform.position.x, 0, transform.position.z);

        if (renderWater)
        {
            RenderObject(waterObject, 7, true);
        }
        else
        {
            RenderObject(waterObject, 7, false);
        }

        if (renderClouds)
        {
            RenderObject(cloudsObject, 500, true);
        }
        else
        {
            RenderObject(cloudsObject, 500, false);
        }
    }

    private void RenderObject(GameObject _object, int height, bool render)
    {
        if (render)
        {
            if (!_object.activeInHierarchy)
            {
                _object.SetActive(true);
            }

            ObjectPositon(cameraPosition, _object, height);
        }
        else
        {
            _object.SetActive(false);
        }

    }



    public void SetWaterRendering(bool render)
    {
        renderWater = render;
    }

    public void SetCloudsRendering(bool render)
    {
        renderClouds = render;
    }


    void ObjectPositon(Vector3 cameraPosition, GameObject _objectToMove, int objectHeight)
    {
        Vector3 _objectPosition = new Vector3(cameraPosition.x, objectHeight, cameraPosition.z);
        _objectToMove.transform.position = _objectPosition;
    }


}