using NAudio.Lame;
using NAudio.Wave;

using QuarterMaster.Debugging;

using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace QuarterMaster.Media
{
    public static class SoundConversion
    {
        public static string GetSliceFromFile(string filename, int sliceLength)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            if (!File.Exists(filename))
                throw new FileNotFoundException(filename + " not found");
            byte[] buffer = new byte[sliceLength];
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                if (fs.Length >= sliceLength)
                    fs.Read(buffer, 0, sliceLength);
                else
                    fs.Read(buffer, 0, (int)fs.Length);
            }
            return System.Text.Encoding.Default.GetString(buffer);
        }

        public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
            using AudioFileReader audioFileReader = new AudioFileReader(waveFileName);
            using LameMP3FileWriter lameMp3FileWriter = new LameMP3FileWriter(mp3FileName, audioFileReader.WaveFormat, bitRate, (ID3TagData)null);
            audioFileReader.CopyTo((Stream)lameMp3FileWriter);
        }

        public static void MP3ToWave(string mp3FileName, string waveFileName)
        {
            using Mp3FileReader mp3FileReader = new Mp3FileReader(mp3FileName);
            using WaveFileWriter waveFileWriter = new WaveFileWriter(waveFileName, mp3FileReader.WaveFormat);
            mp3FileReader.CopyTo((Stream)waveFileWriter);
        }

        public static void M4AToMP3(string m4aFileName, string mp3FileName, int bitRate = 128)
        {
            using MediaFoundationReader foundationReader = new MediaFoundationReader(m4aFileName);
            using LameMP3FileWriter lameMp3FileWriter = new LameMP3FileWriter(mp3FileName, foundationReader.WaveFormat, bitRate, (ID3TagData)null);
            foundationReader.CopyTo((Stream)lameMp3FileWriter);
        }

        public static void MP3ToM4A(string mp3FileName, string m4aFileName)
        {
            using Mp3FileReader mp3FileReader = new Mp3FileReader(mp3FileName);
            MediaFoundationEncoder.EncodeToAac((IWaveProvider)mp3FileReader, m4aFileName, 192000);
        }
    }
}
