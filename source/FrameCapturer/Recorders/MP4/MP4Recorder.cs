using System;
using System.IO;
using UnityEngine;
using UnityEngine.Recorder.FrameRecorder;
using UnityEngine.Recorder.FrameRecorder.DataSource;

namespace UTJ.FrameCapturer.Recorders
{
    [FrameRecorderClass]
    public class MP4Recorder : BaseImageRecorder<MP4RecorderSettings>
    {
        fcAPI.fcMP4Context m_ctx;

        public static RecorderInfo GetRecorderInfo()
        {
            return RecorderInfo.Instantiate<MP4Recorder, MP4RecorderSettings>("Video", "UTJ/MPeg-4");
        }

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            if (!Directory.Exists(m_Settings.m_DestinationPath))
                Directory.CreateDirectory(m_Settings.m_DestinationPath);

            return true;
        }

        public override void EndRecording(RecordingSession session)
        {
            m_ctx.Release();
            base.EndRecording(session);
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Sources.Count != 1)
                throw new Exception("Unsupported number of sources");

            var source = (RenderTextureInput)m_Sources[0];
            var frame = source.outputRT;

            if(!m_ctx)
            {
                var settings = m_Settings.m_MP4EncoderSettings;
                settings.video = true;
                settings.audio = false;
                settings.videoWidth = frame.width;
                settings.videoHeight = frame.height;
                settings.videoTargetFramerate = 60; // ?
                m_ctx = fcAPI.fcMP4OSCreateContext(ref settings, BuildOutputPath(session));
            }

            fcAPI.fcLock(frame, TextureFormat.RGB24, (data, fmt) =>
            {
                fcAPI.fcMP4AddVideoFramePixels(m_ctx, data, fmt, session.m_CurrentFrameStartTS);
            });
        }

        string BuildOutputPath(RecordingSession session)
        {
            var outputPath = m_Settings.m_DestinationPath;
            if (outputPath.Length > 0 && !outputPath.EndsWith("/"))
                outputPath += "/";
            outputPath += (settings as MP4RecorderSettings).m_BaseFileName + ".mp4";
            return outputPath;
        }

    }
}
