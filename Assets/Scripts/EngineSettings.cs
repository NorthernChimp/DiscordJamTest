using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EngineSettings", menuName = "Settings/EngineSettings")]
public class EngineSettings :ScriptableObject
{
    bool affectedByGravity = false;public bool GetGravity() { return affectedByGravity; }public void SetAffectedByGravity(bool b) { affectedByGravity = b; }
    bool controlled = false; public bool GetControlled() { return controlled; }public void SetControlled(bool b) { controlled = b; }
    [SerializeField]
    float maxSpeed = 0.95f;public float GetMaxSpeed() { return maxSpeed; }public void AddToMaxSpeed(float a) { maxSpeed += a; }
    float mass = 1f;public float GetMass() { return mass; }public void AddToMass(float a) { mass += a; }public void SetMass(float f) { mass = f; }
    [SerializeField]
    float moveSpeed = 242.0f;public float GetMoveSpeed() { return moveSpeed; }public void AddToMoveSpeed(float a) { moveSpeed += a; }
    float accelerationDecay = 0.05f;public float GetAccelerationDecay() { return accelerationDecay; }public void AddToAccelerationDecay(float a) { accelerationDecay += a; }
    [SerializeField]
    float jumpAmount = 0.25f; public float GetJumpAmount() { return jumpAmount; }public void AddToJumpAmount(float a) { jumpAmount += a; }
    float bounceAmount = 0.25f; public float GetBounceAmount() { return bounceAmount; }public void AddToBounceAmount(float a) { bounceAmount += a; }
    public void CopySettings(EngineSettings s)// ANY NEW VARIABLES MUST BE ADDED HERE OR THEY WILL NOT BE COPIED FROM DEFAULT SETTINGS
    {
        affectedByGravity = s.GetGravity();
        maxSpeed = s.GetMaxSpeed();
        accelerationDecay = s.GetAccelerationDecay();
        jumpAmount = s.GetJumpAmount();
        controlled = s.GetControlled();
        moveSpeed = s.GetMoveSpeed();
        mass = s.GetMass();
    }
}
public class EngineSettingsAffector     //this class is really just a data class for when you want to change the settings in some way without changing default settings
{                                       //if you want to change the settings forever change the default settings, otherwise these affectors can temporarily or permenantly change the functionality of the engine (the player)
    EngineSettingsAffectorType typeOfAffector; public EngineSettingsAffectorType GetTypeOfAffector() { return typeOfAffector; }
    float amount; public float GetAmount() { return amount; }
    bool endsOverTime = false;
    Counter endCounter;
    public bool UpdateAffector(float timePassed)
    {
        if (endsOverTime) { return endCounter.UpdateCounter(timePassed); }//if it ends over time return true when the counter has finished
        return false;
    }
    public EngineSettingsAffector(float amt, EngineSettingsAffectorType typeOfAff, float time)
    {
        typeOfAffector = typeOfAff;
        amount = amt;
        if (time > 0f)
        {
            endsOverTime = true;
            endCounter = new Counter(time);
        }
    }
}
public enum EngineSettingsAffectorType { addToSpeed, addToMultiplier, addToMaxSpeed, addToTimeHeldDownMultiplier, addToFireRate, negateDownwardMomentum,addToJump }
