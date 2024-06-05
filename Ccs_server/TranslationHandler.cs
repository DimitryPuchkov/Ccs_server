using Ccs_server.data;
using Ccs_server.SORT;
using Microsoft.ML.Data;
using Nager.VideoStream;
using System.Drawing;
using System.Timers;

namespace Ccs_server
{
    record class Line(Point p1, Point p2);
    public class TranslationHandler
    {
        private CancellationTokenSource cancelTokenSource;
        private VideoStreamClient client;
        private Task frameProcessTask;
        System.Timers.Timer sendCountDataTimer;
        public Camera StreamCamera { get; private set; }
        public string Name { get; init; }
        public int VisitorsCount;
        private DetectionWrapper detector;
        private Sort tracker;
        private List<Box> prevBoxes;
        private Dictionary<int, bool> intersectedFirstBorder;
        private Line firstBorder, secondBorder;



        public TranslationHandler(Camera camera)
        {
            StreamCamera = camera;
            sendCountDataTimer = new System.Timers.Timer(60 * 60 * 1000); //one hour in milliseconds
            sendCountDataTimer.Elapsed += new ElapsedEventHandler(SendVisitorsCount);
            cancelTokenSource = new CancellationTokenSource();
            client = new VideoStreamClient();
            client.NewImageReceived += NewImageReceived;
            prevBoxes = new List<Box>();
            intersectedFirstBorder = new Dictionary<int, bool>();
            firstBorder = new Line(new Point(camera.Fbx1, camera.Fby1), new Point(camera.Fbx2, camera.Fby2));
            secondBorder = new Line(new Point(camera.Sbx1, camera.Sby1), new Point(camera.Sbx2, camera.Sby2));
        }

        public void Start()
        {
            frameProcessTask = client.StartFrameReaderAsync(new StreamInputSource(StreamCamera.Address), OutputImageFormat.Bmp, cancelTokenSource.Token);
            sendCountDataTimer.Start();
            VisitorsCount = 0;
            prevBoxes.Clear();
        }
        public void Stop()
        {
            cancelTokenSource.Cancel();
            cancelTokenSource.Dispose();

        }

        private void NewImageReceived(byte[] imageData)
        {
            MLImage frame;
            using (var stream = new MemoryStream(imageData))
            {
                frame = MLImage.CreateFromStream(stream);
            }
            var detBoxes = detector.Detect(frame);
            var currentBoxes = tracker.Update(detBoxes);

            // Counting
            foreach(Box currentBox in currentBoxes)
            {
                foreach(Box prevBox in prevBoxes)
                {
                    if(currentBox.object_id == prevBox.object_id)
                    {
                        var dPosition = new Line(new Point((prevBox.x1+ prevBox.x2)/2, prevBox.y1), new Point((currentBox.x1 + currentBox.x2) / 2, currentBox.y1));
                        if (isIntersect(firstBorder, dPosition))
                            intersectedFirstBorder[currentBox.object_id] = !intersectedFirstBorder[currentBox.object_id];

                        if (intersectedFirstBorder[currentBox.object_id] && isIntersect(secondBorder, dPosition))
                        {
                            intersectedFirstBorder[currentBox.object_id] = !intersectedFirstBorder[currentBox.object_id];
                            VisitorsCount++;
                        }
                    }
                }
            }


            prevBoxes = currentBoxes;

        }

        private void SendVisitorsCount(object source, ElapsedEventArgs e)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var value = new Value { Time = DateTime.UtcNow, PersonCount = VisitorsCount, CameraId = StreamCamera.Id };
                db.values.Add(value);
                db.SaveChanges();
            }
            VisitorsCount = 0;
        }

        private bool isIntersect(Line border, Line dPosition)
        {
            // Find the four orientations needed for general and 
            // special cases 
            var p1 = border.p1;
            var p2 = border.p2;
            var q1 = dPosition.p1;
            var q2 = dPosition.p2;
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are collinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are collinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are collinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are collinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        } 

        private bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        private int orientation(Point p, Point q, Point r)
        {
            // for details of below formula. 
            int val = (q.Y - p.Y) * (r.X - q.X) -
                    (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0; // collinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        } 
    }
}
