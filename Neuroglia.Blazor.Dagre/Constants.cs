namespace Neuroglia.Blazor.Dagre
{
    public static class Constants
    {
        public const double NodeWidth = 80;
        public const double NodeHeight = 40;
        public const double NodeRadius = 5;

        public const double ClusterWidth = 120;
        public const double ClusterHeight = 80;
        public const double ClusterRadius = 10;
        /**
         * Observed cluster padding, don't know where is comes from. 
         * The "ranksep" and "nodesep" default values at 50...?
         */
        public const double ClusterPaddingX = 50;
        public const double ClusterPaddingY = 70;

        public const double LabelHeight = 25;

        public const string EdgeEndArrowId = "end-arrow";

        public const decimal MinScale = 0.2M;
        public const decimal MaxScale = 5M;
    }
}
