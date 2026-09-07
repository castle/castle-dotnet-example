namespace CastleDemo.Demos
{
    /// <summary>
    /// Fixture values for the simulated user the demo signs in as. Only the
    /// Castle publishable key and API secret need to be supplied via
    /// configuration; everything else is baked in so the workflows are
    /// reproducible without a real database or auth system.
    /// </summary>
    public class DemoConfig
    {
        public string Pk { get; set; } = "";
        public string ApiSecret { get; set; } = "";

        public string ValidUsername { get; set; } = "clark.kent@dailyplanet.com";
        public string ValidName { get; set; } = "Clark Kent";
        public string ValidUserId { get; set; } = "00000000";
        public string ValidPassword { get; set; } = "1234";
        public string InvalidPassword { get; set; } = "qwerty";
        public string RegisteredAt { get; set; } = "2020-02-23T22:28:55.387Z";

        public string NewUserName { get; set; } = "Lois Lane";
        public string NewUserEmail { get; set; } = "lois.lane@dailyplanet.com";
    }
}
