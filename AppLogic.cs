using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutoArchiveX
{
    public class FolderMapping
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }

    public class AppConfig
    {
        public string FolderRule { get; set; }
        public string CustomFolderName { get; set; }
        public string RenamePattern { get; set; }
        public string OwnerSignature { get; set; }
        public string SdLabels { get; set; }
        public string WebhookUrl { get; set; }
        public bool WmShadow { get; set; }
        public bool WmExif { get; set; }

        public bool WmChkOwner { get; set; }
        public bool WmChkCamera { get; set; }
        public bool WmChkLens { get; set; }
        public bool WmChkFocal { get; set; }
        public bool WmChkAperture { get; set; }
        public bool WmChkShutter { get; set; }
        public bool WmChkIso { get; set; }
        public string WmPosition { get; set; }
        public string WmTargetFolder { get; set; }
        public double WmScaleLandscape { get; set; }
        public double WmScalePortrait { get; set; }
        public string WmFont { get; set; }
        public string WmColor { get; set; }
        public bool WmOverwrite { get; set; }
        public string WmPrefix { get; set; }
        public bool WmSameFolder { get; set; }
        public string WmOutputFolder { get; set; }
        public string Language { get; set; }
        public bool StartMaximized { get; set; }
        public List<FolderMapping> Mappings { get; set; }

        public AppConfig()
        {
            Language = "EN";
            StartMaximized = false;
            FolderRule = "exif";
            CustomFolderName = "";
            RenamePattern = "";
            OwnerSignature = "";
            SdLabels = "";
            WebhookUrl = "";
            WmShadow = true;
            WmExif = true;
            WmChkOwner = true;
            WmChkCamera = true;
            WmChkLens = true;
            WmChkFocal = true;
            WmChkAperture = true;
            WmChkShutter = true;
            WmChkIso = true;
            WmPosition = "bottomright";
            WmTargetFolder = "";
            WmOverwrite = true;
            WmPrefix = "wm_";
            WmSameFolder = true;
            WmOutputFolder = "";
            WmScaleLandscape = 1.6;
            WmScalePortrait = 1.6;
            WmFont = "Arial";
            WmColor = "#FFFFFF";
            Mappings = new List<FolderMapping>();
        }
    }

    public static class AppLogic
    {
        private static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath, Encoding.UTF8);
                    return DeserializeConfig(json);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to load config: " + ex.Message);
            }
            return new AppConfig();
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = SerializeConfig(config);
                File.WriteAllText(ConfigPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to save config: " + ex.Message);
            }
        }

        private static string SerializeConfig(AppConfig config)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine(string.Format("  \"FolderRule\": \"{0}\",", EscapeJson(config.FolderRule)));
            sb.AppendLine(string.Format("  \"CustomFolderName\": \"{0}\",", EscapeJson(config.CustomFolderName)));
            sb.AppendLine(string.Format("  \"RenamePattern\": \"{0}\",", EscapeJson(config.RenamePattern)));
            sb.AppendLine(string.Format("  \"OwnerSignature\": \"{0}\",", EscapeJson(config.OwnerSignature)));
            sb.AppendLine(string.Format("  \"SdLabels\": \"{0}\",", EscapeJson(config.SdLabels)));
            sb.AppendLine(string.Format("  \"WebhookUrl\": \"{0}\",", EscapeJson(config.WebhookUrl)));
            sb.AppendLine(string.Format("  \"WmShadow\": {0},", config.WmShadow.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmExif\": {0},", config.WmExif.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkOwner\": {0},", config.WmChkOwner.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkCamera\": {0},", config.WmChkCamera.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkLens\": {0},", config.WmChkLens.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkFocal\": {0},", config.WmChkFocal.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkAperture\": {0},", config.WmChkAperture.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkShutter\": {0},", config.WmChkShutter.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmChkIso\": {0},", config.WmChkIso.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmPosition\": \"{0}\",", EscapeJson(config.WmPosition)));
            sb.AppendLine(string.Format("  \"WmTargetFolder\": \"{0}\",", EscapeJson(config.WmTargetFolder)));
            sb.AppendLine(string.Format("  \"WmOverwrite\": {0},", config.WmOverwrite.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmPrefix\": \"{0}\",", EscapeJson(config.WmPrefix)));
            sb.AppendLine(string.Format("  \"WmSameFolder\": {0},", config.WmSameFolder.ToString().ToLower()));
            sb.AppendLine(string.Format("  \"WmOutputFolder\": \"{0}\",", EscapeJson(config.WmOutputFolder)));
            sb.AppendLine(string.Format("  \"WmScaleLandscape\": {0},", config.WmScaleLandscape.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sb.AppendLine(string.Format("  \"WmScalePortrait\": {0},", config.WmScalePortrait.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sb.AppendLine(string.Format("  \"WmFont\": \"{0}\",", EscapeJson(config.WmFont)));
            sb.AppendLine(string.Format("  \"WmColor\": \"{0}\",", EscapeJson(config.WmColor)));
            sb.AppendLine(string.Format("  \"Language\": \"{0}\",", EscapeJson(config.Language)));
            sb.AppendLine(string.Format("  \"StartMaximized\": {0},", config.StartMaximized.ToString().ToLower()));
            
            sb.AppendLine("  \"Mappings\": [");
            for (int i = 0; i < config.Mappings.Count; i++)
            {
                var m = config.Mappings[i];
                sb.Append(string.Format("    {{ \"Source\": \"{0}\", \"Destination\": \"{1}\" }}", EscapeJson(m.Source), EscapeJson(m.Destination)));
                if (i < config.Mappings.Count - 1) sb.AppendLine(",");
                else sb.AppendLine();
            }
            sb.AppendLine("  ]");
            sb.Append("}");
            return sb.ToString();
        }

        private static AppConfig DeserializeConfig(string json)
        {
            AppConfig config = new AppConfig();
            config.FolderRule = GetJsonStringValue(json, "FolderRule") ?? config.FolderRule;
            config.CustomFolderName = GetJsonStringValue(json, "CustomFolderName") ?? config.CustomFolderName;
            config.RenamePattern = GetJsonStringValue(json, "RenamePattern") ?? config.RenamePattern;
            config.OwnerSignature = GetJsonStringValue(json, "OwnerSignature") ?? config.OwnerSignature;
            config.SdLabels = GetJsonStringValue(json, "SdLabels") ?? config.SdLabels;
            config.WebhookUrl = GetJsonStringValue(json, "WebhookUrl") ?? config.WebhookUrl;
            config.WmShadow = GetJsonBoolValue(json, "WmShadow", config.WmShadow);
            config.WmExif = GetJsonBoolValue(json, "WmExif", config.WmExif);
            config.WmChkOwner = GetJsonBoolValue(json, "WmChkOwner", config.WmChkOwner);
            config.WmChkCamera = GetJsonBoolValue(json, "WmChkCamera", config.WmChkCamera);
            config.WmChkLens = GetJsonBoolValue(json, "WmChkLens", config.WmChkLens);
            config.WmChkFocal = GetJsonBoolValue(json, "WmChkFocal", config.WmChkFocal);
            config.WmChkAperture = GetJsonBoolValue(json, "WmChkAperture", config.WmChkAperture);
            config.WmChkShutter = GetJsonBoolValue(json, "WmChkShutter", config.WmChkShutter);
            config.WmChkIso = GetJsonBoolValue(json, "WmChkIso", config.WmChkIso);
            config.WmPosition = GetJsonStringValue(json, "WmPosition") ?? config.WmPosition;
            config.WmTargetFolder = GetJsonStringValue(json, "WmTargetFolder") ?? config.WmTargetFolder;
            config.WmOverwrite = GetJsonBoolValue(json, "WmOverwrite", config.WmOverwrite);
            config.WmPrefix = GetJsonStringValue(json, "WmPrefix") ?? config.WmPrefix;
            config.WmSameFolder = GetJsonBoolValue(json, "WmSameFolder", config.WmSameFolder);
            config.WmOutputFolder = GetJsonStringValue(json, "WmOutputFolder") ?? config.WmOutputFolder;
            config.WmScaleLandscape = GetJsonDoubleValue(json, "WmScaleLandscape", config.WmScaleLandscape);
            config.WmScalePortrait = GetJsonDoubleValue(json, "WmScalePortrait", config.WmScalePortrait);
            config.WmFont = GetJsonStringValue(json, "WmFont") ?? config.WmFont;
            config.WmColor = GetJsonStringValue(json, "WmColor") ?? config.WmColor;
            config.Language = GetJsonStringValue(json, "Language") ?? config.Language;
            config.StartMaximized = GetJsonBoolValue(json, "StartMaximized", config.StartMaximized);

            Match arrayMatch = Regex.Match(json, @"\""Mappings\""\s*:\s*\[(.*?)\]", RegexOptions.Singleline);
            if (arrayMatch.Success)
            {
                string mappingsStr = arrayMatch.Groups[1].Value;
                MatchCollection objectMatches = Regex.Matches(mappingsStr, @"\{\s*\""Source\""\s*:\s*\""(.*?)\""\s*,\s*\""Destination\""\s*:\s*\""(.*?)\""\s*\}");
                foreach (Match m in objectMatches)
                {
                    config.Mappings.Add(new FolderMapping
                    {
                        Source = UnescapeJson(m.Groups[1].Value),
                        Destination = UnescapeJson(m.Groups[2].Value)
                    });
                }
            }
            return config;
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private static string UnescapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r");
        }

        private static string GetJsonStringValue(string json, string key)
        {
            Match m = Regex.Match(json, "\"" + key + "\"[\\s:]*\"(.*?)(?<!\\\\)\"");
            return m.Success ? UnescapeJson(m.Groups[1].Value) : null;
        }

        private static bool GetJsonBoolValue(string json, string key, bool defaultVal)
        {
            Match m = Regex.Match(json, "\"" + key + "\"[\\s:]*(true|false)", RegexOptions.IgnoreCase);
            return m.Success ? bool.Parse(m.Groups[1].Value) : defaultVal;
        }

        private static double GetJsonDoubleValue(string json, string key, double defaultVal)
        {
            Match m = Regex.Match(json, "\"" + key + "\"[\\s:]*([0-9.]+)");
            double val;
            if (m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
            {
                return val;
            }
            return defaultVal;
        }

        public static int CopyAndBackup(List<FolderMapping> mappings, string folderRule, string customFolderName, Action<string, bool> logCallback)
        {
            int fileCount = 0;
            foreach (var map in mappings)
            {
                if (!Directory.Exists(map.Source))
                {
                    logCallback(string.Format("Source directory not found: {0}", map.Source), true);
                    continue;
                }

                logCallback(string.Format("Syncing: '{0}' ➔ '{1}'", map.Source, map.Destination), false);
                try
                {
                    var files = Directory.GetFiles(map.Source, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        
                        string subFolder = "";
                        if (folderRule == "exif")
                        {
                            DateTime date = fileInfo.LastWriteTime;
                            try
                            {
                                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                                using (Image img = Image.FromStream(fs, false, false))
                                {
                                    PropertyItem dateItem = img.GetPropertyItem(0x9003);
                                    if (dateItem != null)
                                    {
                                        string dtStr = Encoding.ASCII.GetString(dateItem.Value).Trim('\0');
                                        date = DateTime.ParseExact(dtStr.Substring(0, 19), "yyyy:MM:dd HH:mm:ss", null);
                                    }
                                }
                            }
                            catch {}
                            subFolder = Path.Combine(date.ToString("yyyy"), Path.Combine(date.ToString("yyyy_MM"), date.ToString("yyyyMMdd")));
                        }
                        else if (folderRule == "today")
                        {
                            subFolder = DateTime.Today.ToString("yyyy-MM-dd");
                        }
                        else if (folderRule == "custom")
                        {
                            subFolder = customFolderName;
                        }

                        string relative = file.Substring(map.Source.Length).TrimStart(Path.DirectorySeparatorChar);
                        string destDir = string.IsNullOrEmpty(subFolder) ? map.Destination : Path.Combine(map.Destination, subFolder);
                        string destFile = Path.Combine(destDir, relative);

                        string dir = Path.GetDirectoryName(destFile);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        if (!File.Exists(destFile) || new FileInfo(destFile).Length != fileInfo.Length || new FileInfo(destFile).LastWriteTime != fileInfo.LastWriteTime)
                        {
                            File.Copy(file, destFile, true);
                            fileCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logCallback(string.Format("Backup Error on mapping: {0}", ex.Message), true);
                }
            }
            return fileCount;
        }

        public static int RenameFiles(List<FolderMapping> mappings, string renamePattern, Action<string, bool> logCallback)
        {
            if (string.IsNullOrWhiteSpace(renamePattern)) return 0;
            int renameCount = 0;

            foreach (var map in mappings)
            {
                if (!Directory.Exists(map.Source)) continue;
                try
                {
                    string[] files = Directory.GetFiles(map.Source, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        string filename = Path.GetFileNameWithoutExtension(file);
                        if (Regex.IsMatch(filename, @"^\d{8}_\d{6}")) continue;

                        DateTime date = File.GetLastWriteTime(file);
                        try
                        {
                            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                            using (Image img = Image.FromStream(fs, false, false))
                            {
                                PropertyItem dateItem = img.GetPropertyItem(0x9003);
                                if (dateItem != null)
                                {
                                    string dtStr = Encoding.ASCII.GetString(dateItem.Value).Trim('\0');
                                    date = DateTime.ParseExact(dtStr.Substring(0, 19), "yyyy:MM:dd HH:mm:ss", null);
                                }
                            }
                        }
                        catch {}

                        string ext = Path.GetExtension(file);
                        int index = 0;
                        string newName = FormatRenamePattern(renamePattern, date, index) + ext;
                        string newPath = Path.Combine(Path.GetDirectoryName(file), newName);

                        while (File.Exists(newPath))
                        {
                            index++;
                            newName = FormatRenamePattern(renamePattern, date, index) + ext;
                            newPath = Path.Combine(Path.GetDirectoryName(file), newName);
                        }

                        File.Move(file, newPath);
                        renameCount++;
                    }
                }
                catch (Exception ex)
                {
                    logCallback(string.Format("Rename Error: {0}", ex.Message), true);
                }
            }
            return renameCount;
        }

        private static string FormatRenamePattern(string pattern, DateTime date, int index)
        {
            return pattern
                .Replace("{yyyy}", date.ToString("yyyy"))
                .Replace("{MM}", date.ToString("MM"))
                .Replace("{dd}", date.ToString("dd"))
                .Replace("{HH}", date.ToString("HH"))
                .Replace("{mm}", date.ToString("mm"))
                .Replace("{ss}", date.ToString("ss"))
                .Replace("{index}", index.ToString());
        }        
        
        public static int ApplyWatermarks(List<FolderMapping> mappings, AppConfig config, Action<string, bool> logCallback)
        {
            List<string> targetFolders = new List<string>();
            if (!string.IsNullOrEmpty(config.WmTargetFolder) && Directory.Exists(config.WmTargetFolder))
            {
                targetFolders.Add(config.WmTargetFolder);
            }

            if (targetFolders.Count == 0)
            {
                logCallback("Warning: Watermark target folder is not configured or does not exist.", true);
                return -2;
            }
 
            int wmCount = 0;
            try
            {
                List<string> filesList = new List<string>();
                foreach (var folder in targetFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        filesList.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
                    }
                }
                string[] files = filesList.ToArray();

                bool hasJpg = false;
                foreach (var f in files)
                {
                    string e = Path.GetExtension(f).ToLower();
                    if (e == ".jpg" || e == ".jpeg") { hasJpg = true; break; }
                }
                if (!hasJpg) return -1;
 
                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext != ".jpg" && ext != ".jpeg") continue;
  
                    string name = Path.GetFileName(file);
                    string prefix = string.IsNullOrEmpty(config.WmPrefix) ? "wm_" : config.WmPrefix;
                    if (!config.WmOverwrite && name.StartsWith(prefix)) continue;
 
                    try
                    {
                        byte[] imgBytes = File.ReadAllBytes(file);
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        using (Image rawImg = Image.FromStream(ms))
                        {
                                RotateFlipType rotateType = RotateFlipType.RotateNoneFlipNone;
                                try
                                {
                                    if (Array.IndexOf(rawImg.PropertyIdList, 274) >= 0)
                                    {
                                        int orientation = rawImg.GetPropertyItem(274).Value[0];
                                        if (orientation == 6) rotateType = RotateFlipType.Rotate90FlipNone;
                                        else if (orientation == 8) rotateType = RotateFlipType.Rotate270FlipNone;
                                        else if (orientation == 3) rotateType = RotateFlipType.Rotate180FlipNone;
                                    }
                                }
                                catch {}

                                if (rotateType != RotateFlipType.RotateNoneFlipNone)
                                {
                                    rawImg.RotateFlip(rotateType);
                                    try
                                    {
                                        PropertyItem prop = rawImg.GetPropertyItem(274);
                                        prop.Value = new byte[] { 1, 0 };
                                        rawImg.SetPropertyItem(prop);
                                    }
                                    catch {}
                                }

                                string camera = config.WmChkCamera ? GetExifString(rawImg, 272) : "";
                                string lens = config.WmChkLens ? GetExifString(rawImg, 0xA434) : "";
                                string focal = config.WmChkFocal ? GetExifRational(rawImg, 0x920A) : "";
                                string aperture = config.WmChkAperture ? GetExifRational(rawImg, 0x829D, "f/") : "";
                                string shutter = config.WmChkShutter ? GetExifShutter(rawImg, 0x829A) : "";
                                string iso = config.WmChkIso ? GetExifIso(rawImg, 34855) : "";

                                List<string> lines = new List<string>();
                                if (config.WmChkOwner && !string.IsNullOrEmpty(config.OwnerSignature))
                                {
                                    lines.Add("©" + config.OwnerSignature);
                                }
                                
                                string line2 = camera;
                                if (!string.IsNullOrEmpty(lens))
                                {
                                    if (lens.Contains("DG DN") || lens.Contains("Contemporary"))
                                    {
                                        if (!lens.Contains("SIGMA")) lens = "SIGMA " + lens;
                                    }
                                    lens = lens.Replace("Contemporary 021", "").Replace("G Master", "GM").Replace("DG DN", "").Replace("|", "").Trim();
                                    line2 = string.IsNullOrEmpty(line2) ? lens : line2 + " | " + lens;
                                }
                                if (!string.IsNullOrEmpty(line2)) lines.Add(line2);

                                string line3 = "";
                                if (!string.IsNullOrEmpty(focal)) line3 += focal + " ";
                                if (!string.IsNullOrEmpty(aperture)) line3 += aperture + " ";
                                if (!string.IsNullOrEmpty(shutter)) line3 += shutter + " ";
                                if (!string.IsNullOrEmpty(iso)) line3 += "ISO " + iso;
                                line3 = line3.Trim();
                                if (!string.IsNullOrEmpty(line3)) lines.Add(line3);

                                if (lines.Count == 0) continue;

                                string wmContent = string.Join("\n", lines.ToArray());

                                using (Bitmap bmp = new Bitmap(rawImg.Width, rawImg.Height))
                                {
                                    bmp.SetResolution(rawImg.HorizontalResolution, rawImg.VerticalResolution);
                                    using (Graphics g = Graphics.FromImage(bmp))
                                    {
                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                        g.DrawImage(rawImg, 0, 0);

                                        bool isLandscape = rawImg.Width >= rawImg.Height;
                                        double scaleRatio = isLandscape ? config.WmScaleLandscape : config.WmScalePortrait;
                                        float testFontSize = (float)(rawImg.Height * (scaleRatio / 100.0));
                                        
                                        string fFamily = "Arial";
                                        if (!string.IsNullOrEmpty(config.WmFont))
                                        {
                                            try {
                                                using (FontFamily testFamily = new FontFamily(config.WmFont))
                                                {
                                                    fFamily = config.WmFont;
                                                }
                                            } catch {}
                                        }

                                        Color textCol = Color.White;
                                        if (!string.IsNullOrEmpty(config.WmColor))
                                        {
                                            try {
                                                textCol = ColorTranslator.FromHtml(config.WmColor);
                                            } catch {}
                                        }

                                        using (Font font = new Font(fFamily, testFontSize, FontStyle.Regular))
                                        {
                                            SizeF textSize = g.MeasureString(wmContent, font);
                                            float padding = rawImg.Height * 0.015f;
                                            float x = rawImg.Width - textSize.Width - padding;
                                            float y = rawImg.Height - textSize.Height - padding;
                                            string pos = (config.WmPosition ?? "bottomright").ToLower();
                                            if (pos == "pattern")
                                            {
                                                var state = g.Save();
                                                
                                                g.TranslateTransform(rawImg.Width / 2, rawImg.Height / 2);
                                                g.RotateTransform(-30);
                                                
                                                int range = (int)Math.Sqrt(rawImg.Width * rawImg.Width + rawImg.Height * rawImg.Height) * 2;
                                                int stepX = (int)(textSize.Width * 1.8);
                                                int stepY = (int)(textSize.Height * 3.5);
                                                if (stepX < 200) stepX = 200;
                                                if (stepY < 100) stepY = 100;
 
                                                using (SolidBrush patternBrush = new SolidBrush(Color.FromArgb(22, textCol.R, textCol.G, textCol.B)))
                                                {
                                                    int rowCount = 0;
                                                    for (int py = -range; py < range; py += stepY)
                                                    {
                                                        int shiftX = (rowCount % 2 == 0) ? 0 : stepX / 2;
                                                        for (int px = -range; px < range; px += stepX)
                                                        {
                                                            g.DrawString(wmContent, font, patternBrush, px + shiftX, py);
                                                        }
                                                        rowCount++;
                                                    }
                                                }
                                                g.Restore(state);
                                            }
                                            else
                                            {
                                                if (pos == "topleft")
                                                {
                                                    x = padding;
                                                    y = padding;
                                                }
                                                else if (pos == "topright")
                                                {
                                                    x = rawImg.Width - textSize.Width - padding;
                                                    y = padding;
                                                }
                                                else if (pos == "bottomleft")
                                                {
                                                    x = padding;
                                                    y = rawImg.Height - textSize.Height - padding;
                                                }
                                                else if (pos == "bottomright")
                                                {
                                                    x = rawImg.Width - textSize.Width - padding;
                                                    y = rawImg.Height - textSize.Height - padding;
                                                }

                                                if (config.WmShadow)
                                                {
                                                    float offset = testFontSize * 0.06f;
                                                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
                                                    {
                                                        g.DrawString(wmContent, font, shadowBrush, x + offset, y + offset);
                                                    }
                                                }

                                                using (SolidBrush textBrush = new SolidBrush(textCol))
                                                {
                                                    g.DrawString(wmContent, font, textBrush, x, y);
                                                }
                                            }
                                        }
                                    }
                                    string destFile = file;
                                    if (!config.WmOverwrite)
                                    {
                                        if (!config.WmSameFolder && !string.IsNullOrEmpty(config.WmOutputFolder))
                                        {
                                            try
                                            {
                                                if (!Directory.Exists(config.WmOutputFolder)) Directory.CreateDirectory(config.WmOutputFolder);
                                                destFile = Path.Combine(config.WmOutputFolder, name);
                                            }
                                            catch
                                            {
                                                destFile = Path.Combine(Path.GetDirectoryName(file), prefix + name);
                                            }
                                        }
                                        else
                                        {
                                            destFile = Path.Combine(Path.GetDirectoryName(file), prefix + name);
                                        }
                                    }
                                    bmp.Save(destFile, ImageFormat.Jpeg);
                                    wmCount++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logCallback(string.Format("Watermark processing failed for {0}: {1}", Path.GetFileName(file), ex.Message), true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logCallback(string.Format("Watermark processing general error: {0}", ex.Message), true);
                }
            return wmCount;
        }

        private static string GetExifString(Image img, int tagId)
        {
            try
            {
                if (Array.IndexOf(img.PropertyIdList, tagId) >= 0)
                {
                    return Encoding.UTF8.GetString(img.GetPropertyItem(tagId).Value).TrimEnd('\0').Trim();
                }
            }
            catch {}
            return "";
        }

        private static string GetExifRational(Image img, int tagId, string prefix = "")
        {
            try
            {
                if (Array.IndexOf(img.PropertyIdList, tagId) >= 0)
                {
                    byte[] data = img.GetPropertyItem(tagId).Value;
                    if (data.Length >= 8)
                    {
                        uint num = BitConverter.ToUInt32(data, 0);
                        uint den = BitConverter.ToUInt32(data, 4);
                        if (den > 0)
                        {
                            double val = (double)num / den;
                            return prefix + Math.Round(val, 1).ToString() + (tagId == 0x920A ? "mm" : "");
                        }
                    }
                }
            }
            catch {}
            return "";
        }

        private static string GetExifShutter(Image img, int tagId)
        {
            try
            {
                if (Array.IndexOf(img.PropertyIdList, tagId) >= 0)
                {
                    byte[] data = img.GetPropertyItem(tagId).Value;
                    if (data.Length >= 8)
                    {
                        uint num = BitConverter.ToUInt32(data, 0);
                        uint den = BitConverter.ToUInt32(data, 4);
                        if (num > 0 && den > 0)
                        {
                            if (num < den) return string.Format("1/{0}", Math.Round((double)den / num));
                            return string.Format("{0}\"", num / den);
                        }
                    }
                }
            }
            catch {}
            return "";
        }

        private static string GetExifIso(Image img, int tagId)
        {
            try
            {
                if (Array.IndexOf(img.PropertyIdList, tagId) >= 0)
                {
                    byte[] data = img.GetPropertyItem(tagId).Value;
                    return BitConverter.ToUInt16(data, 0).ToString();
                }
            }
            catch {}
            return "";
        }

        public static int RemoveEmptyFolders(List<FolderMapping> mappings, Action<string, bool> logCallback)
        {
            int count = 0;
            foreach (var map in mappings)
            {
                if (Directory.Exists(map.Source)) count += CleanDir(map.Source);
                if (Directory.Exists(map.Destination)) count += CleanDir(map.Destination);
            }
            return count;
        }

        private static int CleanDir(string path)
        {
            int deleted = 0;
            try
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    deleted += CleanDir(directory);
                }

                if (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0)
                {
                    Directory.Delete(path, false);
                    deleted++;
                }
            }
            catch {}
            return deleted;
        }

        public static void SendDiscordNotification(string webhookUrl, string content)
        {
            if (string.IsNullOrEmpty(webhookUrl)) return;
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
                    string body = string.Format("{{ \"content\": \"{0}\" }}", EscapeJson(content));
                    client.UploadData(webhookUrl, "POST", Encoding.UTF8.GetBytes(body));
                }
            }
            catch {}
        }
    }
}