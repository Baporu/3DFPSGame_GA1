using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour
{
    private enum Mode
    {
        Login, // 로그인
        Register, // 회원가입
    }

    [Header("입력 칸")]
    [SerializeField] private TMP_InputField _idInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _passwordConfirmInput; // 회원가입 모드에서만 보인다.

    [Header("버튼")]
    [SerializeField] private Button _loginButton; // 로그인 모드: 로그인
    [SerializeField] private Button _gotoRegisterButton; // 로그인 모드: 회원가입 모드로
    [SerializeField] private Button _registerButton; // 회원가입 모드: 가입하기
    [SerializeField] private Button _gotoLoginButton; // 회원가입 모드: 로그인 모드로

    [Header("기타")]
    [SerializeField] private TextMeshProUGUI _messageText; // 안내 글자
    [SerializeField] private string _gameSceneName = "GameScene"; // 로그인하면 불러올 씬

    private Mode _mode = Mode.Login;

    // 계정 데이터 보관 클래스
    // 로그인 로직은 다른 프로젝트에서 재활용할 수도 있으니 데이터 저장을 별도 책임으로 분리한다.
    // (데이터 보관하는 클래스명만 변경하면 되므로 -> 레포지토리(Repository) 패턴)
    private PlayerPrefsAccountRepository _repository = new PlayerPrefsAccountRepository();


    private void Start()
    {
        _loginButton.onClick.AddListener(Login);
        _gotoRegisterButton.onClick.AddListener(GotoRegister);
        _registerButton.onClick.AddListener(Register);
        _gotoLoginButton.onClick.AddListener(GotoLogin);
        Refresh();
    }

    // 지금 모드에 맞는 칸과 버튼만 보여 준다.
    private void Refresh()
    {
        bool isLogin = _mode == Mode.Login;
        _passwordConfirmInput.gameObject.SetActive(!isLogin);
        _loginButton.gameObject.SetActive(isLogin);
        _gotoRegisterButton.gameObject.SetActive(isLogin);
        _registerButton.gameObject.SetActive(!isLogin);
        _gotoLoginButton.gameObject.SetActive(!isLogin);
    }

    private void GotoRegister()
    {
        _mode = Mode.Register;
        _messageText.text = "";
        Refresh();
    }

    private void GotoLogin()
    {
        _mode = Mode.Login;
        _messageText.text = "";
        Refresh();
    }

    // 로그인: 입력을 확인하고, 맞으면 게임 씬을 불러온다.
    private void Login()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }

        if (_repository.CheckPassword(id, password) == false)
        {
            _messageText.text = "아이디와 비밀번호를 확인해 주세요.";
            return;
        }

        SceneManager.LoadScene(_gameSceneName);
    }

    private void Register()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;
        string passwordConfirm = _passwordConfirmInput.text;

        // Todo: ID와 비밀번호 (+ 닉네임) 규칙
        // ID: 이메일
        // 비밀번호: 영어만 되며 특수문자나 대문자 하나 이상을 포함한 7~15 글자
        // 규칙이 여기서만 쓰일까?
        // 정리 과제
        //   - 정규표현식
        //   - 명세 패턴

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }

        if (password != passwordConfirm)
        {
            _messageText.text = "비밀번호 확인이 맞지 않습니다.";
            return;
        }

        // 중복 ID 검사
        if (_repository.Exists(id))
        {
            _messageText.text = "이미 존재하는 아이디입니다.";
            return;
        }

        // 실제 회원가입 데이터 저장
        _repository.Register(id, password);

        GotoLogin();
        _messageText.text = "가입했습니다. 로그인 해주세요.";
    }
}