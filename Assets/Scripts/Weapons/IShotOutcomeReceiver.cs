namespace PolishTanks.Weapons
{
    /// <summary>
    /// Implemented by a component on the shooter's rigidbody object that wants to learn how its shots landed
    /// (an AI judging whether it can penetrate a target). Told once per impact, after the target has answered.
    /// </summary>
    public interface IShotOutcomeReceiver
    {
        void OnShotOutcome(in ShotOutcome outcome);
    }
}
