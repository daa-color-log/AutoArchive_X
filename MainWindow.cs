using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutoArchiveX
{
    public class MainWindow : Window
    {
        // 코어 비즈니스 데이터 및 다국어 상태 관리
        private AppConfig config;
        private List<FolderMapping> folderMappings;
        private string currentLang = "EN"; 

        // --- 내비게이션 및 뷰 제어 리스트 ---
        private List<Button> navButtons;
        private List<FrameworkElement> viewPanels;

        // --- 좌측 사이드바 메뉴 버튼 요소 ---
        private Button btnNavMapping;
        private Button btnNavConfig;
        private Button btnNavWatermark;
        private Button btnNavSettings;
        private Button btnNavLogs;

        // --- 우측 메인 콘텐츠 뷰 패널 ---
        private Border viewMapping;
        private ScrollViewer viewConfig;
        private ScrollViewer viewWatermark;
        private Border viewSettings;
        private Border viewLogs;

        // --- 하단 글로벌 상태 컨트롤 영역 ---
        private TextBlock lblStatusText;
        private ProgressBar prgStatus;
        private Button btnOpBackup;
        private Button btnOpRename;
        private Button btnOpWatermark;
        private Button btnOpClean;
        private Button btnOpAll;
        private CheckBox chkShutdown;
        private CheckBox chkStartMaximized;

        // --- [View 1] 폴더 매핑 및 연결 저장소 모니터 요소 ---
        private TextBlock lblPanelMapping;
        private Button btnAddMapping;
        private TextBlock hdrSource;
        private TextBlock hdrDest;
        private TextBlock hdrActions;
        private StackPanel mappingListContainer;
        private ScrollViewer mappingScrollViewer;
        private TextBlock lblPanelStorage;
        private StackPanel storageListContainer;

        // --- [View 2] 아카이브 규칙 설정 레이아웃 요소 ---
        private TextBlock lblPanelConfig;
        private GroupBox grpFolderRule;
        private TextBlock lblFolderRule;
        private ComboBox folderRuleSelect;
        private TextBlock lblCustomFolder;
        private TextBox txtCustomFolder;
        private StackPanel customFolderGroup;
        
        private GroupBox grpRenameRule;
        private TextBlock lblRenameFmt;
        private TextBox txtRenameFmt;
        private TextBlock lblRenameHint;

        private GroupBox grpIntegration;
        private TextBlock lblWebhook;
        private TextBox txtWebhookUrl;
        private TextBlock lblSdLabels;
        private TextBox txtSdLabels;

        // --- [View 3] 워터마크 상세 설정 및 실시간 미리보기 요소 ---
        private TextBlock lblPanelWatermark;
        private GroupBox grpWmTarget;
        private TextBlock lblWmTargetFolder;
        private TextBox txtWmTargetFolder;
        private Button btnBrowseWmTarget;
        private CheckBox chkWmOverwrite;
        private CheckBox chkWmSameFolder;
        private TextBlock lblWmOutputFolder;
        private TextBox txtWmOutputFolder;
        private Button btnBrowseWmOutput;
        private TextBlock lblWmPrefix;
        private TextBox txtWmPrefix;

        private GroupBox grpWmItems;
        private TextBlock lblWmItemsHint;
        private CheckBox chkWmOwner;
        private CheckBox chkWmCamera;
        private CheckBox chkWmLens;
        private CheckBox chkWmFocal;
        private CheckBox chkWmAperture;
        private CheckBox chkWmShutter;
        private CheckBox chkWmIso;
        private CheckBox chkWmShadow;
        private CheckBox chkWmExif;

        private GroupBox grpWmStyle;
        private TextBlock lblWmOwnerSignature;
        private TextBox txtWmOwnerSignature;
        private TextBlock lblWmPosition;
        private ComboBox cmbWmPosition;
        
        // 추가된 가로/세로 비율 제어 슬라이더
        private TextBlock lblWmScaleLandscape;
        private Slider sldWmScaleLandscape;
        private TextBlock lblWmScaleLandscapeVal;
        private TextBlock lblWmScalePortrait;
        private Slider sldWmScalePortrait;
        private TextBlock lblWmScalePortraitVal;

        private TextBlock lblWmFont;
        private ComboBox wmFontSelect;
        private TextBlock lblWmColor;
        private TextBox txtWmColorHex;
        private Border borderWmColorPreview;
        private TextBlock lblPreviewTitle;
        
        // 가로/세로 분리형 더블 캔버스 프리뷰 컨트롤
        private TextBlock lblPreviewLTitle;
        private TextBlock lblPreviewPTitle;
        private Border previewLandscapeBorder;
        private Border previewPortraitBorder;
        private StackPanel previewLandscapeTextStack;
        private StackPanel previewPortraitTextStack;

        // --- [View 4] 독립형 전용 글로벌 설정 요소 ---
        private GroupBox grpLangSetting;
        private TextBlock lblLangSelect;
        private ComboBox langSelect;
        private TextBlock lblAppInfo;

        // --- [View 5] 실시간 콘솔 로그 레이아웃 요소 ---
        private TextBox txtLogConsole;
        private Button btnClearLog;

        // 가이드 창 번역용 리소스 사전
        private Dictionary<string, Dictionary<string, string>> i18nGuide = new Dictionary<string, Dictionary<string, string>>();
        private Button btnShowGuide;
        private Button btnExitApp;

        public MainWindow()
        {
            InitializeGuideTranslations();

            // 윈도우 대시보드 최적화 프레임 규격 설정
            Width = 1150;
            Height = 780;
            MinWidth = 1100;
            MinHeight = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(246, 248, 250));

            folderMappings = new List<FolderMapping>();
            navButtons = new List<Button>();
            viewPanels = new List<FrameworkElement>();

            // 구성 설정 세팅 로드
            config = AppLogic.LoadConfig();
            if (config.Mappings != null)
            {
                folderMappings.AddRange(config.Mappings);
            }

            // 매핑정보는 최소한 1개는 항시 표시되도록 보장
            if (folderMappings.Count == 0)
            {
                folderMappings.Add(new FolderMapping { Source = "", Destination = "" });
            }

            // 윈도우가 닫힐 때 설정을 안전하게 저장
            this.Closing += (s, e) => SaveUIToConfig();

            // UI 컴포넌트 동적 초기화 및 렌더링
            InitializeComponentElements();

            // 데이터 바인딩 연동
            LoadConfigToUI();

            // config에 저장된 언어로 로컬라이제이션 설정 (기본값 EN)
            ApplyLocalization(string.IsNullOrEmpty(config.Language) ? "EN" : config.Language);

            // ESC 키 입력 시 전체 화면(테두리 없음 최대화) 모드 해제 기능 연동
            this.KeyDown += (s, e) => {
                if (e.Key == System.Windows.Input.Key.Escape) {
                    if (chkStartMaximized != null && chkStartMaximized.IsChecked == true) {
                        chkStartMaximized.IsChecked = false;
                    }
                }
            };

            // 연결된 저장 장치 실시간 모니터링 타이머 가동
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) => RefreshStorageVolumes();
            timer.Start();
            RefreshStorageVolumes();
        }

        private void InitializeComponentElements()
        {
            // 메인 윈도우 그리드 분할 (상단 타이틀 바 / 중앙 메인 프레임 / 하단 진행 상태 바)
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(45) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(95) }); // 하단 2단 구조를 위해 높이 확대

            // ==========================================
            // [상단 글로벌 헤더 영역 - 탑 플랫 배너]
            // ==========================================
            Border headerBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(36, 41, 47)), Padding = new Thickness(20, 0, 20, 0) };
            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock lblTitle = new TextBlock { Text = "⚙️ AUTO ARCHIVE X", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold, FontSize = 13 };
            headerGrid.Children.Add(lblTitle);
            Grid.SetColumn(lblTitle, 0);

            btnShowGuide = new Button 
            { 
                Height = 28, Padding = new Thickness(12, 0, 12, 0),
                Background = new SolidColorBrush(Color.FromRgb(55, 62, 69)), BorderThickness = new Thickness(0),
                Foreground = Brushes.White, FontWeight = FontWeights.SemiBold, Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
            btnShowGuide.Click += (s, e) => ShowUserGuide();
            headerGrid.Children.Add(btnShowGuide);
            Grid.SetColumn(btnShowGuide, 1);

            btnExitApp = new Button 
            { 
                Height = 28, Padding = new Thickness(12, 0, 12, 0),
                Background = new SolidColorBrush(Color.FromRgb(209, 36, 47)), BorderThickness = new Thickness(0),
                Foreground = Brushes.White, FontWeight = FontWeights.Bold, Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0)
            };
            btnExitApp.Click += (s, e) => this.Close();
            headerGrid.Children.Add(btnExitApp);
            Grid.SetColumn(btnExitApp, 2);

            headerBorder.Child = headerGrid;
            mainGrid.Children.Add(headerBorder);

            // ==========================================
            // [중앙 대시보드 스플릿 프레임워크 설계]
            // ==========================================
            Grid centerGrid = new Grid();
            // 컬럼 분할: 왼쪽 내비게이션 사이드바 (210px) | 오른쪽 고정 가변 콘텐츠 패널 (1 Star)
            centerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) });
            centerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(centerGrid, 1);
            mainGrid.Children.Add(centerGrid);

            // ------------------------------------------
            // [좌측 내비게이션 전용 사이드바 렉트 베이스]
            // ------------------------------------------
            Border sidebarBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(43, 49, 55)) };
            StackPanel pnlSidebar = new StackPanel { Margin = new Thickness(0, 10, 0, 10) };

            // 내부 공통 사이드바 버튼 생성 팩토리 헬퍼 함수
            Func<string, Button> CreateNavButton = (iconText) => {
                Button btn = new Button {
                    Height = 42,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(15, 0, 0, 0),
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(Color.FromRgb(149, 157, 165)),
                    BorderThickness = new Thickness(0),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                pnlSidebar.Children.Add(btn);
                navButtons.Add(btn);
                return btn;
            };

            btnNavMapping = CreateNavButton("📁 Mappings");
            btnNavConfig = CreateNavButton("⚙️ Rules & Sync");
            btnNavWatermark = CreateNavButton("🎨 Watermark");
            btnNavSettings = CreateNavButton("🌐 Language & Settings");
            btnNavLogs = CreateNavButton("🖥️ Console Logs");

            // 버튼 클릭 인터랙션 핸들러 일괄 연결
            btnNavMapping.Click += (s, e) => SwitchTab(0);
            btnNavConfig.Click += (s, e) => SwitchTab(1);
            btnNavWatermark.Click += (s, e) => SwitchTab(2);
            btnNavSettings.Click += (s, e) => SwitchTab(3);
            btnNavLogs.Click += (s, e) => SwitchTab(4);

            sidebarBorder.Child = pnlSidebar;
            Grid.SetColumn(sidebarBorder, 0);
            centerGrid.Children.Add(sidebarBorder);

            // ------------------------------------------
            // [우측 대시보드 메인 콘텐츠 디스플레이 컨테이너]
            // ------------------------------------------
            Grid rightContentGrid = new Grid { Background = Brushes.White };
            Grid.SetColumn(rightContentGrid, 1);
            centerGrid.Children.Add(rightContentGrid);

            // 공통 뷰 추가 프로세스 자동화 매핑 함수
            Action<FrameworkElement> RegisterViewPanel = (panel) => {
                panel.Visibility = Visibility.Collapsed;
                rightContentGrid.Children.Add(panel);
                viewPanels.Add(panel);
            };

            // [View 1] 폴더 매핑 및 연결 저장소 모니터 인터페이스 모듈화 구성
            viewMapping = new Border { Padding = new Thickness(20) };
            Grid mainMapGrid = new Grid();
            mainMapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.4, GridUnitType.Star) });
            mainMapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
            viewMapping.Child = mainMapGrid;

            // 좌측: 폴더 매핑 리스트
            Grid gridMapTab = new Grid { Margin = new Thickness(0, 0, 15, 0) };
            gridMapTab.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            gridMapTab.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            gridMapTab.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            gridMapTab.RowDefinitions.Add(new RowDefinition { Height = new GridLength(45) });

            lblPanelMapping = new TextBlock { FontSize = 12, Foreground = Brushes.DimGray, VerticalAlignment = VerticalAlignment.Center };
            gridMapTab.Children.Add(lblPanelMapping);

            Grid gridMapHeader = new Grid();
            gridMapHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridMapHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridMapHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            hdrSource = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(5, 0, 0, 0) };
            hdrDest = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(5, 0, 0, 0) };
            hdrActions = new TextBlock { FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center };
            Grid.SetColumn(hdrSource, 0); Grid.SetColumn(hdrDest, 1); Grid.SetColumn(hdrActions, 2);
            gridMapHeader.Children.Add(hdrSource); gridMapHeader.Children.Add(hdrDest); gridMapHeader.Children.Add(hdrActions);
            Grid.SetRow(gridMapHeader, 1);
            gridMapTab.Children.Add(gridMapHeader);

            mappingScrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 5, 0, 5), BorderBrush = new SolidColorBrush(Color.FromRgb(225, 228, 232)), BorderThickness = new Thickness(1) };
            mappingListContainer = new StackPanel { Margin = new Thickness(5) };
            mappingScrollViewer.Content = mappingListContainer;
            Grid.SetRow(mappingScrollViewer, 2);
            gridMapTab.Children.Add(mappingScrollViewer);

            btnAddMapping = new Button { Height = 32, Background = new SolidColorBrush(Color.FromRgb(46, 164, 79)), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0) };
            btnAddMapping.Click += (s, e) => {
                var newMap = new FolderMapping { Source = "", Destination = "" };
                folderMappings.Add(newMap);
                AddMappingRowToUI(newMap);
            };
            Grid.SetRow(btnAddMapping, 3);
            gridMapTab.Children.Add(btnAddMapping);
            mainMapGrid.Children.Add(gridMapTab);
            Grid.SetColumn(gridMapTab, 0);

            // 우측: 연결 드라이브 용량 실시간 모니터
            Border storageBorder = new Border { BorderBrush = new SolidColorBrush(Color.FromRgb(225, 228, 232)), BorderThickness = new Thickness(1, 0, 0, 0), Padding = new Thickness(15, 0, 0, 0) };
            Grid storageGrid = new Grid();
            storageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            storageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            lblPanelStorage = new TextBlock { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.DimGray, VerticalAlignment = VerticalAlignment.Center };
            ScrollViewer storageScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            storageListContainer = new StackPanel();
            storageScroll.Content = storageListContainer;

            storageGrid.Children.Add(lblPanelStorage); Grid.SetRow(lblPanelStorage, 0);
            storageGrid.Children.Add(storageScroll); Grid.SetRow(storageScroll, 1);
            storageBorder.Child = storageGrid;
            mainMapGrid.Children.Add(storageBorder);
            Grid.SetColumn(storageBorder, 1);

            RegisterViewPanel(viewMapping);

            // [View 2] 아카이브 스플릿팅 및 동기화 규칙 설정 뷰 구성
            StackPanel pnlConfigTab = new StackPanel { Margin = new Thickness(0) };
            lblPanelConfig = new TextBlock { FontSize = 12, Foreground = Brushes.DimGray, Margin = new Thickness(0, 0, 0, 15) };
            pnlConfigTab.Children.Add(lblPanelConfig);

            grpFolderRule = new GroupBox { Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            StackPanel pnlFolderRule = new StackPanel();
            lblFolderRule = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            folderRuleSelect = new ComboBox { Height = 26, Margin = new Thickness(0, 0, 0, 10) };
            folderRuleSelect.Items.Add(new ComboBoxItem { Tag = "exif" });
            folderRuleSelect.Items.Add(new ComboBoxItem { Tag = "today" });
            folderRuleSelect.Items.Add(new ComboBoxItem { Tag = "custom" });
            folderRuleSelect.SelectionChanged += (s, e) => {
                ComboBoxItem item = folderRuleSelect.SelectedItem as ComboBoxItem;
                if (item != null)
                    customFolderGroup.Visibility = (item.Tag.ToString() == "custom") ? Visibility.Visible : Visibility.Collapsed;
            };

            customFolderGroup = new StackPanel { Visibility = Visibility.Collapsed };
            lblCustomFolder = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            txtCustomFolder = new TextBox { Height = 26 };
            customFolderGroup.Children.Add(lblCustomFolder);
            customFolderGroup.Children.Add(txtCustomFolder);

            pnlFolderRule.Children.Add(lblFolderRule);
            pnlFolderRule.Children.Add(folderRuleSelect);
            pnlFolderRule.Children.Add(customFolderGroup);
            grpFolderRule.Content = pnlFolderRule;
            pnlConfigTab.Children.Add(grpFolderRule);

            grpRenameRule = new GroupBox { Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            StackPanel pnlRenameRule = new StackPanel();
            lblRenameFmt = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            txtRenameFmt = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 5) };
            lblRenameHint = new TextBlock { FontSize = 11, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap };
            pnlRenameRule.Children.Add(lblRenameFmt);
            pnlRenameRule.Children.Add(txtRenameFmt);
            pnlRenameRule.Children.Add(lblRenameHint);
            grpRenameRule.Content = pnlRenameRule;
            pnlConfigTab.Children.Add(grpRenameRule);

            grpIntegration = new GroupBox { Padding = new Thickness(10) };
            StackPanel pnlIntegration = new StackPanel();
            lblWebhook = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            txtWebhookUrl = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10) };
            lblSdLabels = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            txtSdLabels = new TextBox { Height = 26 };
            pnlIntegration.Children.Add(lblWebhook);
            pnlIntegration.Children.Add(txtWebhookUrl);
            pnlIntegration.Children.Add(lblSdLabels);
            pnlIntegration.Children.Add(txtSdLabels);
            grpIntegration.Content = pnlIntegration;
            pnlConfigTab.Children.Add(grpIntegration);

            viewConfig = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(20), Content = pnlConfigTab };
            RegisterViewPanel(viewConfig);

            // [View 3] 워터마크 상세 설정 및 실시간 미리보기 뷰 구성
            StackPanel pnlWatermarkTab = new StackPanel { Margin = new Thickness(0) };
            lblPanelWatermark = new TextBlock { FontSize = 12, Foreground = Brushes.DimGray, Margin = new Thickness(0, 0, 0, 15) };
            pnlWatermarkTab.Children.Add(lblPanelWatermark);

            grpWmTarget = new GroupBox { Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            Grid gridWmTarget = new Grid();
            gridWmTarget.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridWmTarget.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridWmTarget.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridWmTarget.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridWmTarget.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            lblWmTargetFolder = new TextBlock { Margin = new Thickness(0, 5, 0, 5), FontWeight = FontWeights.Bold };
            Grid.SetRow(lblWmTargetFolder, 0); gridWmTarget.Children.Add(lblWmTargetFolder);

            Grid subGridTgt = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            subGridTgt.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            subGridTgt.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            txtWmTargetFolder = new TextBox { Height = 26 };
            txtWmTargetFolder.TextChanged += (s, e) => LiveUpdatePreviewCanvas();
            btnBrowseWmTarget = new Button { Content = "...", Margin = new Thickness(5, 0, 0, 0) };
            btnBrowseWmTarget.Click += (s, e) => HandleFolderBrowse(txtWmTargetFolder);
            Grid.SetColumn(txtWmTargetFolder, 0); Grid.SetColumn(btnBrowseWmTarget, 1);
            subGridTgt.Children.Add(txtWmTargetFolder); subGridTgt.Children.Add(btnBrowseWmTarget);
            Grid.SetRow(subGridTgt, 1); gridWmTarget.Children.Add(subGridTgt);

            StackPanel pnlWmOptions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            chkWmOverwrite = new CheckBox { Margin = new Thickness(0, 0, 15, 0), VerticalAlignment = VerticalAlignment.Center };
            chkWmSameFolder = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
            chkWmSameFolder.Checked += (s, e) => ToggleOutputFolderVisibility();
            chkWmSameFolder.Unchecked += (s, e) => ToggleOutputFolderVisibility();
            pnlWmOptions.Children.Add(chkWmOverwrite);
            pnlWmOptions.Children.Add(chkWmSameFolder);
            Grid.SetRow(pnlWmOptions, 2); gridWmTarget.Children.Add(pnlWmOptions);

            lblWmOutputFolder = new TextBlock { Margin = new Thickness(0, 5, 0, 5), FontWeight = FontWeights.Bold };
            Grid.SetRow(lblWmOutputFolder, 3); gridWmTarget.Children.Add(lblWmOutputFolder);

            Grid subGridOut = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            subGridOut.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            subGridOut.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            txtWmOutputFolder = new TextBox { Height = 26 };
            btnBrowseWmOutput = new Button { Content = "...", Margin = new Thickness(5, 0, 0, 0) };
            btnBrowseWmOutput.Click += (s, e) => HandleFolderBrowse(txtWmOutputFolder);
            Grid.SetColumn(txtWmOutputFolder, 0); Grid.SetColumn(btnBrowseWmOutput, 1);
            subGridOut.Children.Add(txtWmOutputFolder); subGridOut.Children.Add(btnBrowseWmOutput);
            Grid.SetRow(subGridOut, 4); gridWmTarget.Children.Add(subGridOut);
            grpWmTarget.Content = gridWmTarget;
            pnlWatermarkTab.Children.Add(grpWmTarget);

            grpWmItems = new GroupBox { Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            StackPanel pnlWmItems = new StackPanel();
            lblWmItemsHint = new TextBlock { FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 8), TextWrapping = TextWrapping.Wrap };
            pnlWmItems.Children.Add(lblWmItemsHint);

            chkWmOwner = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmCamera = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmLens = new CheckBox { Margin = new Thickness(0, 3, 0, 3) }; // chkWmLens mapping
            chkWmFocal = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmAperture = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmShutter = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmIso = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmShadow = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };
            chkWmExif = new CheckBox { Margin = new Thickness(0, 3, 0, 3) };

            chkWmOwner.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmCamera.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmLens.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmFocal.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmAperture.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmShutter.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmIso.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmShadow.Click += (s, e) => LiveUpdatePreviewCanvas();
            chkWmExif.Click += (s, e) => LiveUpdatePreviewCanvas();

            pnlWmItems.Children.Add(chkWmOwner);
            pnlWmItems.Children.Add(chkWmCamera);
            pnlWmItems.Children.Add(chkWmLens);
            pnlWmItems.Children.Add(chkWmFocal);
            pnlWmItems.Children.Add(chkWmAperture);
            pnlWmItems.Children.Add(chkWmShutter);
            pnlWmItems.Children.Add(chkWmIso);
            pnlWmItems.Children.Add(chkWmShadow);
            pnlWmItems.Children.Add(chkWmExif);
            grpWmItems.Content = pnlWmItems;
            pnlWatermarkTab.Children.Add(grpWmItems);

            grpWmStyle = new GroupBox { Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 15) };
            StackPanel pnlWmStyle = new StackPanel();
            lblWmOwnerSignature = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            txtWmOwnerSignature = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10) };
            txtWmOwnerSignature.TextChanged += (s, e) => LiveUpdatePreviewCanvas();
            lblWmPosition = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            cmbWmPosition = new ComboBox { Height = 26, Margin = new Thickness(0, 0, 0, 10) };
            cmbWmPosition.Items.Add(new ComboBoxItem { Tag = "bottomright" });
            cmbWmPosition.Items.Add(new ComboBoxItem { Tag = "bottomleft" });
            cmbWmPosition.Items.Add(new ComboBoxItem { Tag = "topright" });
            cmbWmPosition.Items.Add(new ComboBoxItem { Tag = "topleft" });
            cmbWmPosition.Items.Add(new ComboBoxItem { Tag = "pattern" });
            cmbWmPosition.SelectionChanged += (s, e) => LiveUpdatePreviewCanvas();
            
            lblWmPrefix = new TextBlock { Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.Bold };
            txtWmPrefix = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 10) };

            // 추가된 가로/세로 비율 입력 필드 구성 - 슬라이더로 대체 예정
            lblWmScaleLandscape = new TextBlock();
            lblWmScalePortrait = new TextBlock();

            pnlWmStyle.Children.Add(lblWmOwnerSignature);
            pnlWmStyle.Children.Add(txtWmOwnerSignature);
            pnlWmStyle.Children.Add(lblWmPosition);
            pnlWmStyle.Children.Add(cmbWmPosition);
            pnlWmStyle.Children.Add(lblWmPrefix);
            pnlWmStyle.Children.Add(txtWmPrefix);
            grpWmStyle.Content = pnlWmStyle;
            pnlWatermarkTab.Children.Add(grpWmStyle);

            // 추가: 폰트, 컬러 선택 (시스템 폰트 목록 로드 및 커스텀 HEX 컬러 코드 입력 기능)
            Grid styleCustomGrid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            styleCustomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            styleCustomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
            
            StackPanel fontPane = new StackPanel { Margin = new Thickness(0, 0, 10, 0) };
            lblWmFont = new TextBlock { FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 5, 5) };
            wmFontSelect = new ComboBox { Height = 26, IsEditable = true };
            
            // 시스템에 설치된 전체 폰트 로드
            try {
                foreach (FontFamily fontFam in System.Windows.Media.Fonts.SystemFontFamilies)
                {
                    wmFontSelect.Items.Add(fontFam.Source);
                }
            } catch {
                wmFontSelect.Items.Add("Arial");
                wmFontSelect.Items.Add("Consolas");
                wmFontSelect.Items.Add("Segoe UI");
                wmFontSelect.Items.Add("Malgun Gothic");
            }
            wmFontSelect.SelectedIndex = wmFontSelect.Items.IndexOf("Segoe UI") >= 0 ? wmFontSelect.Items.IndexOf("Segoe UI") : 0;
            wmFontSelect.SelectionChanged += (s, e) => LiveUpdatePreviewCanvas();
            fontPane.Children.Add(lblWmFont); fontPane.Children.Add(wmFontSelect);
            
            StackPanel colorPane = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            lblWmColor = new TextBlock { FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 5, 5) };
            
            Grid colorInputGrid = new Grid();
            colorInputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            colorInputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) }); // 색상 칩 미리보기
            
            txtWmColorHex = new TextBox { Height = 26, Text = "#FFFFFF", VerticalContentAlignment = VerticalAlignment.Center };
            borderWmColorPreview = new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(3), Background = Brushes.White, BorderBrush = Brushes.Silver, BorderThickness = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            
            txtWmColorHex.TextChanged += (s, e) => {
                try {
                    string hex = txtWmColorHex.Text.Trim();
                    if (!hex.StartsWith("#")) hex = "#" + hex;
                    var brushConverter = new BrushConverter();
                    Brush brush = (Brush)brushConverter.ConvertFromString(hex);
                    borderWmColorPreview.Background = brush;
                    LiveUpdatePreviewCanvas();
                } catch {
                    // 잘못된 HEX 입력 시 에러 방지
                }
            };
            
            Grid.SetColumn(txtWmColorHex, 0);
            Grid.SetColumn(borderWmColorPreview, 1);
            colorInputGrid.Children.Add(txtWmColorHex);
            colorInputGrid.Children.Add(borderWmColorPreview);
            
            colorPane.Children.Add(lblWmColor);
            colorPane.Children.Add(colorInputGrid);

            // Preset Color Palette WrapPanel
            WrapPanel presetColorPalette = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            string[] presetColors = new string[] { "#FFFFFF", "#E0E0E0", "#9E9E9E", "#000000", "#FFFF00", "#FF9800", "#F44336", "#4CAF50", "#2196F3" };
            foreach (string colorHex in presetColors) {
                var brushConverter = new BrushConverter();
                Brush colorBrush = (Brush)brushConverter.ConvertFromString(colorHex);
                Border colorChip = new Border {
                    Width = 18,
                    Height = 18,
                    CornerRadius = new CornerRadius(9),
                    Background = colorBrush,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 0, 6, 4),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = colorHex
                };
                string captureColor = colorHex;
                colorChip.MouseDown += (s, e) => {
                    txtWmColorHex.Text = captureColor;
                };
                presetColorPalette.Children.Add(colorChip);
            }
            colorPane.Children.Add(presetColorPalette);
            
            styleCustomGrid.Children.Add(fontPane); Grid.SetColumn(fontPane, 0);
            styleCustomGrid.Children.Add(colorPane); Grid.SetColumn(colorPane, 1);
            pnlWatermarkTab.Children.Add(styleCustomGrid);

            lblPreviewTitle = new TextBlock { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(3, 102, 214)), Margin = new Thickness(0, 0, 0, 8) };
            pnlWatermarkTab.Children.Add(lblPreviewTitle);
            
            // 더블 미리보기 캔버스 및 하단 비율 설정 배치 (가로 프레임 320x240 및 세로 프레임 180x240)
            Grid previewGrid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
            previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel pnlPreviewL = new StackPanel { Margin = new Thickness(0, 0, 10, 0) };
            lblPreviewLTitle = new TextBlock { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 4) };
            previewLandscapeBorder = new Border { Height = 240, Background = new SolidColorBrush(Color.FromRgb(20, 22, 26)), BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(12), ClipToBounds = true };
            previewLandscapeTextStack = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right };
            previewLandscapeBorder.Child = previewLandscapeTextStack;

            // 가로 사진 글꼴 비율 입력 필드를 이미지 바로 밑에 배치 (슬라이더 및 수치 라벨)
            lblWmScaleLandscape = new TextBlock { Margin = new Thickness(0, 8, 0, 4), FontWeight = FontWeights.SemiBold, FontSize = 11 };
            
            Grid gridSldL = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            gridSldL.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridSldL.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            
            sldWmScaleLandscape = new Slider { Minimum = 0.5, Maximum = 10.0, TickFrequency = 0.1, IsSnapToTickEnabled = true, VerticalAlignment = VerticalAlignment.Center };
            lblWmScaleLandscapeVal = new TextBlock { Text = "1.6%", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(5, 0, 0, 0) };
            
            sldWmScaleLandscape.ValueChanged += (s, e) => {
                lblWmScaleLandscapeVal.Text = string.Format("{0:0.0}%", sldWmScaleLandscape.Value);
                LiveUpdatePreviewCanvas();
            };
            
            Grid.SetColumn(sldWmScaleLandscape, 0);
            Grid.SetColumn(lblWmScaleLandscapeVal, 1);
            gridSldL.Children.Add(sldWmScaleLandscape);
            gridSldL.Children.Add(lblWmScaleLandscapeVal);

            pnlPreviewL.Children.Add(lblPreviewLTitle);
            pnlPreviewL.Children.Add(previewLandscapeBorder);
            pnlPreviewL.Children.Add(lblWmScaleLandscape);
            pnlPreviewL.Children.Add(gridSldL);

            StackPanel pnlPreviewP = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            lblPreviewPTitle = new TextBlock { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 4) };
            previewPortraitBorder = new Border { Height = 240, Width = 180, HorizontalAlignment = HorizontalAlignment.Center, Background = new SolidColorBrush(Color.FromRgb(20, 22, 26)), BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(12), ClipToBounds = true };
            previewPortraitTextStack = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right };
            previewPortraitBorder.Child = previewPortraitTextStack;

            // 세로 사진 글꼴 비율 입력 필드를 이미지 바로 밑에 배치 (슬라이더 및 수치 라벨)
            lblWmScalePortrait = new TextBlock { Margin = new Thickness(0, 8, 0, 4), FontWeight = FontWeights.SemiBold, FontSize = 11 };
            
            Grid gridSldP = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            gridSldP.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridSldP.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            
            sldWmScalePortrait = new Slider { Minimum = 0.5, Maximum = 10.0, TickFrequency = 0.1, IsSnapToTickEnabled = true, VerticalAlignment = VerticalAlignment.Center };
            lblWmScalePortraitVal = new TextBlock { Text = "1.6%", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(5, 0, 0, 0) };
            
            sldWmScalePortrait.ValueChanged += (s, e) => {
                lblWmScalePortraitVal.Text = string.Format("{0:0.0}%", sldWmScalePortrait.Value);
                LiveUpdatePreviewCanvas();
            };
            
            Grid.SetColumn(sldWmScalePortrait, 0);
            Grid.SetColumn(lblWmScalePortraitVal, 1);
            gridSldP.Children.Add(sldWmScalePortrait);
            gridSldP.Children.Add(lblWmScalePortraitVal);

            pnlPreviewP.Children.Add(lblPreviewPTitle);
            pnlPreviewP.Children.Add(previewPortraitBorder);
            pnlPreviewP.Children.Add(lblWmScalePortrait);
            pnlPreviewP.Children.Add(gridSldP);

            previewGrid.Children.Add(pnlPreviewL); Grid.SetColumn(pnlPreviewL, 0);
            previewGrid.Children.Add(pnlPreviewP); Grid.SetColumn(pnlPreviewP, 1);
            pnlWatermarkTab.Children.Add(previewGrid);

            viewWatermark = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(20), Content = pnlWatermarkTab };
            RegisterViewPanel(viewWatermark);

            // [View 4] 독립 분리형 언어 및 시스템 설정 뷰 구성
            viewSettings = new Border { Padding = new Thickness(20) };
            StackPanel pnlSettingsTab = new StackPanel();
            
            grpLangSetting = new GroupBox { Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 15) };
            StackPanel pnlLangInner = new StackPanel();
            lblLangSelect = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 6) };
            
            langSelect = new ComboBox { Height = 28 };
            langSelect.Items.Add(new ComboBoxItem { Content = "English (Default)", Tag = "EN" });
            langSelect.Items.Add(new ComboBoxItem { Content = "日本語 (JA)", Tag = "JA" });
            langSelect.Items.Add(new ComboBoxItem { Content = "한국어 (KO)", Tag = "KO" });
            langSelect.SelectedIndex = 0;
            langSelect.SelectionChanged += (s, e) => {
                ComboBoxItem sel = langSelect.SelectedItem as ComboBoxItem;
                if (sel != null && sel.Tag != null) {
                    ApplyLocalization(sel.Tag.ToString());
                    LiveUpdatePreviewCanvas();
                }
            };
            pnlLangInner.Children.Add(lblLangSelect);
            pnlLangInner.Children.Add(langSelect);

            chkStartMaximized = new CheckBox { Margin = new Thickness(0, 12, 0, 0), FontWeight = FontWeights.SemiBold, Foreground = Brushes.DarkSlateGray };
            chkStartMaximized.Checked += (s, e) => {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            };
            chkStartMaximized.Unchecked += (s, e) => {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
            };
            pnlLangInner.Children.Add(chkStartMaximized);

            grpLangSetting.Content = pnlLangInner;
            pnlSettingsTab.Children.Add(grpLangSetting);

            lblAppInfo = new TextBlock { FontSize = 11, Foreground = Brushes.DarkGray, Margin = new Thickness(5, 10, 0, 0), LineHeight = 16 };
            pnlSettingsTab.Children.Add(lblAppInfo);
            viewSettings.Child = pnlSettingsTab;
            RegisterViewPanel(viewSettings);

            // [View 5] 실시간 시스템 실시간 로그 콘솔 뷰 구성
            viewLogs = new Border { Padding = new Thickness(20) };
            Grid gridLogsTab = new Grid();
            gridLogsTab.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            gridLogsTab.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            viewLogs.Child = gridLogsTab;

            txtLogConsole = new TextBox { IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Background = new SolidColorBrush(Color.FromRgb(29, 31, 33)), Foreground = new SolidColorBrush(Color.FromRgb(197, 200, 198)), FontFamily = new FontFamily("Consolas"), FontSize = 12, Padding = new Thickness(8) };
            Grid.SetRow(txtLogConsole, 0);
            gridLogsTab.Children.Add(txtLogConsole);

            btnClearLog = new Button { Height = 26, Width = 140, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            btnClearLog.Click += (s, e) => txtLogConsole.Clear();
            Grid.SetRow(btnClearLog, 1);
            gridLogsTab.Children.Add(btnClearLog);
            RegisterViewPanel(viewLogs);

            // ==========================================
            // [하단 고정 글로벌 상태 제어 바 영역]
            // ==========================================
            Border bottomBarBorder = new Border { Background = Brushes.White, Padding = new Thickness(20, 10, 20, 10), BorderBrush = new SolidColorBrush(Color.FromRgb(225, 228, 232)), BorderThickness = new Thickness(0, 1, 0, 0) };
            Grid bottomBarGrid = new Grid();
            bottomBarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 상태 텍스트 / 프로그레스 바
            bottomBarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 실행 버튼 목록
            bottomBarBorder.Child = bottomBarGrid;

            // Row 0: 상태 바 영역
            Grid statusInfoGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            statusInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            lblStatusText = new TextBlock { FontWeight = FontWeights.SemiBold, Foreground = Brushes.DarkSlateGray, FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
            prgStatus = new ProgressBar { Height = 10, Minimum = 0, Maximum = 100, Value = 0, Background = new SolidColorBrush(Color.FromRgb(235, 237, 240)), Width = 220, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };

            statusInfoGrid.Children.Add(lblStatusText); Grid.SetColumn(lblStatusText, 0);
            statusInfoGrid.Children.Add(prgStatus); Grid.SetColumn(prgStatus, 1);
            bottomBarGrid.Children.Add(statusInfoGrid); Grid.SetRow(statusInfoGrid, 0);

            // Row 1: 실행 제어 버튼 및 컴퓨터 자동 종료 옵션
            Grid actionGrid = new Grid();
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel btnStack = new StackPanel { Orientation = Orientation.Horizontal };
            Func<string, SolidColorBrush, Button> CreateActionButton = (btnText, bgBrush) => {
                return new Button {
                    Content = btnText,
                    Height = 30,
                    Padding = new Thickness(15, 0, 15, 0),
                    Margin = new Thickness(0, 0, 8, 0),
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Background = bgBrush,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
            };

            btnOpBackup = CreateActionButton("📂 Sync", new SolidColorBrush(Color.FromRgb(3, 102, 214)));
            btnOpRename = CreateActionButton("📝 Rename", new SolidColorBrush(Color.FromRgb(3, 102, 214)));
            btnOpWatermark = CreateActionButton("📷 Watermark", new SolidColorBrush(Color.FromRgb(3, 102, 214)));
            btnOpClean = CreateActionButton("🧹 Clean", new SolidColorBrush(Color.FromRgb(3, 102, 214)));
            btnOpAll = CreateActionButton("🚀 Run All", new SolidColorBrush(Color.FromRgb(46, 164, 79)));

            btnOpBackup.Click += (s, e) => RunSingleTask("Sync", log => AppLogic.CopyAndBackup(folderMappings, config.FolderRule, config.CustomFolderName, log));
            btnOpRename.Click += (s, e) => RunSingleTask("Rename", log => AppLogic.RenameFiles(folderMappings, config.RenamePattern, log));
            btnOpWatermark.Click += (s, e) => RunSingleTask("Watermark", log => AppLogic.ApplyWatermarks(folderMappings, config, log));
            btnOpClean.Click += (s, e) => RunSingleTask("Clean", log => AppLogic.RemoveEmptyFolders(folderMappings, log));
            btnOpAll.Click += BtnOpAll_Click;

            btnStack.Children.Add(btnOpBackup);
            btnStack.Children.Add(btnOpRename);
            btnStack.Children.Add(btnOpWatermark);
            btnStack.Children.Add(btnOpClean);
            btnStack.Children.Add(btnOpAll);

            chkShutdown = new CheckBox { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold, Foreground = Brushes.DarkSlateGray };

            actionGrid.Children.Add(btnStack); Grid.SetColumn(btnStack, 0);
            actionGrid.Children.Add(chkShutdown); Grid.SetColumn(chkShutdown, 1);
            bottomBarGrid.Children.Add(actionGrid); Grid.SetRow(actionGrid, 1);

            Grid.SetRow(bottomBarBorder, 2);
            mainGrid.Children.Add(bottomBarBorder);

            this.Content = mainGrid;

            // 마스터 내비게이션 초기 화면 지정 (첫 번째 매핑 패널 활성화)
            SwitchTab(0);
            RefreshMappingListUI();
        }

        // 대시보드 탭 스위칭 유기적 화면 제어 코어 함수
        private void SwitchTab(int index)
        {
            if (index < 0 || index >= viewPanels.Count) return;

            // 1) 전체 뷰 패널 가시성 숨김 처리
            for (int i = 0; i < viewPanels.Count; i++)
            {
                viewPanels[i].Visibility = Visibility.Collapsed;
            }
            // 2) 선택된 특정 단일 패널만 투명도 해제 및 표출
            viewPanels[index].Visibility = Visibility.Visible;

            // 3) 사이드바 버튼 활성화 유저 익스피리언스 음영 효과 적용
            Brush normalText = new SolidColorBrush(Color.FromRgb(149, 157, 165));
            Brush activeText = Brushes.White;
            Brush activeBg = new SolidColorBrush(Color.FromRgb(55, 62, 69));

            for (int i = 0; i < navButtons.Count; i++)
            {
                if (i == index) {
                    navButtons[i].Background = activeBg;
                    navButtons[i].Foreground = activeText;
                } else {
                    navButtons[i].Background = Brushes.Transparent;
                    navButtons[i].Foreground = normalText;
                }
            }
        }

        private void RefreshMappingListUI()
        {
            mappingListContainer.Children.Clear();
            foreach (var map in folderMappings)
            {
                AddMappingRowToUI(map);
            }
        }

        private void AddMappingRowToUI(FolderMapping mapping)
        {
            Grid rowGrid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });

            Grid srcGrid = new Grid { Margin = new Thickness(0, 0, 4, 0) };
            srcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            srcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            TextBox txtSrc = new TextBox { Height = 24, Text = mapping.Source };
            txtSrc.TextChanged += (s, e) => mapping.Source = txtSrc.Text;
            Button btnBrowseSrc = new Button { Content = "...", Height = 24 };
            btnBrowseSrc.Click += (s, e) => { HandleFolderBrowse(txtSrc); };
            Grid.SetColumn(txtSrc, 0); Grid.SetColumn(btnBrowseSrc, 1);
            srcGrid.Children.Add(txtSrc); srcGrid.Children.Add(btnBrowseSrc);

            Grid dstGrid = new Grid { Margin = new Thickness(4, 0, 4, 0) };
            dstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            dstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            TextBox txtDst = new TextBox { Height = 24, Text = mapping.Destination };
            txtDst.TextChanged += (s, e) => mapping.Destination = txtDst.Text;
            Button btnBrowseDst = new Button { Content = "...", Height = 24 };
            btnBrowseDst.Click += (s, e) => { HandleFolderBrowse(txtDst); };
            Grid.SetColumn(txtDst, 0); Grid.SetColumn(btnBrowseDst, 1);
            dstGrid.Children.Add(txtDst); dstGrid.Children.Add(btnBrowseDst);

            Button btnDel = new Button { Content = "✕", Height = 24, Background = new SolidColorBrush(Color.FromRgb(209, 36, 47)), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Margin = new Thickness(4, 0, 0, 0) };
            btnDel.Click += (s, e) => {
                folderMappings.Remove(mapping);
                mappingListContainer.Children.Remove(rowGrid);
            };

            Grid.SetColumn(srcGrid, 0); Grid.SetColumn(dstGrid, 1); Grid.SetColumn(btnDel, 2);
            rowGrid.Children.Add(srcGrid); rowGrid.Children.Add(dstGrid); rowGrid.Children.Add(btnDel);
            mappingListContainer.Children.Add(rowGrid);
            UpdatePlaceholdersOnRow(rowGrid);
        }

        private void UpdatePlaceholdersOnRow(Grid rowGrid)
        {
            Grid srcGrid = rowGrid.Children[0] as Grid;
            Grid dstGrid = rowGrid.Children[1] as Grid;
            if (srcGrid != null && dstGrid != null)
            {
                TextBox txtSrc = srcGrid.Children[0] as TextBox;
                TextBox txtDst = dstGrid.Children[0] as TextBox;
                if (txtSrc != null) txtSrc.SetPlaceholderText(currentLang == "KO" ? "예: C:\\SourceFolder" : (currentLang == "JA" ? "例: C:\\Source" : "e.g. C:\\SourceFolder"));
                if (txtDst != null) txtDst.SetPlaceholderText(currentLang == "KO" ? "예: D:\\BackupStorage" : (currentLang == "JA" ? "例: D:\\Backup" : "e.g. D:\\BackupStorage"));
            }
        }

        private void HandleFolderBrowse(TextBox targetTextBox)
        {
            string title = "Select Folder";
            if (currentLang == "JA") title = "フォルダ選択";
            else if (currentLang == "KO") title = "폴더 선택";

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = title
            };
            if (dialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    targetTextBox.Text = Path.GetDirectoryName(dialog.FileName);
                }
            }
        }

        private void ToggleOutputFolderVisibility()
        {
            bool isSame = chkWmSameFolder.IsChecked ?? false;
            if (isSame)
            {
                lblWmOutputFolder.Visibility = Visibility.Collapsed;
                txtWmOutputFolder.Visibility = Visibility.Collapsed;
                btnBrowseWmOutput.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblWmOutputFolder.Visibility = Visibility.Visible;
                txtWmOutputFolder.Visibility = Visibility.Visible;
                btnBrowseWmOutput.Visibility = Visibility.Visible;
            }
        }

        private void LoadConfigToUI()
        {
            txtCustomFolder.Text = config.CustomFolderName;
            txtRenameFmt.Text = config.RenamePattern;
            txtWebhookUrl.Text = config.WebhookUrl;
            txtSdLabels.Text = config.SdLabels;

            txtWmTargetFolder.Text = config.WmTargetFolder;
            chkWmOverwrite.IsChecked = config.WmOverwrite;
            chkWmSameFolder.IsChecked = config.WmSameFolder;
            txtWmOutputFolder.Text = config.WmOutputFolder;
            txtWmPrefix.Text = config.WmPrefix;

            chkWmOwner.IsChecked = config.WmChkOwner;
            chkWmCamera.IsChecked = config.WmChkCamera;
            chkWmLens.IsChecked = config.WmChkLens;
            chkWmFocal.IsChecked = config.WmChkFocal;
            chkWmAperture.IsChecked = config.WmChkAperture;
            chkWmShutter.IsChecked = config.WmChkShutter;
            chkWmIso.IsChecked = config.WmChkIso;
            chkWmShadow.IsChecked = config.WmShadow;
            chkWmExif.IsChecked = config.WmExif;

            txtWmOwnerSignature.Text = config.OwnerSignature;
            sldWmScaleLandscape.Value = config.WmScaleLandscape;
            sldWmScalePortrait.Value = config.WmScalePortrait;
            lblWmScaleLandscapeVal.Text = string.Format("{0:0.0}%", config.WmScaleLandscape);
            lblWmScalePortraitVal.Text = string.Format("{0:0.0}%", config.WmScalePortrait);

            foreach (ComboBoxItem item in folderRuleSelect.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == config.FolderRule)
                {
                    folderRuleSelect.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in cmbWmPosition.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == config.WmPosition)
                {
                    cmbWmPosition.SelectedItem = item;
                    break;
                }
            }
            foreach (ComboBoxItem item in langSelect.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == config.Language)
                {
                    langSelect.SelectedItem = item;
                    break;
                }
            }
            chkStartMaximized.IsChecked = config.StartMaximized;
            ToggleOutputFolderVisibility();
            LiveUpdatePreviewCanvas();
        }

        private void SaveUIToConfig()
        {
            config.CustomFolderName = txtCustomFolder.Text;
            config.RenamePattern = txtRenameFmt.Text;
            config.WebhookUrl = txtWebhookUrl.Text;
            config.SdLabels = txtSdLabels.Text;
            config.WmTargetFolder = txtWmTargetFolder.Text;
            config.WmOverwrite = chkWmOverwrite.IsChecked ?? false;
            config.WmSameFolder = chkWmSameFolder.IsChecked ?? false;
            config.WmOutputFolder = txtWmOutputFolder.Text;
            config.WmPrefix = txtWmPrefix.Text;

            config.WmChkOwner = chkWmOwner.IsChecked ?? false;
            config.WmChkCamera = chkWmCamera.IsChecked ?? false;
            config.WmChkLens = chkWmLens.IsChecked ?? false;
            config.WmChkFocal = chkWmFocal.IsChecked ?? false;
            config.WmChkAperture = chkWmAperture.IsChecked ?? false;
            config.WmChkShutter = chkWmShutter.IsChecked ?? false;
            config.WmChkIso = chkWmIso.IsChecked ?? false;
            config.WmShadow = chkWmShadow.IsChecked ?? false;
            config.WmExif = chkWmExif.IsChecked ?? false;

            config.OwnerSignature = txtWmOwnerSignature.Text;

            config.WmScaleLandscape = Math.Round(sldWmScaleLandscape.Value, 1);
            config.WmScalePortrait = Math.Round(sldWmScalePortrait.Value, 1);

            ComboBoxItem itemRule = folderRuleSelect.SelectedItem as ComboBoxItem;
            if (itemRule != null && itemRule.Tag != null)
                config.FolderRule = itemRule.Tag.ToString();
            ComboBoxItem itemPos = cmbWmPosition.SelectedItem as ComboBoxItem;
            if (itemPos != null && itemPos.Tag != null)
                config.WmPosition = itemPos.Tag.ToString();

            config.Language = currentLang;
            config.StartMaximized = chkStartMaximized.IsChecked ?? false;
            config.Mappings = new List<FolderMapping>(folderMappings);
            AppLogic.SaveConfig(config);
        }

        // 글로벌 로컬라이제이션 변환 엔진 전역화 정의부
        private void ApplyLocalization(string lang)
        {
            currentLang = lang;

            if (lang == "EN")
            {
                this.Title = "AutoArchiveX";
                btnNavMapping.Content = "📁 Directory Mappings";
                btnNavConfig.Content = "⚙️ Rules & Sync";
                btnNavWatermark.Content = "🎨 Watermark Tool";
                btnNavSettings.Content = "🌐 Language & Settings";
                btnNavLogs.Content = "🖥️ System Log Console";
                btnShowGuide.Content = "📖 System Guide";

                btnExitApp.Content = "Exit System";
                lblPanelMapping.Text = "Manage source directories and destination drive pairs for media archiving seamlessly.";
                hdrSource.Text = "Source Directory Path";
                hdrDest.Text = "Destination Archive Path";
                hdrActions.Text = "Del";
                btnAddMapping.Content = "+ Add New Sync Folder Mapping";
                lblPanelStorage.Text = "Active Disk Volumes & Health Monitor";

                lblPanelConfig.Text = "Configure folder hierarchy splitting logic and mass renaming rules upon file copy.";
                grpFolderRule.Header = "Automatic Directory Hierarchy Rule";
                lblFolderRule.Text = "Archive Directory Tree Generation Logic";
                lblCustomFolder.Text = "Custom Static Root Folder Name";
                grpRenameRule.Header = "File Mass Renaming Pattern Templates";
                lblRenameFmt.Text = "Renaming Template Pattern String";
                lblRenameHint.Text = "Available Dynamic Tokens:\n{yyyy}: Year, {MM}: Month, {dd}: Day, {HH}: Hour, {mm}: Min, {ss}: Sec, {index}: Sequential Counter";
                grpIntegration.Header = "External Web Communication Hook";
                lblWebhook.Text = "Discord Application Webhook URL Endpoint";
                lblSdLabels.Text = "SD Card Custom Workflow Identification Label";

                lblPanelWatermark.Text = "Extract internal EXIF metadata headers and print artistic signatures & camera specs on images.";
                grpWmTarget.Header = "I/O Target Tracking Configuration";
                lblWmTargetFolder.Text = "Target Input Images Folder for Processing";
                chkWmOverwrite.Content = "Allow directly overwriting original source image files";
                chkWmSameFolder.Content = "Save processed image inside the same source folder";
                lblWmOutputFolder.Text = "Distinct Target Folder for Saving Watermarked Results";
                lblWmPrefix.Text = "Output Filename Prefix Labeling (Default: wm_)";

                grpWmItems.Header = "Select EXIF Metadata Fields to Imprint";
                lblWmItemsHint.Text = "Only successfully detected hardware parameters inside the file metadata will be printed.";
                chkWmOwner.Content = "Imprint Copyright Owner Initial Signature";
                chkWmCamera.Content = "Imprint Camera Body Make & Model Name";
                chkWmLens.Content = "Imprint Optics Lens Hardware Specifications";
                chkWmFocal.Content = "Imprint Focal Length Spec Value (mm)";
                chkWmAperture.Content = "Imprint Lens Aperture Stop Value (f/)";
                chkWmShutter.Content = "Imprint Shutter Speed Exposure Value (1/s)";
                chkWmIso.Content = "Imprint Light Sensitivity ISO Value";
                chkWmShadow.Content = "Apply Double Layer Drop Shadow Effect for Readability";
                chkWmExif.Content = "Keep & Clone Original Structural EXIF Meta Header Fields";

                grpWmStyle.Header = "Signature Typography Styles & Rendering Coordinates";
                lblWmOwnerSignature.Text = "© Copyright Owner Signature String";
                lblWmPosition.Text = "Watermark Alignment Target Position Coordinates";
                lblWmScaleLandscape.Text = "Landscape Photo Font Ratio (%)";
                lblWmScalePortrait.Text = "Portrait Photo Font Ratio (%)";

                lblWmFont.Text = "Font Family Selection";
                lblWmColor.Text = "Typography Fill Color";
                lblPreviewTitle.Text = "WYSIWYG Live Previews";
                lblPreviewLTitle.Text = "Landscape Preview (Aspect Ratio 4:3)";
                lblPreviewPTitle.Text = "Portrait Preview (Aspect Ratio 3:4)";

                grpLangSetting.Header = "Global Desktop Localization System";
                lblLangSelect.Text = "Select Primary Operational User Interface Language";
                chkStartMaximized.Content = "Start application in Maximized (Full Screen) mode";
                lblAppInfo.Text = "Application Module: AutoArchiveX Core Suite\nBuild Version: v1.4.2 Enterprise\nFramework Context: .NET Framework 4.5 Runtime Dynamic WPF Environment";

                btnClearLog.Content = "Clear Console Text";
                btnOpBackup.Content = "📂 Sync Folders";
                btnOpRename.Content = "📝 Rename Files";
                btnOpWatermark.Content = "📷 Add Watermark";
                btnOpClean.Content = "🧹 Prune Folders";
                btnOpAll.Content = "🚀 RUN ALL PIPELINE";
                chkShutdown.Content = "Shutdown computer when job finishes";
                lblStatusText.Text = "Ready Standby";
            }
            else if (lang == "JA")
            {
                this.Title = "AutoArchiveX";
                btnNavMapping.Content = "📁 フォルダマッピング";
                btnNavConfig.Content = "⚙️ ルールと同期";
                btnNavWatermark.Content = "🎨 ウォーターマーク";
                btnNavSettings.Content = "🌐 言語と一般設定";
                btnNavLogs.Content = "🖥️ 実行ログコンソール";
                btnShowGuide.Content = "📖 システムガイド";

                btnExitApp.Content = "システム終了";
                lblPanelMapping.Text = "メディアを安全にアーカイブするための、転送元ソースパスと転送先ドライブのマッピングペアを管理します。";
                hdrSource.Text = "転送元ディレクトリ (Source)";
                hdrDest.Text = "転送先ディレクトリ (Destination)";
                hdrActions.Text = "削除";
                btnAddMapping.Content = "+ 新しい同期フォルダマッピングを追加";
                lblPanelStorage.Text = "接続ストレージステータス (空き容量)";

                lblPanelConfig.Text = "ファイルの移動およびコピー時における、自動分類フォルダの生成ルールとファイル名変更ルールを制御します。";
                grpFolderRule.Header = "フォルダ自動分割生成ルール";
                lblFolderRule.Text = "アーカイブツリー構造生成アルゴリズム";
                lblCustomFolder.Text = "ユーザー定義固定ルートフォルダ名";
                grpRenameRule.Header = "ファイル名一括置換パターン";
                lblRenameFmt.Text = "ネーミング変更テンプレート";
                lblRenameHint.Text = "置換予約トークンガイド:\n{yyyy}: 年, {MM}: 月, {dd}: 日, {HH}: 時, {mm}: 分, {ss}: 秒, {index}: 連番(自動インクリメント)";
                grpIntegration.Header = "外部通信エンドポイント連携";
                lblWebhook.Text = "Discordウェブフック通知URL";
                lblSdLabels.Text = "分類に使用するSDカードのカスタムラベル名";

                lblPanelWatermark.Text = "写真画像のEXIFメタデータを抽出し、画像に署名や撮影スペックを自動でインプリントします。";
                grpWmTarget.Header = "ウォーターマーク処理の入出力追跡制御";
                lblWmTargetFolder.Text = "ウォーターマーク一括変換の対象フォルダ";
                chkWmOverwrite.Content = "元の画像ファイルに直接上書きすることを許可する";
                chkWmSameFolder.Content = "対象ファイルと同じパスに結果を保存する";
                lblWmOutputFolder.Text = "別途分離して保管するウォーターマーク出力先";
                lblWmPrefix.Text = "出力ファイル名の接頭辞 (wm_)";

                grpWmItems.Header = "合成するEXIFメタデータ要素の選択";
                lblWmItemsHint.Text = "カメラおよびファイルメタデータから実際に検出された情報のみが印刷対象に含まれます。";
                chkWmOwner.Content = "著作権所有者の署名表記 (©)";
                chkWmCamera.Content = "カメラの製造元およびモデル名";
                chkWmLens.Content = "使用レンズのハードウェア仕様";
                chkWmFocal.Content = "撮影画角・焦点距離情報 (mm)";
                chkWmAperture.Content = "レンズ絞り値 (f/)";
                chkWmShutter.Content = "シャッタースピード露出時間 (1/s)";
                chkWmIso.Content = "ISO感度スペック";
                chkWmShadow.Content = "視認性向上のための文字ドロップシャドウ効果を適用";
                chkWmExif.Content = "元のEXIF構造化メタデータヘッダーを保持・複製する";

                grpWmStyle.Header = "テキスト署名の詳細とレンダリング位置情報";
                lblWmOwnerSignature.Text = "© 所有者のイニシャル署名テキスト";
                lblWmPosition.Text = "画像上の署名配置ポジション";
                lblWmScaleLandscape.Text = "横写真のフォント縮尺比率 (%)";
                lblWmScalePortrait.Text = "縦写真のフォント縮尺比率 (%)";

                lblWmFont.Text = "フォントタイプ選択";
                lblWmColor.Text = "文字表示カラー選択";
                lblPreviewTitle.Text = "WYSIWYG リアルタイムプレビュー";
                lblPreviewLTitle.Text = "横写真プレビュー (アスペクト比 4:3)";
                lblPreviewPTitle.Text = "縦写真プレビュー (アスペクト比 3:4)";

                grpLangSetting.Header = "グローバル言語ローカリゼーションシステム";
                lblLangSelect.Text = "メインの操作ユーザーインターフェース言語を選択";
                chkStartMaximized.Content = "起動時にアプリを最大化(全画面)表示する";
                lblAppInfo.Text = "アプリケーション名: AutoArchiveX Core Suite\nビルドバージョン: v1.4.2 Enterprise\nフレームワーク環境: .NET Framework 4.5 準拠 WPF ランタイム環境";

                btnClearLog.Content = "コンソール履歴のクリア";
                btnOpBackup.Content = "📂 フォルダ同期";
                btnOpRename.Content = "📝 ファイル名変更";
                btnOpWatermark.Content = "📷 透かし合成";
                btnOpClean.Content = "🧹 空フォルダ削除";
                btnOpAll.Content = "🚀 全自動実行パイプライン";
                chkShutdown.Content = "処理完了後にPCを自動シャットダウンする";
                lblStatusText.Text = "待機状態";
            }
            else // KO
            {
                this.Title = "AutoArchiveX";
                btnNavMapping.Content = "📁 폴더 매핑";
                btnNavConfig.Content = "⚙️ 규칙 및 동기화";
                btnNavWatermark.Content = "🎨 워터마크 마킹";
                btnNavSettings.Content = "🌐 언어 및 환경 설정";
                btnNavLogs.Content = "🖥️ 실행 콘솔 로그";
                btnShowGuide.Content = "📖 시스템 가이드";

                btnExitApp.Content = "시스템 종료";
                lblPanelMapping.Text = "안전하게 미디어를 아카이브할 원본 소스 경로와 대상 드라이브 매핑 쌍을 관리합니다.";
                hdrSource.Text = "원본 소스 디렉토리 (Source)";
                hdrDest.Text = "대상 저장 디렉토리 (Destination)";
                hdrActions.Text = "제거";
                btnAddMapping.Content = "+ 새 동기화 폴더 매핑 추가";
                lblPanelStorage.Text = "연결된 하드웨어 저장 장치 실시간 상태";

                lblPanelConfig.Text = "파일 이동 및 복사 시 자동 분류 폴더 생성 규칙과 파일명 변경 규칙을 제어합니다.";
                grpFolderRule.Header = "폴더 자동 분할 생성 규칙";
                lblFolderRule.Text = "아카이브 트리 구조 생성 알고리즘";
                lblCustomFolder.Text = "사용자정의 고정 루트 폴더 이름";
                grpRenameRule.Header = "파일 이름 일괄 일치 치환 패턴";
                lblRenameFmt.Text = "네이밍 변경 정규 템플릿";
                lblRenameHint.Text = "치환 예약 토큰 가이드:\n{yyyy}: 년도, {MM}: 월, {dd}: 일, {HH}: 시, {mm}: 분, {ss}: 초, {index}: 일련번호 순차 증가증분값";
                grpIntegration.Header = "외부 통신 엔드포인트 연동";
                lblWebhook.Text = "Discord 웹훅 리포트 전송 URL 주소";
                lblSdLabels.Text = "분류에 사용할 SD 카드 커스텀 레이블 태그명";

                lblPanelWatermark.Text = "사진 이미지의 EXIF 정보 메타데이터를 추출해 사진 상단/하단에 서명 및 촬영 스펙을 자동 인쇄합니다.";
                grpWmTarget.Header = "워터마크 처리 입출력 추적 제어";
                lblWmTargetFolder.Text = "워터마크 일괄 변환 처리 대상 폴더";
                chkWmOverwrite.Content = "원본 이미지 파일 위에 그대로 덮어쓰기 권한 허용";
                chkWmSameFolder.Content = "대상 파일과 동일한 경로에 결과 저장";
                lblWmOutputFolder.Text = "별도 분리 보관할 워터마크 출력 저장소";
                lblWmPrefix.Text = "워터마크 출력 파일 접두사 (wm_)";

                grpWmItems.Header = "인쇄할 EXIF 사진 메타데이터 필드 선택";
                lblWmItemsHint.Text = "카메라 및 파일 메타데이터에서 실제 감지된 정보만 선별 인쇄 연산에 포함됩니다.";
                chkWmOwner.Content = "카피라이트 크리에이터 소유자 서명 표기 (©)";
                chkWmCamera.Content = "바디 카메라 제조사 및 모델 기명";
                chkWmLens.Content = "사용 렌즈 서브 하드웨어 제원 인쇄";
                chkWmFocal.Content = "촬영 화각 초점 거리 정보 (mm)";
                chkWmAperture.Content = "렌즈 조리개 개방 수치 조절값 (f/)";
                chkWmShutter.Content = "셔터 스피드 노출 타이밍 연산값 (1/s)";
                chkWmIso.Content = "감도 ISO 스펙 필드";
                chkWmShadow.Content = "가독성 향상을 위한 글자 그림자 이중 레이어 음영 효과 적용";
                chkWmExif.Content = "EXIF 원본 메타데이터 보존 및 하이재킹 복사";

                grpWmStyle.Header = "텍스트 서명 디테일 및 렌더링 위치 정보";
                lblWmOwnerSignature.Text = "© 소유자 이니셜 서명 텍스트 값";
                lblWmPosition.Text = "이미지 상의 서명 정렬 포지션 위치";
                lblWmScaleLandscape.Text = "가로 사진 글꼴 비율 (%)";
                lblWmScalePortrait.Text = "세로 사진 글꼴 비율 (%)";

                lblWmFont.Text = "워터마크 글꼴 선택";
                lblWmColor.Text = "워터마크 색상 지정";
                lblPreviewTitle.Text = "워터마크 레이아웃 실시간 미리보기";
                lblPreviewLTitle.Text = "가로 사진 미리보기 (종횡비 4:3)";
                lblPreviewPTitle.Text = "세로 사진 미리보기 (종횡비 3:4)";

                grpLangSetting.Header = "글로벌 언어 로컬라이제이션 설정";
                lblLangSelect.Text = "데스크톱 애플리케이션의 기본 주 동작 언어 선택";
                chkStartMaximized.Content = "애플리케이션 시작 시 창을 최대화(전체 화면)로 기동";
                lblAppInfo.Text = "애플리케이션 모듈: AutoArchiveX Core Suite\nBuild Version: v1.4.2 Enterprise\nFramework Context: .NET Framework 4.5 규격 WPF 런타임 환경 소스";

                btnClearLog.Content = "콘솔 기록 초기화";
                btnOpBackup.Content = "📂 폴더 동기화";
                btnOpRename.Content = "📝 파일명 변경";
                btnOpWatermark.Content = "📷 워터마크 합성";
                btnOpClean.Content = "🧹 빈 폴더 정리";
                btnOpAll.Content = "🚀 전체 파이프라인 일괄 실행";
                chkShutdown.Content = "작업 완료 시 컴퓨터 전원 자동 종료";
                lblStatusText.Text = "대기 상태";
            }

            // 아카이브 분할 생성 규칙 콤보박스 다국어 동적 처리
            foreach (ComboBoxItem item in folderRuleSelect.Items)
            {
                if (item.Tag == null) continue;
                string tag = item.Tag.ToString();
                if (lang == "EN") {
                    if (tag == "exif") item.Content = "Split by EXIF Shooting Date";
                    if (tag == "today") item.Content = "Split by Today's Run Date";
                    if (tag == "custom") item.Content = "Fixed Custom Root Folder";
                } else if (lang == "JA") {
                    if (tag == "exif") item.Content = "EXIF撮影日に基づいて分割";
                    if (tag == "today") item.Content = "本日の実行日に基づいて分割";
                    if (tag == "custom") item.Content = "固定カスタムルートフォルダ";
                } else {
                    if (tag == "exif") item.Content = "EXIF 촬영 날짜 기준 분할 생성";
                    if (tag == "today") item.Content = "오늘 실행 날짜 기준 분할 생성";
                    if (tag == "custom") item.Content = "고정 커스텀 루트 폴더 사용";
                }
            }

            // 워터마크 배치 설정 콤보박스 다국어 동적 처리
            foreach (ComboBoxItem item in cmbWmPosition.Items)
            {
                if (item.Tag == null) continue;
                string tag = item.Tag.ToString();
                if (lang == "EN") {
                    if (tag == "bottomright") item.Content = "Bottom Right Corner";
                    if (tag == "bottomleft") item.Content = "Bottom Left Corner";
                    if (tag == "topright") item.Content = "Top Right Corner";
                    if (tag == "topleft") item.Content = "Top Left Corner";
                    if (tag == "pattern") item.Content = "Grid Matrix Pattern";
                } else if (lang == "JA") {
                    if (tag == "bottomright") item.Content = "右下";
                    if (tag == "bottomleft") item.Content = "左下";
                    if (tag == "topright") item.Content = "右上";
                    if (tag == "topleft") item.Content = "左上";
                    if (tag == "pattern") item.Content = "全面グリッドマトリクスパターン";
                } else {
                    if (tag == "bottomright") item.Content = "우측 하단";
                    if (tag == "bottomleft") item.Content = "좌측 하단";
                    if (tag == "topright") item.Content = "우측 상단";
                    if (tag == "topleft") item.Content = "좌측 상단";
                    if (tag == "pattern") item.Content = "사진 전면 격자 무늬 패턴";
                }
            }

            // 매핑 리스트 플레이스홀더 실시간 갱신
            foreach (Grid row in mappingListContainer.Children)
            {
                UpdatePlaceholdersOnRow(row);
            }

            // 개별 텍스트 박스 플레이스홀더 정의
            txtCustomFolder.SetPlaceholderText(currentLang == "KO" ? "예: Archive_2026" : "e.g. Archive_2026");
            txtRenameFmt.SetPlaceholderText(currentLang == "KO" ? "예: {yyyy}{MM}{dd}_{index}" : "e.g. {yyyy}{MM}{dd}_{index}");
            txtWmOwnerSignature.SetPlaceholderText("john_doe");
            txtSdLabels.SetPlaceholderText(currentLang == "KO" ? "예: BACKUP_SD, STORAGE_CARD" : "e.g. STORAGE_SD");
            txtWmTargetFolder.SetPlaceholderText(currentLang == "KO" ? "예: C:\\WatermarkInput" : "e.g. C:\\PhotoRoot");
            txtWmPrefix.SetPlaceholderText("wm_");
        }

        // 실시간 워터마크 미리보기 드로잉 엔진
        private void LiveUpdatePreviewCanvas()
        {
            if (previewLandscapeTextStack == null || previewPortraitTextStack == null || wmFontSelect == null || txtWmColorHex == null) return;

            previewLandscapeTextStack.Children.Clear();
            previewPortraitTextStack.Children.Clear();

            string selectedFont = "Segoe UI";
            if (wmFontSelect.SelectedItem != null) {
                selectedFont = wmFontSelect.SelectedItem.ToString();
            } else if (!string.IsNullOrEmpty(wmFontSelect.Text)) {
                selectedFont = wmFontSelect.Text;
            }

            Brush textBrush = Brushes.White;
            try {
                string hexColor = txtWmColorHex.Text.Trim();
                if (!string.IsNullOrEmpty(hexColor)) {
                    if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;
                    textBrush = (Brush)new BrushConverter().ConvertFromString(hexColor);
                }
            } catch {
                textBrush = Brushes.White;
            }

            System.Windows.Media.Effects.DropShadowEffect shadowEffectL = null;
            System.Windows.Media.Effects.DropShadowEffect shadowEffectP = null;
            if (chkWmShadow.IsChecked == true) {
                shadowEffectL = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Black, Direction = 315, ShadowDepth = 1.2, Opacity = 0.9, BlurRadius = 2 };
                shadowEffectP = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Black, Direction = 315, ShadowDepth = 1.2, Opacity = 0.9, BlurRadius = 2 };
            }

            ComboBoxItem posItem = cmbWmPosition.SelectedItem as ComboBoxItem;
            string pos = posItem != null && posItem.Tag != null ? posItem.Tag.ToString() : "bottomright";

            // Set alignment for Landscape Preview
            if (pos == "pattern") {
                previewLandscapeTextStack.VerticalAlignment = VerticalAlignment.Center;
                previewLandscapeTextStack.HorizontalAlignment = HorizontalAlignment.Center;
            } else if (pos == "topright" || pos == "topleft") {
                previewLandscapeTextStack.VerticalAlignment = VerticalAlignment.Top;
            } else {
                previewLandscapeTextStack.VerticalAlignment = VerticalAlignment.Bottom;
            }
            if (pos == "bottomleft" || pos == "topleft") {
                previewLandscapeTextStack.HorizontalAlignment = HorizontalAlignment.Left;
            } else if (pos != "pattern") {
                previewLandscapeTextStack.HorizontalAlignment = HorizontalAlignment.Right;
            }

            // Set alignment for Portrait Preview
            if (pos == "pattern") {
                previewPortraitTextStack.VerticalAlignment = VerticalAlignment.Center;
                previewPortraitTextStack.HorizontalAlignment = HorizontalAlignment.Center;
            } else if (pos == "topright" || pos == "topleft") {
                previewPortraitTextStack.VerticalAlignment = VerticalAlignment.Top;
            } else {
                previewPortraitTextStack.VerticalAlignment = VerticalAlignment.Bottom;
            }
            if (pos == "bottomleft" || pos == "topleft") {
                previewPortraitTextStack.HorizontalAlignment = HorizontalAlignment.Left;
            } else if (pos != "pattern") {
                previewPortraitTextStack.HorizontalAlignment = HorizontalAlignment.Right;
            }

            double landscapeScale = sldWmScaleLandscape != null ? sldWmScaleLandscape.Value : 1.6;
            double portraitScale = sldWmScalePortrait != null ? sldWmScalePortrait.Value : 1.6;

            // Calculate font sizes relative to the preview heights (240px)
            double fontSizeL = Math.Max(5, 240.0 * (landscapeScale / 100.0));
            double fontSizeP = Math.Max(5, 240.0 * (portraitScale / 100.0));

            List<string> mockLines = new List<string>();
            string signature = string.IsNullOrEmpty(txtWmOwnerSignature.Text) ? "standard_user" : txtWmOwnerSignature.Text;

            if (pos == "pattern") {
                mockLines.Add("©" + signature + " | SAMPLE");
                mockLines.Add("©" + signature + " | SAMPLE");
            } else {
                if (chkWmOwner.IsChecked == true) mockLines.Add("©" + signature);
                if (chkWmCamera.IsChecked == true || chkWmLens.IsChecked == true) {
                    string l1 = "";
                    if (chkWmCamera.IsChecked == true) l1 += "Standard Body";
                    if (chkWmLens.IsChecked == true) l1 += (l1 == "" ? "" : " | ") + "Prime Lens";
                    mockLines.Add(l1);
                }
                string l2 = "";
                if (chkWmFocal.IsChecked == true) l2 += "50mm ";
                if (chkWmAperture.IsChecked == true) l2 += "f/1.4 ";
                if (chkWmShutter.IsChecked == true) l2 += "1/250s ";
                if (chkWmIso.IsChecked == true) l2 += "ISO 100";
                if (!string.IsNullOrEmpty(l2.Trim())) mockLines.Add(l2.Trim());
            }

            // Render Landscape Preview
            if (pos == "pattern") {
                previewLandscapeTextStack.Opacity = 0.15;
                previewLandscapeTextStack.LayoutTransform = new RotateTransform(-30);
                previewLandscapeTextStack.Width = 700;
                previewLandscapeTextStack.Height = 700;
                previewLandscapeTextStack.VerticalAlignment = VerticalAlignment.Center;
                previewLandscapeTextStack.HorizontalAlignment = HorizontalAlignment.Center;
                for (int r = -6; r <= 6; r++) {
                    StackPanel rowPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                    if (r % 2 == 0) {
                        rowPanel.Margin = new Thickness(fontSizeL * 2.5, 3, 0, 3);
                    } else {
                        rowPanel.Margin = new Thickness(0, 3, 0, 3);
                    }
                    for (int c = 0; c < 6; c++) {
                        TextBlock tb = new TextBlock {
                            Text = "©" + signature + " | SAMPLE",
                            FontFamily = new FontFamily(selectedFont),
                            FontSize = fontSizeL * 0.75,
                            Foreground = textBrush,
                            Margin = new Thickness(14, 6, 14, 6),
                            Effect = shadowEffectL
                        };
                        rowPanel.Children.Add(tb);
                    }
                    previewLandscapeTextStack.Children.Add(rowPanel);
                }
            } else {
                previewLandscapeTextStack.Opacity = 1.0;
                previewLandscapeTextStack.LayoutTransform = null;
                previewLandscapeTextStack.Width = Double.NaN;
                previewLandscapeTextStack.Height = Double.NaN;
                foreach (var line in mockLines) {
                    TextBlock tb = new TextBlock { Text = line, FontFamily = new FontFamily(selectedFont), FontSize = fontSizeL, Foreground = textBrush, Margin = new Thickness(1), Effect = shadowEffectL };
                    previewLandscapeTextStack.Children.Add(tb);
                }
            }

            // Render Portrait Preview
            if (pos == "pattern") {
                previewPortraitTextStack.Opacity = 0.15;
                previewPortraitTextStack.LayoutTransform = new RotateTransform(-30);
                previewPortraitTextStack.Width = 600;
                previewPortraitTextStack.Height = 600;
                previewPortraitTextStack.VerticalAlignment = VerticalAlignment.Center;
                previewPortraitTextStack.HorizontalAlignment = HorizontalAlignment.Center;
                for (int r = -6; r <= 6; r++) {
                    StackPanel rowPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                    if (r % 2 == 0) {
                        rowPanel.Margin = new Thickness(fontSizeP * 2.5, 3, 0, 3);
                    } else {
                        rowPanel.Margin = new Thickness(0, 3, 0, 3);
                    }
                    for (int c = 0; c < 5; c++) {
                        TextBlock tb = new TextBlock {
                            Text = "©" + signature + " | SAMPLE",
                            FontFamily = new FontFamily(selectedFont),
                            FontSize = fontSizeP * 0.75,
                            Foreground = textBrush,
                            Margin = new Thickness(12, 6, 12, 6),
                            Effect = shadowEffectP
                        };
                        rowPanel.Children.Add(tb);
                    }
                    previewPortraitTextStack.Children.Add(rowPanel);
                }
            } else {
                previewPortraitTextStack.Opacity = 1.0;
                previewPortraitTextStack.LayoutTransform = null;
                previewPortraitTextStack.Width = Double.NaN;
                previewPortraitTextStack.Height = Double.NaN;
                foreach (var line in mockLines) {
                    TextBlock tb = new TextBlock { Text = line, FontFamily = new FontFamily(selectedFont), FontSize = fontSizeP, Foreground = textBrush, Margin = new Thickness(1), Effect = shadowEffectP };
                    previewPortraitTextStack.Children.Add(tb);
                }
            }
        }

        // 연결 드라이브 용량 실시간 모니터링 연산
        private void RefreshStorageVolumes()
        {
            try {
                this.Dispatcher.Invoke(() => {
                    storageListContainer.Children.Clear();
                    foreach (var drive in DriveInfo.GetDrives()) {
                        if (!drive.IsReady) continue;
                        
                        Grid dRow = new Grid { Margin = new Thickness(0, 5, 0, 5) };
                        dRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); 
                        dRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        dRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                        string diskLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? "Local Drive" : drive.VolumeLabel;
                        TextBlock txtLabel = new TextBlock { Text = string.Format("{0} ({1})", diskLabel, drive.Name.Replace("\\", "")), FontSize = 11, FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
                        
                        double total = drive.TotalSize; double free = drive.TotalFreeSpace;
                        double usedPercent = ((total - free) / total) * 100;

                        ProgressBar bar = new ProgressBar { Height = 8, Minimum = 0, Maximum = 100, Value = usedPercent, Background = new SolidColorBrush(Color.FromRgb(235, 237, 240)), BorderThickness = new Thickness(0), Margin = new Thickness(10, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
                        bar.Foreground = (usedPercent > 85) ? new SolidColorBrush(Color.FromRgb(209, 26, 42)) : new SolidColorBrush(Color.FromRgb(3, 102, 214));

                        TextBlock txtPct = new TextBlock { Text = string.Format("{0}% ({1}GB Free)", Math.Round(usedPercent), Math.Round(free / 1024 / 1024 / 1024)), FontSize = 10, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(88, 96, 105)) };

                        dRow.Children.Add(txtLabel); Grid.SetColumn(txtLabel, 0);
                        dRow.Children.Add(bar); Grid.SetColumn(bar, 1);
                        dRow.Children.Add(txtPct); Grid.SetColumn(txtPct, 2);
                        storageListContainer.Children.Add(dRow);
                    }
                });
            } catch {}
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnOpBackup.IsEnabled = enabled;
            btnOpRename.IsEnabled = enabled;
            btnOpWatermark.IsEnabled = enabled;
            btnOpClean.IsEnabled = enabled;
            btnOpAll.IsEnabled = enabled;
            langSelect.IsEnabled = enabled;
        }

        // 공통 백그라운드 태스크 러너
        private async void RunSingleTask(string jobName, Func<Action<string, bool>, int> taskFunc)
        {
            SetControlsEnabled(false);
            SwitchTab(4); // 콘솔 로그 탭으로 즉시 자동이동
            prgStatus.IsIndeterminate = true;
            SaveUIToConfig();

            await Task.Run(() =>
            {
                Action<string, bool> logCallback = (msg, isError) =>
                {
                    Dispatcher.Invoke(() => {
                        string prefix = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, isError ? "❌ [ERROR] " : "ℹ️ ");
                        txtLogConsole.AppendText(prefix + msg + Environment.NewLine);
                        txtLogConsole.ScrollToEnd();
                        lblStatusText.Text = msg;
                    });
                };

                try
                {
                    logCallback("==================================================", false);
                    logCallback(string.Format("AutoArchiveX Core Engine - {0} Stage Initializing...", jobName), false);
                    logCallback("==================================================", false);

                    int count = taskFunc(logCallback);
                    
                    string msgComplete = string.Format("{0} complete. Executed items count: {1}", jobName, count);
                    if (currentLang == "JA") msgComplete = string.Format("{0}が完了しました。実行結果: {1}件", jobName, count);
                    else if (currentLang == "KO") msgComplete = string.Format("{0} 작업이 완료되었습니다. 처리 대상 개수: {1}개", jobName, count);

                    Dispatcher.Invoke(() => {
                        prgStatus.IsIndeterminate = false;
                        prgStatus.Value = 100;
                        lblStatusText.Text = msgComplete;
                    });
                }
                catch (Exception ex)
                {
                    logCallback(string.Format("Critical failure: {0}", ex.Message), true);
                    Dispatcher.Invoke(() => {
                        prgStatus.IsIndeterminate = false;
                        prgStatus.Value = 0;
                        lblStatusText.Text = "System Fault Thrown";
                        MessageBox.Show("WPF Worker Thread Fault: " + ex.Message, "WPF Task Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });

            SetControlsEnabled(true);
        }

        // 전체 일괄 실행 파이프라인 제어 구조
        private async void BtnOpAll_Click(object sender, RoutedEventArgs e)
        {
            SetControlsEnabled(false);
            SwitchTab(4); 
            prgStatus.IsIndeterminate = true;
            SaveUIToConfig();

            bool shutdownChecked = chkShutdown.IsChecked ?? false;

            await Task.Run(() =>
            {
                Action<string, bool> logCallback = (msg, isError) =>
                {
                    Dispatcher.Invoke(() => {
                        string prefix = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, isError ? "❌ [ERROR] " : "ℹ️ ");
                        txtLogConsole.AppendText(prefix + msg + Environment.NewLine);
                        txtLogConsole.ScrollToEnd();
                        lblStatusText.Text = msg;
                    });
                };

                try
                {
                    logCallback("==================================================", false);
                    logCallback("AutoArchiveX Core Engine - Full Pipeline Sequential Booting...", false);
                    logCallback("==================================================", false);

                    logCallback("Step 1: Synchronizing files and organizing folders...", false);
                    int copiedFiles = AppLogic.CopyAndBackup(folderMappings, config.FolderRule, config.CustomFolderName, logCallback);
                    logCallback(string.Format("Organize Complete. {0} new source assets transferred safely.", copiedFiles), false);

                    int renamedFiles = 0;
                    if (!string.IsNullOrWhiteSpace(config.RenamePattern))
                    {
                        logCallback("Step 2: Triggering bulk file renaming batch pipeline...", false);
                        renamedFiles = AppLogic.RenameFiles(folderMappings, config.RenamePattern, logCallback);
                        logCallback(string.Format("Renaming Complete. {0} assets rebranded to structural patterns.", renamedFiles), false);
                    }
                    else
                    {
                        logCallback("Step 2: Renaming format pattern empty. Bypassing stage.", false);
                    }

                    logCallback("Step 3: Beginning EXIF rendering and watermark embossing process...", false);
                    int wmResult = AppLogic.ApplyWatermarks(folderMappings, config, logCallback);
                    if (wmResult == -1)
                    {
                        logCallback("Watermark Result: Target files matching format (.jpg/.jpeg) not discovered.", false);
                    }
                    else if (wmResult == -2)
                    {
                        logCallback("Watermark Result: Target path invalid or broken. Process aborted.", true);
                    }
                    else
                    {
                        logCallback(string.Format("Watermark Complete. {0} photographic images updated with metadata overlays.", wmResult), false);
                    }

                    logCallback("Step 4: Executing deep cleanup on redundant empty directories...", false);
                    int cleanedDirs = AppLogic.RemoveEmptyFolders(folderMappings, logCallback);
                    logCallback(string.Format("Cleanup Complete. {0} vacant paths expunged.", cleanedDirs), false);

                    if (!string.IsNullOrEmpty(config.WebhookUrl))
                    {
                        logCallback("Step 5: Broadcasting operation payload telemetry to Discord...", false);
                        string message = "";
                        if (currentLang == "EN") {
                            message = string.Format("🟢 **[AutoArchiveX Summary Report]**\n- Copied Files: {0} items\n- Watermarked Images: {1} items\n- Deleted Garbage Folders: {2} paths\nStatus: **All Safe & Secured.**", copiedFiles, wmResult >= 0 ? wmResult : 0, cleanedDirs);
                        } else if (currentLang == "JA") {
                            message = string.Format("🟢 **[AutoArchiveX サマリーレポート]**\n- コピーされたファイル: {0} 個\n- ウォーターマーク処理画像: {1} 個\n- 削除された空フォルダ: {2} 個\nステータス: **すべて安全に保護されました。**", copiedFiles, wmResult >= 0 ? wmResult : 0, cleanedDirs);
                        } else {
                            message = string.Format("🟢 **[AutoArchiveX 요약 리포트]**\n- 복사된 파일: {0} 개\n- 워터마크 처리 이미지: {1} 개\n- 삭제된 빈 폴더: {2} 개\n상태: **모두 안전하게 보관되었습니다.**", copiedFiles, wmResult >= 0 ? wmResult : 0, cleanedDirs);
                        }
                        AppLogic.SendDiscordNotification(config.WebhookUrl, message);
                    }

                    string msgComplete = "All data archiving pipelines completed successfully.";
                    if (currentLang == "JA") msgComplete = "すべてのデータアーカイブパイプラインが正常に完了しました。";
                    else if (currentLang == "KO") msgComplete = "모든 데이터 아카이빙 파이프라인이 성공적으로 완료되었습니다.";

                    Dispatcher.Invoke(() => {
                        prgStatus.IsIndeterminate = false;
                        prgStatus.Value = 100;
                        lblStatusText.Text = msgComplete;
                    });

                    if (shutdownChecked) {
                        logCallback("Initiating automatic system shutdown command in 60 seconds...", false);
                        Process.Start("shutdown", "/s /t 60");
                    }
                }
                catch (Exception ex)
                {
                    logCallback(string.Format("Critical system fault: {0}", ex.Message), true);
                    
                    string msgErrorOccurred = "A critical error occurred during the operation.";
                    string boxTitle = "Pipeline Exception";
                    if (currentLang == "JA") { msgErrorOccurred = "作業中に致命的なエラーが発生しました。"; boxTitle = "パイプライン例外"; }
                    else if (currentLang == "KO") { msgErrorOccurred = "작업 도중 치명적인 오류가 발생했습니다."; boxTitle = "파이프라인 예외"; }

                    Dispatcher.Invoke(() => {
                        prgStatus.IsIndeterminate = false;
                        prgStatus.Value = 0;
                        lblStatusText.Text = msgErrorOccurred;
                        MessageBox.Show("Engine Fault: " + ex.Message, boxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });

            SetControlsEnabled(true);
        }

        private void InitializeGuideTranslations()
        {
            // ==========================================
            // ENGLISH GUIDE
            // ==========================================
            var en = new Dictionary<string, string>();
            en["m_tab_0_title"] = "THEME: BLUE ➔ FOLDER MAPPINGS TAB & STORAGE STATUS";
            en["m_tab_0_body"] = "■ PURPOSE:\nThis view handles directory bindings and mounted drive usage information.\n\n■ KEY CONTROLS & DESCRIPTIONS:\n• [Add New Sync Folder Mapping Button]: Appends a new synchronization path pair (Source to Destination) configuration group.\n• [Delete (✕ Button)]: Instantly deletes the targeted mapping row from the active workspace list.\n• [... (Browse Buttons)]: Spawns Wpf dialog to select source/destination directories easily.\n• [Drive Storage Monitor]: Real-time space usage indicators. Refreshes every 3 seconds. Warnings color changes to Crimson Red if usage passes 85% limits.";
            
            en["m_tab_1_title"] = "THEME: GREEN ➔ RULES & SYNC TAB OPTIONS";
            en["m_tab_1_body"] = "■ PURPOSE:\nConfigures automated chronological tree splitting logic and dynamic mass file renaming templates.\n\n■ ACTION FIELDS:\n• [Archive Directory Tree Logic]: Configures automatic folders sorting structures (e.g. EXIF Date tree, Today's Date, or Static Custom Folder).\n• [Renaming Template Pattern]: Bulk rename C# string engine supporting tokens like {yyyy}, {MM}, {dd}, {HH}, {mm}, {ss}, and {index}.\n• [Discord Webhook Endpoint]: Web communication post logs to Discord Server Channel.";
            
            en["m_tab_2_title"] = "THEME: ORANGE ➔ WATERMARK TOOL OPTIONS";
            en["m_tab_2_body"] = "■ PURPOSE:\nControls EXIF metadata overlays layout and image stamps scaling parameters.\n\n■ FUNCTION KEYWORDS:\n• [Landscape / Portrait Ratio Customizer]: Adjust font proportion (%) dynamically based on photo dimensions.\n• [WYSIWYG Live Previews (Double Canvases)]: Simulated double viewports (Landscape 4:3, Portrait 3:4) showing watermark size, color, typography, shadow and alignment coordinates changes in real-time.\n• [EXIF Checkboxes]: Choose copyright ownership, camera models, lenses specs, focal lengths, aperture stops, shutter exposures, or ISO to imprint on the raw pixels.";
            
            en["m_tab_3_title"] = "THEME: PURPLE ➔ GLOBAL LANGUAGE & SETTINGS TAB";
            en["m_tab_3_body"] = "■ PURPOSE:\nGlobal client environment configurations.\n\n■ KEY COMPONENTS:\n• [Language Selector]: Instantly translates UI layouts, labels, guides, and textboxes placeholders across English, Japanese, and Korean modes.";
            
            en["m_tab_4_title"] = "THEME: GRAY ➔ LIVE PROCESS CONSOLE LOGS TAB";
            en["m_tab_4_body"] = "■ PURPOSE:\nLive monitoring of system operations in real-time.\n\n■ CONTROLS:\n• [Clear Console Text Button]: Clears logs cache stream on terminal screen.\n\n■ GLOBAL PIPELINE BUTTONS (BOTTOM TOOLBAR):\n• [Sync Folders]: Differential backup evaluating modified timestamp and file size.\n• [Rename Files]: Updates file identifiers in bulk according to the custom naming pattern.\n• [Add Watermark]: Extracts structural metadata headers, scales font, and embosses overlays.\n• [Prune Folders]: Safely cleans empty folder paths.\n• [RUN ALL PIPELINE]: Executes all operations in sequence. Check the auto-shutdown option to automatically shut down the PC 60 seconds after completion.";
            i18nGuide["en"] = en;

            // ==========================================
            // JAPANESE GUIDE (ja) - 크래시 해결을 위한 키 수정
            // ==========================================
            var jp = new Dictionary<string, string>();
            jp["m_tab_0_title"] = "テーマ色: ブルー ➔ フォルダマッピングおよび接続ストレージ";
            jp["m_tab_0_body"] = "■ 目的:\n同期先フォルダペアの構築と、接続されているドライブの残容量ステータスを監視します。\n\n■ 主要ボタンとコントロール機能:\n• [新しい同期フォルダマッピングを追加]: 新しい同期元（From）と同期先（To）パスを設定するための行フォームを追加します。\n• [✕ (削除ボタン)]: 対象のフォルダマッピング設定をリアルタイムで削除します。\n• [... (参照ボタン)]: ディレクトリのブラウズダイアログを開き、直感的にフォルダパスを選択できます。\n• [ストレージステータスモニター]: ドライブ空き容量を3秒間隔で取得し、使用率が85%を超えると Crimson Red (深紅) の警告色に自動で変化します。";
            
            jp["m_tab_1_title"] = "テーマ色: グリーン ➔ ルールと同期設定";
            jp["m_tab_1_body"] = "■ 目的:\nフォルダ自動仕分け規則およびファイル名一括変換テンプレートを構成します。\n\n■ 制御設定:\n• [フォルダ分割生成アルゴリズム]: 写真の撮影日(EXIF)、今日の日付、または任意のユーザー定義固定フォルダを指定して自動ツリー分類を行います。\n• [ネーミング変更テンプレート]: {yyyy}、{MM}、{index} などの予約トークンをパースし、大量のファイルを一括改名します。\n• [Discordウェブフック]: 処理結果の要約を Discord チャンネルに POST 送信する通信設定です。";
            
            jp["m_tab_2_title"] = "テーマ色: オレンジ ➔ ウォーターマーク設定 & ダブルプレビュー";
            jp["m_tab_2_body"] = "■ 目的:\n写真のEXIF情報取得および透かしインプリント文字のカスタマイズを実行します。\n\n■ コントロール概要:\n• [横/縦写真のフォント縮尺比率]: 画像の方向（横向き/縦向き）ごとに、透かし文字のサイズ比率(%)を個別に設定できます。\n• [リアルタイムダブルプレビュー]: 横長(4:3)と縦長(3:4)の2つのプレビュー窓で、文字サイズや位置、色、影の効果を実時間でシミュレート表示します。\n• [EXIFメタデータ選択]: 著作権、ボディ名、レンズスペック、絞り値、ISOなどを選択して画像に合成します。";
            
            jp["m_tab_3_title"] = "テーマ色: パープル ➔ 言語と一般設定";
            jp["m_tab_3_body"] = "■ 目的:\nグローバルアプリケーション操作環境を設定します。\n\n■ 設定項目:\n• [主要言語の選択]: UI翻訳（英語/日本語/韓国語）、案内ガイダンス、入力ヒントを該当言語へリアルタイムに変更します。";
            
            jp["m_tab_4_title"] = "テーマ色: グレー ➔ 実行ログコンソール";
            jp["m_tab_4_body"] = "■ 目的:\n各処理パイプラインの動作状態を CLI ログストリーム形式でリアルタイムに追跡します。\n\n■ ボタン機能:\n• [コンソール履歴のクリア]: 表示ログ画面をクリーンアップします。\n\n■ 下部ツールバー (一括＆個別コマンド実行ボタン):\n• [フォルダ同期]: 日付・サイズ比較による知能型差分コピーループを実行します。\n• [ファイル名変更]: テンプレートをベースに変更を適用します。\n• [透かし合成]: EXIF情報をコピーし、比率調整された透かし文字を描画保存します。\n• [空フォルダ削除]: 空となった下位ディレクトリを安全に刈り取ります。\n• [全自動実行パイプライン]: 全工程をシーケンシャルに一括実行します。シャットダウンをチェックすると、処理完了60秒後にPCを自動終了します。";
            i18nGuide["ja"] = jp;

            // ==========================================
            // KOREAN GUIDE (ko)
            // ==========================================
            var ko = new Dictionary<string, string>();
            ko["m_tab_0_title"] = "테마 색상: 블루 ➔ 📁 폴더 매핑 탭 및 드라이브 상태";
            ko["m_tab_0_body"] = "■ 목적:\n미디어 백업 소스와 대상 드라이브 매핑을 관리하며, 장착된 저장 장치의 상태를 추적합니다.\n\n■ 주요 컨트롤 및 설명:\n• [새 동기화 폴더 매핑 추가 버튼]: 동기화 대상 원본 경로와 백업 저장소 경로 쌍을 설정하는 입력 폼을 추가합니다.\n• [✕ (제거 버튼)]: 매핑 리스트에서 해당하는 설정 행을 즉시 삭제합니다.\n• [... (경로 찾기 버튼)]: 운영체제 디렉터리 브라우저 창을 띄워 마우스 클릭으로 경로를 쉽게 지정합니다.\n• [연결된 저장 장치 실시간 상태]: 장착된 드라이브의 볼륨명과 용량을 3초 간격으로 스캔하여 게이지로 보여줍니다. 사용률 85% 초과 드라이브는 진한 적색(Crimson Red) 경고 바색으로 변경되어 가용 공간 부족을 즉각 인지할 수 있습니다.";
            
            ko["m_tab_1_title"] = "테마 색상: 그린 ➔ ⚙️ 규칙 및 동기화 설정 탭";
            ko["m_tab_1_body"] = "■ 목적:\n자동 아카이빙 하위 폴더 분류 형식 및 파일명 일괄 변경 정규식을 정의합니다.\n\n■ 구성 설명:\n• [아카이브 트리 구조 생성 알고리즘]: 촬영일(EXIF), 당일 날짜, 커스텀 명칭 및 구조 없음 등 데이터 분류 폴더 생성 방식을 택합니다.\n• [네이밍 변경 정규 템플릿]: {yyyy}, {MM}, {index} 등의 매개변수 토큰을 파싱해 대량 파일의 이름을 규칙 기반으로 일괄 재작성합니다.\n• [Discord 웹훅]: 작업 흐름이 성공 완료될 때 메타데이터 페이로드를 채널 웹훅으로 전송하여 원격 로그를 수집합니다.";
            
            ko["m_tab_2_title"] = "테마 색상: 오렌지 ➔ 🎨 워터마크 마킹 탭 & 가로/세로 더블 프리뷰";
            ko["m_tab_2_body"] = "■ 목적:\n이미지 상의 서명 스타일 디자인 및 EXIF 메타데이터 합성 옵션을 정밀 제어합니다.\n\n■ 주요 제어 및 연동 로직 명세:\n• [가로/세로 사진 글꼴 비율 (%)]: 사진 종횡 비율(가로사진 / 세로사진)에 따라 인쇄할 워터마크 글꼴 크기 비율을 독립적으로 정밀 설정할 수 있습니다.\n• [실시간 레이아웃 미리보기 (더블 캔버스)]: 가로형(4:3 비율)과 세로형(3:4 비율) 시뮬레이터 캔버스 2개가 가로로 나란히 배치되어 폰트 종류, 색상, 배치 포지션, 섀도우 음영 설정이 적용된 형태를 실시간 가이드라인으로 그려 줍니다.\n• [EXIF 체크박스]: 저작권 기호(©), 카메라 모델, 사용 렌즈 사양, 초점거리, 조리개값, 노출값, ISO 정보를 사진 픽셀 매트릭스 레이어 상에 드로잉 합성하도록 선택합니다.";
            
            ko["m_tab_3_title"] = "테마 색상: 퍼플 ➔ 🌐 언어 및 환경 설정 탭";
            ko["m_tab_3_body"] = "■ 목적:\n글로벌 데스크톱 애플리케이션의 공통 운영 환경을 설정합니다.\n\n■ 설정 옵션:\n• [기본 동작 언어 선택]: 전체 UI 언어(영어/일본어/한국어), 플레이스홀더 텍스트 힌트를 지정한 국가 코드로 변경합니다.";
            
            ko["m_tab_4_title"] = "테마 색상: 그레이 ➔ 🖥️ 실행 콘솔 로그 탭";
            ko["m_tab_4_body"] = "■ 목적:\n각 백그라운드 태스크의 세부 트랜잭션 흐름을 모니터링합니다.\n\n■ 관련 기능:\n• [콘솔 기록 초기화 버튼]: 화면에 누적된 과거 콘솔 로그 문자열 캐시를 지웁니다.\n\n■ 하단 제어 툴바 영역 (태스크 단독 실행 및 전체 일괄 자동화):\n• [📂 Sync (폴더 동기화)]: 용량 및 수정 타임 스탬프를 대조하는 지능형 차분 백업을 기동합니다.\n• [📝 Rename (파일명 변경)]: 설정된 파일 규칙 템플릿에 따라 파일 이름을 재작성합니다.\n• [📷 Watermark (워터마크)]: EXIF 태그 정보를 해독하고 이미지에 워터마크를 각인하여 저장합니다.\n• [🧹 Clean (빈 폴더 정리)]: 실유실 파일 없이 트리 구조 내에 방치된 빈 디렉터리 경로만 전정합니다.\n• [🚀 Run All (전체 실행)]: 위의 4단계 공정을 일괄 배치 파이프라인으로 순차 가동합니다. '자동 종료' 체크 시 작업 완료 60초 후 PC 전원을 종료합니다.";
            i18nGuide["ko"] = ko;
        }

        private void ShowUserGuide()
        {
            string currentGuideLang = currentLang.ToLower();
            // 일본어 'ja' 대응 및 방어 코드
            if (currentGuideLang == "ja" && !i18nGuide.ContainsKey("ja") && i18nGuide.ContainsKey("jp"))
            {
                currentGuideLang = "jp";
            }
            if (!i18nGuide.ContainsKey(currentGuideLang))
            {
                currentGuideLang = "en";
            }

            var dict = i18nGuide[currentGuideLang];

            Window guideWindow = new Window {
                Title = currentLang == "KO" ? "정밀 사양 설명 가이드" : (currentLang == "JA" ? "精密仕様説明ガイド" : "Interactive Product Specification Manual"),
                Width = 840, Height = 640, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = new SolidColorBrush(Color.FromRgb(240, 242, 245))
            };

            Grid mainGrid = new Grid { Margin = new Thickness(16) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid layoutGrid = new Grid();
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.Children.Add(layoutGrid);

            Border menuBorder = new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(220, 224, 230)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(8), Margin = new Thickness(0, 0, 14, 0) };
            StackPanel menuStack = new StackPanel();
            menuBorder.Child = menuStack;
            layoutGrid.Children.Add(menuBorder);
            Grid.SetColumn(menuBorder, 0);

            Border contentBorder = new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(220, 224, 230)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(20) };
            ScrollViewer scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel contentStack = new StackPanel();
            scroll.Content = contentStack;
            contentBorder.Child = scroll;
            layoutGrid.Children.Add(contentBorder);
            Grid.SetColumn(contentBorder, 1);

            menuStack.Children.Add(new TextBlock { Text = "SYSTEM MANUAL", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(106, 115, 125)), Margin = new Thickness(6, 4, 4, 14) });

            // 다국어 가이드 탭 타이틀 로컬라이징 구성
            string[] tabKeys = new string[5];
            if (currentLang == "KO")
            {
                tabKeys[0] = "📁 폴더 매핑";
                tabKeys[1] = "⚙️ 규칙 및 동기화";
                tabKeys[2] = "🎨 워터마크 마킹";
                tabKeys[3] = "🌐 언어 및 설정";
                tabKeys[4] = "🖥️ 실행 콘솔 로그";
            }
            else if (currentLang == "JA")
            {
                tabKeys[0] = "📁 フォルダマッピング";
                tabKeys[1] = "⚙️ ルールと同期";
                tabKeys[2] = "🎨 ウォーターマーク";
                tabKeys[3] = "🌐 言語と一般設定";
                tabKeys[4] = "🖥️ 実行ログコンソール";
            }
            else
            {
                tabKeys[0] = "📁 Folder Mappings";
                tabKeys[1] = "⚙️ Rules & Sync";
                tabKeys[2] = "🎨 Watermark Tool";
                tabKeys[3] = "🌐 Lang & Settings";
                tabKeys[4] = "🖥️ Console Logs";
            }

            Color[] tabColors = new Color[] { Color.FromRgb(3, 102, 214), Color.FromRgb(46, 164, 79), Color.FromRgb(209, 76, 12), Color.FromRgb(140, 100, 200), Color.FromRgb(106, 115, 125) };
            List<Button> tabButtons = new List<Button>();

            Action<int> selectTab = null;
            selectTab = new Action<int>((index) => {
                Color themeColor = tabColors[index];
                for (int i = 0; i < tabButtons.Count; i++) {
                    bool isSelected = (i == index);
                    tabButtons[i].Background = isSelected ? new SolidColorBrush(themeColor) : new SolidColorBrush(Color.FromRgb(246, 248, 250));
                    tabButtons[i].Foreground = isSelected ? Brushes.White : new SolidColorBrush(Color.FromRgb(36, 41, 47));
                }
                contentStack.Children.Clear();
                
                string titleKey = string.Format("m_tab_{0}_title", index);
                string bodyKey = string.Format("m_tab_{0}_body", index);

                AddGuideSection(contentStack, dict[titleKey], dict[bodyKey], themeColor);
            });

            for (int i = 0; i < tabKeys.Length; i++) {
                int tabIdx = i;
                Button tabBtn = new Button { Content = tabKeys[i], Height = 36, Margin = new Thickness(0, 0, 0, 8), HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(12, 0, 12, 0), BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, FontSize = 11, FontWeight = FontWeights.SemiBold };
                tabBtn.Click += (s, e) => selectTab(tabIdx);
                menuStack.Children.Add(tabBtn);
                tabButtons.Add(tabBtn);
            }

            selectTab(0);

            Button closeBtn = new Button { Content = currentLang == "KO" ? "닫기" : (currentLang == "JA" ? "閉じる" : "Close"), Width = 84, Height = 30, Margin = new Thickness(0, 14, 0, 0), HorizontalAlignment = HorizontalAlignment.Right, Background = new SolidColorBrush(Color.FromRgb(36, 41, 47)), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand };
            closeBtn.Click += (s, ev) => guideWindow.Close();
            mainGrid.Children.Add(closeBtn);
            Grid.SetRow(closeBtn, 1);

            guideWindow.Content = mainGrid;
            guideWindow.ShowDialog();
        }

        private void AddGuideSection(StackPanel panel, string title, string text, Color sideColor)
        {
            Grid headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 16) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border leftBar = new Border { Background = new SolidColorBrush(sideColor), CornerRadius = new CornerRadius(3) };
            TextBlock txtTitle = new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(sideColor), Margin = new Thickness(12, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };

            headerGrid.Children.Add(leftBar); Grid.SetColumn(leftBar, 0);
            headerGrid.Children.Add(txtTitle); Grid.SetColumn(txtTitle, 1);
            panel.Children.Add(headerGrid);

            panel.Children.Add(new TextBlock { Text = text, FontSize = 11, LineHeight = 18, Foreground = new SolidColorBrush(Color.FromRgb(36, 41, 47)), TextWrapping = TextWrapping.Wrap });
        }
    }

    public static class TextBoxHelper
    {
        private static Dictionary<TextBox, string> placeholders = new Dictionary<TextBox, string>();
        
        public static void SetPlaceholderText(this TextBox box, string value)
        {
            string oldVal = placeholders.ContainsKey(box) ? placeholders[box] : null;
            placeholders[box] = value;
            if (string.IsNullOrEmpty(box.Text) || box.Foreground == Brushes.Gray || (oldVal != null && box.Text == oldVal)) {
                box.Text = value; box.Foreground = Brushes.Gray; box.FontStyle = FontStyles.Italic;
            }
            box.GotFocus -= Box_GotFocus; box.LostFocus -= Box_LostFocus;
            box.GotFocus += Box_GotFocus; box.LostFocus += Box_LostFocus;
        }

        private static void Box_GotFocus(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            if (box != null && placeholders.ContainsKey(box) && box.Text == placeholders[box]) {
                box.Text = ""; box.Foreground = Brushes.Black; box.FontStyle = FontStyles.Normal;
            }
        }

        private static void Box_LostFocus(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            if (box != null && placeholders.ContainsKey(box) && string.IsNullOrWhiteSpace(box.Text)) {
                box.Text = placeholders[box]; box.Foreground = Brushes.Gray; box.FontStyle = FontStyles.Italic;
            }
        }
    }
}