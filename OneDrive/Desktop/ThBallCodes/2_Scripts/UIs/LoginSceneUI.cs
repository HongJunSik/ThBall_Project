using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DefineGameUtil;

public class LoginSceneUI : MonoBehaviour
{
    // �ɼǰ���
    [Header("�ɼ�")]
    [SerializeField] GameObject _optionBG;
    [SerializeField] GameObject _optionUI;

    // �α��� ����
    [Header("�α���")]
    [SerializeField] InputField _idInput;
    [SerializeField] InputField _pwInput;
    [SerializeField] GameObject _showPWBtn;
    string _nickName;
    EventTrigger _loginBtn;

    // ȸ������ ����
    [Header("ȸ������")]
    [SerializeField] GameObject _memberWnd;
    [SerializeField] InputField _newIDInput;
    [SerializeField] InputField _newPWInput;
    [SerializeField] InputField _CheckPWInput;
    [SerializeField] GameObject _memberShowPWBtn;
    bool _idCheck = false;
    EventTrigger _idOverlapBtn;
    EventTrigger _createBtn;


    [Header("����â")]
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


    // �α��� ��ư Ŭ��
    public void LoginBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _loginBtn.enabled = false;
        ServerManager._instance.SendLogin(_idInput.text, _pwInput.text);
    }
    // �α��� Ȯ�� ���ο� ���� �г��� �޾ƿ���
    public void LoginSuccess(string nickname, long uuid, string id, string pw)
    {
        switch (nickname)
        {
            case "0":
                {
                    ShowInfoText("�̹� �������� �����Դϴ�!");
                    break;
                }
            case "1":
                {
                    ShowInfoText("ID�� PW�� �ٽ� �� �� Ȯ�� �� �ּ���!");
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

    #region[ȸ������]
    // ȸ������â ����
    public void MemberBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _memberWnd.SetActive(true);
    }

    // ȸ������â �ݱ�
    public void CloseMemberUI()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _newIDInput.text = string.Empty;
        _newPWInput.text = string.Empty;
        _CheckPWInput.text = string.Empty;
        _memberWnd.SetActive(false);
    }

    // ȸ������â ��ȣ ���̱�
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
    // ID üũ�ϱ�
    public void OverlapBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (HangulCheck(_newIDInput.text))
        {
            ShowInfoText("ID�� �ѱ��� ���Ե� �� �����ϴ�!!");
        }
        else if (_newIDInput.text.Length < 6)
        {
            ShowInfoText("ID�� �ּ� 6���Դϴ�!!");
        }
        else if (!IDCheck(_newIDInput.text))
        {
            ShowInfoText("ID�� �ݵ�� ����� ���ڰ� ���� �Ǿ�� �մϴ�!!");
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
            ShowInfoText("�̹� ������� ���̵� �Դϴ�!!");
        }
        else
        {
            ShowInfoText("��� ������ ID�Դϴ�.");
            PlayerPrefs.SetString("ID", _newIDInput.text);
            _idCheck = true;
        }
        _idOverlapBtn.enabled = true;
    }

    // PW üũ�ϰ� �ش� id, pw ����
    public void CreateBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        if (_newPWInput.text.Length < 4)
        {
            ShowInfoText("PW�� �ּ� 4���Դϴ�!!");
        }
        else if (!_idCheck)
        {
            ShowInfoText("ID Ȯ���� �ʿ��մϴ�!!");
        }
        else if (!_newPWInput.text.Equals(_CheckPWInput.text))
        {
            ShowInfoText("PW�� ���� ���� �ʽ��ϴ�.\nȮ���� �ʿ��մϴ�!!");
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
            ShowInfoText("ID�� PW�� �����Ǿ����ϴ�.");
            CloseMemberUI();
        }
        else
        {
            ShowInfoText("���� ������ ����Ǿ� ���� ����!!!");
        }
    }
    #endregion

    // ��ȣ ���̱� ��ư Ŭ��
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

    // ����â ����
    public void ShowInfoText(string s)
    {
        _infoText.text = s;
        _infoTextWnd.SetActive(true);
    }
    // ����â �ݱ�
    public void CloseInfoText()
    {
        _infoText.text = string.Empty;
        _infoTextWnd.SetActive(false);
    }

    // �ɼǹ�ư Ŭ��
    public void OptionBtnClick()
    {
        SoundManager._instance.PlaySFXSoundOneShot(eSFX.BtnClick);
        _optionBG.SetActive(true);
        Instantiate(_optionUI, _optionBG.transform);
    }

    // �����ư Ŭ��
    public void QuitBtnClick()
    {
        // TCPClientManager._instance.GracefullyDisconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // �ѱ��� ���ԵǸ� true
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

    // ���� �ҹ���, �빮��, ���ڰ� ���ԵǾ��ִٸ� true
    bool IDCheck(string s)
    {
        char[] arr = s.ToCharArray();

        // ���� �����ߴ��� Ȯ��
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

        // ���� �����ߴ��� Ȯ��
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
