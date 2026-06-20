# راهنمای راه‌اندازی Brevo برای ایمیل‌های Darwin Lingua

## هدف

این سند برای کارهای بیرون از کد است؛ یعنی کارهایی که باید در پنل Brevo، DNS دامنه، و تنظیمات محرمانه انجام شود تا ایمیل‌های واقعی Darwin Lingua ارسال شوند.

کد برنامه آماده‌ی ارسال ایمیل transactional از مسیر Brevo است، اما تا وقتی دامنه، فرستنده، API key، webhook و قرارداد پردازش داده در Brevo کامل نشده باشند، نباید ثبت‌نام آزاد یا بازیابی رمز عبور را به کاربران تستی واقعی بسپاریم.

در مرحله‌ی توسعه، آدرس عمومی موقت سیستم این است:

```text
https://lingua.vafadar.pro
```

آدرس `https://www.lingua.vafadar.pro` معیار تست نیست، مگر اینکه بعداً عمداً برای آن DNS و redirect تنظیم شود.

## چیزی که داخل سیستم آماده است

- ارسال ایمیل از طریق `TransactionalEmail:Mode=BrevoApi`.
- ارسال محتوای HTML طراحی‌شده همراه نسخه‌ی plain text.
- استفاده از Brevo sandbox mode برای تست بدون ارسال واقعی.
- ثبت delivery log، provider message id، و وضعیت webhook.
- suppression داخلی برای bounce، spam، complaint، blocked و invalid email.
- endpoint webhook:

```text
https://lingua.vafadar.pro/webhooks/brevo/transactional-email
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
2. برای فاز توسعه، یک دامنه یا زیردامنه‌ی ارسال انتخاب کنید. مثال‌های مناسب:

```text
mail.lingua.vafadar.pro
no-reply@lingua.vafadar.pro
```

مقدار دقیق باید با مالک DNS هماهنگ شود. اگر بعداً دامنه‌ی اصلی محصول عوض شود، تنظیمات sender، DNS و webhook هم باید همزمان عوض شوند.

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
URL: https://lingua.vafadar.pro/webhooks/brevo/transactional-email
Method: POST
Authentication: Bearer token یا custom header
```

11. برای webhook، eventهای transactional لازم را فعال کنید:

```text
request / sent
delivered
deferred
softBounce / soft_bounce
hardBounce / hard_bounce
blocked
invalid / invalid_email
spam
complaint
opened / uniqueOpened
click
unsubscribed
```

اگر نام event در پنل Brevo کمی متفاوت بود، نزدیک‌ترین event رسمی transactional را انتخاب کنید. برنامه چند نام رایج Brevo را می‌پذیرد.

12. قرارداد پردازش داده یا DPA مربوط به Brevo را برای GDPR/EU operation بررسی و قبول کنید.

## تنظیماتی که بعد از Brevo باید در برنامه وارد شود

این مقدارها باید بیرون از source control تنظیم شوند:

```json
{
  "TransactionalEmail": {
    "Mode": "BrevoApi",
    "PublicBaseUrl": "https://lingua.vafadar.pro",
    "ProductName": "Darwin Lingua",
    "FromEmail": "no-reply@<verified-domain>",
    "FromName": "Darwin Lingua",
    "ReplyToEmail": "support@<verified-domain>",
    "SupportEmail": "support@<verified-domain>",
    "BrevoApiBaseUrl": "https://api.brevo.com",
    "BrevoApiKey": "<secret-from-brevo>",
    "BrevoWebhookSecret": "<strong-random-secret>",
    "BrevoSandboxMode": false,
    "BrevoAllowQuerySecretFallback": false
  }
}
```

در توسعه می‌توان `BrevoSandboxMode=true` گذاشت تا Brevo درخواست را اعتبارسنجی کند ولی ایمیل واقعی نفرستد. برای تست واقعی inbox باید مقدار آن `false` باشد.

## چیزهایی که باید به توسعه‌دهنده/اپراتور سیستم داده شود

- نام حساب یا سازمان در Brevo.
- دامنه یا زیردامنه‌ی انتخاب‌شده برای ارسال.
- From email و Reply-to email نهایی.
- تأیید اینکه DNS records داخل Brevo verified شده‌اند.
- API key، فقط از مسیر امن تنظیمات محرمانه.
- webhook secret، فقط از مسیر امن تنظیمات محرمانه.
- تأیید اینکه DPA/data-processing terms بررسی و پذیرفته شده‌اند.
- تصمیم نهایی برای اینکه self-registration در تست کاربر فعال باشد یا فعلاً حساب‌ها از قبل ساخته شوند.

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
  -RequireRealDelivery
```

بعد از pass شدن readiness check:

1. یک کاربر تستی ثبت‌نام کند و ایمیل تأیید دریافت شود.
2. لینک تأیید باید از `https://lingua.vafadar.pro` شروع شود.
3. password reset روی یک inbox واقعی تست شود.
4. HTML email در Gmail/Outlook/mobile inbox بررسی شود.
5. plain text alternative هم قابل خواندن باشد.
6. Brevo transactional logs باید message id و delivery status را نشان دهد.
7. یک webhook event یا manual provider event باید در Admin Email Diagnostics دیده شود.
8. bounce/spam/complaint باید suppression داخلی بسازد.

## تصمیم پیشنهادی فعلی

در این مرحله از توسعه، پیشنهاد این است:

- ایمیل‌ها با HTML inline داخل کد Darwin Lingua طراحی و نسخه‌بندی شوند.
- از Brevo فقط برای ارسال، deliverability، log، webhook و suppression استفاده شود.
- فعلاً از Brevo templateId استفاده نشود، چون مدیریت دستی بیرون از Git اضافه می‌کند و تست‌پذیری تغییرات ایمیل را پایین می‌آورد.
- وقتی Web به بلوغ رسید و دامنه‌ی اصلی انتخاب شد، sender domain، `PublicBaseUrl`، legal pages و webhook URL در یک release step واحد به دامنه‌ی اصلی منتقل شوند.

## منابع رسمی Brevo

- Transactional email API: `https://developers.brevo.com/docs/send-a-transactional-email`
- Send transactional email endpoint: `https://developers.brevo.com/reference/send-transac-email`
- Transactional webhooks: `https://developers.brevo.com/docs/transactional-webhooks`
- Secured webhooks: `https://developers.brevo.com/docs/secured-webhooks`
- Sandbox mode: `https://developers.brevo.com/docs/using-sandbox-mode`
- Transactional reports: `https://help.brevo.com/hc/en-us/articles/208858829-Review-your-transactional-email-reports`
