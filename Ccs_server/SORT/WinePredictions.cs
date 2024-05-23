using Microsoft.ML.Data;

namespace Ccs_server.SORT
{
    public class WinePredictions
    {
        [ColumnName("output0")]
        public float[] PredictedLabels { get; set; }
    }
}
