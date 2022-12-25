using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework.Content.Pipeline.Tasks;

namespace XNBCompiler;

public class XNBBuilder : BuildContent
{
    public List<string> Errors = new List<string>();

    public BuildEngine buildEngine;

    public bool BuildAudioAsSoundEffects;

    public bool BuildAudioAsSongs;

    public XNBBuilder()
    {
        base.TargetPlatform = "Windows";
        base.TargetProfile = "HiDef";
        base.CompressContent = false;
        base.BuildConfiguration = "Debug";
        BuildAudioAsSongs = false;
        BuildAudioAsSoundEffects = false;
        buildEngine = new BuildEngine();
    }

    public XNBBuilder(bool CompressContent)
    {
        base.CompressContent = CompressContent;
        BuildAudioAsSongs = false;
        BuildAudioAsSoundEffects = false;
        buildEngine = new BuildEngine();
    }

    public XNBBuilder(string targetPlatform, string targetProfile, bool CompressContent)
    {
        base.TargetPlatform = targetPlatform;
        base.TargetProfile = targetProfile;
        base.CompressContent = CompressContent;
        base.BuildConfiguration = "Debug";
        BuildAudioAsSongs = false;
        BuildAudioAsSoundEffects = false;
        buildEngine = new BuildEngine();
    }

    public List<string> GetErrors()
    {
        return Errors;
    }

    public string[] PackageContent(string[] fileNames, string outputDirectory, bool shouldLog, string rootDirectory, out bool buildStatus)
    {
        string[] array = null;
        buildStatus = false;
        try
        {
            if (!shouldLog)
            {
                buildEngine.log = false;
            }
            else
            {
                buildEngine = new BuildEngine("logfile.txt");
            }
            base.OutputDirectory = outputDirectory;
            base.RootDirectory = rootDirectory;
            List<TaskItem> list = new List<TaskItem>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                string value = "." + fileNames[i].Split('.').Last();
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                if (".bmp.dds.dib.hdr.jpg.pfm.png.ppm.tga".Contains(value))
                {
                    dictionary.Add("Importer", "TextureImporter");
                    dictionary.Add("Processor", "TextureProcessor");
                }
                else if (".fbx".Contains(value))
                {
                    dictionary.Add("Importer", "FbxImporter");
                    dictionary.Add("Processor", "ModelProcessor");
                }
                else if (".fx".Contains(value))
                {
                    dictionary.Add("Importer", "EffectImporter");
                    dictionary.Add("Processor", "EffectProcessor");
                }
                else if (".spritefont".Contains(value))
                {
                    dictionary.Add("Importer", "FontDescriptionImporter");
                    dictionary.Add("Processor", "FontDescriptionProcessor");
                }
                else if (".x".Contains(value))
                {
                    dictionary.Add("Importer", "XImporter");
                    dictionary.Add("Processor", "ModelProcessor");
                }
                else if (".xml".Contains(value))
                {
                    dictionary.Add("Importer", "XmlImporter");
                    dictionary.Add("Processor", "PassThroughProcessor");
                }
                else if (".mp3".Contains(value))
                {
                    dictionary.Add("Importer", "Mp3Importer");
                    if (BuildAudioAsSoundEffects)
                    {
                        dictionary.Add("Processor", "SoundEffectProcessor");
                    }
                    else if (BuildAudioAsSongs)
                    {
                        dictionary.Add("Processor", "SongProcessor");
                    }
                    else
                    {
                        dictionary.Add("Processor", "SoundEffectProcessor");
                    }
                }
                else if (".wma".Contains(value))
                {
                    dictionary.Add("Importer", "WmaImporter");
                    if (BuildAudioAsSoundEffects)
                    {
                        dictionary.Add("Processor", "SoundEffectProcessor");
                    }
                    else if (BuildAudioAsSongs)
                    {
                        dictionary.Add("Processor", "SongProcessor");
                    }
                    else
                    {
                        dictionary.Add("Processor", "SoundEffectProcessor");
                    }
                }
                else if (".wav".Contains(value))
                {
                    dictionary.Add("Importer", "WavImporter");
                    if (BuildAudioAsSoundEffects)
                    {
                        dictionary.Add("Processor", "SoundEffectProcessor");
                    }
                    else if (BuildAudioAsSongs)
                    {
                        dictionary.Add("Processor", "SongProcessor");
                    }
                    else
                    {
                        dictionary.Add("Processor", "SoundEffectProcessor");
                    }
                }
                else
                {
                    if (!".wmv".Contains(value))
                    {
                        continue;
                    }
                    dictionary.Add("Importer", "WmvImporter");
                    dictionary.Add("Processor", "VideoProcessor");
                }
                Console.WriteLine("正在编译" + fileNames[i]);
                dictionary.Add("Name", Path.GetFileNameWithoutExtension(fileNames[i]));
                list.Add(new TaskItem(fileNames[i], dictionary));
            }
            ITaskItem[] dlls = list.ToArray();
            ITaskItem[] array4 = base.SourceAssets = dlls;
            ITaskItem[] array5 = array4;
            buildEngine.Begin();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dlls = new TaskItem[8]
            {
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.AudioImporters.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.VideoImporters.dll"),
                new TaskItem(baseDirectory + "Microsoft.Xna.Framework.Content.Pipeline.XImporter.dll")
            };
            array4 = base.PipelineAssemblies = dlls;
            array5 = array4;
            base.BuildEngine = buildEngine;
            base.IntermediateDirectory = Directory.GetCurrentDirectory();
            buildStatus = Execute();
            if (base.OutputContentFiles == null)
            {
                return array;
            }
            array = new string[base.OutputContentFiles.Length];
            for (int j = 0; j < array.Length; j++)
            {
                array[j] = base.OutputContentFiles[j].ToString();
            }
            return array;
        }
        catch
        {
            return array;
        }
        finally
        {
            buildEngine.End();
            Errors.AddRange(buildEngine.GetErrors());
        }
    }
}
