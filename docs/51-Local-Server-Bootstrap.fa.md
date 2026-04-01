# راهنمای فارسی راه‌اندازی اولیه سرور محلی

## هدف

این سند توضیح می‌دهد که چطور سرور محلی Darwin Lingua را برای اولین بار بالا بیاوری تا:

- دیتابیس PostgreSQL محلی آماده شود
- Web API به آن وصل شود
- محتوای اولیه از فایل‌های JSON وارد دیتابیس اصلی شود
- اولین batch منتشرشده برای آپدیت اپ موبایل ساخته شود

این سند برای زمانی است که PostgreSQL را با Docker Desktop بالا آورده‌ای و می‌خواهی کل مسیر import و publish را به‌صورت محلی تست کنی.

---

## قبل از شروع

قبل از اجرای این مسیر، این موارد باید آماده باشند:

1. Docker Desktop نصب و اجرا شده باشد.
2. PostgreSQL طبق سند [49-Local-Postgres-Setup.md](/D:/_Projects/DarwinLingua/docs/49-Local-Postgres-Setup.md) بالا آمده باشد.
3. حداقل یکی از این فایل‌های تنظیمات Web API وجود داشته باشد:
   - فایل پیشنهادی و local-only:
     - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
   - فایل fallback قابل قبول:
     - `src/Apps/DarwinLingua.WebApi/appsettings.Development.json`
4. رمزهای واقعی PostgreSQL فقط در فایل تنظیمات فعال و در فایل زیر قرار گرفته باشند:
   - `tools/Server/Postgres/.env`
5. فایل یا فولدر محتوای JSON آماده باشد، مثلاً:
   - `D:\_Projects\DarwinLingua.Content\A1.json`
   - یا:
   - `D:\_Projects\DarwinLingua.Content`

---

## نکته مهم درباره فایل تنظیمات فعال

اسکریپت بوت‌استرپ الان هر دو حالت را قبول می‌کند:

- `appsettings.Development.Local.json`
- `appsettings.Development.json`

اما پیشنهاد بهتر برای کار محلی این است که از فایل local-only استفاده کنی تا secretها وارد git نشوند.

اگر فایل local-only را می‌سازی:

1. از این فایل کپی بگیر:
   - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.example.json`
2. با این نام ذخیره کن:
   - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
3. connection string و password واقعی PostgreSQL را داخل همان فایل بگذار.

---

## این اسکریپت دقیقاً چه کاری انجام می‌دهد

اسکریپت زیر:

- `tools/Server/Initialize-LocalServerContent.ps1`

این مراحل را انجام می‌دهد:

1. چک می‌کند که یک فایل تنظیمات معتبر برای Web API وجود داشته باشد.
2. اگر خواسته باشی، خود Web API را اجرا می‌کند.
3. صبر می‌کند تا endpoint سلامت (`/health`) پاسخ بدهد.
4. اگر `ContentPath` یک فایل باشد، همان فایل را import می‌کند.
5. اگر `ContentPath` یک فولدر باشد، همه فایل‌های `json` داخل آن را import می‌کند.
6. آخرین draft batch ساخته‌شده را publish می‌کند.
7. خلاصه‌ای از import و packageهای ساخته‌شده چاپ می‌کند.

فقط بالا آمدن Web API برای ساخت tableهای لازم کافی است، چون startup فعلی سرور خودش initialization این بخش‌ها را انجام می‌دهد:

- tableهای shared catalog
- tableهای metadata مربوط به server content
- tableهای publication audit

---

## دستور پیشنهادی

اگر Web API هنوز اجرا نشده:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content"
```

اگر Web API از قبل در حال اجراست:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ContentPath "D:\_Projects\DarwinLingua.Content"
```

اگر فقط می‌خواهی یک فایل را تست کنی:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content\A1.json"
```

همه این دستورها را از ریشه‌ی repo اجرا کن:

- `D:\_Projects\DarwinLingua`

---

## نتیجه‌ای که باید ببینی

در پایان اجرای موفق این flow باید این وضعیت برقرار باشد:

- tableهای لازم در PostgreSQL ساخته شده باشند
- محتوای catalog داخل دیتابیس اصلی سرور وارد شده باشد
- یک batch منتشرشده برای محصول `darwin-deutsch` وجود داشته باشد
- فایل‌های package در این مسیر ساخته شده باشند:
  - `assets/ServerContent/PublishedPackages`

---

## چطور سریع بررسی کنی که همه‌چیز درست بالا آمده

### 1. بررسی سلامت Web API

مرورگر یا ابزار HTTP را باز کن و این آدرس را بزن:

- [http://localhost:5099/health](http://localhost:5099/health)

انتظار:

- پاسخ `200`
- بدون خطای connection

### 2. بررسی manifest موبایل

- [http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch](http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch)

انتظار:

- پاسخ `200`
- metadata مربوط به packageهای منتشرشده برگردد

### 3. بررسی history ادمین

- [http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch](http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch)

انتظار:

- پاسخ `200`
- آخرین batch با وضعیت `Published` دیده شود

---

## اگر خطا رخ داد، اول این‌ها را چک کن

### خطا: فایل تنظیمات محلی پیدا نشد

اول چک کن که حداقل یکی از این فایل‌ها وجود داشته باشد:

- `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
- `src/Apps/DarwinLingua.WebApi/appsettings.Development.json`

### خطا: اتصال دیتابیس برقرار نشد

معمولاً یکی از این‌ها اشتباه است:

- password داخل `tools/Server/Postgres/.env`
- password داخل فایل تنظیمات فعال Web API
- host یا port دیتابیس

### خطا: build یا run روی WebApi.exe قفل می‌شود

احتمالاً یک instance قبلی از Web API هنوز باز است. قبل از اجرای دوباره، instance قبلی را ببند.

---

## بعد از این مرحله چه کاری باقی می‌ماند

بعد از اینکه این bootstrap کامل شد، کار بعدی این است که:

1. اپ موبایل را به Web API زنده وصل کنی
2. آپدیت محتوا را از داخل Settings تست کنی
3. worksheet مربوط به Phase 5 را اجرا کنی:
   - [50-Phase-5-Remote-Update-Validation-Worksheet.md](/D:/_Projects/DarwinLingua/docs/50-Phase-5-Remote-Update-Validation-Worksheet.md)

اگر می‌خواهی کل مسیر را یک‌جا تست کنی، سند اصلی کامل‌تر این است:

- [53-Manual-System-Test-Runbook.fa.md](/D:/_Projects/DarwinLingua/docs/53-Manual-System-Test-Runbook.fa.md)
