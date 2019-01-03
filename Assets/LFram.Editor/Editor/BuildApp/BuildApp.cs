using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

/// <summary>
/// 自动打包APK工具
/// </summary>
public class BuildApp
{
	public static string appName = "releace";
	public static string AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
	public static string IOSPath = Application.dataPath + "/../BuildTarget/IOS/";
	public static string WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";

	[MenuItem("打包工具/打出标准APP(包括AB包)")]
	public static void BuildApk()
	{
		//先打出AB包
		BundleEditor.Build();

		//获取AB包路径
		string abPath = PathConst.BUNDLE_TARGET_PATH + PathConst.GetDirPlatform();

		//把AB包拷贝到streamingAssets下打包，打完包再删除
		Copy(abPath, Application.streamingAssetsPath);

		string savePath = GetBuildPath(EditorUserBuildSettings.activeBuildTarget);
		
		//参数1：要打包的场景列表
		//参数2: 要存储的路径
		//参数3：打包的平台
		BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);

		//打包完成后删除streamingAssets下的文件
		DeleteDir(Application.streamingAssetsPath);
	}

	//根据平台获得打包存储路径
	public static string GetBuildPath(BuildTarget target)
	{
		string platformDir = PathConst.GetDirPlatform();
		switch (target)
		{
			case BuildTarget.Android: return AndroidPath + appName + "_" + platformDir + string.Format("_{0:yyyy_MM_dd_HH_mm}",DateTime.Now) + ".apk";
			case BuildTarget.iOS: return IOSPath + appName + "_" + platformDir + string.Format("_{0:yyyy_MM_dd_HH_mm}",DateTime.Now);//打出的是文件夹
			case BuildTarget.StandaloneWindows: return WindowsPath + appName + "_" + platformDir + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe",DateTime.Now, appName);
			case BuildTarget.StandaloneWindows64: return WindowsPath + appName + "_" + platformDir + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe",DateTime.Now, appName);
		}
		return "";
	}
	
	/// <summary>
	/// 获得所有添加到打包列表里的场景
	/// File --> Build Settings --> Scenes In Build目录下
	/// </summary>
	/// <returns></returns>
	private static string[] FindEnableEditorScenes()
	{
		List<string> editorScenes = new List<string>();

		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if(!scene.enabled)
			{
				continue;
			}
			editorScenes.Add(scene.path);
		}
		return editorScenes.ToArray();
	}


	/// <summary>
	/// 把打包的文件拷贝到streamingAssetsPath
	/// </summary>
	/// <param name="scrPath">原路径</param>
	/// <param name="targetPath">拷贝至目标路径</param>
	private static void Copy(string scrPath, string targetPath)
	{
		try
		{
			if(!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
			}
			
			string scrdir = Path.Combine(targetPath, Path.GetFileName(scrPath));
			if(Directory.Exists(scrPath))
			{
				scrdir += Path.DirectorySeparatorChar;
			}

			if(!Directory.Exists(scrdir))
			{
				Directory.CreateDirectory(scrdir);
			}

			string[] files = Directory.GetFileSystemEntries(scrPath);

			foreach (string file in files)
			{
				if(Directory.Exists(file))
				{
					Copy(file, scrdir);
				}
				else
				{
					File.Copy(file, scrdir + Path.GetFileName(file), true);
				}
			}

		}
		catch (System.Exception e)
		{
			Debug.LogError("无法复制 "+scrPath+" 到目标路径 => "+targetPath);
			Debug.LogError(e);
		}
	}


	/// <summary>
	/// 删除文件夹下所有文件
	/// </summary>
	/// <param name="scrPath"></param>
	public static void DeleteDir(string scrPath)
	{
		try
		{
			DirectoryInfo dir = new DirectoryInfo(scrPath);

			FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();

			foreach (FileSystemInfo info in fileInfo)
			{
				if(info is DirectoryInfo)
				{
					DirectoryInfo subdir = new DirectoryInfo(info.FullName);
					subdir.Delete(true);
				}
				else
				{
					File.Delete(info.FullName);
				}
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("无法删除路径 " + scrPath);
			Debug.LogError(e);
		}
	}
}
