using PatcherYRpp;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;

namespace Extension.FX.Graphic
{
    public class YRGraphic
    {
        private static ShaderResourceView _yrBufferTextureView;
        public static FXDrawObject drawObject;

        public static IntPtr WindowHandle => Process.GetCurrentProcess().MainWindowHandle;
        public static ref Surface PrimarySurface => ref Surface.Primary.Ref;
        public static ref ZBufferClass ZBuffer => ref ZBufferClass.ZBuffer.Ref;
        public static ref ABufferClass ABuffer => ref ABufferClass.ABuffer.Ref;
        public static RectangleStruct SurfaceRect => new RectangleStruct(0, 0, PrimarySurface.GetWidth(), PrimarySurface.GetHeight());
        public static ShaderResourceView BufferTextureView => _yrBufferTextureView;
        public static string PrimaryBufferTextureName => "YR_PrimaryBuffer";


        public static void Initialize(D3D11.Device d3dDevice)
        {
            var rect = SurfaceRect;

            var desc = new Texture2DDescription
            {
                Width = rect.Width,
                Height = rect.Height,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Dynamic,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = SharpDX.DXGI.Format.B5G6R5_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            };
            var texture = new Texture2D(d3dDevice, desc);

            _yrBufferTextureView = new ShaderResourceView(d3dDevice, texture);

            drawObject = new FXDrawObject(PrimaryBufferTextureName);
        }
        public static void Dispose()
        {
            _yrBufferTextureView?.Dispose();
            drawObject?.Dispose();
        }

        public static void FillTexture()
        {
            Pointer<byte> buffer = PrimarySurface.Lock(0, 0);
            if (buffer.IsNull)
            {
                return;
            }

            var rect = SurfaceRect;
            //var coords = TacticalClass.Instance.Ref.ClientToCoords(new Point2D(rect.Width / 2, rect.Height / 2));

            drawObject.SetLocalBuffer(new Definitions.Vector3(rect.Width / 2, 0, int.MaxValue), new Definitions.Vector3(rect.Width / 2, rect.Height, int.MaxValue));

            int rowPitch = PrimarySurface.GetWidth() * PrimarySurface.GetBytesPerPixel();
            int depthPitch = PrimarySurface.GetHeight() * PrimarySurface.GetPitch();
            //Pointer<byte> bufferEnd = buffer + depthPitch;

            //var map = FXGraphic.ImmediateContext.MapSubresource(_yrBufferTextureView.Resource, 0, MapMode.WriteDiscard, MapFlags.None);

            //Pointer<byte> dst = map.DataPointer;
            //Pointer<byte> src = buffer;

            //for (int y = 0; y < rect.Height; y++)
            //{
            //    Helpers.Copy(src, dst, rect.Width * 2);
            //    //SharpDX.Utilities.CopyMemory(dst, src, rect.Width * 2);
            //    dst += map.RowPitch;
            //    src += rowPitch;
            //}

            //FXGraphic.ImmediateContext.UnmapSubresource(_yrBufferTextureView.Resource, 0);

            FXGraphic.ImmediateContext.UpdateSubresource(BufferTextureView.Resource, 0, null, buffer, rowPitch, depthPitch);

            PrimarySurface.Unlock();
        }


        public static IntPtr CopyZBuffer(Pointer<byte> buffer)
        {
            var pZBuffer = ZBuffer.AdjustedGetBufferAt(new Point2D(0, 0));

            int size = (int)(ZBuffer.BufferEndpoint - pZBuffer.Convert<byte>());

            Helpers.Copy(pZBuffer, buffer, size);

            if (size < ZBuffer.BufferSize)
            {
                var pRealZBuffer = pZBuffer + size / 2 - ZBuffer.BufferSize / 2;
                Helpers.Copy(pRealZBuffer, buffer + size, ZBuffer.BufferSize - size);
            }

            return pZBuffer;
        }
        public static IntPtr GetABuffer(short[] aBuffer)
        {
            var pABuffer = ABuffer.AdjustedGetBufferAt(new Point2D(0, 0));

            int size = (int)(ABuffer.BufferEndpoint - pABuffer.Convert<byte>());

            Marshal.Copy(pABuffer, aBuffer, 0, size / 2);

            if (size < ABuffer.BufferSize)
            {
                var pRealABuffer = pABuffer + size / 2 - ABuffer.BufferSize / 2;
                Marshal.Copy(pRealABuffer, aBuffer, size / 2, ZBuffer.BufferSize - size);
            }

            return pABuffer;
        }

        public static string HLSL_zbuffer_vertex =>
            "//render target 1                                                                               " +
            "struct VSOutput                                                                                 " +
            "{                                                                                               " +
            "    vector position : POSITION;                                                                 " +
            "    float3 coords : TEXCOORD1;                                                                  " +
            "    float2 uv : TEXCOORD;                                                                       " +
            "};                                                                                              " +
            "                                                                                                " +
            "VSOutput main(in vector coords : POSITION0, in float2 uv : TEXCOORD0)                           " +
            "{                                                                                               " +
            "    VSOutput output;                                                                            " +
            "                                                                                                " +
            "    output.position = vector((coords.xy - float2(0.5, 0.5)) * float2(2.0, -2.0), 0.0, 1.0);     " +
            "    output.coords = coords.xyz;                                                                 " +
            "    output.uv = uv.yx;                                                                          " +
            "    return output;                                                                              " +
            "}                                                                                               "
            ;

        public static string HLSL_zbuffer_pixel =>
            "Texture2D tex_draw : register(t1);                                                              " +
            "Texture2D tex_zbuffer : register(t0);                                                           " +
            "SamplerState tex_sampler {                                                                      " +
            "    Filter = MIN_MAG_MIP_LINEAR;                                                                " +
            "    AddressU = Wrap;                                                                            " +
            "    AddressV = Wrap;                                                                            " +
            "};                                                                                              " +
            "                                                                                                " +
            "//render target 1                                                                               " +
            "struct VSOutput                                                                                 " +
            "{                                                                                               " +
            "    vector position : POSITION;                                                                 " +
            "    float3 coords : TEXCOORD1;                                                                  " +
            "    float2 uv : TEXCOORD;                                                                       " +
            "};                                                                                              " +
            "                                                                                                " +
            "                                                                                                " +
            "vector main(in VSOutput input) : SV_TARGET                                                      " +
            "{                                                                                               " +
            "    float2 zbuffer_uv = input.coords.xy;                                                        " +
            "    float zvalue = input.coords.z;                                                              " +
            "                                                                                                " +
            "    // vector zcolor = tex2D(tex_zbuffer, zbuffer_uv);                                          " +
            "    vector zcolor = tex_zbuffer.Sample(tex_sampler, zbuffer_uv);                                " +
            "    int rvalue = ceil(zcolor.r * 31) * 32 * 64;                                                 " +
            "    int gvalue = ceil(zcolor.g * 63) * 32;                                                      " +
            "    int bvalue = zcolor.b * 31;                                                                 " +
            "                                                                                                " +
            "    float zbuffer_val = rvalue + gvalue + bvalue;                                               " +
            "    if (zbuffer_val < zvalue)                                                                   " +
            "        discard;                                                                                " +
            "                                                                                                " +
            "    //float u = (input.uv.x - floor(input.uv.x)) * (1 - (input.uv.x - floor(input.uv.x)));      " +
            "    //float v = (input.uv.y - floor(input.uv.y)) * (1 - (input.uv.y - floor(input.uv.y)));      " +
            "    //return vector(saturate((u + v) / 2), 0, 0, 1);                                            " +
            "    return vector(saturate(tex_draw.Sample(tex_sampler, input.uv).rgb), 1.0);                   " +
            "}                                                                                               "
            ;

    }
}




























