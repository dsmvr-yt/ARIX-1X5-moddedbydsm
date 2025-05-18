#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using static NebosPMsWindow;

public class ARIX1X5 : EditorWindow
{
    private const string GitHubApiUrl = "https://api.github.com/repos/dsmvr-yt/ARIX-1X5-moddedbydsm/contents";
    private const string RawGitHubUrl = "https://raw.githubusercontent.com/dsmvr-yt/ARIX-1X5-moddedbydsm/main";
    private const string SplashImagePath = "Assets/Keos Stuff/Icons/Arix 1X5 Banner.png";
    private const string IntroAudioPath = "Assets/Keos Stuff/Icons/ARIX 1X5 Intro.mp3";
    private const string FilesFolder = "files";

    private const string GitHubToken = "YOUR_GITHUB_PERSONAL_ACCESS_TOKEN_HERE";

    private Vector2 scrollPosition;
    private List<FolderInfo> folderInfoList = new List<FolderInfo>();
    private List<FolderInfo> filteredFolderList = new List<FolderInfo>();
    private FolderInfo selectedFolder;
    private GUIStyle headerStyle;
    private GUIStyle titleStyle;
    private GUIStyle descriptionStyle;
    private GUIStyle detailsBoxStyle;
    private GUIStyle buttonStyle;
    private GUIStyle linkStyle;
    private GUIStyle searchBoxStyle;
    private Texture2D folderIcon;
    private Texture2D backgroundTexture;
    private Texture2D splashImage;
    private Texture2D blackTexture;
    private bool isLoading = true;
    private string errorMessage = "";
    private bool stylesInitialized = false;

    private bool showSplash = true;
    private float splashStartTime;
    private float splashDuration = 3.0f;

    private AudioClip introAudio;
    private bool isAudioPlaying = false;

    private string searchText = "";
    private bool filterHasTutorial = false;
    private bool filterHasPackage = false;
    private bool filterHasScripts = false;
    private bool filterHasMusic = false;

    private Color selectedColor = new Color(0.3f, 0.5f, 0.7f, 0.5f);
    private Color normalEntryColor = new Color(0.25f, 0.25f, 0.25f, 0.3f);
    private Color detailsBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    private Color linkColor = new Color(0.4f, 0.6f, 1.0f);

    private Dictionary<string, string> _responseCache = new Dictionary<string, string>();
    private DateTime _lastRequestTime = DateTime.MinValue;
    private Queue<GitHubItem> _foldersToLoad = new Queue<GitHubItem>();
    private bool _isLoadingFolder = false;
    private string _currentLoadingFolder = "";
    private float _loadProgress = 0f;
    private const int RequestIntervalMs = 1000;
    private const int MaxConcurrentRequests = 2;
    private int _activeRequests = 0;
    private readonly object _requestLock = new object();
    private bool isInitialLoading = true;

    [MenuItem("Window/ARIX1X5 Browser")]
    public static void ShowWindow()
    {
        GetWindow<ARIX1X5>("ARIX1X5 Browser");
    }

    void OnEnable()
    {
        LoadFolderIcon();
        LoadSplashImage();
        LoadIntroAudio();
        CreateBlackTexture();

        splashStartTime = (float)EditorApplication.timeSinceStartup;
        showSplash = true;
        isAudioPlaying = false;
        FetchRepositoryContents();

        EditorApplication.update += Update;
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
        StopAudio();
    }

    private void LoadIntroAudio()
    {
        introAudio = AssetDatabase.LoadAssetAtPath<AudioClip>(IntroAudioPath);
        if (introAudio == null)
        {
            Debug.LogWarning("ARIX1X5: Intro audio not found at path: " + IntroAudioPath);
        }
    }

    private void PlayIntroAudio()
    {
        if (introAudio != null && !isAudioPlaying)
        {
            AudioUtility.PlayClip(introAudio);
            isAudioPlaying = true;
        }
    }

    private void StopAudio()
    {
        if (isAudioPlaying)
        {
            AudioUtility.StopAllClips();
            isAudioPlaying = false;
        }
    }

    private void CreateBlackTexture()
    {
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    private void Update()
    {
        if (showSplash)
        {
            float elapsedTime = (float)EditorApplication.timeSinceStartup - splashStartTime;

            if (elapsedTime > 0.1f && !isAudioPlaying)
            {
                PlayIntroAudio();
            }

            if (elapsedTime >= splashDuration)
            {
                showSplash = false;
                StopAudio();
                Repaint();
            }
            else
            {
                Repaint();
            }
        }
    }

    private void LoadSplashImage()
    {
        splashImage = AssetDatabase.LoadAssetAtPath<Texture2D>(SplashImagePath);
        if (splashImage == null)
        {
            Debug.LogWarning("ARIX1X5: Splash image not found at path: " + SplashImagePath);
        }
    }

    private void InitializeStyles()
    {
        if (stylesInitialized)
            return;

        headerStyle = new GUIStyle();
        headerStyle.normal.textColor = Color.white;
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.margin = new RectOffset(10, 10, 10, 10);

        titleStyle = new GUIStyle();
        titleStyle.normal.textColor = Color.white;
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;

        descriptionStyle = new GUIStyle();
        descriptionStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        descriptionStyle.fontSize = 11;
        descriptionStyle.wordWrap = true;

        detailsBoxStyle = new GUIStyle(EditorStyles.helpBox);
        detailsBoxStyle.normal.textColor = Color.white;
        detailsBoxStyle.padding = new RectOffset(15, 15, 15, 15);
        detailsBoxStyle.margin = new RectOffset(5, 5, 5, 5);

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.fontSize = 12;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.padding = new RectOffset(10, 10, 5, 5);

        linkStyle = new GUIStyle(EditorStyles.label);
        linkStyle.normal.textColor = linkColor;
        linkStyle.active.textColor = Color.white;
        linkStyle.hover.textColor = Color.cyan;
        linkStyle.fontStyle = FontStyle.Bold;

        searchBoxStyle = new GUIStyle(EditorStyles.toolbarSearchField);
        searchBoxStyle.margin = new RectOffset(5, 5, 5, 5);
        searchBoxStyle.fixedHeight = 22;

        CreateBackgroundTexture();
        stylesInitialized = true;
    }

    private void LoadFolderIcon()
    {
        folderIcon = EditorGUIUtility.FindTexture("Folder Icon");
    }

    private void CreateBackgroundTexture()
    {
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f));
        backgroundTexture.Apply();
    }

    async void FetchRepositoryContents()
    {
        isInitialLoading = true;
        folderInfoList.Clear();
        filteredFolderList.Clear();
        selectedFolder = null;
        errorMessage = "";
        _foldersToLoad.Clear();
        _isLoadingFolder = false;
        _currentLoadingFolder = "";
        _loadProgress = 0f;

        try
        {
            List<GitHubItem> rootItems = await GetRepositoryContents(GitHubApiUrl);
            if (rootItems == null)
            {
                errorMessage = "Failed to fetch root repository contents.";
                isInitialLoading = false;
                Repaint();
                return;
            }
            GitHubItem filesItem = rootItems.FirstOrDefault(i => i.type == "dir" && i.name == FilesFolder);

            if (filesItem == null)
            {
                errorMessage = $"Could not find '{FilesFolder}' folder in repository";
                Debug.LogError(errorMessage);
                isInitialLoading = false;
                Repaint();
                return;
            }

            List<GitHubItem> filesContents = await GetRepositoryContents(...); // make sure you call it with parentheses and arguments
            if (filesContents == null)
            {
                errorMessage = $"Failed to fetch contents of folder '{FilesFolder}'.";
                isInitialLoading = false;
                Repaint();
                return;
            }
            foreach (var item in filesContents)
            {
                if (item.type == "dir")
                {
                    _foldersToLoad.Enqueue(item);
                }
            }

            isInitialLoading = false;
            ProcessFolderQueue();
        }
        catch (Exception e)
        {
            errorMessage = "Error loading repository: " + e.Message;
            isInitialLoading = false;
            Debug.LogError(errorMessage);
            Repaint();
        }
    }

    private async void ProcessFolderQueue()
    {
        while (_foldersToLoad.Count > 0)
        {
            if (_activeRequests >= MaxConcurrentRequests)
            {
                await Task.Delay(100);
                continue;
            }

            var folderItem = _foldersToLoad.Dequeue();
            _currentLoadingFolder = folderItem.name;
            _isLoadingFolder = true;
            _activeRequests++;

            try
            {
                var folderFiles = await GetRepositoryContents(folderItem.url);
                if (folderFiles != null)
                {
                    AddFolderInfo(folderItem.name, folderFiles);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading folder '{folderItem.name}': {e.Message}");
            }
            finally
            {
                _activeRequests--;
                _isLoadingFolder = false;
            }

            Repaint();
        }
    }

    private void AddFolderInfo(string folderName, List<GitHubItem> folderFiles)
    {
        FolderInfo info = new FolderInfo
        {
            name = folderName,
            files = folderFiles,
            url = $"{RawGitHubUrl}/{FilesFolder}/{folderName}"
        };

        folderInfoList.Add(info);
        ApplyFiltersAndSearch();
    }

    private void ApplyFiltersAndSearch()
    {
        filteredFolderList = folderInfoList.Where(f =>
        {
            if (!string.IsNullOrEmpty(searchText) && !f.name.ToLower().Contains(searchText.ToLower()))
                return false;

            if (filterHasTutorial && !f.HasFileWithKeyword("tutorial"))
                return false;
            if (filterHasPackage && !f.HasFileWithKeyword(".unitypackage"))
                return false;
            if (filterHasScripts && !f.HasFileWithKeyword(".cs"))
                return false;
            if (filterHasMusic && !f.HasFileWithKeyword(".mp3"))
                return false;

            return true;
        }).ToList();

        if (!filteredFolderList.Contains(selectedFolder))
        {
            selectedFolder = null;
        }
    }

    private async Task<List<GitHubItem>> GetRepositoryContents(string url)
    {
        // Rate limit: wait if last request was less than 1 second ago
        while ((DateTime.UtcNow - _lastRequestTime).TotalMilliseconds < RequestIntervalMs)
        {
            await Task.Delay(100);
        }

        _lastRequestTime = DateTime.UtcNow;

        if (_responseCache.TryGetValue(url, out var cachedJson))
        {
            return JsonConvert.DeserializeObject<List<GitHubItem>>(cachedJson);
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("User-Agent", "ARIX1X5-UnityEditor");
            if (!string.IsNullOrEmpty(GitHubToken))
            {
                request.SetRequestHeader("Authorization", $"token {GitHubToken}");
            }

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GitHub API request failed: {request.error} - URL: {url}");
                return null;
            }

            string json = request.downloadHandler.text;
            _responseCache[url] = json;

            return JsonConvert.DeserializeObject<List<GitHubItem>>(json);
        }
    }

    void OnGUI()
    {
        InitializeStyles();

        if (showSplash)
        {
            DrawSplashScreen();
            return;
        }

        DrawMainUI();
    }

    private void DrawSplashScreen()
    {
        if (splashImage != null)
        {
            Rect rect = new Rect(0, 0, position.width, position.height);
            GUI.DrawTexture(rect, splashImage, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.black);
            GUI.Label(new Rect(10, 10, position.width, 30), "ARIX1X5 Loading...", headerStyle);
        }
    }

    private void DrawMainUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        GUILayout.Label("ARIX1X5 - Folder Browser", headerStyle);

        if (isInitialLoading)
        {
            GUILayout.Label("Loading repository folders, please wait...");
            GUILayout.EndVertical();
            return;
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            if (GUILayout.Button("Retry"))
            {
                FetchRepositoryContents();
            }
            GUILayout.EndVertical();
            return;
        }

        GUILayout.BeginHorizontal();
        DrawSearchAndFilters();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();

        DrawFolderList();

        DrawFolderDetails();

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawSearchAndFilters()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        GUILayout.Label("Search & Filters", titleStyle);

        string newSearchText = EditorGUILayout.TextField(searchText, searchBoxStyle);
        if (newSearchText != searchText)
        {
            searchText = newSearchText;
            ApplyFiltersAndSearch();
        }

        bool newFilterTutorial = EditorGUILayout.ToggleLeft("Has tutorial", filterHasTutorial);
        bool newFilterPackage = EditorGUILayout.ToggleLeft("Has .unitypackage", filterHasPackage);
        bool newFilterScripts = EditorGUILayout.ToggleLeft("Has .cs scripts", filterHasScripts);
        bool newFilterMusic = EditorGUILayout.ToggleLeft("Has .mp3 music", filterHasMusic);

        if (newFilterTutorial != filterHasTutorial ||
            newFilterPackage != filterHasPackage ||
            newFilterScripts != filterHasScripts ||
            newFilterMusic != filterHasMusic)
        {
            filterHasTutorial = newFilterTutorial;
            filterHasPackage = newFilterPackage;
            filterHasScripts = newFilterScripts;
            filterHasMusic = newFilterMusic;
            ApplyFiltersAndSearch();
        }
        GUILayout.EndVertical();
    }

    private void DrawFolderList()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        GUILayout.Label("Folders", titleStyle);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 150));

        if (filteredFolderList.Count == 0)
        {
            GUILayout.Label("No folders match the current search and filters.", descriptionStyle);
        }
        else
        {
            foreach (var folder in filteredFolderList)
            {
                GUI.backgroundColor = folder == selectedFolder ? selectedColor : normalEntryColor;
                if (GUILayout.Button(folder.name, buttonStyle))
                {
                    selectedFolder = folder;
                }
                GUI.backgroundColor = Color.white;
            }
        }

        GUILayout.EndScrollView();

        if (_isLoadingFolder)
        {
            EditorGUILayout.LabelField($"Loading folder: {_currentLoadingFolder} ...");
        }

        GUILayout.EndVertical();
    }

    private void DrawFolderDetails()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.65f));
        GUILayout.Label("Folder Details", titleStyle);

        if (selectedFolder == null)
        {
            GUILayout.Label("Select a folder to see its details.", descriptionStyle);
        }
        else
        {
            GUILayout.BeginVertical(detailsBoxStyle);

            GUILayout.Label(selectedFolder.name, headerStyle);
            GUILayout.Space(5);

            GUILayout.Label($"URL: {selectedFolder.url}", linkStyle);
            if (GUILayout.Button("Open in Browser"))
            {
                Application.OpenURL(selectedFolder.url);
            }
            GUILayout.Space(10);

            GUILayout.Label("Files:", titleStyle);

            if (selectedFolder.files == null || selectedFolder.files.Count == 0)
            {
                GUILayout.Label("No files found in this folder.", descriptionStyle);
            }
            else
            {
                foreach (var file in selectedFolder.files)
                {
                    GUILayout.Label($"{file.name} ({file.type})", descriptionStyle);
                }
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();
    }

    // Helper classes

    public class FolderInfo
    {
        public string name;
        public List<GitHubItem> files;
        public string url;

        public bool HasFileWithKeyword(string keyword)
        {
            if (files == null)
                return false;

            keyword = keyword.ToLower();

            foreach (var f in files)
            {
                if (f.name.ToLower().Contains(keyword))
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public class GitHubItem
    {
        public string name;
        public string path;
        public string sha;
        public int size;
        public string url;
        public string html_url;
        public string git_url;
        public string download_url;
        public string type;
        public GitHubItem[] _links;
    }
}

static class AudioUtility
{
    static AudioSource _audioSource;
    public static void PlayClip(AudioClip clip)
    {
        if (_audioSource == null)
        {
            var go = new GameObject("ARIX1X5AudioSource");
            go.hideFlags = HideFlags.HideAndDontSave;
            _audioSource = go.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    public static void StopAllClips()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
            UnityEngine.Object.DestroyImmediate(_audioSource.gameObject);
            _audioSource = null;
        }
    }
}
#endif
