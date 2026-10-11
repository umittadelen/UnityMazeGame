
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GitHubUpdateChecker : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button updateButton;

    private const string ReleasesApi =
        "https://api.github.com/repos/umittadelen/UnityMazeGame/releases";

    [Serializable]
    private class ReleaseInfo
    {
        public string tag_name;
        public string html_url;
        public bool draft;
        public bool prerelease;
    }

    [Serializable]
    private class ReleaseList
    {
        public ReleaseInfo[] releases;
    }

    private void Start()
    {
        updateButton.gameObject.SetActive(false);
        StartCoroutine(CheckForUpdates());
    }

    private IEnumerator CheckForUpdates()
    {
        string suffix = GetPlatformSuffix();

        if (string.IsNullOrEmpty(suffix))
            yield break;

        if (updateButton == null)
        {
            Debug.LogError("Update button is not assigned!");
            yield break;
        }

        using var request = UnityWebRequest.Get(ReleasesApi);
        request.timeout = 8;
        request.SetRequestHeader("Accept", "application/vnd.github+json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning(
                "Could not check for game updates: " + request.error);
            yield break;
        }

        ReleaseList releaseList;

        try
        {
            string json = "{\"releases\":" +
                          request.downloadHandler.text + "}";

            releaseList = JsonUtility.FromJson<ReleaseList>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not parse GitHub releases: " + e.Message);
            yield break;
        }

        if (releaseList?.releases == null)
            yield break;

        if (!TryParseVersion(Application.version, out Version currentVersion))
        {
            Debug.LogWarning(
                "Invalid Player version: " + Application.version);
            yield break;
        }

        ReleaseInfo newestRelease = null;
        Version newestVersion = null;

        foreach (ReleaseInfo release in releaseList.releases)
        {
            if (release == null ||
                release.draft ||
                release.prerelease ||
                string.IsNullOrEmpty(release.tag_name) ||
                string.IsNullOrEmpty(release.html_url))
            {
                continue;
            }

            // Only consider releases for this player's platform.
            if (!release.tag_name.EndsWith(
                suffix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryParseVersion(release.tag_name, out Version version))
                continue;

            // Find the highest version for this platform.
            if (newestVersion == null ||
                version.CompareTo(newestVersion) > 0)
            {
                newestVersion = version;
                newestRelease = release;
            }
        }

        if (newestRelease == null || newestVersion == null)
        {
            Debug.Log("No compatible releases found for " + suffix);
            yield break;
        }

        if (newestVersion.CompareTo(currentVersion) <= 0)
        {
            Debug.Log("Game is up to date: " + Application.version);
            yield break;
        }

        Debug.Log(
            $"Update available: {currentVersion} -> {newestVersion}");

        // Enable the update button
        updateButton.onClick.AddListener(
            () => Application.OpenURL(newestRelease.html_url));

        updateButton.gameObject.SetActive(true);
    }

    private static string GetPlatformSuffix()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return "-win";

            case RuntimePlatform.Android:
                return "-and";

            case RuntimePlatform.IPhonePlayer:
                return "-ios";

            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                return "-mac";

            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                return "-lnx";

            default:
                return null;
        }
    }

    private static bool TryParseVersion(
        string value, out Version version)
    {
        version = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        // Remove the leading v from tags such as v1.2.0-win.
        if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            value = value.Substring(1);

        // Remove the platform suffix.
        int suffixIndex = value.IndexOf('-');

        if (suffixIndex >= 0)
            value = value.Substring(0, suffixIndex);

        return Version.TryParse(value, out version);
    }
}
