﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonCanvas : MonoBehaviour

{
    public Transform playercamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(playercamera.position);
    }
}
