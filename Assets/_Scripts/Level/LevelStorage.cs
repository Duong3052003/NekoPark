using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelStorage : MonoBehaviour
{
    [SerializeField] public GameObject playerModel;
    [SerializeField] public Sprite[] skins;
    [SerializeField] public RuntimeAnimatorController[] controllers;
    [SerializeField] public GameObject[] items;
    [SerializeField] public bool[] isHost;
    [SerializeField] public bool[] isParent;
    [SerializeField] public float[] itemsTransformX;
    [SerializeField] public float[] itemsTransformY;
}