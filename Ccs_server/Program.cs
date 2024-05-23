using Ccs_server.data;
using Ccs_server.SORT;
using Microsoft.ML.Data;
using Nager.VideoStream;



List<Box> prevBoxes = new List<Box>();
var detector = new DetectionWrapper();
var sort = new Sort(15);
int count = 0;
int frameCount = 0;

using (ApplicationContext db = new ApplicationContext())
{
    var cameras = db.cameras.ToList();
    foreach (var camera in cameras)
    {
        var inputSource = new StreamInputSource(camera.Address);
        var cancellationTokenSource = new CancellationTokenSource();

        var client = new VideoStreamClient("/usr/bin/ffmpeg");
        client.NewImageReceived += NewImageReceived;
        var task = client.StartFrameReaderAsync(inputSource, OutputImageFormat.Bmp, cancellationTokenSource.Token);
    }
}

void NewImageReceived(byte[] imageData)
{
    MLImage frame;
    using (var stream = new MemoryStream(imageData))
    {
        frame = MLImage.CreateFromStream(stream);
    }
    var detBoxes = detector.Detect(frame);
    var finalBoxes = sort.Update(detBoxes);

    if (prevBoxes.Count > 0)
    {
        count += CountClients(finalBoxes, prevBoxes);
    }
    frameCount++;
    if (frameCount == 15 * 60)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var value = new Value { Time = DateTime.UtcNow, PersonCount = count, CameraId = 1 };
            db.values.Add(value);
            db.SaveChanges();
        }
    }
}

int CountClients(List<Box> currentBoxes, List<Box> prevBoxes)
{
    // здесь будет логика подсчета
    return 0;
}


var builder = WebApplication.CreateBuilder();
var app = builder.Build();



app.Map("/{user}/{camera}/", (string user, string camera) =>
{

    try
    {
        //получение данных
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = db.users.ToList();
            User u = users.Find(x => x.Name == user);
            if (u == null)
                throw new Exception($"User: {user} not found");
            var cameras = db.cameras.ToList();
            Camera cam = cameras.Find(x=> x.User == u);
            if (cam == null) throw new Exception($"Camera: {camera} not found");

            var values = db.values.ToList();
            var result_values = values.FindAll(x=>x.Camera == cam && x.Time> DateTime.Now.Date);
            int clients = 0;
            foreach (Value value in result_values) clients += value.PersonCount;
            return $"Today clents: {clients}";
        }
    }
    catch (Exception ex)
    {
         return ex.ToString();
    }
    
});

app.Run();