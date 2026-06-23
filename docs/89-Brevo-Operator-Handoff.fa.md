# راهنمای راه‌اندازی Brevo برای ایمیل‌های Darwin Lingua

## هدف

این سند برای کارهای بیرون از کد است؛ یعنی کارهایی که باید در پنل Brevo، DNS دامنه، و تنظیمات محرمانه انجام شود تا ایمیل‌های واقعی Darwin Lingua ارسال شوند.

کد برنامه آماده‌ی ارسال ایمیل transactional از مسیر Brevo است. وضعیت 2026-06-23: API key و webhook secret بیرون از Git و در local secret configuration ذخیره شده‌اند، دامنه و sender طبق readiness check تأیید شده‌اند، DPA پذیرفته شده است، live API check بدون blocker/warning پاس شده، smoke مسیر واقعی ثبت‌نام/فراموشی رمز از خود Web هم در `WebEmailDeliveryLogs` با provider message id ثبت شده است، smoke لینک‌های واقعی Brevo-tracked ثابت کرده که تایید ایمیل، ریست پسورد، و تغییر ایمیل روی `https://darwinlingua.com` کامل می‌شوند، و smoke عمومی webhook ثابت کرده که endpoint عمومی `hardBounce` را با Bearer token می‌پذیرد، delivery log را failed می‌کند و suppression داخلی می‌سازد. قبل از باز کردن ثبت‌نام عمومی گسترده، تنها بررسی انسانیِ ایمیل این است که نمایش ظاهری ایمیل‌ها در inbox واقعی مثل Gmail/Outlook/mobile مرور شود.

دامنه‌ی اصلی Web برای تست و سپس production این است:

```text
https://darwinlingua.com
```

آدرس `https://www.darwinlingua.com` معیار تست نیست، مگر اینکه بعداً عمداً برای آن DNS و redirect تنظیم شود. دامنه‌ی API باید جداگانه روی `https://api.darwinlingua.com` منتشر شود.

## چیزی که داخل سیستم آماده است

- ارسال ایمیل از طریق `TransactionalEmail:Mode=BrevoApi`.
- ارسال محتوای HTML طراحی‌شده همراه نسخه‌ی plain text.
- استفاده از Brevo sandbox mode برای تست بدون ارسال واقعی.
- ثبت delivery log، provider message id، و وضعیت webhook.
- suppression داخلی برای bounce، spam، complaint، blocked و invalid email.
- endpoint webhook:

```text
https://darwinlingua.com/webhooks/brevo/transactional-email
```

- دو روش امن برای احراز هویت webhook:

```text
Authorization: Bearer <TransactionalEmail:BrevoWebhookSecret>
```

یا:

```text
X-DarwinLingua-Brevo-Webhook-Secret: <TransactionalEmail:BrevoWebhookSecret>
```

روش query-string فقط برای عیب‌یابی محلی است و در محیط واقعی نباید فعال باشد.

## کارهایی که باید در Brevo انجام شود

1. یک Brevo account متعلق به مالک عملیاتی Darwin Lingua بسازید یا حساب موجود را انتخاب کنید.
2. برای ارسال ایمیل transactional، دامنه‌ی `darwinlingua.com` را در Brevo authenticate کنید. Sender پیشنهادی:

```text
no-reply@darwinlingua.com
```

`support@darwinlingua.com` برای Reply-To و ارتباط پشتیبانی استفاده شود. اگر inbox جدا برای `no-reply` ساخته نمی‌شود، حداقل یک alias/forward برای آن به `info@darwinlingua.com` بسازید تا bounce یا خطای انسانی گم نشود.

3. در Brevo بخش domain authentication را باز کنید و دامنه‌ی ارسال را اضافه کنید.
4. رکوردهای DNS را دقیقاً از Brevo کپی کنید. مقدارهای SPF، DKIM و DMARC برای هر حساب و دامنه می‌توانند متفاوت باشند؛ نباید دستی حدس زده شوند.
5. در DNS دامنه، رکوردهایی را که Brevo داده است منتشر کنید.
6. صبر کنید تا Brevo دامنه و sender را verified نشان دهد.
7. یک sender transactional بسازید، مثلاً:

```text
From email: no-reply@<verified-domain>
From name: Darwin Lingua
Reply-to: support@<verified-domain>
```

8. یک API key مخصوص Darwin Lingua بسازید. این کلید نباید داخل Git یا چت ذخیره شود. باید در local/production secret configuration قرار بگیرد.
9. یک webhook secret قوی و تصادفی بسازید. همین مقدار هم در تنظیمات Darwin Lingua و هم در Brevo webhook authentication استفاده می‌شود.
10. یک transactional webhook در Brevo بسازید:

```text
URL: https://darwinlingua.com/webhooks/brevo/transactional-email
Method: POST
Authentication: Token
```

در UI فعلی Brevo برای webhook سه گزینه‌ی `No`, `Basic`, و `Token` دیده می‌شود. برای Darwin Lingua گزینه‌ی درست `Token` است. مقدار token باید دقیقاً همان `TransactionalEmail:BrevoWebhookSecret` باشد. طبق مستندات رسمی secure webhooks، Brevo در حالت token مقدار را به‌صورت Bearer token می‌فرستد، و کد برنامه همین مقدار را از هدر `Authorization: Bearer <token>` می‌خواند. `No` ناامن است و `Basic` با endpoint فعلی ما تطبیق ندارد.

11. برای webhook، eventهای transactional لازم را فعال کنید. در UI فعلی Brevo ابتدا `Event category` را روی `Transactional email` بگذارید و بعد همه‌ی eventهای موجود و مرتبط را فعال کنید:

```text
request / sent
delivered
deferred
softBounce / soft_bounce
hardBounce / hard_bounce
blocked
invalid / invalidEmail / invalid_email
spam
opened / uniqueOpened / unique_opened
click / clicked
unsubscribed
```

نکته‌ی مهم: مرجع رسمی API برای ساخت webhook نوع `transactional` این eventها را به‌عنوان مقدارهای معتبر نشان می‌دهد: `sent` یا `request`, `delivered`, `hardBounce`, `softBounce`, `blocked`, `spam`, `invalid`, `deferred`, `click`, `opened`, `uniqueOpened`, و `unsubscribed`. خود صفحه‌ی راهنمای transactional webhooks در Brevo همچنین نمونه/بخش‌هایی مثل `Error`, `Proxy Open`, و `Unique Proxy Open` را توضیح می‌دهد. اگر UI فعلی Brevo همه‌ی این نام‌ها را دقیقاً با همین label نشان نداد، همه‌ی eventهای موجود در همان دسته‌ی `Transactional email` را فعال کنید و بعد از Brevo logs یا API بررسی کنید که چه eventهایی واقعاً ثبت شده‌اند. انتظار دیدن `complaint` در همه‌ی فرم‌های transactional Brevo نداشته باشید؛ اگر provider چنین event یا reason مشابهی بفرستد، کد Darwin Lingua آن را برای suppression/diagnostics نرمال‌سازی می‌کند.

12. قرارداد پردازش داده یا DPA مربوط به Brevo را برای GDPR/EU operation بررسی و قبول کنید.

اگر Brevo API هنگام بررسی webhookها یا ارسال تست خطای `unrecognised IP address` داد، IP سرور یا ماشین اجرای تست را در Brevo از مسیر `https://app.brevo.com/security/authorised_ips` به Authorized IPs اضافه کنید. بدون این کار ممکن است API key درست باشد ولی Brevo درخواست را با 401 رد کند.

### منابع رسمی Brevo که مبنای تنظیمات بالا هستند

- `https://help.brevo.com/hc/en-us/articles/27824932835474-Create-outbound-webhooks-to-send-real-time-data-from-Brevo-to-an-external-app`
  - مسیر جدید UI را توضیح می‌دهد: `Integrations > Webhooks > Add webhook > Outbound webhook`.
  - برای outbound webhook سه حالت احراز هویت را می‌گوید: بدون احراز هویت، Basic authentication، یا Token authentication.
  - برای انتخاب event می‌گوید اول `Event category` انتخاب می‌شود و بعد eventهای همان دسته فعال/غیرفعال می‌شوند.
- `https://developers.brevo.com/docs/secured-webhooks`
  - روش secure webhook با Bearer token را برای `auth.type = bearer` و `token` توضیح می‌دهد. بنابراین برای Darwin Lingua در UI گزینه‌ی `Token` درست است و مقدار token باید همان secret ذخیره‌شده در برنامه باشد.
- `https://developers.brevo.com/docs/transactional-webhooks`
  - لیست eventهای transactional email را توضیح می‌دهد. در UI ممکن است labelها کمی متفاوت باشند، اما برای دسته‌ی `Transactional email` eventهای مهم برای Darwin Lingua همان delivery, bounce, blocked, spam/unsubscribe/open/click و invalid/deferred/error/proxy-open در صورت وجود هستند.
- `https://help.brevo.com/hc/en-us/articles/208858829-Review-your-transactional-email-reports`
  - تفاوت عملیاتی `Hard bounce`, `Soft bounce`, `Blocked`, و spam/complaint metrics را توضیح می‌دهد. در گزارش‌های Brevo ممکن است عبارت `Complaint` برای گزارش spam دیده شود، حتی اگر event انتخابی در webhook UI با label `Spam` یا نام نزدیک نمایش داده شود.

## تنظیماتی که بعد از Brevo باید در برنامه وارد شود

این مقدارها باید بیرون از source control تنظیم شوند:

```json
{
  "TransactionalEmail": {
    "Mode": "BrevoApi",
    "PublicBaseUrl": "https://darwinlingua.com",
    "ProductName": "Darwin Lingua",
    "FromEmail": "no-reply@darwinlingua.com",
    "FromName": "Darwin Lingua",
    "ReplyToEmail": "support@darwinlingua.com",
    "SupportEmail": "support@darwinlingua.com",
    "BrevoApiBaseUrl": "https://api.brevo.com",
    "BrevoApiKey": "<secret-from-brevo>",
    "BrevoWebhookSecret": "<strong-random-secret>",
    "BrevoSandboxMode": false,
    "BrevoAllowQuerySecretFallback": false
  }
}
```

در توسعه می‌توان `BrevoSandboxMode=true` گذاشت تا Brevo درخواست را اعتبارسنجی کند ولی ایمیل واقعی نفرستد. برای تست واقعی inbox باید مقدار آن `false` باشد.

برای وارد کردن secretها در همین محیط توسعه، از helper زیر استفاده کنید تا کلیدها داخل ASP.NET Core user-secrets ذخیره شوند و وارد Git نشوند:

```powershell
.\scripts\Set-Web-LocalSecrets.ps1 `
  -ConfigureBrevo `
  -PublicBaseUrl "https://darwinlingua.com" `
  -BrevoSandboxMode $false `
  -BrevoAllowQuerySecretFallback $false
```

این command به صورت امن API key و webhook secret را prompt می‌کند. اگر می‌خواهید non-interactive اجرا شود، فقط در همان session مقدارهای زیر را به عنوان environment variable بگذارید و بعد command را با `-UseEnvironment` اجرا کنید:

```powershell
$env:DARWINLINGUA_BREVO_API_KEY = "<secret-from-brevo>"
$env:DARWINLINGUA_BREVO_WEBHOOK_SECRET = "<strong-random-secret>"

.\scripts\Set-Web-LocalSecrets.ps1 `
  -ConfigureBrevo `
  -UseEnvironment `
  -PublicBaseUrl "https://darwinlingua.com" `
  -BrevoSandboxMode $false `
  -BrevoAllowQuerySecretFallback $false
```

بعد از تغییر secretها باید `DarwinLingua.Web` restart شود، چون تنظیمات ایمیل هنگام startup خوانده می‌شوند.

## چیزهایی که باید به توسعه‌دهنده/اپراتور سیستم داده شود

- نام حساب یا سازمان در Brevo.
- دامنه یا زیردامنه‌ی انتخاب‌شده برای ارسال.
- From email و Reply-to email نهایی.
- تأیید اینکه DNS records داخل Brevo verified شده‌اند.
- API key، فقط از مسیر امن تنظیمات محرمانه.
- webhook secret، فقط از مسیر امن تنظیمات محرمانه.
- تأیید اینکه DPA/data-processing terms بررسی و پذیرفته شده‌اند.
- تصمیم نهایی برای اینکه self-registration در تست کاربر فعال باشد یا فعلاً حساب‌ها از قبل ساخته شوند.
- خروجی readiness check بعد از وارد کردن secretها، مخصوصاً تعداد blockerها و فایل report ساخته‌شده.

## تست بعد از تنظیم Brevo

بعد از کامل شدن Brevo و secretها، این ابزار را اجرا کنید:

```powershell
.\tools\Web\Invoke-BrevoProductionReadinessCheck.ps1 `
  -ConfigPath .\src\Apps\DarwinLingua.Web\appsettings.Development.Local.json `
  -SendingDomain "<verified-domain>" `
  -SenderVerified `
  -DnsAuthenticated `
  -WebhookConfigured `
  -DpaAccepted `
  -RequireRealDelivery `
  -VerifyBrevoApi
```

اگر هنوز نمی‌خواهید DNS lookup انجام شود و فقط وضعیت verified داخل Brevo را ملاک می‌گیرید، موقتاً `-SkipDnsLookup` هم اضافه کنید. برای تصمیم نهایی production بهتر است DNS واقعی هم بررسی شود. اگر گزارش با `brevo.accountApi` و متن `unrecognised IP address` شکست خورد، IP نمایش داده‌شده در همان گزارش را در Brevo از مسیر `https://app.brevo.com/security/authorised_ips` اضافه کنید و همین command را دوباره اجرا کنید.

بعد از pass شدن readiness check:

اول smoke کنترل‌شده‌ی ارسال واقعی را اجرا کنید. این command بدون `-ConfirmSend` هیچ ایمیل واقعی نمی‌فرستد:

```powershell
.\tools\Web\Invoke-BrevoRealDeliverySmoke.ps1 `
  -RecipientEmail "info@darwinlingua.com" `
  -SenderVerified `
  -DnsAuthenticated `
  -WebhookConfigured `
  -DpaAccepted `
  -ConfirmSend
```

این ابزار قبل از ارسال، همان readiness check را با `-VerifyBrevoApi` اجرا می‌کند. اگر هنوز IP در Brevo مجاز نشده باشد، ارسال را متوقف می‌کند و گزارش را در مسیر زیر می‌نویسد:

```text
artifacts/validation/brevo-real-delivery-smoke/
```

بعد از smoke موفق:

1. هر دو ایمیل smoke باید در inbox واقعی دیده شوند: یکی برای email confirmation و یکی برای password reset.
2. HTML email در Gmail/Outlook/mobile inbox بررسی شود.
3. plain text alternative هم قابل خواندن باشد.
4. Brevo transactional logs باید message id و delivery status را نشان دهد.
5. یک webhook event یا manual provider event باید در Admin Email Diagnostics دیده شود.
6. bounce/spam/complaint باید suppression داخلی بسازد.

برای smoke واقعی از خود UI، ابزار زیر را اجرا کنید:

```powershell
.\tools\Web\Invoke-WebAccountEmailFlowSmoke.ps1
```

برای تست کامل لینک‌های واقعی داخل ایمیل‌های ارسال‌شده توسط Brevo، ابزار زیر اجرا شده و باید سبز بماند:

```powershell
.\tools\Web\Invoke-WebAccountEmailLinkSmoke.ps1
```

این ابزار لینک کامل، token، reset code، API key، webhook secret یا provider message id کامل را در گزارش ذخیره نمی‌کند. فقط hash کوتاه و نتیجه‌ی pass/fail را ثبت می‌کند. مسیر evidence فعلی:

```text
artifacts/validation/web-account-email-link-smoke/web-account-email-link-smoke-20260623-153300.md
```

این ابزار از صفحه‌ی public ثبت‌نام و صفحه‌ی public فراموشی رمز استفاده می‌کند، anti-forgery token را از HTML می‌خواند، یک کاربر تستی timestamped می‌سازد، برای یک کاربر موجود password reset می‌فرستد، و سپس در PostgreSQL بررسی می‌کند که `WebEmailDeliveryLogs` برای `Account.EmailConfirmation` و `Account.PasswordReset` provider برابر `brevo-api` و provider message id داشته باشد. گزارش آن در این مسیر ذخیره می‌شود:

```text
artifacts/validation/web-account-email-smoke/
```

برای تست کنترل‌شده‌ی webhook و suppression داخلی، ابزار زیر را اجرا کنید:

```powershell
.\tools\Web\Invoke-BrevoWebhookSuppressionSmoke.ps1
```

این ابزار هیچ ایمیلی به گیرنده‌ی واقعی نمی‌فرستد. یک delivery log ساختگی با recipient hash کنترل‌شده می‌سازد، یک event از نوع `hardBounce` را به endpoint عمومی زیر با `Authorization: Bearer <token>` می‌فرستد، و بعد در PostgreSQL بررسی می‌کند که همان delivery log به `Failed` تغییر کرده و یک suppression داخلی با reason برابر `brevo:hard_bounce` ساخته شده است:

```text
https://darwinlingua.com/webhooks/brevo/transactional-email
```

گزارش این smoke در مسیر زیر ذخیره می‌شود:

```text
artifacts/validation/brevo-webhook-suppression-smoke/
```

بعد از آن، برای اطمینان از اینکه ارسال بعدی به گیرنده‌ی suppress شده واقعاً به Brevo فرستاده نمی‌شود، ابزار زیر را اجرا کنید:

```powershell
.\tools\Web\Invoke-BrevoSuppressedSendSmoke.ps1
```

این ابزار برای یک حساب تأییدشده یک suppression موقت می‌سازد، فرم public فراموشی رمز را submit می‌کند، بررسی می‌کند که در `WebEmailDeliveryLogs` یک رکورد با وضعیت `Suppressed` و `FailureCode=recipient-suppressed` ساخته شده و provider message id ندارد، و در پایان suppression موقتی را که خودش ساخته پاک می‌کند. گزارش آن در مسیر زیر ذخیره می‌شود:

```text
artifacts/validation/brevo-suppressed-send-smoke/
```

برای بررسی UI ادمین و اینکه provider message id، provider event، suppression و وضعیت readiness واقعاً در صفحه‌ی admin دیده می‌شوند، smoke زیر را اجرا کنید:

```powershell
.\tools\Web\Invoke-WebEmailDiagnosticsAdminSmoke.ps1 -UseLocalDevelopmentSeed
```

این ابزار با admin seed محلی وارد می‌شود، صفحه‌ی `/admin/email-diagnostics` را با فیلترهای واقعی باز می‌کند و بررسی می‌کند که Brevo readiness، provider message id، provider event، suppression data و کنترل‌های admin-only در UI دیده شوند. گزارش آن در مسیر زیر ذخیره می‌شود:

```text
artifacts/validation/web-email-diagnostics-admin-smoke/
```

## تصمیم پیشنهادی فعلی

در این مرحله از توسعه، پیشنهاد این است:

- ایمیل‌ها با HTML inline داخل کد Darwin Lingua طراحی و نسخه‌بندی شوند.
- از Brevo فقط برای ارسال، deliverability، log، webhook و suppression استفاده شود.
- فعلاً از Brevo templateId استفاده نشود، چون مدیریت دستی بیرون از Git اضافه می‌کند و تست‌پذیری تغییرات ایمیل را پایین می‌آورد.
- دامنه‌ی اصلی انتخاب شده است: `darwinlingua.com`. از این به بعد sender domain، `PublicBaseUrl`، legal pages و webhook URL باید روی همین دامنه بررسی شوند.

## منابع رسمی Brevo

- Transactional email API: `https://developers.brevo.com/docs/send-a-transactional-email`
- Send transactional email endpoint: `https://developers.brevo.com/reference/send-transac-email`
- Transactional webhooks: `https://developers.brevo.com/docs/transactional-webhooks`
- Secured webhooks: `https://developers.brevo.com/docs/secured-webhooks`
- Sandbox mode: `https://developers.brevo.com/docs/using-sandbox-mode`
- Transactional reports: `https://help.brevo.com/hc/en-us/articles/208858829-Review-your-transactional-email-reports`
