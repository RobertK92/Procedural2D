
namespace Game1
{
    public struct InitializeWorldProgressArgs
    {
        public readonly RegionId RegionId;
        public readonly int Percentage;

        public InitializeWorldProgressArgs(RegionId id, int percentage)
        {
            this.RegionId = id;
            this.Percentage = percentage;
        }
    }
}
