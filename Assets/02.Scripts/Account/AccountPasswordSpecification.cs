using System.Text.RegularExpressions;

public class AccountPasswordSpecification
{
    // 비밀번호 규칙: 대문자, 소문자, 숫자, 기호를 모두 포함한 7~15 글자 (공백/한글 등 ASCII 밖 문자 불가)
    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[\x21-\x7E]{7,15}\z");

    public string ErrorMessage => "비밀번호는 대문자, 소문자, 숫자, 기호를 모두 포함한 7~15글자여야 합니다.";


    public bool IsSatisfiedBy(string password)
    {
        return PasswordRegex.IsMatch(password);
    }
}