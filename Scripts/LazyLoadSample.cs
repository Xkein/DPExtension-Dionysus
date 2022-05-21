using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.FileFormats;

namespace Scripts
{
    [Serializable]
    public class LazyLoadSample : TechnoScriptable
    {
        public LazyLoadSample(TechnoExt owner) : base(owner) { }

        public override void Awake()
        {
            GameObject.StartCoroutine(LazyLoad());
        }
        
        private static string fileToLoad = "bonk.png";
        private static Task _downloadTask;
        private static WebClient _web;
        static LazyLoadSample()
        {
            if (File.Exists(fileToLoad))
            {
                File.Delete(fileToLoad);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private IEnumerator LazyLoad()
        {

            if (!File.Exists(fileToLoad))
            {
                Logger.Log("downloading {0}.", fileToLoad);
                while (true)
                {
                    _web = new WebClient();
                    _web.DownloadProgressChanged += (sender, args) =>
                    {
                        Logger.Log(fileToLoad + ": {0:00.0%} | {1}/{2}", args.BytesReceived / (float)args.TotalBytesToReceive, args.BytesReceived, args.TotalBytesToReceive);
                    };

                    try
                    {
                        _downloadTask?.Wait();
                    }
                    catch (Exception) { }
                    _downloadTask = _web.DownloadFileTaskAsync(
                        "https://github.com/Xkein/Images/raw/master/DynamicPatcher/" + fileToLoad, fileToLoad);
                    yield return _downloadTask;
                    if (loaded || _downloadTask is { IsFaulted: false })
                        break;
                    Logger.Log("re-downloading {0}.", fileToLoad);
                }
            }
            
            if (surface == null)
            {
                yield return Task.Run(() =>
                {
                    lock (fileToLoad)
                    {
                        if (surface == null && _downloadTask.IsCompleted)
                        {
                            var bitmap = new Bitmap(fileToLoad);
                            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                            bitmap = bitmap.Clone(rect, PixelFormat.Format16bppRgb565);
                            surface = new YRClassHandle<BSurface>(bitmap.Width, bitmap.Height);

                            Logger.Log("loading image...");

                            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                            surface.Ref.Allocate(2);
                            Helpers.Copy(data.Scan0, surface.Ref.BaseSurface.Buffer, data.Stride * data.Height);

                            bitmap.UnlockBits(data);

                            Logger.Log("lazy load finished.");
                            loaded = true;

                            _web.Dispose();
                            _downloadTask.Dispose();

                            _web = null;
                            _downloadTask = null;
                        }
                    }
                });
            }
        }

        private static YRClassHandle<BSurface> surface;
        private static bool loaded = false;
        public override void OnUpdate()
        {
        }

        public override void OnRender()
        {
            if (loaded)
            {
                ref var srcSurface = ref surface.Ref.BaseSurface;

                Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords());
                point += new Point2D(0, -srcSurface.Height);

                var rect = new Rectangle(point.X - srcSurface.Width / 2, point.Y - srcSurface.Height / 2,
                    srcSurface.Width, srcSurface.Height);
                rect = Rectangle.Intersect(rect, new Rectangle(0, 0, Surface.Current.Ref.Width, Surface.Current.Ref.Height));

                var drawRect = new RectangleStruct(rect.X, rect.Y, rect.Width, rect.Height);

                Surface.Current.Ref.Blit(Surface.ViewBound, drawRect
                    , surface.Pointer.Convert<Surface>(), srcSurface.GetRect(), srcSurface.GetRect(), true, true);
            }
        }
    }
}
