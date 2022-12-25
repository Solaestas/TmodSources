using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XNBCompiler;

internal class Program
{
    private static Dictionary<string, DateTime> m_fileAccessCache;

    private static List<string> m_shouldLocateFileExtensions = new List<string> { ".fx" };

    private static XNBBuilder m_builder;

    private static int m_totalFiles;

    private static int m_successFiles;

    private static string m_rootDirectory;

    public static string lastMessage;

    private static void ReadCache(string inputPath)
    {
        Console.WriteLine("加载缓存中...");
        m_fileAccessCache = new Dictionary<string, DateTime>();
        string path = "shader.cache";
        if (!File.Exists(path))
        {
            return;
        }
        using FileStream stream = File.OpenRead(path);
        using StreamReader streamReader = new StreamReader(stream);
        string text;
        while ((text = streamReader.ReadLine()) != null)
        {
            string[] array = text.Split(',');
            m_fileAccessCache[array[0]] = new DateTime(long.Parse(array[1]), DateTimeKind.Unspecified);
        }
    }

    private static void WriteCache()
    {
        using (FileStream stream = File.OpenWrite("shader.cache"))
        {
            using StreamWriter streamWriter = new StreamWriter(stream);
            foreach (KeyValuePair<string, DateTime> item in m_fileAccessCache)
            {
                streamWriter.WriteLine("{0},{1}", item.Key, item.Value.Ticks);
            }
        }
        Console.WriteLine("写入缓存完成！");
    }

    private static void WalkDirectories(string currentPath, string shortName)
    {
        string[] files = Directory.GetFiles(currentPath);
        List<string> list = new List<string>();
        string[] array = files;
        string[] array2 = array;
        foreach (string filename in array2)
        {
            if (m_shouldLocateFileExtensions.Any(filename.EndsWith))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filename);
                if (!m_fileAccessCache.ContainsKey(filename) || m_fileAccessCache[filename] < lastWriteTime || !File.Exists(Path.ChangeExtension(filename, "xnb")))
                {
                    list.Add(filename);
                }
            }
        }
        m_totalFiles += list.Count;
        CompileThisDirectory(list, currentPath, shortName);
        array = Directory.GetDirectories(currentPath);
        string[] array3 = array;
        foreach (string text in array3)
        {
            WalkDirectories(Path.Combine(currentPath, text), text);
        }
    }

    private static void CompileThisDirectory(List<string> sources, string currentPath, string shortName)
    {
        if (sources.Count == 0)
        {
            return;
        }
        string[] array = m_builder.PackageContent(sources.ToArray(), shortName, shouldLog: true, currentPath, out bool buildStatus);
        if (!buildStatus)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("编译失败");
            Console.ForegroundColor = color;
        }
        if (array == null)
        {
            return;
        }
        m_successFiles += array.Length;
        HashSet<string> hashSet = new HashSet<string>();
        string[] array2 = array;
        string[] array3 = array2;
        foreach (string path in array3)
        {
            hashSet.Add(Path.GetFileNameWithoutExtension(path));
        }
        foreach (string source in sources)
        {
            if (hashSet.Contains(Path.GetFileNameWithoutExtension(source)))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(source);
                if (!m_fileAccessCache.ContainsKey(source))
                {
                    m_fileAccessCache.Add(source, lastWriteTime);
                }
                else
                {
                    m_fileAccessCache[source] = lastWriteTime;
                }
            }
        }
    }

    private static void Main(string[] args)
    {
        string path = Directory.GetCurrentDirectory();
        if (args.Length == 1)
        {
            path = args[0];
        }
        m_rootDirectory = Path.GetFullPath(path);
        Console.WriteLine(m_rootDirectory);
        m_totalFiles = m_successFiles = 0;
        ReadCache(m_rootDirectory);
        m_builder = new XNBBuilder();
        WalkDirectories(m_rootDirectory, ".");
        try
        {
            WriteCache();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        if (File.Exists("ContentPipeline.xml"))
        {
            File.Delete("ContentPipeline.xml");
        }
        Console.WriteLine($"编译完成：成功 {m_successFiles} 个，失败 {m_totalFiles - m_successFiles}个");
        if (m_totalFiles - m_successFiles > 0)
        {
            foreach (var error in m_builder.GetErrors())
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ForegroundColor = color;
            }
        }
    }
}
