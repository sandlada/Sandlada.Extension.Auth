# Sandlada Extension Auth Backend Agent Notes

## 項目簡介

- 項目名稱：Sandlada.Extension.Auth
- 項目類型：ASP.NET Core Web API + Application + Domain + Infrastructure + Extension
- 項目描述：提供用戶身份驗證、會話管理和相關功能的後端服務，支持前端和其他後端集成。項目可獨立運行，也可以通過nuget包形式集成到其它後端項目中。

## 項目結構

| 包                                     |                                                               作用 |
| :------------------------------------- | -----------------------------------------------------------------: |
| Sandlada.Extension.Auth.Api            | 包含ASP.NET Core Web API項目，負責處理HTTP請求、身份驗證和會話管理 |
| Sandlada.Extension.Auth.Application    |                包含用例、服務接口和DTO，負責協調業務邏輯和應用規則 |
| Sandlada.Extension.Auth.Domain         |                 包含核心業務邏輯、實體、值對象、領域錯誤和倉儲接口 |
| Sandlada.Extension.Auth.Infrastructure |                       包含外部系統的實現，如數據庫訪問、身份驗證等 |
| Sandlada.Extension.Auth.Extension      |                                     包含對外部開發時使用的擴展工具 |

### 項目結構説明

- 項目采用 六邊形架構 + 整潔架構 的最佳實踐。
- Domain 層 Aggregates 采用 DDD 模式，封裝業務邏輯和規則。
- Application 層負責協調用例和服務接口。保持業務邏輯與外部實現分離。
- Api 層處理 HTTP 請求，使用 ASP.NET Core Identity 進行身份驗證和會話管理，保持控制器簡潔。
- Api 層使用 Swagger/OpenAPI 進行API文檔和測試，方便前端和其他消費者集成。
- Api 曾使用 Server Side Cookies 進行會話管理，保持安全性和簡化前端實現；Server Side Cookies 方案采用 “混合/有狀態” 模式可即时 Kick 用戶會話。 Client Side Cookie 不保存任何隱私信息。

## 技術棧

- 數據庫使用 SQLite，搭配 EFCore
- 當 Domain 模型變更影響資料庫 Schema 時，需生成並提交 EF Core Migration：`dotnet ef migrations add <MigrationName> --project Sandlada.Extension.Auth.Infrastructure --startup-project Sandlada.Extension.Auth.Api`
- 在 Api 層和 Application 層使用 MediatR
- ASP.NET Core Identity
- 開發環境使用 Swagger + SwaggerUI + OpenAPI
- 郵箱服務使用MailKit + MimeKit；郵箱服務器使用Rnwood.Smtp4dev-win-x64

## 命名規範

### 增刪改查

對於 Repository, Command, Query, handler, Service, Controller，Api 等的增刪改查，請使用以下命名規範：

- FindOne / FindMany 用於查詢方法，返回單個或多個實體。
- FindOneByXxx / FindManyByXxx 用於帶有條件的查詢方法，根據特定條件返回單個或多個實體。
- InsertOne / InsertMany 用於插入方法，插入單個或多個實體。
- UpdateOne / UpdateMany 用於更新方法，更新單個或多個實體。
- RemoveOne / RemoveMany 用於刪除方法，刪除單個或多個實體。

_通常情況下我們不使用 Get，Create，Delete，而是使用Find，Insert，Remove；我們不用GetAll也不用FindAll，而是用FindMany表示查詢所有；一般情況下我們很少需要UpdateMany和RemoveMany，因爲多數情況下我們使用UpdateOne和RemoveOne完成操作。_

對於文件名，同樣使用如上規則。例如：

- QueryOneUserById.cs
- CommandInsertOneUser.cs

### 縮寫詞

- （Api Url除外）對於簡短的縮寫詞，如API，URL等，保持全大寫以提高可讀性。
- 對於較長的縮寫詞，如Authentication，Authorization等，使用首字母大寫的駝峰式命名（PascalCase），以保持一致性和可讀性。例如，AuthenticationService、AuthorizationHandler等。

### Api Url

对于Api层的前端控制器（Controller）：

- URL路径使用单词首字母大写的驼峰式命名（PascalCase），以提高可读性和一致性。例如，`Api/User/Logout`、`Api/User/InsertOne`、`Api/User/FindOneById`等。

## 项目最佳規範

### 依赖注入类主構造器优先

对于需要依赖注入的類，優先使用主構造器。例如：

```cs
public sealed class XxxController(
    ISender sender
) : ControllerBase {
    // Controller actions here
}
```

## 聚合、值對象等實體的創建優先使用 From 工廠方法

Domain層優先使用工廠方法（From方法）來創建實體和聚合。

### 關於Args

對於值對象、聚合、查詢、命令等需要傳入參數的對象，需要使用帶有必要參數的 XxxConstructorArgs 或 XxxArgs 來創建參數，而不是使用無參構造函數和公共可設置屬性；其中 Domain 層聚合與值對象的建構參數使用 XxxConstructorArgs，Application 和 Api 層的操作輸入（commands、queries、controller requests）使用 XxxArgs。這樣可以確保對象在創建時具有完整的狀態，並且可以在構造函數中添加必要的驗證邏輯，以防止創建無效的對象。

以Domain層的User聚合爲例：

```cs
namespace Sandlada.Extension.Auth.Domain.Aggregates;

public sealed record UserConstructorArgs {
    public required Guid Id { get; init; }
    // 此處可添加用於構造 User 實體的其他必要參數，例如 EmailAddress, DisplayName, Role 等，這些參數應該包含在 User 的構造函數中，以確保實體在創建時具有完整的狀態。
}

public sealed class User : IAggregate<Guid> {
    #region Properties
    public Guid Id { get; private set; }
    // 此處可添加 User 實體的其他屬性，例如 EmailAddress, DisplayName, Role 等。
    #endregion

    #region Constructors
    private User() { }
    private User(UserConstructorArgs args) {
        this.Id = args.Id;
        // 此處可添加用於初始化 User 實體的邏輯。通常情況下我們不會在此處修改傳入的數據，即使數據是從外部來的，我們也會在 From 方法中添加驗證邏輯，而不是在構造函數中修改數據。
    }
    public static IResult<User> From(UserConstructorArgs args) {
        // 此處可添加用於檢查 UserConstructorArgs 的業務規則和驗證邏輯，例如檢查 DisplayName 的長度等。
        return Result.Success(new User(args));
    }
    #endregion

    #region Methods: Update Properties
    // 此處可添加用於更新實體屬性的方法，例如 UpdateEmailAddress, UpdateDisplayName 等，這些方法應該包含必要的業務規則和驗證邏輯。
    #endregion
}
```

### 關於 Application 层文件命名規範

Application 層按照 Command、Query等類型和具體操作（如 InsertOneUserCommand 是一個 Command，所以它應該在 Application 層的 Commands/User 文件夾中）來組織文件夾：

- Commands
  - User
    - InsertOneUserCommandArgs.cs
    - InsertOneUserCommand.cs
    - InsertOneUserCommandHandler.cs
    - InsertOneUserCommandResponse.cs
- Queries
  - User
    - FindOneUserByIdQueryArgs.cs
    - FindOneUserByIdQuery.cs
    - FindOneUserByIdQueryHandler.cs
    - FindOneUserByIdQueryResponse.cs

以Application層 InsertOneUserCommand 為例，應該包含以下文件：

- InsertOneUserCommandArgs.cs
- InsertOneUserCommand.cs
- InsertOneUserCommandHandler.cs
- InsertOneUserCommandResponse.cs

_在API中，應該使用 InsertOneUserCommandArgs 來接收前端請求，並將其映射到 InsertOneUserCommand 中；在 Application 層中，InsertOneUserCommandHandler 處理 InsertOneUserCommand 並返回 InsertOneUserCommandResponse；在 Api 層中，控制器接收 InsertOneUserCommandArgs，調用 MediatR 發送 InsertOneUserCommand，並根據結果返回對應的 HTTP 響應。_

### 關於 Domain 層值對象的序列化支持

所有的值對象都必須可序列化和反序列化，且必須實現IEquatable和IParsable。

對於IParsable，假設值對象只有一個字段叫Name，則從序列化字符串恢復到C#對象，需要支持兩種格式：

格式一:

```plaintext
"{\"Name\": \"my-value\"}"
```

格式二:

```plaintext
"my-value"
```

序列化采用格式一。

以UserRole爲例：

```cs
[JsonConverter(typeof(UserRoleJsonConverter))]
[TypeConverter(typeof(UserRoleTypeConverter))]
public sealed record UserRole : IEquatable<UserRole>, IParsable<UserRole>{
    #region Properties
    public string Value { get; private init; }
    #endregion
    #region Convertors
    public sealed class UserRoleJsonConverter : JsonConverter<UserRole> {
        // Implement this
    }
    public sealed class UserRoleTypeConverter : TypeConverter {
        // Implement this
    }
    public static UserRole Parse(string s, IFormatProvider? provider) {
        // Implement this
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UserRole result) {
        // Implement this
    }
    #endregion
}
```

UserRole的JsonConverter和TypeConverter需要支持上述兩種格式的反序列化，並且序列化時使用格式一。

## Working Roles

- When selecting NuGet packages for new features, prefer Microsoft-provided libraries (e.g., Microsoft.Extensions.*) over third-party alternatives, unless an existing third-party library (e.g., MediatR) is already established in the codebase. For authentication and session persistence, prefer ASP.NET Core cookie authentication and server-side cookies unless a task explicitly requires another scheme.
- Keep business rules in Domain, use cases and ports in Application, and external implementations in Infrastructure.
- Reuse the existing Result/IResult and partial DomainError patterns for validation and failures instead of throwing for normal domain flow.

## Conventions

- Target framework is .NET 10 with nullable enabled and implicit usings enabled.
- Follow [.editorconfig](.editorconfig): 4-space indentation, LF line endings, trim trailing whitespace, and use `var` where the type is obvious.
- Keep aggregates sealed, value objects sealed, and domain errors organized by area.
- Use xUnit for tests and keep test names in the `Method_Scenario_ExpectedResult` style.

## Commands

- Build the solution with `dotnet build Sandlada.Extension.Auth.slnx`.
- Run tests with `dotnet test`.
- Start the API with `dotnet run --project Sandlada.Extension.Auth.Api/Sandlada.Extension.Auth.Api.csproj`.

## Useful References

- Domain model patterns: [Sandlada.Extension.Auth.Domain/Aggregates/User.cs](Sandlada.Extension.Auth.Domain/Aggregates/User.cs), [Sandlada.Extension.Auth.Domain/ValueObjects/EmailAddress.cs](Sandlada.Extension.Auth.Domain/ValueObjects/EmailAddress.cs), [Sandlada.Extension.Auth.Domain/ValueObjects/UserRole.cs](Sandlada.Extension.Auth.Domain/ValueObjects/UserRole.cs)
- Result and error handling: [Sandlada.Extension.Auth.Domain/Commons/Result.cs](Sandlada.Extension.Auth.Domain/Commons/Result.cs), [Sandlada.Extension.Auth.Domain/Commons/DomainError.cs](Sandlada.Extension.Auth.Domain/Commons/DomainError.cs)
- Repository contract: [Sandlada.Extension.Auth.Domain/Repositories/IUserRepository.cs](Sandlada.Extension.Auth.Domain/Repositories/IUserRepository.cs)
- Existing test style: [Sandlada.Extension.Auth.Domain.Tests/ValueObjects/UserRoleTests.cs](Sandlada.Extension.Auth.Domain.Tests/ValueObjects/UserRoleTests.cs)
- API host entry point: [Sandlada.Extension.Auth.Api/Program.cs](Sandlada.Extension.Auth.Api/Program.cs)
- API launch profiles: [Sandlada.Extension.Auth.Api/Properties/launchSettings.json](Sandlada.Extension.Auth.Api/Properties/launchSettings.json)

## Editing Guidance

- Make minimal changes and avoid reformatting unrelated code.
- When changing domain rules, extend or add domain tests in Sandlada.Extension.Auth.Domain.Tests.
- When a domain model change affects the database schema, generate and include an EF Core migration using `dotnet ef migrations add <MigrationName> --project Sandlada.Extension.Auth.Infrastructure --startup-project Sandlada.Extension.Auth.Api`.
- If a change touches auth or session behavior, wire it through Application contracts and Infrastructure implementations rather than putting business logic in controllers.
