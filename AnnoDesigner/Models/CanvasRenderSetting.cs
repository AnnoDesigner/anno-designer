namespace AnnoDesigner.Models
{
    public class CanvasRenderSetting
    {
        public int? GridSize { get; set; }

        public bool RenderStatistics { get; set; }
        public bool RenderVersion { get; set; }
        public bool RenderGrid { get; set; }
        public bool RenderIcon { get; set; }
        public bool RenderLabel { get; set; }
        public bool RenderHarborBlockedArea { get; set; }
        public bool RenderPanorama { get; set; }
        public bool RenderTrueInfluenceRange { get; set; }
        public bool RenderInfluences { get; set; }
    }
}
