using UnityEngine;
using MemoryFracture.UI;
using MemoryFracture.Puzzles;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 게임 오브젝트들을 자동으로 생성하는 빌더
    /// </summary>
    public class GameBuilder : MonoBehaviour
    {
        [Header("Game Objects")]
        public GameObject playerPrefab;
        public GameObject mirrorPrefab;
        public GameObject lightSourcePrefab;
        public GameObject targetPrefab;
        public GameObject wallPrefab;
        public GameObject floorPrefab;
        
        [Header("UI Prefabs")]
        public GameObject mainMenuPrefab;
        public GameObject gameHUDPrefab;
        public GameObject pauseMenuPrefab;
        
        [Header("Materials")]
        public Material mirrorMaterial;
        public Material wallMaterial;
        public Material floorMaterial;
        public Material playerMaterial;
        
        private void Start()
        {
            BuildCompleteGame();
        }
        
        /// <summary>
        /// 완전한 게임 환경 구축
        /// </summary>
        public void BuildCompleteGame()
        {
            Debug.Log("Memory Fracture 게임 구축 시작...");
            
            // 1. 게임 매니저들 생성
            CreateGameManagers();
            
            // 2. 챕터 1 환경 구축
            BuildChapter1Environment();
            
            // 3. UI 시스템 구축
            BuildUISystem();
            
            // 4. 플레이어 생성
            CreatePlayer();
            
            // 5. 퍼즐 배치
            SetupPuzzles();
            
            Debug.Log("Memory Fracture 게임 구축 완료!");
        }
        
        /// <summary>
        /// 게임 매니저들 생성
        /// </summary>
        private void CreateGameManagers()
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
                DontDestroyOnLoad(gameManagerObj);
            }
            
            // NetworkManager
            if (NetworkManager.Instance == null)
            {
                GameObject networkManagerObj = new GameObject("NetworkManager");
                networkManagerObj.AddComponent<NetworkManager>();
                DontDestroyOnLoad(networkManagerObj);
            }
            
            // UIManager
            if (UIManager.Instance == null)
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManagerObj.AddComponent<UIManager>();
                DontDestroyOnLoad(uiManagerObj);
            }
            
            // GameSettings
            if (GameSettings.Instance == null)
            {
                GameObject gameSettingsObj = new GameObject("GameSettings");
                gameSettingsObj.AddComponent<GameSettings>();
                DontDestroyOnLoad(gameSettingsObj);
            }
        }
        
        /// <summary>
        /// 챕터 1 환경 구축 (거울 복도)
        /// </summary>
        private void BuildChapter1Environment()
        {
            // 바닥 생성
            CreateFloor();
            
            // 벽들 생성
            CreateWalls();
            
            // 조명 설정
            SetupLighting();
            
            // 환경 오브젝트들
            CreateEnvironmentObjects();
        }
        
        /// <summary>
        /// 바닥 생성
        /// </summary>
        private void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(10, 1, 10);
            
            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }
            else
            {
                // 기본 재질 생성
                Material defaultFloorMaterial = new Material(Shader.Find("Standard"));
                defaultFloorMaterial.color = new Color(0.3f, 0.3f, 0.3f);
                floor.GetComponent<Renderer>().material = defaultFloorMaterial;
            }
        }
        
        /// <summary>
        /// 벽들 생성
        /// </summary>
        private void CreateWalls()
        {
            // 외벽들
            CreateWall("Wall_North", new Vector3(0, 2, 5), new Vector3(20, 4, 0.2f));
            CreateWall("Wall_South", new Vector3(0, 2, -5), new Vector3(20, 4, 0.2f));
            CreateWall("Wall_East", new Vector3(10, 2, 0), new Vector3(0.2f, 4, 10));
            CreateWall("Wall_West", new Vector3(-10, 2, 0), new Vector3(0.2f, 4, 10));
            
            // 내부 벽들 (거울 복도 구조)
            CreateWall("Wall_Internal_1", new Vector3(-3, 2, 0), new Vector3(0.2f, 4, 8));
            CreateWall("Wall_Internal_2", new Vector3(3, 2, 0), new Vector3(0.2f, 4, 8));
            CreateWall("Wall_Internal_3", new Vector3(0, 2, -2), new Vector3(6, 4, 0.2f));
        }
        
        /// <summary>
        /// 개별 벽 생성
        /// </summary>
        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            
            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
            else
            {
                // 기본 재질 생성
                Material defaultWallMaterial = new Material(Shader.Find("Standard"));
                defaultWallMaterial.color = new Color(0.5f, 0.5f, 0.5f);
                wall.GetComponent<Renderer>().material = defaultWallMaterial;
            }
        }
        
        /// <summary>
        /// 조명 설정
        /// </summary>
        private void SetupLighting()
        {
            // 메인 조명
            GameObject mainLight = new GameObject("MainLight");
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.8f;
            light.color = Color.white;
            mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            // 환경 조명
            GameObject ambientLight = new GameObject("AmbientLight");
            Light ambient = ambientLight.AddComponent<Light>();
            ambient.type = LightType.Point;
            ambient.intensity = 0.3f;
            ambient.range = 20f;
            ambient.color = new Color(0.2f, 0.3f, 0.5f);
            ambientLight.transform.position = new Vector3(0, 5, 0);
        }
        
        /// <summary>
        /// 환경 오브젝트들 생성
        /// </summary>
        private void CreateEnvironmentObjects()
        {
            // 거울 복도의 분위기를 위한 오브젝트들
            CreateMirrorFrame(new Vector3(-5, 1.5f, 0), Quaternion.Euler(0, 45, 0));
            CreateMirrorFrame(new Vector3(5, 1.5f, 0), Quaternion.Euler(0, -45, 0));
            
            // 장식용 오브젝트들
            CreateDecorationObject("Decoration_1", new Vector3(-7, 0.5f, 3));
            CreateDecorationObject("Decoration_2", new Vector3(7, 0.5f, -3));
        }
        
        /// <summary>
        /// 거울 프레임 생성
        /// </summary>
        private void CreateMirrorFrame(Vector3 position, Quaternion rotation)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "MirrorFrame";
            frame.transform.position = position;
            frame.transform.rotation = rotation;
            frame.transform.localScale = new Vector3(0.1f, 2, 1.5f);
            
            // 거울 재질
            if (mirrorMaterial != null)
            {
                frame.GetComponent<Renderer>().material = mirrorMaterial;
            }
            else
            {
                Material mirrorMat = new Material(Shader.Find("Standard"));
                mirrorMat.color = new Color(0.8f, 0.8f, 1f);
                mirrorMat.SetFloat("_Metallic", 1f);
                mirrorMat.SetFloat("_Smoothness", 0.9f);
                frame.GetComponent<Renderer>().material = mirrorMat;
            }
        }
        
        /// <summary>
        /// 장식 오브젝트 생성
        /// </summary>
        private void CreateDecorationObject(string name, Vector3 position)
        {
            GameObject decoration = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            decoration.name = name;
            decoration.transform.position = position;
            decoration.transform.localScale = new Vector3(0.5f, 1, 0.5f);
            
            Material decoMaterial = new Material(Shader.Find("Standard"));
            decoMaterial.color = new Color(0.6f, 0.4f, 0.2f);
            decoration.GetComponent<Renderer>().material = decoMaterial;
        }
        
        /// <summary>
        /// UI 시스템 구축
        /// </summary>
        private void BuildUISystem()
        {
            // Canvas 생성
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 메인 메뉴 UI
            CreateMainMenuUI(canvas);
            
            // 게임 HUD
            CreateGameHUD(canvas);
            
            // 일시정지 메뉴
            CreatePauseMenu(canvas);
        }
        
        /// <summary>
        /// 메인 메뉴 UI 생성
        /// </summary>
        private void CreateMainMenuUI(GameObject canvas)
        {
            GameObject mainMenu = new GameObject("MainMenu");
            mainMenu.transform.SetParent(canvas.transform, false);
            
            // 배경 패널
            GameObject background = CreateUIPanel(mainMenu, "Background", new Vector2(0, 0), new Vector2(1, 1));
            UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
            
            // 제목
            CreateUIText(mainMenu, "Title", "Memory Fracture", new Vector2(0, 0.3f), new Vector2(0.8f, 0.2f), 48);
            
            // 버튼들
            CreateUIButton(mainMenu, "StartButton", "게임 시작", new Vector2(0, 0), new Vector2(0.3f, 0.1f));
            CreateUIButton(mainMenu, "MultiplayerButton", "멀티플레이어", new Vector2(0, -0.15f), new Vector2(0.3f, 0.1f));
            CreateUIButton(mainMenu, "SettingsButton", "설정", new Vector2(0, -0.3f), new Vector2(0.3f, 0.1f));
            CreateUIButton(mainMenu, "QuitButton", "종료", new Vector2(0, -0.45f), new Vector2(0.3f, 0.1f));
        }
        
        /// <summary>
        /// 게임 HUD 생성
        /// </summary>
        private void CreateGameHUD(GameObject canvas)
        {
            GameObject hud = new GameObject("GameHUD");
            hud.transform.SetParent(canvas.transform, false);
            
            // 게임 시간
            CreateUIText(hud, "GameTime", "00:00:00", new Vector2(-0.8f, 0.9f), new Vector2(0.2f, 0.1f), 24);
            
            // 챕터 정보
            CreateUIText(hud, "ChapterInfo", "Chapter 1/5", new Vector2(0.8f, 0.9f), new Vector2(0.2f, 0.1f), 24);
            
            // 플래그 슬라이더들
            CreateFlagSliders(hud);
        }
        
        /// <summary>
        /// 플래그 슬라이더들 생성
        /// </summary>
        private void CreateFlagSliders(GameObject parent)
        {
            string[] flagNames = { "진실", "망각", "희생", "손상", "추적자", "협동" };
            Color[] flagColors = { Color.white, Color.gray, Color.red, Color.black, Color.yellow, Color.green };
            
            for (int i = 0; i < flagNames.Length; i++)
            {
                Vector2 position = new Vector2(-0.9f, 0.7f - i * 0.1f);
                CreateFlagSlider(parent, flagNames[i], position, flagColors[i]);
            }
        }
        
        /// <summary>
        /// 개별 플래그 슬라이더 생성
        /// </summary>
        private void CreateFlagSlider(GameObject parent, string flagName, Vector2 position, Color color)
        {
            GameObject sliderObj = new GameObject($"FlagSlider_{flagName}");
            sliderObj.transform.SetParent(parent.transform, false);
            
            // 슬라이더
            UnityEngine.UI.Slider slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.anchorMin = position;
            sliderRect.anchorMax = position + new Vector2(0.3f, 0.05f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            
            // 배경
            GameObject background = CreateUIPanel(sliderObj, "Background", Vector2.zero, Vector2.one);
            UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = Color.black;
            
            // 채우기 영역
            GameObject fillArea = CreateUIPanel(sliderObj, "FillArea", Vector2.zero, Vector2.one);
            UnityEngine.UI.Image fillImage = fillArea.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = color;
            
            // 라벨
            CreateUIText(sliderObj, "Label", flagName, new Vector2(-0.1f, 0), new Vector2(0.08f, 1f), 12);
        }
        
        /// <summary>
        /// 일시정지 메뉴 생성
        /// </summary>
        private void CreatePauseMenu(GameObject canvas)
        {
            GameObject pauseMenu = new GameObject("PauseMenu");
            pauseMenu.transform.SetParent(canvas.transform, false);
            pauseMenu.SetActive(false);
            
            // 배경
            GameObject background = CreateUIPanel(pauseMenu, "Background", new Vector2(0, 0), new Vector2(1, 1));
            UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);
            
            // 메뉴 패널
            GameObject menuPanel = CreateUIPanel(pauseMenu, "MenuPanel", new Vector2(0.3f, 0.3f), new Vector2(0.4f, 0.4f));
            UnityEngine.UI.Image panelImage = menuPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // 버튼들
            CreateUIButton(menuPanel, "ResumeButton", "계속하기", new Vector2(0, 0.1f), new Vector2(0.6f, 0.1f));
            CreateUIButton(menuPanel, "SettingsButton", "설정", new Vector2(0, -0.05f), new Vector2(0.6f, 0.1f));
            CreateUIButton(menuPanel, "MainMenuButton", "메인 메뉴", new Vector2(0, -0.2f), new Vector2(0.6f, 0.1f));
        }
        
        /// <summary>
        /// UI 패널 생성
        /// </summary>
        private GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            return panel;
        }
        
        /// <summary>
        /// UI 텍스트 생성
        /// </summary>
        private void CreateUIText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = Color.white;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        }
        
        /// <summary>
        /// UI 버튼 생성
        /// </summary>
        private void CreateUIButton(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMin + anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f);
            
            // 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 18;
            tmpText.color = Color.white;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        }
        
        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0, 1, -4);
            
            // 플레이어 컨트롤러 추가
            player.AddComponent<CharacterController>();
            PlayerController controller = player.AddComponent<PlayerController>();
            controller.SetPlayerId("PlayerA");
            controller.SetLocalPlayer(true);
            
            // 플레이어 재질
            if (playerMaterial != null)
            {
                player.GetComponent<Renderer>().material = playerMaterial;
            }
            else
            {
                Material playerMat = new Material(Shader.Find("Standard"));
                playerMat.color = Color.blue;
                player.GetComponent<Renderer>().material = playerMat;
            }
            
            // 카메라 설정
            GameObject camera = new GameObject("PlayerCamera");
            camera.transform.SetParent(player.transform);
            camera.transform.localPosition = new Vector3(0, 1.6f, 0);
            Camera cam = camera.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            
            // 오디오 리스너
            camera.AddComponent<AudioListener>();
        }
        
        /// <summary>
        /// 퍼즐 설정
        /// </summary>
        private void SetupPuzzles()
        {
            // 거울 반사 퍼즐 생성
            CreateMirrorReflectionPuzzle();
        }
        
        /// <summary>
        /// 거울 반사 퍼즐 생성
        /// </summary>
        private void CreateMirrorReflectionPuzzle()
        {
            GameObject puzzleObj = new GameObject("MirrorReflectionPuzzle");
            puzzleObj.transform.position = new Vector3(0, 0, 2);
            
            MirrorReflectionPuzzle puzzle = puzzleObj.AddComponent<MirrorReflectionPuzzle>();
            
            // 거울들 생성
            Transform[] mirrors = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mirror.name = $"Mirror_{i}";
                mirror.transform.SetParent(puzzleObj.transform);
                mirror.transform.localPosition = new Vector3(-2 + i * 2, 1.5f, 0);
                mirror.transform.localScale = new Vector3(0.1f, 1.5f, 1f);
                
                // 거울 재질
                Material mirrorMat = new Material(Shader.Find("Standard"));
                mirrorMat.color = new Color(0.8f, 0.8f, 1f);
                mirrorMat.SetFloat("_Metallic", 1f);
                mirrorMat.SetFloat("_Smoothness", 0.9f);
                mirror.GetComponent<Renderer>().material = mirrorMat;
                
                mirrors[i] = mirror.transform;
            }
            
            // 빛 소스 생성
            GameObject lightSource = new GameObject("LightSource");
            lightSource.transform.SetParent(puzzleObj.transform);
            lightSource.transform.localPosition = new Vector3(-5, 1.5f, 0);
            
            Light light = lightSource.AddComponent<Light>();
            light.type = LightType.Spot;
            light.intensity = 2f;
            light.spotAngle = 30f;
            light.color = Color.yellow;
            lightSource.transform.LookAt(puzzleObj.transform.position);
            
            // 목표물 생성
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.name = "Target";
            target.transform.SetParent(puzzleObj.transform);
            target.transform.localPosition = new Vector3(5, 1.5f, 0);
            target.transform.localScale = Vector3.one * 0.5f;
            
            Material targetMat = new Material(Shader.Find("Standard"));
            targetMat.color = Color.red;
            target.GetComponent<Renderer>().material = targetMat;
            
            // 퍼즐 설정
            puzzle.mirrors = mirrors;
            puzzle.lightSource = lightSource.transform;
            puzzle.target = target.transform;
            
            // 콜라이더 추가 (퍼즐 영역)
            BoxCollider trigger = puzzleObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(12, 3, 4);
        }
    }
}