using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DefineGameUtil;

public class LoginSceneUI : MonoBehaviour
{
    // 옵션관련
    [Header("옵션")]
    [SerializeField] GameObject _optionBG;
    [SerializeField] GameObject _optionUI;

    // 로그인 관련
    [Header("로그인")]
    [SerializeField] InputField _idInput;
    [SerializeField] InputField _pwInput;
    [SerializeField] GameObject _showPWBtn;
    string _nickName;
    EventTrigger _loginBtn;

    // 회원가입 관련
    [Header("회원가입")]
    [SerializeField] GameObject _memberWnd;
    [SerializeField] InputField _newIDInput;
    [SerializeField] InputField _newPWInput;
    [SerializeField] InputField _CheckPWInput;
    [SerializeField] GameObject _memberShowPWBtn;
    bool _idCheck = false;
    EventTrigger _idOverlapBtn;
    EventTrigger _createBtn;


    [Header("설명창")]
    [SerializeField] GameObject _infoTextWnd;
    [SerializeField] Text _infoText;

    void Start()
    {
        _createBtn = GameObject.Find("CreateBtn").GetComponent<EventTrigger>();
        _idOverlapBtn = GameObject.Find("OverlapCheck").GetComponent<EventTrigger>();
        _loginBtn = GameObject.Find("LoginBtn").GetComponent<EventTrigger>();
        CloseMemberUI();
        CloseInfoText();
        if (!PlayerPrefs.HasKey("ID"))
        {
            PlayerPrefs.SetString("ID", "");
            PlayerPrefs.SetString("PW", "");
            _idInput.text = string.Empty;
            _pwInput.text = string.Empty;
        }
        else
        {
            _idInput.text = PlayerPrefs.GetString("ID");
            _pwInput.text = PlayerPrefs.GetString("PW");
        }
    }

    void Update()
    {
        if (_idInput.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            _pwInput.Select();
        }
    }


    // 로그인 버튼 클릭
    public void LoginBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _loginBtn.enabled = false;
        ServerManager._instance.SendLogin(_idInput.text, _pwInput.text);
    }
    // 로그인 확인 여부에 따른 닉네임 받아오기
    public void LoginSuccess(string nickname, long uuid, string id, string pw)
    {
        switch (nickname)
        {
            case "0":
                {
                    ShowInfoText("이미 접속중인 유저입니다!");
                    break;
                }
            case "1":
                {
                    ShowInfoText("ID와 PW를 다시 한 번 확인 해 주세요!");
                    break;
                }
            default:
                {
                    UserInfos._instance.SetLoginInfo(uuid, id, pw);
                    UserInfos._instance.SetNickNameInfo(nickname);
                    PlayerPrefs.SetString("ID", id);
                    PlayerPrefs.SetString("PW", pw);
                    NetworkManager.instance.Connect();
                    SceneControlManager._instance.LoginToLobbyScene(nickname);
                    break;
                }
        }
        _loginBtn.enabled = true;
    }

    #region[회원가입]
    // 회원가입창 열기
    public void MemberBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _memberWnd.SetActive(true);
    }

    // 회원가입창 닫기
    public void CloseMemberUI()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _newIDInput.text = string.Empty;
        _newPWInput.text = string.Empty;
        _CheckPWInput.text = string.Empty;
        _memberWnd.SetActive(false);
    }

    // 회원가입창 암호 보이기
    public void MemberShowPWBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (_newPWInput.contentType == InputField.ContentType.Password)
        {
            _newPWInput.contentType = InputField.ContentType.Standard;
            string s = _newPWInput.text;
            _newPWInput.text = string.Empty;
            _newPWInput.text = s;

            _CheckPWInput.contentType = InputField.ContentType.Standard;
            s = _CheckPWInput.text;
            _CheckPWInput.text = string.Empty;
            _CheckPWInput.text = s;

            _memberShowPWBtn.SetActive(true);
        }
        else
        {
            _newPWInput.contentType = InputField.ContentType.Password;
            string s = _newPWInput.text;
            _newPWInput.text = string.Empty;
            _newPWInput.text = s;

            _CheckPWInput.contentType = InputField.ContentType.Password;
            s = _CheckPWInput.text;
            _CheckPWInput.text = string.Empty;
            _CheckPWInput.text = s;

            _memberShowPWBtn.SetActive(false);
        }
    }
    // ID 체크하기
    public void OverlapBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (HangulCheck(_newIDInput.text))
        {
            ShowInfoText("ID에 한글이 포함될 수 없습니다!!");
        }
        else if (_newIDInput.text.Length < 6)
        {
            ShowInfoText("ID는 최소 6자입니다!!");
        }
        else if (!IDCheck(_newIDInput.text))
        {
            ShowInfoText("ID에 반드시 영어와 숫자가 포함 되어야 합니다!!");
        }
        else
        {
            _idOverlapBtn.enabled = false;
            ServerManager._instance.SendCheckID(_newIDInput.text);
        }
    }
    public void IDcheck(string id)
    {
        if (id.Equals(string.Empty))
        {
            ShowInfoText("이미 사용중인 아이디 입니다!!");
        }
        else
        {
            ShowInfoText("사용 가능한 ID입니다.");
            PlayerPrefs.SetString("ID", _newIDInput.text);
            _idCheck = true;
        }
        _idOverlapBtn.enabled = true;
    }

    // PW 체크하고 해당 id, pw 저장
    public void CreateBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (_newPWInput.text.Length < 4)
        {
            ShowInfoText("PW는 최소 4자입니다!!");
        }
        else if (!_idCheck)
        {
            ShowInfoText("ID 확인이 필요합니다!!");
        }
        else if (!_newPWInput.text.Equals(_CheckPWInput.text))
        {
            ShowInfoText("PW가 서로 같지 않습니다.\n확인이 필요합니다!!");
        }
        else
        {
            _createBtn.enabled = false;
            PlayerPrefs.SetString("PW", _newPWInput.text);
            ServerManager._instance.SendLoginInfo();
        }
    }
    public void CreateSuccess(bool result)
    {
        _createBtn.enabled = true;
        if (result)
        {
            ShowInfoText("ID와 PW가 생성되었습니다.");
            CloseMemberUI();
        }
        else
        {
            ShowInfoText("현재 서버와 연결되어 있지 않음!!!");
        }
    }
    #endregion

    // 암호 보이기 버튼 클릭
    public void ShowPWBtnClick()
    {
        if (_pwInput.contentType == InputField.ContentType.Password)
        {
            _pwInput.contentType = InputField.ContentType.Standard;
            string s = _pwInput.text;
            _pwInput.text = string.Empty;
            _pwInput.text = s;
            _showPWBtn.SetActive(true);
        }
        else
        {
            _pwInput.contentType = InputField.ContentType.Password;
            string s = _pwInput.text;
            _pwInput.text = string.Empty;
            _pwInput.text = s;
            _showPWBtn.SetActive(false);
        }
    }

    // 인포창 열기
    public void ShowInfoText(string s)
    {
        _infoText.text = s;
        _infoTextWnd.SetActive(true);
    }
    // 인포창 닫기
    public void CloseInfoText()
    {
        _infoText.text = string.Empty;
        _infoTextWnd.SetActive(false);
    }

    // 옵션버튼 클릭
    public void OptionBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _optionBG.SetActive(true);
        Instantiate(_optionUI, _optionBG.transform);
    }

    // 종료버튼 클릭
    public void QuitBtnClick()
    {
        // TCPClientManager._instance.GracefullyDisconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 한글이 포함되면 true
    bool HangulCheck(string s)
    {
        char[] arr = s.ToCharArray();

        foreach (char c in arr)
        {
            if (char.GetUnicodeCategory(c).Equals(System.Globalization.UnicodeCategory.OtherLetter))
            {
                return true;
            }
        }

        return false;
    }

    // 영어 소문자, 대문자, 숫자가 포함되어있다면 true
    bool IDCheck(string s)
    {
        char[] arr = s.ToCharArray();

        // 영어 포함했는지 확인
        Regex regex = new Regex(@"[A-Z]");
        Regex regex2 = new Regex(@"[a-z]");
        bool isMatch1 = false;
        foreach (char c in arr)
        {
            if (regex.IsMatch(c.ToString()) || regex2.IsMatch(c.ToString()))
            {
                isMatch1 = true;
            }
        }

        if (!isMatch1) return false;

        // 숫자 포함했는지 확인
        regex = new Regex(@"[0-9]");
        bool isMatch3 = false;
        foreach (char c in arr)
        {
            if (regex.IsMatch(c.ToString()))
            {
                isMatch3 = true;
            }
        }
        if (!isMatch3) return false;

        return true;
    }
}
