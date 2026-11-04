using System.Text.RegularExpressions;

public class AccountIdSpecification
{
    // ID 규칙: 이메일 또는 영문, 숫자, -, @, _를 사용한 3~20 글자
    // 이메일: 로컬 부분(영문 숫자 . _ + -) @ 도메인(영문 숫자 - 를 점으로 구분, 점 1개 이상)
    private static readonly Regex IdRegex = new Regex(
        @"^(?:[A-Za-z0-9._+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)+|[A-Za-z0-9@_-]{3,20})\z");

    public string ErrorMessage => "아이디는 이메일 또는 영문, 숫자, -, @, _를 사용한 3~20글자여야 합니다.";


    public bool IsSatisfiedBy(string id)
    {
        return IdRegex.IsMatch(id);
    }
}