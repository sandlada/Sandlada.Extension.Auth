namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError(string Code, string Message) {

    public static class General {
        public static readonly DomainError None = new(string.Empty, string.Empty);
    }

}
