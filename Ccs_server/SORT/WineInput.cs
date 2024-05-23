using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace Ccs_server.SORT
{
    public class WineInput
    {
        [ImageType(DetectorConfig.ModelInputSize, DetectorConfig.ModelInputSize)]
        public MLImage Image { get; set; }
    }
}
