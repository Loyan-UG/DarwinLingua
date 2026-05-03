param(
    [string]$ContainerName = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$DatabaseUser = "postgres",
    [string]$OutputPath = "D:\_Projects\DarwinLingua\content\taxonomy\darwinlingua-taxonomy-v1.json"
)

$ErrorActionPreference = "Stop"

$languages = @("de", "ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr")

function New-StableGuid([string]$Value) {
    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value.ToLowerInvariant())
        return [Guid]::new($md5.ComputeHash($bytes)).ToString()
    }
    finally {
        $md5.Dispose()
    }
}

function L([string]$de, [string]$ar, [string]$ckb, [string]$en, [string]$fa, [string]$kmr, [string]$pl, [string]$ro, [string]$ru, [string]$sq, [string]$tr) {
    return ,[pscustomobject]([ordered]@{
        de = $de; ar = $ar; ckb = $ckb; en = $en; fa = $fa; kmr = $kmr
        pl = $pl; ro = $ro; ru = $ru; sq = $sq; tr = $tr
    })
}

function Topic([string]$key, [int]$sort, [object]$names) {
    return ,[pscustomobject]([ordered]@{ key = $key; sortOrder = $sort; localizations = $names })
}

function Label([string]$kind, [string]$key, [int]$sort, [object]$names) {
    return ,[pscustomobject]([ordered]@{ kind = $kind; key = $key; displayName = $names.en; sortOrder = $sort; localizations = $names })
}

$topics = @(
    Topic "everyday-life" 10 (L "Alltag" "الحياة اليومية" "ژیانی ڕۆژانە" "Everyday Life" "زندگی روزمره" "Jiyana rojane" "Życie codzienne" "Viața de zi cu zi" "Повседневная жизнь" "Jeta e përditshme" "Günlük yaşam")
    Topic "work-and-jobs" 20 (L "Arbeit und Beruf" "العمل والمهن" "کار و پیشە" "Work and Jobs" "کار و شغل" "Kar û pîşe" "Praca i zawody" "Muncă și profesii" "Работа и профессии" "Puna dhe profesionet" "İş ve meslekler")
    Topic "business-communication" 30 (L "Geschäftskommunikation" "التواصل التجاري" "پەیوەندیی بازرگانی" "Business Communication" "ارتباطات تجاری" "Têkiliya bazirganî" "Komunikacja biznesowa" "Comunicare de afaceri" "Деловое общение" "Komunikim biznesi" "İş iletişimi")
    Topic "meetings-and-presentations" 40 (L "Besprechungen und Präsentationen" "الاجتماعات والعروض" "کۆبوونەوە و پێشکەشکردن" "Meetings and Presentations" "جلسات و ارائه‌ها" "Civîn û pêşkêşî" "Spotkania i prezentacje" "Ședințe și prezentări" "Встречи и презентации" "Takime dhe prezantime" "Toplantılar ve sunumlar")
    Topic "erp-and-business-systems" 50 (L "ERP und Geschäftssysteme" "أنظمة ERP والأعمال" "ERP و سیستەمی کار" "ERP and Business Systems" "ERP و سیستم‌های کسب‌وکار" "ERP û pergalên kar" "ERP i systemy biznesowe" "ERP și sisteme de afaceri" "ERP и бизнес-системы" "ERP dhe sisteme biznesi" "ERP ve iş sistemleri")
    Topic "finance-and-accounting" 60 (L "Finanzen und Buchhaltung" "المالية والمحاسبة" "دارایی و ژمێریاری" "Finance and Accounting" "مالی و حسابداری" "Darayî û hesabdarî" "Finanse i księgowość" "Finanțe și contabilitate" "Финансы и бухгалтерия" "Financa dhe kontabilitet" "Finans ve muhasebe")
    Topic "sales-and-customers" 70 (L "Vertrieb und Kunden" "المبيعات والعملاء" "فرۆشتن و کڕیاران" "Sales and Customers" "فروش و مشتریان" "Firotin û xerîdar" "Sprzedaż i klienci" "Vânzări și clienți" "Продажи и клиенты" "Shitje dhe klientë" "Satış ve müşteriler")
    Topic "procurement-and-suppliers" 80 (L "Einkauf und Lieferanten" "المشتريات والموردون" "کڕین و دابینکەران" "Procurement and Suppliers" "خرید و تأمین‌کنندگان" "Kirin û dabînker" "Zakupy i dostawcy" "Achiziții și furnizori" "Закупки и поставщики" "Prokurim dhe furnizues" "Satın alma ve tedarikçiler")
    Topic "warehouse-and-logistics" 90 (L "Lager und Logistik" "المستودع واللوجستيات" "کۆگا و لۆجستیک" "Warehouse and Logistics" "انبار و لجستیک" "Embar û lojîstîk" "Magazyn i logistyka" "Depozit și logistică" "Склад и логистика" "Magazinë dhe logjistikë" "Depo ve lojistik")
    Topic "planning-and-projects" 100 (L "Planung und Projekte" "التخطيط والمشاريع" "پلان و پڕۆژە" "Planning and Projects" "برنامه‌ریزی و پروژه‌ها" "Plansazî û projeyên" "Planowanie i projekty" "Planificare și proiecte" "Планирование и проекты" "Planifikim dhe projekte" "Planlama ve projeler")
    Topic "management-and-leadership" 110 (L "Management und Führung" "الإدارة والقيادة" "بەڕێوەبردن و ڕابەرایەتی" "Management and Leadership" "مدیریت و رهبری" "Rêvebirin û serokati" "Zarządzanie i przywództwo" "Management și leadership" "Управление и руководство" "Menaxhim dhe udhëheqje" "Yönetim ve liderlik")
    Topic "documents-and-administration" 120 (L "Dokumente und Verwaltung" "الوثائق والإدارة" "بەڵگەنامە و کارگێڕی" "Documents and Administration" "اسناد و امور اداری" "Belge û rêveberî" "Dokumenty i administracja" "Documente și administrație" "Документы и администрирование" "Dokumente dhe administratë" "Belgeler ve idare")
    Topic "law-and-compliance" 130 (L "Recht und Compliance" "القانون والامتثال" "یاسا و پابەندبوون" "Law and Compliance" "حقوق و انطباق" "Yasa û lihevhatin" "Prawo i zgodność" "Drept și conformitate" "Право и соответствие" "Ligj dhe pajtueshmëri" "Hukuk ve uyum")
    Topic "data-and-reporting" 140 (L "Daten und Berichte" "البيانات والتقارير" "داتا و ڕاپۆرت" "Data and Reporting" "داده و گزارش‌دهی" "Dane û rapor" "Dane i raportowanie" "Date și raportare" "Данные и отчетность" "Të dhëna dhe raportim" "Veri ve raporlama")
    Topic "technology-and-it" 150 (L "Technologie und IT" "التقنية وتقنية المعلومات" "تەکنەلۆژیا و IT" "Technology and IT" "فناوری و IT" "Teknolojî û IT" "Technologia i IT" "Tehnologie și IT" "Технологии и ИТ" "Teknologji dhe IT" "Teknoloji ve BT")
    Topic "customer-service" 160 (L "Kundenservice" "خدمة العملاء" "خزمەتی کڕیار" "Customer Service" "خدمات مشتریان" "Xizmeta xerîdar" "Obsługa klienta" "Serviciu clienți" "Клиентский сервис" "Shërbim ndaj klientit" "Müşteri hizmetleri")
    Topic "human-resources" 170 (L "Personalwesen" "الموارد البشرية" "سەرچاوە مرۆییەکان" "Human Resources" "منابع انسانی" "Çavkaniyên mirovî" "Zasoby ludzkie" "Resurse umane" "Кадры" "Burime njerëzore" "İnsan kaynakları")
    Topic "education-and-training" 180 (L "Bildung und Schulung" "التعليم والتدريب" "پەروەردە و ڕاهێنان" "Education and Training" "آموزش و یادگیری" "Perwerde û perwerdehî" "Edukacja i szkolenia" "Educație și instruire" "Образование и обучение" "Arsim dhe trajnim" "Eğitim ve öğretim")
    Topic "healthcare-and-appointments" 190 (L "Gesundheit und Termine" "الصحة والمواعيد" "تەندروستی و کاتگرتن" "Healthcare and Appointments" "سلامت و قرارها" "Tenduristî û randevû" "Zdrowie i wizyty" "Sănătate și programări" "Здоровье и приемы" "Shëndetësi dhe takime" "Sağlık ve randevular")
    Topic "housing-and-real-estate" 200 (L "Wohnen und Immobilien" "السكن والعقارات" "نیشتەجێبوون و خانووبەرە" "Housing and Real Estate" "مسکن و املاک" "Xanî û milk" "Mieszkanie i nieruchomości" "Locuințe și imobiliare" "Жилье и недвижимость" "Banim dhe pasuri të paluajtshme" "Konut ve emlak")
    Topic "shopping-and-services" 210 (L "Einkaufen und Dienstleistungen" "التسوق والخدمات" "کڕین و خزمەتگوزاری" "Shopping and Services" "خرید و خدمات" "Kirîn û xizmet" "Zakupy i usługi" "Cumpărături și servicii" "Покупки и услуги" "Blerje dhe shërbime" "Alışveriş ve hizmetler")
    Topic "transport-and-travel" 220 (L "Verkehr und Reisen" "النقل والسفر" "گواستنەوە و گەشت" "Transport and Travel" "حمل‌ونقل و سفر" "Veguhestin û rêwîti" "Transport i podróże" "Transport și călătorii" "Транспорт и путешествия" "Transport dhe udhëtim" "Ulaşım ve seyahat")
    Topic "public-services" 230 (L "Öffentliche Dienstleistungen" "الخدمات العامة" "خزمەتگوزاری گشتی" "Public Services" "خدمات عمومی" "Xizmetên giştî" "Usługi publiczne" "Servicii publice" "Государственные услуги" "Shërbime publike" "Kamu hizmetleri")
    Topic "contracts-and-negotiation" 240 (L "Verträge und Verhandlungen" "العقود والتفاوض" "گرێبەست و دانوسان" "Contracts and Negotiation" "قراردادها و مذاکره" "Peyman û danûstandin" "Umowy i negocjacje" "Contracte și negociere" "Контракты и переговоры" "Kontrata dhe negociata" "Sözleşmeler ve müzakere")
    Topic "quality-and-risk" 250 (L "Qualität und Risiko" "الجودة والمخاطر" "کوالیتی و مەترسی" "Quality and Risk" "کیفیت و ریسک" "Kalîte û risk" "Jakość i ryzyko" "Calitate și risc" "Качество и риски" "Cilësi dhe rrezik" "Kalite ve risk")
    Topic "production-and-maintenance" 260 (L "Produktion und Wartung" "الإنتاج والصيانة" "بەرهەمهێنان و چاککردنەوە" "Production and Maintenance" "تولید و نگهداری" "Hilberîn û lênihêrîn" "Produkcja i utrzymanie" "Producție și mentenanță" "Производство и обслуживание" "Prodhim dhe mirëmbajtje" "Üretim ve bakım")
    Topic "environment-and-sustainability" 270 (L "Umwelt und Nachhaltigkeit" "البيئة والاستدامة" "ژینگە و بەردەوامی" "Environment and Sustainability" "محیط‌زیست و پایداری" "Jîngeh û domdarî" "Środowisko i zrównoważony rozwój" "Mediu și sustenabilitate" "Экология и устойчивость" "Mjedis dhe qëndrueshmëri" "Çevre ve sürdürülebilirlik")
    Topic "social-and-relationships" 280 (L "Soziales und Beziehungen" "العلاقات الاجتماعية" "کۆمەڵایەتی و پەیوەندییەکان" "Social Life and Relationships" "روابط اجتماعی" "Jiyana civakî û têkilî" "Życie społeczne i relacje" "Viață socială și relații" "Общение и отношения" "Jetë sociale dhe marrëdhënie" "Sosyal yaşam ve ilişkiler")
    Topic "culture-and-media" 290 (L "Kultur und Medien" "الثقافة والإعلام" "کلتوور و میدیا" "Culture and Media" "فرهنگ و رسانه" "Çand û medya" "Kultura i media" "Cultură și media" "Культура и медиа" "Kulturë dhe media" "Kültür ve medya")
    Topic "advanced-analysis" 300 (L "Fortgeschrittene Analyse" "التحليل المتقدم" "شیکردنەوەی پێشکەوتوو" "Advanced Analysis" "تحلیل پیشرفته" "Analîza pêşketî" "Zaawansowana analiza" "Analiză avansată" "Продвинутый анализ" "Analizë e avancuar" "Gelişmiş analiz")
)

$labels = @(
    Label "Usage" "everyday" 10 (L "Alltäglich" "يومي" "ڕۆژانە" "Everyday" "روزمره" "Rojane" "Codzienny" "De zi cu zi" "Повседневный" "I përditshëm" "Günlük")
    Label "Usage" "formal" 20 (L "Formell" "رسمي" "فەرمی" "Formal" "رسمی" "Fermî" "Formalny" "Formal" "Формальный" "Formal" "Resmi")
    Label "Usage" "informal" 30 (L "Informell" "غير رسمي" "نافەرمی" "Informal" "غیررسمی" "Nefermî" "Nieformalny" "Informal" "Неформальный" "Joformal" "Gayri resmi")
    Label "Usage" "spoken" 40 (L "Gesprochen" "شفهي" "زارەکی" "Spoken" "گفتاری" "Devkî" "Mówiony" "Vorbit" "Разговорный" "I folur" "Konuşma dili")
    Label "Usage" "written" 50 (L "Schriftlich" "كتابي" "نووسراو" "Written" "نوشتاری" "Nivîskî" "Pisemny" "Scris" "Письменный" "I shkruar" "Yazılı")
    Label "Usage" "business" 60 (L "Geschäftlich" "تجاري" "بازرگانی" "Business" "تجاری" "Bazirganî" "Biznesowy" "De afaceri" "Деловой" "Biznesi" "İş")
    Label "Usage" "workplace" 70 (L "Am Arbeitsplatz" "في مكان العمل" "لە شوێنی کار" "Workplace" "محیط کار" "Li cihê kar" "W miejscu pracy" "La locul de muncă" "На рабочем месте" "Në vendin e punës" "İş yeri")
    Label "Usage" "technical" 80 (L "Technisch" "تقني" "تەکنیکی" "Technical" "فنی" "Teknîkî" "Techniczny" "Tehnic" "Технический" "Teknik" "Teknik")
    Label "Usage" "academic" 90 (L "Akademisch" "أكاديمي" "ئەکادیمی" "Academic" "آکادمیک" "Akademîk" "Akademicki" "Academic" "Академический" "Akademik" "Akademik")
    Label "Usage" "legal" 100 (L "Rechtlich" "قانوني" "یاسایی" "Legal" "حقوقی" "Yasayî" "Prawny" "Juridic" "Юридический" "Ligjor" "Hukuki")
    Label "Usage" "medical" 110 (L "Medizinisch" "طبي" "پزیشکی" "Medical" "پزشکی" "Bijîşkî" "Medyczny" "Medical" "Медицинский" "Mjekësor" "Tıbbi")
    Label "Usage" "administrative" 120 (L "Administrativ" "إداري" "کارگێڕی" "Administrative" "اداری" "Rêveberî" "Administracyjny" "Administrativ" "Административный" "Administrativ" "İdari")
    Label "Usage" "email" 130 (L "E-Mail" "البريد الإلكتروني" "ئیمەیڵ" "Email" "ایمیل" "E-name" "E-mail" "E-mail" "Электронная почта" "Email" "E-posta")
    Label "Usage" "meeting" 140 (L "Besprechung" "اجتماع" "کۆبوونەوە" "Meeting" "جلسه" "Civîn" "Spotkanie" "Ședință" "Встреча" "Takim" "Toplantı")
    Label "Usage" "presentation" 150 (L "Präsentation" "عرض تقديمي" "پێشکەشکردن" "Presentation" "ارائه" "Pêşkêşî" "Prezentacja" "Prezentare" "Презентация" "Prezantim" "Sunum")
    Label "Usage" "negotiation" 160 (L "Verhandlung" "تفاوض" "دانوسان" "Negotiation" "مذاکره" "Danûstandin" "Negocjacje" "Negociere" "Переговоры" "Negociatë" "Müzakere")
    Label "Usage" "customer-facing" 170 (L "Kundenorientiert" "موجه للعملاء" "ڕووی کڕیار" "Customer-facing" "مربوط به مشتری" "Ji bo xerîdar" "Dla klienta" "Pentru clienți" "Для клиентов" "Për klientë" "Müşteriye dönük")
    Label "Usage" "reporting" 180 (L "Berichtswesen" "إعداد التقارير" "ڕاپۆرتکردن" "Reporting" "گزارش‌دهی" "Rapor kirin" "Raportowanie" "Raportare" "Отчетность" "Raportim" "Raporlama")
    Label "Usage" "planning" 190 (L "Planung" "تخطيط" "پلانکردن" "Planning" "برنامه‌ریزی" "Plansazî" "Planowanie" "Planificare" "Планирование" "Planifikim" "Planlama")
    Label "Usage" "analysis" 200 (L "Analyse" "تحليل" "شیکردنەوە" "Analysis" "تحلیل" "Analîz" "Analiza" "Analiză" "Анализ" "Analizë" "Analiz")
    Label "Usage" "process" 210 (L "Prozessbezogen" "متعلق بالعملية" "پڕۆسەیی" "Process-related" "فرایندی" "Girêdayî pêvajoyê" "Procesowy" "Legat de proces" "Процессный" "I lidhur me procesin" "Süreçle ilgili")
    Label "Usage" "urgent" 220 (L "Dringend" "عاجل" "بەپەلە" "Urgent" "فوری" "Lezgin" "Pilny" "Urgent" "Срочный" "Urgjent" "Acil")
    Label "Usage" "polite" 230 (L "Höflich" "مهذب" "بەڕێز" "Polite" "مودبانه" "Bi rêz" "Uprzejmy" "Politicos" "Вежливый" "I sjellshëm" "Kibar")
    Label "Usage" "sensitive" 240 (L "Sensibel" "حساس" "هەستیار" "Sensitive" "حساس" "Hestiyar" "Wrażliwy" "Sensibil" "Деликатный" "I ndjeshëm" "Hassas")
    Label "Usage" "high-frequency" 250 (L "Sehr häufig" "شائع جدًا" "زۆر باو" "High-frequency" "بسیار پرکاربرد" "Pir bikaranîn" "Bardzo częsty" "Foarte frecvent" "Очень частый" "Shumë i shpeshtë" "Çok yaygın")
    Label "Usage" "advanced" 260 (L "Fortgeschritten" "متقدم" "پێشکەوتوو" "Advanced" "پیشرفته" "Pêşketî" "Zaawansowany" "Avansat" "Продвинутый" "I avancuar" "İleri")
    Label "Context" "erp" 1000 (L "ERP" "ERP" "ERP" "ERP" "ERP" "ERP" "ERP" "ERP" "ERP" "ERP" "ERP")
    Label "Context" "accounting" 1010 (L "Buchhaltung" "المحاسبة" "ژمێریاری" "Accounting" "حسابداری" "Hesabdarî" "Księgowość" "Contabilitate" "Бухгалтерия" "Kontabilitet" "Muhasebe")
    Label "Context" "finance" 1020 (L "Finanzen" "المالية" "دارایی" "Finance" "مالی" "Darayî" "Finanse" "Finanțe" "Финансы" "Financa" "Finans")
    Label "Context" "sales" 1030 (L "Vertrieb" "المبيعات" "فرۆشتن" "Sales" "فروش" "Firotin" "Sprzedaż" "Vânzări" "Продажи" "Shitje" "Satış")
    Label "Context" "procurement" 1040 (L "Einkauf" "المشتريات" "کڕین" "Procurement" "خرید" "Kirin" "Zakupy" "Achiziții" "Закупки" "Prokurim" "Satın alma")
    Label "Context" "warehouse" 1050 (L "Lager" "مستودع" "کۆگا" "Warehouse" "انبار" "Embar" "Magazyn" "Depozit" "Склад" "Magazinë" "Depo")
    Label "Context" "logistics" 1060 (L "Logistik" "اللوجستيات" "لۆجستیک" "Logistics" "لجستیک" "Lojîstîk" "Logistyka" "Logistică" "Логистика" "Logjistikë" "Lojistik")
    Label "Context" "hr" 1070 (L "Personalwesen" "الموارد البشرية" "سەرچاوە مرۆییەکان" "HR" "منابع انسانی" "Çavkaniyên mirovî" "HR" "HR" "HR" "HR" "İK")
    Label "Context" "management" 1080 (L "Management" "الإدارة" "بەڕێوەبردن" "Management" "مدیریت" "Rêvebirin" "Zarządzanie" "Management" "Управление" "Menaxhim" "Yönetim")
    Label "Context" "reporting" 1090 (L "Reporting" "التقارير" "ڕاپۆرت" "Reporting" "گزارش‌دهی" "Rapor" "Raportowanie" "Raportare" "Отчетность" "Raportim" "Raporlama")
    Label "Context" "analytics" 1100 (L "Analytik" "التحليلات" "شیکاری" "Analytics" "تحلیل داده" "Analîtîk" "Analityka" "Analitică" "Аналитика" "Analitikë" "Analitik")
    Label "Context" "crm" 1110 (L "CRM" "إدارة علاقات العملاء" "CRM" "CRM" "CRM" "CRM" "CRM" "CRM" "CRM" "CRM" "CRM")
    Label "Context" "customer-service" 1120 (L "Kundenservice" "خدمة العملاء" "خزمەتی کڕیار" "Customer Service" "خدمات مشتریان" "Xizmeta xerîdar" "Obsługa klienta" "Serviciu clienți" "Клиентский сервис" "Shërbim klienti" "Müşteri hizmetleri")
    Label "Context" "support" 1130 (L "Support" "الدعم" "پاڵپشتی" "Support" "پشتیبانی" "Piştgirî" "Wsparcie" "Suport" "Поддержка" "Mbështetje" "Destek")
    Label "Context" "it" 1140 (L "IT" "تقنية المعلومات" "IT" "IT" "IT" "IT" "IT" "IT" "ИТ" "IT" "BT")
    Label "Context" "software" 1150 (L "Software" "البرمجيات" "نەرمەواڵە" "Software" "نرم‌افزار" "Nermalav" "Oprogramowanie" "Software" "Программное обеспечение" "Softuer" "Yazılım")
    Label "Context" "data" 1160 (L "Daten" "البيانات" "داتا" "Data" "داده" "Dane" "Dane" "Date" "Данные" "Të dhëna" "Veri")
    Label "Context" "security" 1170 (L "Sicherheit" "الأمن" "ئاسایش" "Security" "امنیت" "Ewlehî" "Bezpieczeństwo" "Securitate" "Безопасность" "Siguri" "Güvenlik")
    Label "Context" "compliance" 1180 (L "Compliance" "الامتثال" "پابەندبوون" "Compliance" "انطباق" "Lihevhatin" "Zgodność" "Conformitate" "Соответствие" "Pajtueshmëri" "Uyum")
    Label "Context" "legal" 1190 (L "Recht" "القانون" "یاسا" "Legal" "حقوقی" "Yasa" "Prawo" "Juridic" "Право" "Ligjor" "Hukuk")
    Label "Context" "contracts" 1200 (L "Verträge" "العقود" "گرێبەستەکان" "Contracts" "قراردادها" "Peyman" "Umowy" "Contracte" "Контракты" "Kontrata" "Sözleşmeler")
    Label "Context" "documents" 1210 (L "Dokumente" "الوثائق" "بەڵگەنامەکان" "Documents" "اسناد" "Belge" "Dokumenty" "Documente" "Документы" "Dokumente" "Belgeler")
    Label "Context" "administration" 1220 (L "Verwaltung" "الإدارة" "کارگێڕی" "Administration" "اداره" "Rêveberî" "Administracja" "Administrație" "Администрирование" "Administratë" "İdare")
    Label "Context" "healthcare" 1230 (L "Gesundheitswesen" "الرعاية الصحية" "تەندروستی" "Healthcare" "حوزه سلامت" "Tenduristî" "Opieka zdrowotna" "Sănătate" "Здравоохранение" "Shëndetësi" "Sağlık")
    Label "Context" "appointments" 1240 (L "Termine" "المواعيد" "کاتگرتن" "Appointments" "قرارها" "Randevû" "Wizyty" "Programări" "Приемы" "Takime" "Randevular")
    Label "Context" "housing" 1250 (L "Wohnen" "السكن" "نیشتەجێبوون" "Housing" "مسکن" "Xanî" "Mieszkanie" "Locuire" "Жилье" "Banim" "Konut")
    Label "Context" "real-estate" 1260 (L "Immobilien" "العقارات" "خانووبەرە" "Real Estate" "املاک" "Milk" "Nieruchomości" "Imobiliare" "Недвижимость" "Pasuri të paluajtshme" "Emlak")
    Label "Context" "shopping" 1270 (L "Einkaufen" "التسوق" "کڕین" "Shopping" "خرید" "Kirîn" "Zakupy" "Cumpărături" "Покупки" "Blerje" "Alışveriş")
    Label "Context" "travel" 1280 (L "Reisen" "السفر" "گەشت" "Travel" "سفر" "Rêwîtî" "Podróże" "Călătorii" "Путешествия" "Udhëtim" "Seyahat")
    Label "Context" "transport" 1290 (L "Verkehr" "النقل" "گواستنەوە" "Transport" "حمل‌ونقل" "Veguhestin" "Transport" "Transport" "Транспорт" "Transport" "Ulaşım")
    Label "Context" "public-services" 1300 (L "Öffentliche Dienste" "الخدمات العامة" "خزمەتگوزاری گشتی" "Public Services" "خدمات عمومی" "Xizmetên giştî" "Usługi publiczne" "Servicii publice" "Госуслуги" "Shërbime publike" "Kamu hizmetleri")
    Label "Context" "education" 1310 (L "Bildung" "التعليم" "پەروەردە" "Education" "آموزش" "Perwerde" "Edukacja" "Educație" "Образование" "Arsim" "Eğitim")
    Label "Context" "training" 1320 (L "Schulung" "تدريب" "ڕاهێنان" "Training" "آموزش عملی" "Perwerdehî" "Szkolenie" "Instruire" "Обучение" "Trajnim" "Eğitim")
    Label "Context" "manufacturing" 1330 (L "Fertigung" "التصنيع" "بەرهەمهێنان" "Manufacturing" "تولید صنعتی" "Hilberîn" "Produkcja" "Producție" "Производство" "Prodhim" "İmalat")
    Label "Context" "maintenance" 1340 (L "Wartung" "الصيانة" "چاککردنەوە" "Maintenance" "نگهداری" "Lênihêrîn" "Utrzymanie" "Mentenanță" "Обслуживание" "Mirëmbajtje" "Bakım")
    Label "Context" "quality" 1350 (L "Qualität" "الجودة" "کوالیتی" "Quality" "کیفیت" "Kalîte" "Jakość" "Calitate" "Качество" "Cilësi" "Kalite")
    Label "Context" "risk" 1360 (L "Risiko" "المخاطر" "مەترسی" "Risk" "ریسک" "Risk" "Ryzyko" "Risc" "Риск" "Rrezik" "Risk")
    Label "Context" "marketing" 1370 (L "Marketing" "التسويق" "مارکێتینگ" "Marketing" "بازاریابی" "Marketing" "Marketing" "Marketing" "Маркетинг" "Marketing" "Pazarlama")
    Label "Context" "strategy" 1380 (L "Strategie" "الاستراتيجية" "ستراتیژی" "Strategy" "استراتژی" "Stratejî" "Strategia" "Strategie" "Стратегия" "Strategji" "Strateji")
    Label "Context" "sustainability" 1390 (L "Nachhaltigkeit" "الاستدامة" "بەردەوامی" "Sustainability" "پایداری" "Domdarî" "Zrównoważony rozwój" "Sustenabilitate" "Устойчивость" "Qëndrueshmëri" "Sürdürülebilirlik")
    Label "Context" "research" 1400 (L "Forschung" "البحث" "توێژینەوە" "Research" "پژوهش" "Lêkolîn" "Badania" "Cercetare" "Исследования" "Kërkim" "Araştırma")
)

$taxonomy = [ordered]@{
    version = "2026.05.02.1"
    languages = $languages
    topics = @($topics | ForEach-Object {
        $topic = $_
        [ordered]@{
            key = $topic.key
            sortOrder = $topic.sortOrder
            localizations = @($languages | ForEach-Object {
                [ordered]@{ language = $_; name = $topic.localizations.$_ }
            })
        }
    })
    labels = @($labels | ForEach-Object {
        $label = $_
        [ordered]@{
            kind = $label.kind
            key = $label.key
            displayName = $label.displayName
            sortOrder = $label.sortOrder
            localizations = @($languages | ForEach-Object {
                [ordered]@{ language = $_; name = $label.localizations.$_ }
            })
        }
    })
}

New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
$taxonomy | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding UTF8

function SqlString([string]$Value) {
    return "'" + ($Value -replace "'", "''") + "'"
}

$sql = [System.Text.StringBuilder]::new()
[void]$sql.AppendLine("BEGIN;")

foreach ($topic in $topics) {
    $topicId = New-StableGuid "topic:$($topic.key)"
    [void]$sql.AppendLine("INSERT INTO ""Topics"" (""Id"", ""Key"", ""SortOrder"", ""IsSystem"", ""CreatedAtUtc"", ""UpdatedAtUtc"") VALUES ('$topicId', $(SqlString $topic.key), $($topic.sortOrder), TRUE, NOW(), NOW()) ON CONFLICT (""Key"") DO UPDATE SET ""SortOrder"" = EXCLUDED.""SortOrder"", ""IsSystem"" = TRUE, ""UpdatedAtUtc"" = NOW();")
    foreach ($language in $languages) {
        $localizationId = New-StableGuid "topic-localization:$($topic.key):$language"
        $displayName = $topic.localizations.$language
        [void]$sql.AppendLine("INSERT INTO ""TopicLocalizations"" (""Id"", ""TopicId"", ""LanguageCode"", ""DisplayName"", ""CreatedAtUtc"", ""UpdatedAtUtc"") VALUES ('$localizationId', (SELECT ""Id"" FROM ""Topics"" WHERE ""Key"" = $(SqlString $topic.key)), $(SqlString $language), $(SqlString $displayName), NOW(), NOW()) ON CONFLICT (""TopicId"", ""LanguageCode"") DO UPDATE SET ""DisplayName"" = EXCLUDED.""DisplayName"", ""UpdatedAtUtc"" = NOW();")
    }
}

foreach ($label in $labels) {
    $labelId = New-StableGuid "label:$($label.kind):$($label.key)"
    [void]$sql.AppendLine("INSERT INTO ""LabelDefinitions"" (""Id"", ""Kind"", ""Key"", ""DisplayName"", ""SortOrder"", ""IsSystem"", ""CreatedAtUtc"", ""UpdatedAtUtc"") VALUES ('$labelId', $(SqlString $label.kind), $(SqlString $label.key), $(SqlString $label.displayName), $($label.sortOrder), TRUE, NOW(), NOW()) ON CONFLICT (""Kind"", ""Key"") DO UPDATE SET ""DisplayName"" = EXCLUDED.""DisplayName"", ""SortOrder"" = EXCLUDED.""SortOrder"", ""IsSystem"" = TRUE, ""UpdatedAtUtc"" = NOW();")
    foreach ($language in $languages) {
        $localizationId = New-StableGuid "label-localization:$($label.kind):$($label.key):$language"
        $displayName = $label.localizations.$language
        [void]$sql.AppendLine("INSERT INTO ""LabelDefinitionLocalizations"" (""Id"", ""LabelDefinitionId"", ""LanguageCode"", ""DisplayName"", ""CreatedAtUtc"", ""UpdatedAtUtc"") VALUES ('$localizationId', (SELECT ""Id"" FROM ""LabelDefinitions"" WHERE ""Kind"" = $(SqlString $label.kind) AND ""Key"" = $(SqlString $label.key)), $(SqlString $language), $(SqlString $displayName), NOW(), NOW()) ON CONFLICT (""LabelDefinitionId"", ""LanguageCode"") DO UPDATE SET ""DisplayName"" = EXCLUDED.""DisplayName"", ""UpdatedAtUtc"" = NOW();")
    }
}

[void]$sql.AppendLine("COMMIT;")

$sqlText = $sql.ToString()
$sqlText | docker exec -i $ContainerName psql -v ON_ERROR_STOP=1 -U $DatabaseUser -d $DatabaseName | Out-Host

Write-Host "Taxonomy written to $OutputPath"
