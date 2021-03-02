namespace Wyrobot2.Data.Models
{
    public class LevelReward
    {
        public LevelReward(int requiredLevel, ulong roleId)
        {
            RequiredLevel = requiredLevel;
            RoleId = roleId;
        }
        public int RequiredLevel { get; set; }
        public ulong RoleId { get; set; }
    }
}