# MCP Server Registry with Azure API Center

> Git ベースのワークフロー（Pull Request）、GitHub Copilot SDK、Azure API Center を組み合わせた MCP サーバー自動登録システム

## 全体概要

このリポジトリは、組織内の MCP（Model Context Protocol）サーバーを一元管理するためのレジストリです。

**登録者が行う操作は、MCPサーバーのリポジトリURLを入力するだけです。** GitHub Copilot SDK が自動的にリポジトリを解析してメタデータを生成し、ユーザーが確認・承認すると、登録に必要なファイルの生成・ブランチ作成・コミット・プッシュ・Pull Request 作成までを自動化します。Pull Request がマージされると、GitHub Actions が Azure API Center へ自動登録します。

主な特長：

- **🔗 URLのみで登録**: 対象 MCP サーバーのリポジトリ URL を入力するだけで登録を開始できます
- **🤖 Copilot SDK によるメタデータ自動生成**: GitHub Copilot SDK がリポジトリを解析し、名前・説明・認証方式などのメタデータをすべて自動生成します
- **📝 Git ベースのガバナンス**: Pull Request を通じたレビュー・承認フローにより、登録内容の品質と追跡可能性を確保します
- **☁️ Azure API Center への自動登録**: PR マージ後、GitHub Actions が自動で Azure API Center へ登録します
- **📚 セルフホスト型ポータル**: Azure API Center Portal を通じて登録済み MCP サーバーを検索・閲覧できます

---

## ロジック

### 登録フロー（リポジトリ URL を入力してから API Center に登録されるまで）

```
[ユーザー]
   │
   │ 1. CLIツールを起動
   │    dotnet run --project src/McpRegistration.Cli
   │
   │ 2. MCPサーバーのリポジトリURLを入力（これだけ！）
   │    例: https://github.com/org/my-mcp-server
   │
   ▼
[GitHub Copilot SDK]
   │
   │ 3. リポジトリURLを解析し、以下のメタデータを自動生成：
   │    - name, description, summary, version
   │    - company, owner, status, lifecycle
   │    - authMethod, endpointUrl, documentationUrl, tags
   │
   │ 4. ask_user ツールを通じて生成内容をユーザーに提示し確認を求める
   │    「これらの値で登録を進めますか？ (yes/no)」
   │
   ▼
[CLIツール（ユーザーが "yes" と回答した場合）]
   │
   │ 5. メタデータのバリデーション
   │
   │ 6. 登録ファイルを apis/<mcp-name>/ に生成：
   │    - metadata.json  （Azure API Center 用メタデータ）
   │    - openapi.json   （OpenAPI 3.0 仕様）
   │    - README.md      （ドキュメント）
   │
   │ 7. Git ブランチを作成: mcp-registration/<mcp-name>
   │
   │ 8. 変更をコミット
   │
   │ 9. ブランチをプッシュし、Pull Request を自動作成
   │
   ▼
[GitHub（レビュー・承認プロセス）]
   │
   │ 10. チームによる Pull Request レビュー
   │
   │ 11. 承認後、main ブランチへマージ
   │
   ▼
[GitHub Actions]
   │
   │ 12. apis/ 配下の変更を検知
   │
   │ 13. Azure CLI (az apic) を使って Azure API Center へ登録：
   │     - API エントリの作成
   │     - バージョンの作成
   │     - OpenAPI 仕様のインポート
   │
   ▼
[Azure API Center]
   │
   │ 14. MCP サーバーが API Center のカタログに追加される
   │
   ▼
[API Center Portal（セルフホスト）]
   │
   │ 15. 登録済み MCP サーバーを検索・閲覧できる
```

---

## アーキテクチャ

```
┌───────────────────────────────────────────────────────────────┐
│  ローカル環境                                                  │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  McpRegistration.Cli (CLIツール)                        │  │
│  │                                                         │  │
│  │  ┌──────────────────┐   ┌──────────────────────────┐   │  │
│  │  │ CopilotService   │   │ FileGenerationService    │   │  │
│  │  │                  │   │                          │   │  │
│  │  │ GitHub Copilot   │   │ metadata.json 生成        │   │  │
│  │  │ SDK を使って      │   │ openapi.json 生成         │   │  │
│  │  │ URLからメタデータ │   │ README.md 生成            │   │  │
│  │  │ を自動生成        │   │                          │   │  │
│  │  └────────┬─────────┘   └──────────────────────────┘   │  │
│  │           │                                             │  │
│  │  ┌────────▼─────────┐   ┌──────────────────────────┐   │  │
│  │  │ ValidationService│   │ GitService               │   │  │
│  │  │ メタデータ検証    │   │ ブランチ作成・コミット・  │   │  │
│  │  │                  │   │ プッシュ・PR作成          │   │  │
│  │  └──────────────────┘   └──────────────────────────┘   │  │
│  └─────────────────────────────────────────────────────────┘  │
└─────────────────────┬─────────────────────────────────────────┘
                      │ git push + PR 作成
                      ▼
┌───────────────────────────────────────────────────────────────┐
│  GitHub Repository                                            │
│                                                               │
│  apis/                                                        │
│  └── <mcp-name>/                                              │
│      ├── metadata.json                                        │
│      ├── openapi.json                                         │
│      └── README.md                                            │
│                                                               │
│  .github/workflows/                                           │
│  └── register-to-api-center.yml  ← PR マージ後に起動          │
└─────────────────────┬─────────────────────────────────────────┘
                      │ PR マージ → GitHub Actions 起動
                      ▼
┌───────────────────────────────────────────────────────────────┐
│  GitHub Actions                                               │
│                                                               │
│  1. 変更された apis/ ディレクトリを検出                        │
│  2. Azure Login (サービスプリンシパル認証)                    │
│  3. az apic api create         → API エントリ作成             │
│  4. az apic api version create → バージョン作成               │
│  5. az apic api definition ... → OpenAPI 仕様インポート       │
└─────────────────────┬─────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────────────────────┐
│  Azure API Center（システム・オブ・レコード）                  │
│                                                               │
│  登録済み MCP サーバーの一元管理・カタログ                     │
└─────────────────────┬─────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────────────────────┐
│  API Center Portal（セルフホスト）                             │
│                                                               │
│  MCP サーバーの検索・閲覧・ドキュメント参照                    │
└───────────────────────────────────────────────────────────────┘
```

---

## GitHub Copilot SDK について

### 役割

`CopilotService`（`src/McpRegistration.Core/Services/CopilotService.cs`）が GitHub Copilot SDK（`GitHub.Copilot.SDK` パッケージ）を使用し、以下の処理を担います：

1. **リポジトリ解析とメタデータ自動生成**: 入力されたリポジトリ URL をもとに、Copilot が以下のメタデータをすべて自動生成します：
   - `name`（MCP サーバー名）
   - `description`（詳細説明）
   - `summary`（1行サマリー）
   - `version`（バージョン）
   - `company`（組織名）
   - `owner`（オーナー）
   - `status`（active / deprecated / planned）
   - `lifecycle`（development / production など）
   - `authMethod`（認証方式）
   - `endpointUrl`（エンドポイント URL）
   - `documentationUrl`（ドキュメント URL）
   - `tags`（タグ一覧）

2. **ユーザーへの確認（ask_user ツール）**: 生成したメタデータをユーザーに提示し、`ask_user` ツールを通じてインタラクティブに確認を求めます。ユーザーが承認した場合のみ登録処理を継続します。

### 導入目的

従来のCLIツールでは、ユーザーが名前・説明・認証方式・ライフサイクルなど多数のフィールドをすべて手動で入力する必要がありました。GitHub Copilot SDK を導入することで：

- **入力の簡素化**: ユーザーが入力するのは**リポジトリ URL のみ**になり、登録の手間を大幅に削減
- **メタデータ品質の向上**: Copilot がリポジトリの内容を解析して適切な値を提案するため、一貫性のある高品質なメタデータを生成
- **確認フローの組み込み**: 自動生成された内容をユーザーが確認・承認するフローを維持することで、ガバナンスを確保

### フォールバック動作

GitHub Copilot SDK が利用できない環境では、`GenerateFallbackMetadata` が呼び出され、リポジトリ URL から基本的なメタデータのみを生成します（名前・バージョンなど最低限の情報のみ）。

---

## クイックスタート

### 前提条件

- .NET 10.0 SDK 以降
- Git
- このリポジトリへのアクセス権
- （デプロイ用）Azure サブスクリプションと API Center インスタンス

### セットアップ

1. リポジトリをクローン：
```bash
git clone https://github.com/tatatatamami/copilot-agent-mcp-registry.git
cd copilot-agent-mcp-registry
```

2. CLI ツールをビルド：
```bash
dotnet build
```

### MCP サーバーの登録

1. 登録アシスタントを起動：
```bash
dotnet run --project src/McpRegistration.Cli
```

2. MCPサーバーのリポジトリ URL を入力（これだけ！）：
```
Repository URL (where the MCP server is hosted): https://github.com/your-org/your-mcp-server
```

3. Copilot がリポジトリを解析し、メタデータを自動生成した後、確認を求めます：
```
🤖 Copilot: Generated metadata:
  name: your-mcp-server
  description: ...
  Would you like to proceed with registration? (yes/no)
Your answer: yes
```

4. 以下が自動的に実行されます：
   - ✅ メタデータのバリデーション
   - ✅ `apis/<mcp-name>/` への登録ファイル生成（metadata.json, openapi.json, README.md）
   - ✅ Git ブランチ作成・コミット

5. ブランチのプッシュと Pull Request 作成の確認が表示されます：
```
Push branch and create Pull Request? (y/n): y
```

6. Pull Request が承認・マージされると、GitHub Actions が自動で Azure API Center に登録します

---

## リポジトリ構成

```
├── .github/
│   ├── workflows/
│   │   └── register-to-api-center.yml    # 自動登録ワークフロー（PR マージ時に起動）
│   └── PULL_REQUEST_TEMPLATE/
│       └── mcp-registration.md           # MCP 登録用 PR テンプレート
├── apis/
│   └── <mcp-name>/                       # MCP サーバーごとのディレクトリ
│       ├── openapi.json                  # OpenAPI 3.0 仕様
│       ├── metadata.json                 # Azure API Center 用メタデータ
│       └── README.md                     # ドキュメント
├── catalog/
│   └── index.yaml                        # カタログインデックス（オプション）
├── src/
│   ├── McpRegistration.Cli/              # CLIツール（エントリポイント）
│   │   └── Program.cs
│   └── McpRegistration.Core/             # コアライブラリ
│       ├── Models/
│       │   └── McpMetadata.cs            # データモデル
│       └── Services/
│           ├── CopilotService.cs         # GitHub Copilot SDK 連携
│           ├── FileGenerationService.cs  # 登録ファイル生成
│           ├── GitService.cs             # Git 操作
│           └── ValidationService.cs      # バリデーション
└── README.md
```

---

## メタデータスキーマ

### 必須フィールド

| フィールド | 型 | 説明 | 例 |
|-----------|-----|------|-----|
| `name` | string | MCP サーバー名（小文字・ハイフン区切り） | `my-awesome-mcp` |
| `description` | string | 詳細説明（2〜3文） | `Provides data analysis capabilities` |
| `version` | string | セマンティックバージョン | `1.0.0` |
| `company` | string | 所有組織名 | `Contoso Ltd` |
| `owner` | string | オーナー名 | `Platform Team` |
| `status` | string | 現在のステータス | `active`, `deprecated`, `planned` |
| `lifecycle` | string | 開発ステージ | `production`, `development`, etc. |
| `authMethod` | string | 認証方式 | `api-key`, `oauth2`, `entra-id`, `none` |

### オプションフィールド

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `summary` | string | 1行サマリー（100文字以内） |
| `contactEmail` | string | 連絡先メールアドレス |
| `endpointUrl` | string | MCP サーバーのエンドポイント URL |
| `documentationUrl` | string | ドキュメント URL |
| `tags` | array | カテゴリタグ |
| `customProperties` | object | 追加カスタムプロパティ |
| `repositoryUrl` | string | MCP サーバーのソースリポジトリ URL |

---

## GitHub Actions セットアップ

### 必要なシークレット

GitHub リポジトリの設定で以下のシークレットを設定してください：

| シークレット | 説明 |
|------------|------|
| `AZURE_CLIENT_ID` | サービスプリンシパルのクライアント ID |
| `AZURE_CLIENT_SECRET` | サービスプリンシパルのクライアントシークレット |
| `AZURE_TENANT_ID` | Azure AD テナント ID |
| `AZURE_SUBSCRIPTION_ID` | Azure サブスクリプション ID |
| `AZURE_API_CENTER_NAME` | API Center インスタンス名 |
| `AZURE_RESOURCE_GROUP` | リソースグループ名 |

### Azure セットアップ

1. Azure API Center インスタンスを作成：
```bash
az apic create \
  --resource-group <resource-group> \
  --name <api-center-name> \
  --location <region>
```

2. GitHub Actions 用サービスプリンシパルを作成：
```bash
az ad sp create-for-rbac \
  --name "GitHub-Actions-API-Center" \
  --role "Contributor" \
  --scopes /subscriptions/<subscription-id>/resourceGroups/<resource-group> \
  --sdk-auth
```

---

## ライフサイクルステージ

| ステージ | 説明 |
|---------|------|
| `design` | 設計フェーズ |
| `development` | 開発中（新規 MCP のデフォルト） |
| `testing` | テスト・QA フェーズ |
| `preview` | パブリックプレビュー・ベータ |
| `production` | 本番稼働中（安定版） |
| `deprecated` | 廃止予定（引き続きサポートされるが非推奨） |
| `retired` | 廃止済み |

---

## セルフホスト型 API Center Portal

登録済み API を閲覧するためのポータルを設定する方法：

1. Microsoft Learn のガイドを参照: [Self-host API Center Portal](https://learn.microsoft.com/azure/api-center/self-host-api-center-portal)

2. ポータルを自分の API Center インスタンスに接続するよう設定

3. Azure App Service などのホスティングプラットフォームにデプロイ

ポータルでできること：
- 🔍 登録済み MCP の検索・発見
- 📖 API ドキュメントの閲覧
- 🏷️ メタデータ（組織・ライフサイクル・タグ）によるフィルタリング
- 📋 OpenAPI 仕様の参照

---

## 開発者向け情報

### ソリューションのビルド

```bash
dotnet build
```

### CLI のローカル実行

```bash
cd src/McpRegistration.Cli
dotnet run
```

---

## 参考リンク

- [Azure API Center ドキュメント](https://learn.microsoft.com/azure/api-center/)
- [GitHub Actions による API 登録](https://learn.microsoft.com/azure/api-center/register-apis-github-actions)
- [Self-host API Center Portal](https://learn.microsoft.com/azure/api-center/self-host-api-center-portal)
- [MCP（Model Context Protocol）仕様](https://modelcontextprotocol.io/)
- [GitHub Copilot SDK](https://www.nuget.org/packages/GitHub.Copilot.SDK)

---

## ライセンス

このプロジェクトは MIT ライセンスのもとで公開されています。詳細は LICENSE ファイルを参照してください。

---

**システム・オブ・レコード**: Azure API Center がすべての登録済み MCP サーバーの正規の情報源です。このリポジトリは登録インターフェースおよび監査証跡として機能します。
