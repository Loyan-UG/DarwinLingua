# راهنمای فارسی تست دستی کامل سیستم

## هدف

این سند مسیر کامل و یکپارچه‌ی تست دستی سیستم Darwin Lingua را توضیح می‌دهد.

با استفاده از این سند می‌توانی:

- PostgreSQL محلی را بالا بیاوری
- Web API محلی را اجرا کنی
- محتوا را به دیتابیس اصلی سرور وارد کنی
- آن محتوا را publish کنی
- اپ موبایل را به Web API زنده وصل کنی
- sync محتوا از سرور به اپ را تست کنی
- flowهای اصلی اپ را بعد از sync بررسی کنی
- نتیجه‌ی مورد انتظار هر مرحله را دقیق بدانی

این سند برای اجرای واقعی طراحی شده، نه فقط مرور معماری.

---

## این سند چه چیزهایی را پوشش می‌دهد

این runbook این موارد را پوشش می‌دهد:

- Docker Desktop و PostgreSQL محلی
- Web API محلی
- import و publish محتوا در سرور
- download و sync محتوا در اپ موبایل
- بررسی browse, search, word detail, favorites, practice
- بررسی failure behavior

این سند جای worksheetهای فازها را کامل نمی‌گیرد، ولی بهترین مسیر اصلی برای اجرای end-to-end است.

---

## پیش‌نیازها

قبل از شروع مطمئن شو که این موارد آماده هستند:

- Docker Desktop نصب و در حال اجرا باشد
- WSL 2 فعال باشد
- `.NET 10 SDK` نصب باشد
- workloadهای MAUI نصب باشند
- یک Android emulator یا دستگاه تست در اختیار داشته باشی
- فایل‌های JSON بازبینی‌شده‌ی محتوا آماده باشند، مثلاً:
  - `D:\_Projects\DarwinLingua.Content`

پیشنهادی:

- Visual Studio برای deploy راحت‌تر MAUI
- pgAdmin برای بررسی چشمی دیتابیس PostgreSQL

---

## فایل‌هایی که باید آماده کنی

### 1. فایل env مربوط به PostgreSQL

این فایل را بساز:

- `tools/Server/Postgres/.env`

از روی:

- `tools/Server/Postgres/.env.example`

همه passwordهای نمونه را عوض کن.

### 2. فایل تنظیمات محلی Web API

در حال حاضر bootstrap محلی با هر دو فایل زیر کار می‌کند:

- فایل local-only پیشنهادی:
  - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
- فایل fallback قابل قبول:
  - `src/Apps/DarwinLingua.WebApi/appsettings.Development.json`

اگر بخواهی secretها داخل git نروند، بهتر است فایل local-only را بسازی.

برای این کار:

1. از این فایل کپی بگیر:
   - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.example.json`
2. آن را با این نام ذخیره کن:
   - `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
3. password و connection string واقعی PostgreSQL را وارد کن.

### 3. فایل یا فولدر محتوای JSON

حداقل یک فایل reviewشده آماده کن، مثلاً:

- `D:\_Projects\DarwinLingua.Content\A1.json`

یا یک فولدر کامل:

- `D:\_Projects\DarwinLingua.Content`

---

## خلاصه مسیر تست

ترتیب پیشنهادی اجرا:

1. PostgreSQL را بالا بیاور
2. PostgreSQL را verify کن
3. Web API و دیتابیس سرور را bootstrap کن
4. محتوا را در دیتابیس اصلی import کن
5. batch سروری را publish کن
6. endpointهای manifest و download را تست کن
7. اپ موبایل را اجرا کن
8. از داخل اپ محتوا را sync کن
9. flowهای اصلی اپ را تست کن
10. دوباره محتوا را تغییر بده و round trip را تست کن
11. failure behavior را هم تست کن

---

## مرحله 1: بالا آوردن PostgreSQL

از ریشه‌ی repo این دستور را اجرا کن:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml up -d
```

### نتیجه‌ی مورد انتظار

- PostgreSQL بدون خطا بالا بیاید
- اگر pgAdmin هم در compose فعال است، آن هم بالا بیاید
- خطای conflict روی پورت `5432` نداشته باشی

### برای بررسی وضعیت

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml ps
```

### نتیجه‌ی مورد انتظار

- container مربوط به PostgreSQL در وضعیت `running` باشد
- container مربوط به pgAdmin هم در صورت فعال‌بودن `running` باشد

---

## مرحله 2: بررسی PostgreSQL

این دستور را اجرا کن:

```powershell
docker exec -it darwinlingua-postgres psql -U postgres -d darwinlingua_shared -c "\dt"
```

### نتیجه‌ی مورد انتظار

- اتصال با موفقیت برقرار شود
- اگر اولین بار است، ممکن است هنوز table خاصی نبینی
- نباید خطای authentication یا connection داشته باشی

اگر authentication fail شد، دو جا را بررسی کن:

- `tools/Server/Postgres/.env`
- فایل تنظیمات فعال Web API

passwordها باید با هم سازگار باشند.

---

## مرحله 3: bootstrap کردن سرور محلی

اگر Web API هنوز اجرا نشده:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content"
```

اگر Web API از قبل اجرا شده:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ContentPath "D:\_Projects\DarwinLingua.Content"
```

اگر فقط می‌خواهی با یک فایل تست کنی:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -StartWebApi -ContentPath "D:\_Projects\DarwinLingua.Content\A1.json"
```

### این مرحله دقیقاً چه کار می‌کند

اسکریپت:

1. چک می‌کند فایل تنظیمات معتبر Web API وجود داشته باشد
2. در صورت نیاز Web API را اجرا می‌کند
3. منتظر می‌ماند تا `/health` پاسخ بدهد
4. یک فایل یا همه فایل‌های `json` را import می‌کند
5. draft batch می‌سازد
6. آخرین draft را publish می‌کند
7. summary کوتاه چاپ می‌کند

### نتیجه‌ی مورد انتظار

- tableهای PostgreSQL اگر وجود نداشته باشند ساخته می‌شوند
- داده‌های catalog وارد دیتابیس اصلی سرور می‌شوند
- یک published batch ساخته می‌شود
- فایل‌های package در این مسیر ساخته می‌شوند:
  - `assets/ServerContent/PublishedPackages`

### اگر این مرحله fail شد

اول این موارد را چک کن:

- فایل تنظیمات فعال Web API وجود داشته باشد
- password دیتابیس در آن فایل با `.env` یکسان باشد
- process دیگری `DarwinLingua.WebApi.exe` را lock نکرده باشد

---

## مرحله 4: بررسی اینکه data واقعاً روی سرور وجود دارد

### 4.1 بررسی health

این آدرس را باز کن:

- [http://localhost:5099/health](http://localhost:5099/health)

### نتیجه‌ی مورد انتظار

- پاسخ `200`
- پاسخ سالم بدون خطای داخلی

### 4.2 بررسی manifest موبایل

این آدرس را باز کن:

- [http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch](http://localhost:5099/api/mobile/content/manifest?clientProductKey=darwin-deutsch)

### نتیجه‌ی مورد انتظار

- پاسخ `200`
- metadata مربوط به packageهای منتشرشده برگردد
- packageهای draft دیده نشوند

### 4.3 بررسی draftها و batchها

این آدرس را باز کن:

- [http://localhost:5099/api/admin/content/catalog/drafts?clientProductKey=darwin-deutsch](http://localhost:5099/api/admin/content/catalog/drafts?clientProductKey=darwin-deutsch)

### نتیجه‌ی مورد انتظار

- پاسخ `200`
- حداقل یک batch دیده شود

### 4.4 بررسی history

این آدرس را باز کن:

- [http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch](http://localhost:5099/api/admin/content/catalog/history?clientProductKey=darwin-deutsch)

### نتیجه‌ی مورد انتظار

- پاسخ `200`
- آخرین batch با وضعیت `Published` دیده شود

### 4.5 بررسی download package

این آدرس را باز کن:

- [http://localhost:5099/api/mobile/content/download/full?clientProductKey=darwin-deutsch&clientSchemaVersion=1](http://localhost:5099/api/mobile/content/download/full?clientProductKey=darwin-deutsch&clientSchemaVersion=1)

### نتیجه‌ی مورد انتظار

- پاسخ `200`
- بدنه‌ی JSON package را ببینی

---

## مرحله 5: بررسی مستقیم دیتابیس سرور

اگر بخواهی مطمئن شوی import واقعاً در دیتابیس اصلی انجام شده، این queryها را در pgAdmin یا `psql` اجرا کن:

```sql
select count(*) from "WordEntries";
select count(*) from "ContentPackages";
select count(*) from "PublishedPackages";
select count(*) from "ClientProducts";
```

### نتیجه‌ی مورد انتظار

- `WordEntries` بزرگ‌تر از `0` باشد
- `ContentPackages` بزرگ‌تر از `0` باشد
- `PublishedPackages` بزرگ‌تر از `0` باشد
- `ClientProducts` حداقل یک رکورد برای `darwin-deutsch` داشته باشد

---

## مرحله 6: اجرای اپ موبایل در برابر Web API زنده

### 6.1 اگر لازم بود، Web API را دستی اجرا کن

```powershell
dotnet run --project .\src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj
```

### نتیجه‌ی مورد انتظار

- API بدون خطای connection به دیتابیس بالا بیاید

### 6.2 اپ موبایل را deploy کن

اپ MAUI را روی emulator یا دستگاه واقعی اجرا کن.

نکته مهم در dev محلی:

- Android emulator باید از این آدرس استفاده کند:
  - `http://10.0.2.2:5099`
- بقیه targetهای محلی معمولاً از این آدرس استفاده می‌کنند:
  - `http://localhost:5099`

### نتیجه‌ی مورد انتظار

- اپ بدون crash باز شود
- صفحه Settings بدون مشکل باز شود

---

## مرحله 7: تست sync محتوا از سرور به اپ

به این بخش برو:

- `Settings`

باید این بخش‌ها را ببینی:

- `Update All Content`
- `Word catalog`
- دکمه‌های جدا برای:
  - `A1`
  - `A2`
  - `B1`
  - `B2`
  - `C1`
  - `C2`

### 7.1 تست آپدیت کامل

مراحل:

1. `Settings` را باز کن
2. diagnostics آپدیت کامل را ببین
3. `Update All Content` را بزن
4. منتظر بمان تا کامل شود

### نتیجه‌ی مورد انتظار

- اپ crash نکند
- پیام موفقیت نشان داده شود
- diagnostics به آخرین package منتشرشده تغییر کند
- محتوای جدید در browse/search/detail قابل مشاهده شود

### 7.2 تست آپدیت catalog

مراحل:

1. در `Settings` بخش catalog را پیدا کن
2. دکمه آپدیت آن را بزن

### نتیجه‌ی مورد انتظار

- اپ crash نکند
- آپدیت کامل شود
- diagnostics مربوط به catalog تغییر کند

### 7.3 تست آپدیت یک سطح CEFR

مراحل:

1. در `Settings` دکمه `A1` را پیدا کن
2. آن را بزن

### نتیجه‌ی مورد انتظار

- اپ crash نکند
- diagnostics مربوط به `A1` آپدیت شود
- محتوای `A1` در browse قابل مشاهده باشد

---

## مرحله 8: تست flowهای اصلی اپ

بعد از اینکه حداقل یک آپدیت موفق داشتی، این صفحه‌ها را تست کن.

### 8.1 Home

مراحل:

1. `Home` را باز کن
2. header و shortcutهای CEFR را بررسی کن

### نتیجه‌ی مورد انتظار

- صفحه سریع باز شود
- blank state ناشی از نبود محتوا نبینی

### 8.2 CEFR Browse

مراحل:

1. روی `A1` بزن
2. منتظر بمان تا اولین کلمه باز شود
3. `Next` را بزن
4. `Previous` را بزن
5. `List` را بزن

### نتیجه‌ی مورد انتظار

- اولین کلمه سریع‌تر از flow قدیمی باز شود
- جابه‌جایی next/previous درست کار کند
- وقتی دوباره همان سطح را باز می‌کنی از آخرین کلمه‌ی دیده‌شده شروع شود
- باز شدن list باعث freeze طولانی نشود

### 8.3 Search

مراحل:

1. `Search` را باز کن
2. یک کلمه‌ی واردشده را جستجو کن
3. روی نتیجه بزن

### نتیجه‌ی مورد انتظار

- نتیجه‌ها ظاهر شوند
- با زدن روی نتیجه، صفحه word detail باز شود
- خطای نامناسب یا crash نبینی

### 8.4 Topic Browse

مراحل:

1. `Browse` را باز کن
2. یک topic را انتخاب کن
3. در list اسکرول کن

### نتیجه‌ی مورد انتظار

- list باز شود
- در اسکرول، آیتم‌ها ادامه پیدا کنند
- freeze یا crash واضح نباشد

### 8.5 Word Detail

مراحل:

1. یک کلمه را باز کن
2. meaningها، exampleها، usage، grammar notes، collocations، family، relationها را بررسی کن
3. favorite را بزن
4. known/difficult را بزن
5. دکمه‌های speech را بزن

### نتیجه‌ی مورد انتظار

- صفحه بدون crash باز شود
- sectionهای metadata اگر داده داشته باشند درست نمایش داده شوند
- state دکمه‌ها عوض شود
- اگر دستگاه TTS engine و voice آلمانی فعال داشته باشد، speech هم کار کند

### 8.6 Favorites

مراحل:

1. یک کلمه را favorite کن
2. `Favorites` را باز کن

### نتیجه‌ی مورد انتظار

- کلمه در favorites دیده شود
- بعد از content update هم باقی بماند

### 8.7 Practice

مراحل:

1. `Practice` را باز کن
2. یک flashcard session شروع کن
3. حداقل به یک مورد پاسخ بده
4. یک quiz session هم شروع کن

### نتیجه‌ی مورد انتظار

- بخش Practice بعد از remote update هم کار کند
- user state یادگیری از بین نرود

---

## مرحله 9: تست round trip کامل تغییر محتوا

این مهم‌ترین تست editorial loop است.

### مراحل

1. یکی از فایل‌های reviewشده را در `D:\_Projects\DarwinLingua.Content` تغییر بده یا یک کلمه جدید اضافه کن
2. دوباره این دستور را اجرا کن:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Server\Initialize-LocalServerContent.ps1 -ContentPath "D:\_Projects\DarwinLingua.Content"
```

3. بررسی کن که manifest یا history سرور batch جدیدتری نشان بدهد
4. اپ را باز کن
5. برو به `Settings`
6. یکی از این‌ها را بزن:
   - `Update All Content`
   - یا update مربوط به همان slice
7. در اپ، کلمه‌ی تغییرکرده یا جدید را پیدا کن

### نتیجه‌ی مورد انتظار

- import روی سرور با موفقیت انجام شود
- یک published batch جدید ساخته شود
- اپ package جدید را بگیرد و apply کند
- کلمه‌ی تغییرکرده یا جدید در اپ دیده شود
- favorites، known/difficult، و practice history باقی بمانند

---

## مرحله 10: تست failure behavior

### 10.1 وقتی Web API در دسترس نیست

مراحل:

1. Web API را متوقف کن
2. `Settings` را باز کن
3. یکی از update actionها را بزن

### نتیجه‌ی مورد انتظار

- اپ crash نکند
- خطای مناسب نشان داده شود
- last failure message به‌روزرسانی شود
- محتوای محلی موجود پاک نشود

### 10.2 محافظت در برابر draft package

مراحل:

1. یک import انجام بده ولی batch جدید را publish نکن
2. دوباره diagnostics اپ را ببین

### نتیجه‌ی مورد انتظار

- اپ فقط آخرین package منتشرشده را ببیند
- draft package به client پیشنهاد نشود

---

## مرحله 11: اجرای worksheetهای تکمیلی

اگر می‌خواهی sign-off کامل‌تر انجام بدهی، بعد از این runbook این worksheetها را هم اجرا کن:

- [44-Phase-1-Manual-Validation-Worksheet.md](/D:/_Projects/DarwinLingua/docs/44-Phase-1-Manual-Validation-Worksheet.md)
- [46-Phase-2-Practice-Validation-Worksheet.md](/D:/_Projects/DarwinLingua/docs/46-Phase-2-Practice-Validation-Worksheet.md)
- [47-Phase-3-Mobile-UX-Validation-Worksheet.md](/D:/_Projects/DarwinLingua/docs/47-Phase-3-Mobile-UX-Validation-Worksheet.md)
- [50-Phase-5-Remote-Update-Validation-Worksheet.md](/D:/_Projects/DarwinLingua/docs/50-Phase-5-Remote-Update-Validation-Worksheet.md)

اگر می‌خواهی یک bundle آماده برای ثبت نتایج بسازی:

```powershell
pwsh ./tools/Mobile/Start-MobileValidationBundle.ps1
```

---

## خطاهای رایج و علت‌های محتمل

### اسکریپت می‌گوید فایل تنظیمات محلی Web API پیدا نشد

چک کن که یکی از این دو فایل وجود داشته باشد:

- `src/Apps/DarwinLingua.WebApi/appsettings.Development.Local.json`
- `src/Apps/DarwinLingua.WebApi/appsettings.Development.json`

### PostgreSQL بالا می‌آید ولی import fail می‌شود

معمولاً یکی از این‌ها مشکل دارد:

- password داخل `tools/Server/Postgres/.env`
- password داخل فایل تنظیمات فعال Web API
- host یا port دیتابیس

### اپ موبایل در emulator اندروید به Web API محلی وصل نمی‌شود

در emulator باید از این آدرس استفاده شود:

- `http://10.0.2.2:5099`

از `localhost` داخل خود emulator برای دسترسی به host machine استفاده نکن.

### remote update fail می‌شود ولی داده محلی باقی مانده است

این رفتار صحیح است.

اپ باید این موارد را حفظ کند:

- محتوای SQLite محلی که قبلاً apply شده
- favorites
- user word state
- practice state

---

## نتیجه نهایی مورد انتظار

اگر تست دستی کامل موفق باشد، در انتها باید این وضعیت را داشته باشی:

- PostgreSQL محلی در حال اجراست
- دیتابیس اصلی سرور شامل محتوای importشده است
- Web API manifest و package endpointهای منتشرشده را برمی‌گرداند
- اپ موبایل می‌تواند packageها را دانلود و apply کند
- browse و search و word detail روی محتوای importشده کار می‌کنند
- user state بعد از content update حفظ می‌شود
- خطاها safe fail می‌شوند و داده محلی را پاک نمی‌کنند

اگر همه این موارد برقرار باشند، سیستم local end-to-end فعلی درست کار می‌کند.
