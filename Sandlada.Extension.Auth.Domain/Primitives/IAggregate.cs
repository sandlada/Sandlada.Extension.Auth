namespace Sandlada.Extension.Auth.Domain.Primitives;

public interface IAggregate<out ID> where ID : notnull {
    public ID Id { get; }

}
