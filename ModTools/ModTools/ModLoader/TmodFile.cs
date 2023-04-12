using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace ModTools.ModLoader;

// warning class is not threadsafe
public class TmodFile : IEnumerable<TmodFile.FileEntry>
{
	public class FileEntry
	{
		public string Name { get; }

		// from the start of the file
		public int Offset { get; set; }

		public int Length { get; }

		public int CompressedLength { get; }

		// intended to be readonly, but unfortunately no ReadOnlySpan on .NET 4.5
		public byte[] CachedBytes { get; }

		public FileEntry(string name, int offset, int length, int compressedLength, byte[] cachedBytes = null)
		{
			Name = name;
			Offset = offset;
			Length = length;
			CompressedLength = compressedLength;
			CachedBytes = cachedBytes;
		}

		public bool IsCompressed => Length != CompressedLength;
	}

	public const uint MIN_COMPRESS_SIZE = 1 << 10; // 1KB
	public const uint MAX_CACHE_SIZE = 1 << 17; // 128KB
	public const float COMPRESSION_TRADEOFF = 0.9f;

	private static string Sanitize(string path) => path.Replace('\\', '/');

	public readonly string Path;

	private FileStream fileStream;
	private IDictionary<string, FileEntry> files = new ConcurrentDictionary<string, FileEntry>();
	private FileEntry[] fileTable;

	public Version TModLoaderVersion { get; private set; }

	public string Name { get; private set; }

	public Version Version { get; private set; }

	public byte[] Hash { get; private set; }

	public byte[] Signature { get; private set; } = new byte[256];

	public TmodFile(string path, string name = null, Version version = null)
	{
		Path = path;
		Name = name;
		Version = version;
	}

	/// <summary>
	/// Adds a (fileName -&gt; content) entry to the compressed payload This method is not
	/// threadsafe with reads, but is threadsafe with multiple concurrent AddFile calls
	/// </summary>
	/// <param name="fileName">The public filepath, will be slash sanitised automatically</param>
	/// <param name="data">
	/// The file content to add. WARNING, data is kept as a shallow copy, so modifications to the
	/// passed byte array will affect file content
	/// </param>
	//public void AddFile(string fileName, byte[] data)
	//{
	//	fileName = Sanitize(fileName);
	//	int size = data.Length;

	//	if (size > MIN_COMPRESS_SIZE && ShouldCompress(fileName))
	//	{
	//		using var ms = new MemoryStream(data.Length);
	//		using (var ds = new DeflateStream(ms, CompressionMode.Compress))
	//		{
	//			ds.Write(data, 0, data.Length);
	//		}

	//		var compressed = ms.ToArray();
	//		if (compressed.Length < size * COMPRESSION_TRADEOFF)
	//		{
	//			data = compressed;
	//		}
	//	}

	//	lock (files)
	//	{
	//		files[fileName] = new FileEntry(fileName, -1, size, data.Length, data);
	//	}

	//	fileTable = null;
	//}

	public async Task AddFileAsync(string fileName, string filePath)
	{
		fileName = Sanitize(fileName);
		var fileInfo = new FileInfo(filePath);
		int size = ((int)fileInfo.Length);
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		var data = new byte[size];
		await stream.ReadAsync(data, 0, size);
		files[fileName] = new FileEntry(fileName, -1, size, size, data);
	}

	public void AddFile(string fileName, byte[] data)
	{
		fileName = Sanitize(fileName);
		int size = data.Length;
		files[fileName] = new FileEntry(fileName, -1, size, size, data);
	}

	public void RemoveFile(string fileName)
	{
		files.Remove(Sanitize(fileName));
		fileTable = null;
	}

	public int Count => fileTable.Length;

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerator<FileEntry> GetEnumerator()
	{
		foreach (var entry in fileTable)
		{
			yield return entry;
		}
	}

	public void Save()
	{
		if (fileStream != null)
		{
			throw new IOException($"File already open: {Path}");
		}

		// write the general TMOD header and data blob TMOD ascii identifier tModLoader version hash
		// signature data length signed data
		Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
		using (fileStream = File.Create(Path))
		using (var writer = new BinaryWriter(fileStream))
		{
			writer.Write(Encoding.ASCII.GetBytes("TMOD"));
			writer.Write((TModLoaderVersion = BuildInfo.tMLVersion).ToString());

			int hashPos = (int)fileStream.Position;
			writer.Write(new byte[20 + 256 + 4]); // hash, sig, data length

			int dataPos = (int)fileStream.Position;
			writer.Write(Name);
			writer.Write(Version.ToString());

			// write file table file count file-entries: filename uncompressed file size compressed
			// file size (stored size)
			fileTable = files.Values.ToArray();
			writer.Write(fileTable.Length);

			foreach (var f in fileTable)
			{
				if (f.CompressedLength != f.CachedBytes.Length)
				{
					throw new Exception($"CompressedLength ({f.CompressedLength}) != cachedBytes.Length ({f.CachedBytes.Length}): {f.Name}");
				}

				writer.Write(f.Name);
				writer.Write(f.Length);
				writer.Write(f.CompressedLength);
			}

			// write compressed files and update offsets
			int offset = (int)fileStream.Position; // offset starts at end of file table
			foreach (var f in fileTable)
			{
				writer.Write(f.CachedBytes);

				f.Offset = offset;
				offset += f.CompressedLength;
			}

			// update hash
			fileStream.Position = dataPos;
			Hash = SHA1.Create().ComputeHash(fileStream);

			fileStream.Position = hashPos;
			writer.Write(Hash);

			// skip signature
			fileStream.Seek(256, SeekOrigin.Current);

			// write data length
			writer.Write((int)(fileStream.Length - dataPos));
		}

		fileStream = null;
	}

	// Ignore file extensions which don't compress well under deflate to improve build time
	private static bool ShouldCompress(string fileName) =>
		!fileName.EndsWith(".png", StringComparison.Ordinal) &&
		!fileName.EndsWith(".mp3", StringComparison.Ordinal) &&
		!fileName.EndsWith(".ogg", StringComparison.Ordinal);
}