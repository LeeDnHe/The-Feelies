using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// VR 컨트롤러 입력을 처리하여 VRCameraViewSwitcher를 제어합니다.
/// OpenXR 기반으로 오른손 PrimaryButton 입력을 감지합니다.
/// </summary>
public class VRInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRCameraViewSwitcher viewSwitcher;
    
    [Header("Input Settings")]
    [Tooltip("오른손 컨트롤러를 사용")]
    [SerializeField] private bool useRightHand = true;
    [Tooltip("디버그 로그 출력")]
    [SerializeField] private bool debugLog = true;
    
    // 입력 디바이스
    private InputDevice rightHandDevice;
    private InputDevice leftHandDevice;
    
    // 버튼 상태 추적 (중복 입력 방지)
    private bool primaryButtonWasPressed = false;

    void Start()
    {
        // VRCameraViewSwitcher 자동 검색
        if (viewSwitcher == null)
        {
            viewSwitcher = FindObjectOfType<VRCameraViewSwitcher>();
            
            if (viewSwitcher == null)
            {
                Debug.LogError("VRInputHandler: VRCameraViewSwitcher를 찾을 수 없습니다!");
                enabled = false;
                return;
            }
        }
        
        // 컨트롤러 디바이스 초기화
        InitializeControllers();
        
        if (debugLog)
            Debug.Log("VRInputHandler: 초기화 완료");
    }

    void InitializeControllers()
    {
        // 오른손 컨트롤러 가져오기
        var rightHandDevices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            rightHandDevices
        );
        
        if (rightHandDevices.Count > 0)
        {
            rightHandDevice = rightHandDevices[0];
            if (debugLog)
                Debug.Log($"오른손 컨트롤러 연결됨: {rightHandDevice.name}");
        }
        
        // 왼손 컨트롤러 가져오기
        var leftHandDevices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            leftHandDevices
        );
        
        if (leftHandDevices.Count > 0)
        {
            leftHandDevice = leftHandDevices[0];
            if (debugLog)
                Debug.Log($"왼손 컨트롤러 연결됨: {leftHandDevice.name}");
        }
    }

    void Update()
    {
        // 컨트롤러가 초기화되지 않았으면 재시도
        if (!rightHandDevice.isValid || !leftHandDevice.isValid)
        {
            InitializeControllers();
            return;
        }
        
        // Primary Button 입력 체크
        CheckPrimaryButtonInput();
    }

    void CheckPrimaryButtonInput()
    {
        // 사용할 컨트롤러 선택
        InputDevice device = useRightHand ? rightHandDevice : leftHandDevice;
        
        if (!device.isValid)
            return;
        
        // Primary Button 상태 가져오기 (예: Meta Quest의 A/X 버튼)
        bool isPrimaryButtonPressed = false;
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out isPrimaryButtonPressed))
        {
            // 버튼이 눌렸고, 이전에는 눌리지 않았을 때 (Rising Edge)
            if (isPrimaryButtonPressed && !primaryButtonWasPressed)
            {
                OnPrimaryButtonPressed();
            }
            
            primaryButtonWasPressed = isPrimaryButtonPressed;
        }
    }

    void OnPrimaryButtonPressed()
    {
        if (viewSwitcher != null)
        {
            viewSwitcher.ToggleView();
            
            if (debugLog)
            {
                string viewMode = viewSwitcher.IsThirdPerson() ? "3인칭" : "1인칭";
                Debug.Log($"뷰 전환: {viewMode} 모드");
            }
        }
    }

    /// <summary>
    /// 외부에서 호출 가능한 토글 함수 (UnityEvent 등에서 사용)
    /// </summary>
    public void ToggleView()
    {
        if (viewSwitcher != null)
        {
            viewSwitcher.ToggleView();
        }
    }

    // 컨트롤러 연결/해제 이벤트 처리
    void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;
    }

    void OnDeviceConnected(InputDevice device)
    {
        if (debugLog)
            Debug.Log($"VR 디바이스 연결됨: {device.name}");
        
        InitializeControllers();
    }

    void OnDeviceDisconnected(InputDevice device)
    {
        if (debugLog)
            Debug.Log($"VR 디바이스 연결 해제됨: {device.name}");
    }

    // 디버그: 현재 컨트롤러 상태 출력
    [ContextMenu("Print Controller Status")]
    void PrintControllerStatus()
    {
        Debug.Log("=== 컨트롤러 상태 ===");
        Debug.Log($"오른손 컨트롤러 유효: {rightHandDevice.isValid}");
        Debug.Log($"왼손 컨트롤러 유효: {leftHandDevice.isValid}");
        
        if (rightHandDevice.isValid)
        {
            Debug.Log($"오른손 컨트롤러: {rightHandDevice.name}");
            
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primary))
                Debug.Log($"  Primary Button: {primary}");
            
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondary))
                Debug.Log($"  Secondary Button: {secondary}");
        }
    }
}

