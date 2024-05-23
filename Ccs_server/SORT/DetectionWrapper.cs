using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML;

namespace Ccs_server.SORT
{
    public struct Box
    {
        public int x1, x2, y1, y2, object_id;
        public float score;

        public Box()
        {
            object_id = -1;
        }
    }
    public class DetectionWrapper
    {

        PredictionEngine<WineInput, WinePredictions> engine;
        public DetectionWrapper()
        {
            var context = new MLContext();
            var emptyData = new List<WineInput>();
            var data = context.Data.LoadFromEnumerable(emptyData);
            // Preprocess and run model transformation
            var pipeline = context.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "resized", imageWidth: DetectorConfig.ModelInputSize, imageHeight: DetectorConfig.ModelInputSize, inputColumnName: nameof(WineInput.Image))
                .Append(context.Transforms.ExtractPixels(outputColumnName: "images", inputColumnName: "resized", scaleImage: 1.0f / 255.0f))
                .Append(context.Transforms.ApplyOnnxModel(modelFile: DetectorConfig.ModelPath, outputColumnName: "output0", inputColumnName: "images"));
            var model = pipeline.Fit(data);
            engine = context.Model.CreatePredictionEngine<WineInput, WinePredictions>(model);
        }

        public List<Box> Detect(MLImage image)
        {
            float scaleX, scaleY;
            scaleX = (float)image.Width / (float)DetectorConfig.ModelInputSize;
            scaleY = (float)image.Height / (float)DetectorConfig.ModelInputSize;
            var predict = engine.Predict(new WineInput { Image = image });

            return Postprocess(predict.PredictedLabels, scaleX, scaleY);
        }

        public List<Box> Postprocess(float[] rawOutputs, float scaleX, float scaleY)
        {
            var reshapedOutputs = new float[5, 8400];
            const int X = 0, Y = 1, W = 2, H = 3, SCORE = 4;
            Buffer.BlockCopy(rawOutputs, 0, reshapedOutputs, 0, sizeof(float) * rawOutputs.Length);
            var correct = new List<Box>();
            for (int i = 0; i < 8400; i++)
            {
                float score = reshapedOutputs[SCORE, i];
                if (score > 0.3)
                {
                    var box = new Box();
                    box.x1 = (int)((reshapedOutputs[X, i] - reshapedOutputs[W, i] / 2) * scaleX);
                    box.x2 = (int)((reshapedOutputs[X, i] + reshapedOutputs[W, i] / 2) * scaleX);
                    box.y1 = (int)((reshapedOutputs[Y, i] - reshapedOutputs[H, i] / 2) * scaleY);
                    box.y2 = (int)((reshapedOutputs[Y, i] + reshapedOutputs[H, i] / 2) * scaleY);
                    box.score = score;
                    correct.Add(box);
                }

            }
            return Utils.NMS(ref correct);
        }
    }
}
