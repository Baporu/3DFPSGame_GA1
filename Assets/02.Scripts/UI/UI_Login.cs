using System.Text.RegularExpressions;
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

    private AccountIdSpecification _idSpec = new AccountIdSpecification();
    private AccountPasswordSpecification _passwordSpec = new AccountPasswordSpecification();


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

        // ID: 이메일 or 영문, 숫자, -, @, _를 사용한 3~20 글자
        // 비밀번호: 대문자 소문자 숫자 기호를 포함한 7~15 글자

        // 정규표현식(정규식)
        // 문자열에서 특정한 규칙이나 패턴을 찾기 위해 사용하는 문법
        // - 데이터 검증 (이메일 형식, 주민번호 형식, 비밀번호 형식)
        // - 문자열 검색 (ToDo라는 게 있냐?, 한국어가 있냐?)
        // - 문자열 치환 (규칙에 맞는 문자열을 찾아서 다른 문자열로 변경)
        // - 웹 크롤링 & 파싱

        // 규칙이 여기서만 쓰일까?
        // 정리 과제
        //   - 정규표현식
        //   - 명세 패턴

        // 다른 규칙들도 분리를 해야 할까?
        // 해도 되고 안 해도 되고. 필요한 경우에는 분리한다.
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }

        // ID 규칙: 이메일 또는 영문, 숫자, -, @, _를 사용한 3~20 글자
        if (_idSpec.IsSatisfiedBy(id) == false)
        {
            _messageText.text = _idSpec.ErrorMessage;
            return;
        }

        // 비밀번호 규칙: 대문자, 소문자, 숫자, 기호를 모두 포함한 7~15 글자 (공백/한글 등 ASCII 밖 문자 불가)
        if (_passwordSpec.IsSatisfiedBy(password) == false)
        {
            _messageText.text = _passwordSpec.ErrorMessage;
            return;
        }

        // 중복 ID 검사
        if (_repository.Exists(id))
        {
            _messageText.text = "이미 존재하는 아이디입니다.";
            return;
        }

        if (password != passwordConfirm)
        {
            _messageText.text = "비밀번호 확인이 맞지 않습니다.";
            return;
        }

        // 실제 회원가입 데이터 저장
        _repository.Register(id, password);

        GotoLogin();
        _messageText.text = "가입했습니다. 로그인 해주세요.";
    }
}