using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OpenableSensor : MonoBehaviour {

    [Header("References")]
    private Collider trigger;

    [Header("Linked To")]
    [SerializeField, Tooltip("The openable to send a signal to")] private OpenableInteractable[] openables;

    [Header("Settings")]
    [SerializeField, Tooltip("Door's auto close timer will not start until the sensor area is empty")] private bool holdOpen;
    [SerializeField, Tooltip("Objects on these layers will trip the sensor")] private int[] layerWhitelistAry = { 6 }; // for serialization only 
    private HashSet<int> layerWhitelist;
    private HashSet<GameObject> population;

    [Header("Validation")]
    private const int populationValidateTicks = 50;
    private int tickCounter = 0;
    private List<GameObject> invalidPopulation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

        trigger = GetComponent<Collider>();

        foreach (OpenableInteractable openable in openables)
            openable.SetAutoOpenable();

        // swap layer array to HashSet for optimization
        layerWhitelist = new HashSet<int>(layerWhitelistAry);

        // number of things inside;
        population = new HashSet<GameObject>();

        // used for validation in FixedUpdate
        invalidPopulation = new List<GameObject>();

    }

    #region Sensor

    private void OnTriggerEnter(Collider other) {

        // do not activate if stuff is currently sitting inside, or if the object is on an ignored layer

        GameObject obj = other.gameObject;

        if (IsWhitelisted(obj)) {

            // only interact with the openable if this is the first object to enter when it was previously empty
            if (population.Count == 0)
                EngageOpenables();

            population.Add(obj);

        }

    }

    private void OnTriggerStay(Collider other) {

        GameObject obj = other.gameObject;

        if (IsWhitelisted(obj))
            population.Add(obj);

    }



    private void OnTriggerExit(Collider other) {

        GameObject obj = other.gameObject;

        // do not activate if stuff is currently sitting inside, or if the object is on an ignored layer
        if (IsWhitelisted(obj)) {

            population.Remove(obj);

            // only interact with the openable if this is the last object to exit, leaving the trigger region empty
            if (population.Count == 0)
                DisengageOpenables();


        }

    }

    #endregion

    #region Openable Interfacing
    private void EngageOpenables() {

        foreach (OpenableInteractable openable in openables) {

            openable.SensorInteract();

            openable.SetHeldOpen(holdOpen);

        }
    }

    private void DisengageOpenables() {

        foreach (OpenableInteractable openable in openables) {

            if (holdOpen)
                openable.SetHeldOpen(false);
            else
                openable.SensorInteract();

        }

    }

    #endregion

    #region Validation
    private void FixedUpdate() {

        // validate population
        if (population.Count != 0) {

            if (tickCounter == populationValidateTicks) {

                tickCounter = 0;

                // i threw the activeSelf check in here cuz y not. the layer check is the important one
                foreach (GameObject obj in population)
                    if (!IsWhitelisted(obj) || !obj.activeSelf)
                        invalidPopulation.Add(obj);

                bool triggerOpenables = invalidPopulation.Count > 0;

                foreach (GameObject obj in invalidPopulation)
                    population.Remove(obj);

                if (triggerOpenables)
                    DisengageOpenables();

                invalidPopulation.Clear();

            }
            else
                tickCounter++;

        }

    }

    #endregion

    #region Util

    private bool IsWhitelisted(GameObject obj) => layerWhitelist.Contains(obj.layer);

    #endregion
}
