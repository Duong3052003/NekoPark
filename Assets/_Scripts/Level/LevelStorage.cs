using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStorage : MonoBehaviour
{
    [SerializeField] public Sprite[] skins;
    [SerializeField] public RuntimeAnimatorController[] controllers;
    [SerializeField] public GameObject[] items;
    [SerializeField] public float[] itemsTransformX;
    [SerializeField] public float[] itemsTransformY;

}
