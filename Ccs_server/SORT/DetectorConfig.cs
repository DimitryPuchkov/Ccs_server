namespace Ccs_server.SORT
{
    public static class DetectorConfig
    {
        public static string ModelPath { get; } = "./mot17-01-frcnn.onnx";
        public const int ModelInputSize = 640;
    }
}
