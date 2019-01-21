using System;
using UnityEngine;

namespace IllusionInjector
{
    internal class Bootstrapper : MonoBehaviour
    {
        public event Action Destroyed = delegate { };

        private void Awake()
        {
            if (Environment.CommandLine.Contains("--verbose") && (!Screen.fullScreen || Environment.CommandLine.Contains("--ipa-console")))
            {
                Windows.GuiConsole.CreateConsole();
            }
        }

        private void Start()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroyed();
        }
    }
}
