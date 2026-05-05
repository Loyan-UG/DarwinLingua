const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C2';
const levelLower = 'c2';
const batch = '005';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const projectPath = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['andächtig','andenken','anheimfallen','der Anklang','anmahnen','anmaßend'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const tokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(words)}`);
const usedLabels = ['formal','written','advanced','business','workplace','administrative','academic','analysis','sensitive'];
for (const key of usedLabels) if (!labelMap.has(key)) throw new Error(`Missing taxonomy label: ${key}`);
const labels = usedLabels.map(key => labelMap.get(key));
function meanings(obj) { return langs.map(language => ({ language, text: obj[language] })); }
function ex(baseText, obj) { return { baseText, translations: meanings(obj) }; }

const entries = [
  {
    word: 'andächtig', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'an-däch-tig',
    topics: ['culture-and-media','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['adjective; can mean reverent, devout, or deeply absorbed'],
    collocations: [{ text: 'andächtig lauschen', meaning: 'to listen reverently or with deep attention' }],
    wordFamilies: [{ lemma: 'die Andacht', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'خاشع؛ منصت باهتمام عميق', ckb:'بە خشوع؛ بە سەرنجی قووڵ', en:'reverent; devout; deeply attentive', fa:'خاشع؛ با توجه عمیق؛ فروتنانه', kmr:'bi rêzdarî; bi baldarîya kûr', pl:'nabożny; skupiony; pełen czci', ro:'evlavios; profund atent; plin de venerație', ru:'благоговейный; сосредоточенно внимательный', sq:'i përshpirtshëm; i vëmendshëm me nderim', tr:'huşu içinde; derin dikkatle' }),
    examples: [
      ex('Das Publikum lauschte andächtig, als die alte Aufnahme zum ersten Mal öffentlich abgespielt wurde.', { ar:'استمع الجمهور بخشوع عندما شُغّل التسجيل القديم علناً لأول مرة.', ckb:'بینەران بە سەرنجێکی قووڵ گوێیان گرت، کاتێک تۆمارە کۆنەکە بۆ یەکەمجار بە ئاشکرا لێدرا.', en:'The audience listened reverently as the old recording was played publicly for the first time.', fa:'تماشاگران با توجهی عمیق گوش دادند، وقتی ضبط قدیمی برای نخستین بار در برابر عموم پخش شد.', kmr:'Temaşevanan bi baldarîyek kûr guhdarî kirin dema tomara kevn cara yekem bi giştî hate lêdan.', pl:'Publiczność słuchała w skupieniu, gdy stare nagranie po raz pierwszy odtworzono publicznie.', ro:'Publicul a ascultat cu profundă atenție când vechea înregistrare a fost redată public pentru prima dată.', ru:'Публика слушала благоговейно, когда старую запись впервые воспроизвели публично.', sq:'Publiku dëgjoi me përkushtim kur regjistrimi i vjetër u luajt publikisht për herë të parë.', tr:'Eski kayıt ilk kez kamuya açık şekilde çalındığında dinleyiciler huşu içinde dinledi.' }),
      ex('Nach der hitzigen Sitzung wirkte die andächtige Stille im Flur beinahe befremdlich.', { ar:'بعد الاجتماع المحتدم بدا الصمت الخاشع في الممر غريباً تقريباً.', ckb:'دوای دانیشتنە گەرمەکە، بێدەنگییە سەرنجدارەکە لە ڕاڕەوەکەدا نزیک بوو بە نامۆیی.', en:'After the heated meeting, the reverent silence in the hallway seemed almost unsettling.', fa:'پس از جلسه پرتنش، سکوت خاشعانه راهرو تقریباً غریب به نظر می‌رسید.', kmr:'Piştî civîna germ, bêdengiya bi rêzdarî ya di korîdorê de hema hema xerîb xuya dikir.', pl:'Po burzliwym spotkaniu pełna skupienia cisza na korytarzu wydawała się niemal dziwna.', ro:'După ședința aprinsă, liniștea aproape solemnă de pe coridor părea stranie.', ru:'После бурного заседания благоговейная тишина в коридоре казалась почти странной.', sq:'Pas mbledhjes së tensionuar, heshtja përplot përqendrim në korridor dukej gati e çuditshme.', tr:'Hararetli toplantıdan sonra koridordaki huşulu sessizlik neredeyse tuhaf görünüyordu.' })
    ]
  },
  {
    word: 'andenken', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'andenken', pronunciationIpa: null, syllableBreak: 'an-den-ken',
    topics: ['planning-and-projects','management-and-leadership','business-communication'], usageLabels: ['formal','written','business','advanced'], contextLabels: [],
    grammarNotes: ['separable verb; denkt an, dachte an, hat angedacht; often used as past participle angedacht for preliminary plans'],
    collocations: [{ text: 'eine Lösung andenken', meaning: 'to consider or sketch a solution preliminarily' }],
    wordFamilies: [{ lemma: 'angedacht', relationLabel: 'participle', note: 'frequent in administrative and project language' }], relations: [],
    meanings: meanings({ ar:'يفكر مبدئياً في؛ يطرح كفكرة أولية', ckb:'وەک بیرۆکەی سەرەتایی لێی بیر بکاتەوە', en:'to consider preliminarily; to envisage', fa:'به‌طور مقدماتی در نظر گرفتن؛ به عنوان ایده مطرح کردن', kmr:'wek fikirê destpêkê li ser bifikirin', pl:'rozważać wstępnie; planować zarysowo', ro:'a lua în calcul preliminar; a prefigura', ru:'предварительно рассматривать; намечать', sq:'ta shqyrtosh paraprakisht; ta parashikosh si ide', tr:'ön değerlendirmeye almak; tasarlamak' }),
    examples: [
      ex('Für das zweite Quartal ist eine gemeinsame Schulung mit dem Support angedacht, aber noch nicht budgetiert.', { ar:'من المخطط مبدئياً تنظيم تدريب مشترك مع فريق الدعم في الربع الثاني، لكنه لم يُدرج بعد في الميزانية.', ckb:'بۆ چارەکی دووەم ڕاهێنانێکی هاوبەش لەگەڵ پشتگیری وەک پلانێکی سەرەتایی دانراوە، بەڵام هێشتا بودجەی بۆ دانەنراوە.', en:'A joint training session with support is envisaged for the second quarter, but it has not yet been budgeted.', fa:'برای سه‌ماهه دوم یک آموزش مشترک با تیم پشتیبانی در نظر گرفته شده، اما هنوز بودجه‌بندی نشده است.', kmr:'Ji bo çaryeka duyemîn perwerdeyek hevpar bi piştgiriyê re hatiye fikirîn, lê hîn budce nekiriye.', pl:'Na drugi kwartał wstępnie przewidziano wspólne szkolenie z działem wsparcia, ale nie ujęto go jeszcze w budżecie.', ro:'Pentru al doilea trimestru este avută în vedere o instruire comună cu echipa de suport, dar nu a fost încă bugetată.', ru:'На второй квартал предварительно запланировано совместное обучение со службой поддержки, но бюджет на него ещё не выделен.', sq:'Për tremujorin e dytë është menduar një trajnim i përbashkët me mbështetjen, por ende nuk është buxhetuar.', tr:'İkinci çeyrek için destek ekibiyle ortak bir eğitim düşünülüyor, ancak henüz bütçelendirilmedi.' }),
      ex('Die Stadt denkt eine neue Nutzung des alten Bahnhofs an, ohne den Denkmalschutz aufzugeben.', { ar:'تفكر المدينة مبدئياً في استخدام جديد للمحطة القديمة دون التخلي عن حماية الآثار.', ckb:'شارەکە بە شێوەی سەرەتایی بیر لە بەکارهێنانی نوێی وێستگە کۆنەکە دەکاتەوە، بەبێ وازهێنان لە پاراستنی شوێنە مێژووییەکان.', en:'The city is considering a new use for the old station without abandoning heritage protection.', fa:'شهر به‌طور مقدماتی به استفاده‌ای جدید از ایستگاه قدیمی فکر می‌کند، بدون آنکه حفاظت میراثی را کنار بگذارد.', kmr:'Bajar li ser bikaranîna nû ya stasyona kevn difikire bê ku parastina mîratê berdide.', pl:'Miasto rozważa nowe wykorzystanie starego dworca, nie rezygnując z ochrony zabytków.', ro:'Orașul ia în calcul o nouă utilizare a vechii gări fără a renunța la protecția monumentelor.', ru:'Город предварительно рассматривает новое использование старого вокзала, не отказываясь от охраны памятника.', sq:'Qyteti po shqyrton një përdorim të ri të stacionit të vjetër pa hequr dorë nga mbrojtja e trashëgimisë.', tr:'Kent, eski gar için anıt korumasından vazgeçmeden yeni bir kullanım düşünüyor.' })
    ]
  },
  {
    word: 'anheimfallen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'anheimfallen', pronunciationIpa: null, syllableBreak: 'an-heim-fal-len',
    topics: ['advanced-analysis','culture-and-media','quality-and-risk'], usageLabels: ['formal','written','advanced'], contextLabels: [],
    grammarNotes: ['elevated verb; fällt anheim, fiel anheim, ist anheimgefallen; usually with dative object'],
    collocations: [{ text: 'dem Vergessen anheimfallen', meaning: 'to fall into oblivion' }],
    wordFamilies: [], relations: [],
    meanings: meanings({ ar:'يقع تحت سلطة؛ يصبح عرضة لـ', ckb:'بکەوێتە ژێر دەستی؛ بکەوێتە بەر مەترسی شتێک', en:'to fall prey to; to become subject to', fa:'دچار شدن به؛ گرفتار شدن در', kmr:'bikeve bin bandora; bibe qurbana tiştekî', pl:'paść ofiarą; ulec czemuś', ro:'a cădea pradă; a deveni supus unui lucru', ru:'подпасть под; стать жертвой', sq:'të bjerë pre e; t’i nënshtrohet', tr:'bir şeye kapılmak; maruz kalmak' }),
    examples: [
      ex('Ohne systematische Dokumentation fallen selbst zentrale Projektentscheidungen rasch dem Vergessen anheim.', { ar:'من دون توثيق منهجي، حتى القرارات المركزية في المشروع تقع سريعاً في النسيان.', ckb:'بەبێ بەڵگەکردنی سیستەماتیک، تەنانەت بڕیارە سەرەکییەکانی پڕۆژەش بە خێرایی دەکەونە بیرچوونەوە.', en:'Without systematic documentation, even central project decisions quickly fall into oblivion.', fa:'بدون مستندسازی نظام‌مند، حتی تصمیم‌های مرکزی پروژه به‌سرعت به فراموشی سپرده می‌شوند.', kmr:'Bê belgekirina pergalî, heta biryarên navendî yên projeyê jî zû dikevin jibîrkirinê.', pl:'Bez systematycznej dokumentacji nawet kluczowe decyzje projektowe szybko popadają w zapomnienie.', ro:'Fără documentare sistematică, chiar și deciziile centrale ale proiectului cad rapid în uitare.', ru:'Без систематической документации даже ключевые проектные решения быстро предаются забвению.', sq:'Pa dokumentim sistematik, edhe vendimet qendrore të projektit bien shpejt në harresë.', tr:'Sistematik dokümantasyon olmadan, merkezi proje kararları bile hızla unutulmaya yüz tutar.' }),
      ex('Die Erzählung zeigt, wie eine Gesellschaft ihrer eigenen Angst anheimfallen kann.', { ar:'تُظهر الحكاية كيف يمكن لمجتمع أن يقع أسير خوفه الخاص.', ckb:'چیرۆکەکە پیشان دەدات کە کۆمەڵگا چۆن دەتوانێت بکەوێتە دەست ترسی خۆی.', en:'The narrative shows how a society can fall prey to its own fear.', fa:'روایت نشان می‌دهد چگونه یک جامعه می‌تواند گرفتار ترس خود شود.', kmr:'Çîrok nîşan dide ka civakek çawa dikare bikeve bin destê tirsên xwe.', pl:'Opowieść pokazuje, jak społeczeństwo może paść ofiarą własnego lęku.', ro:'Narațiunea arată cum o societate poate cădea pradă propriei frici.', ru:'Повествование показывает, как общество может стать жертвой собственного страха.', sq:'Rrëfimi tregon si një shoqëri mund të bjerë pre e frikës së vet.', tr:'Anlatı, bir toplumun kendi korkusuna nasıl kapılabileceğini gösterir.' })
    ]
  },
  {
    word: 'der Anklang', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'der', plural: 'Anklänge', infinitive: null, pronunciationIpa: null, syllableBreak: 'An-klang',
    topics: ['culture-and-media','business-communication','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'], contextLabels: [],
    grammarNotes: ['masculine noun; often means resonance, echo, or slight resemblance'],
    collocations: [{ text: 'Anklang finden', meaning: 'to meet with approval or resonance' }],
    wordFamilies: [{ lemma: 'anklingen', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings({ ar:'صدى؛ قبول؛ لمحة تشابه', ckb:'دەنگدانەوە؛ پەسەندبوون؛ هاوشێوەییەکی کەم', en:'resonance; approval; echo or hint', fa:'بازتاب؛ استقبال؛ شباهت یا اشاره ظریف', kmr:'vedeng; pejirandin; nîşaneyek sivik', pl:'oddźwięk; uznanie; aluzja', ro:'rezonanță; ecou; aprobare', ru:'отзвук; отклик; одобрение', sq:'jehonë; miratim; nuancë ngjashmërie', tr:'yankı; kabul görme; çağrışım' }),
    examples: [
      ex('Der Vorschlag fand im Lenkungskreis überraschend breiten Anklang.', { ar:'لاقى الاقتراح صدى واسعاً على نحو مفاجئ في لجنة التوجيه.', ckb:'پێشنیارەکە لە کۆمەڵەی ئاراستەکردندا بە شێوەیەکی سەرسوڕهێنەر پەسەندی فراوانی وەرگرت.', en:'The proposal met with surprisingly broad approval in the steering committee.', fa:'پیشنهاد در کمیته راهبری به‌طور غافلگیرکننده‌ای با استقبال گسترده روبه‌رو شد.', kmr:'Pêşniyar di komîteya rêvebirinê de bi awayekî şaşwaz pejirandina fireh dît.', pl:'Propozycja spotkała się w komitecie sterującym z zaskakująco szerokim oddźwiękiem.', ro:'Propunerea a găsit un ecou surprinzător de larg în comitetul de coordonare.', ru:'Предложение неожиданно получило широкую поддержку в руководящем комитете.', sq:'Propozimi gjeti një miratim çuditërisht të gjerë në komitetin drejtues.', tr:'Öneri yürütme kurulunda şaşırtıcı derecede geniş kabul gördü.' }),
      ex('In der Melodie ist ein leiser Anklang an ein altes Volkslied zu hören.', { ar:'في اللحن يمكن سماع صدى خافت لأغنية شعبية قديمة.', ckb:'لە میلۆدییەکەدا دەنگدانەوەیەکی نەرمی گۆرانییەکی کۆنی گەلایەتی دەبیسترێت.', en:'A faint echo of an old folk song can be heard in the melody.', fa:'در ملودی، بازتابی خفیف از یک ترانه محلی قدیمی شنیده می‌شود.', kmr:'Di melodiyê de vedengeke hêdî ya straneke gelêrî ya kevn tê bihîstin.', pl:'W melodii słychać delikatny oddźwięk starej pieśni ludowej.', ro:'În melodie se aude un ecou discret al unui vechi cântec popular.', ru:'В мелодии слышится лёгкий отзвук старой народной песни.', sq:'Në melodi dëgjohet një jehonë e lehtë e një kënge të vjetër popullore.', tr:'Melodide eski bir halk şarkısının hafif bir yankısı duyulur.' })
    ]
  },
  {
    word: 'anmahnen', language: 'de', cefrLevel: level, partOfSpeech: 'Verb', article: null, plural: null, infinitive: 'anmahnen', pronunciationIpa: null, syllableBreak: 'an-mah-nen',
    topics: ['finance-and-accounting','documents-and-administration','business-communication'], usageLabels: ['formal','written','business','administrative'], contextLabels: [],
    grammarNotes: ['separable verb; mahnt an, mahnte an, hat angemahnt; used for reminders, overdue payments, or formally demanding action'],
    collocations: [{ text: 'eine Zahlung anmahnen', meaning: 'to send a formal reminder for a payment' }],
    wordFamilies: [{ lemma: 'die Mahnung', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'يطالب رسمياً؛ يرسل تذكيراً بالدفع أو الإجراء', ckb:'بە فەرمی بیر بخاتەوە؛ داوای پارەدان یان کردارێک بکات', en:'to formally remind or demand; to issue a reminder', fa:'رسماً یادآوری یا مطالبه کردن؛ اخطار پرداخت دادن', kmr:'bi fermî bîr xistin an daxwaz kirin', pl:'upominać się formalnie; wysłać wezwanie', ro:'a soma; a reaminti formal; a cere oficial', ru:'официально напоминать; требовать исполнения', sq:'të kujtosh zyrtarisht; të kërkosh me njoftim', tr:'resmen hatırlatmak; ihtar etmek' }),
    examples: [
      ex('Die Buchhaltung musste die überfällige Rechnung erneut anmahnen, weil der Kunde nicht reagiert hatte.', { ar:'اضطر قسم المحاسبة إلى المطالبة مجدداً بالفاتورة المتأخرة لأن العميل لم يرد.', ckb:'بەشی ژمێریاری ناچار بوو جارێکی تر پسووڵەی دواکەوتوو بیر بخاتەوە، چونکە کڕیارەکە وەڵامی نەدابووەوە.', en:'Accounting had to send another reminder for the overdue invoice because the customer had not responded.', fa:'واحد حسابداری مجبور شد دوباره بابت فاکتور معوق اخطار بدهد، چون مشتری پاسخ نداده بود.', kmr:'Beşa hesabdarî neçar bû careke din fatûreya derengmayî bibîr bixe, ji ber ku xerîdarê bersiv nedabû.', pl:'Księgowość musiała ponownie upomnieć się o zaległą fakturę, ponieważ klient nie zareagował.', ro:'Contabilitatea a trebuit să reamintească din nou factura restantă, deoarece clientul nu răspunsese.', ru:'Бухгалтерии пришлось снова напомнить о просроченном счёте, потому что клиент не ответил.', sq:'Kontabiliteti duhej ta rikujtonte përsëri faturën e vonuar, sepse klienti nuk kishte reaguar.', tr:'Muhasebe, müşteri yanıt vermediği için gecikmiş faturayı yeniden ihtar etmek zorunda kaldı.' }),
      ex('Der Ausschuss mahnte eine sorgfältigere Prüfung der sozialen Folgen an.', { ar:'طالبت اللجنة بإجراء فحص أكثر دقة للعواقب الاجتماعية.', ckb:'لیژنەکە داوای پشکنینێکی وردتری کاریگەرییە کۆمەڵایەتییەکان کرد.', en:'The committee called for a more careful review of the social consequences.', fa:'کمیته خواستار بررسی دقیق‌تر پیامدهای اجتماعی شد.', kmr:'Komîteyê daxwaza vekolîneke baldarane ya encamên civakî kir.', pl:'Komisja zaapelowała o dokładniejszą ocenę skutków społecznych.', ro:'Comisia a solicitat o examinare mai atentă a consecințelor sociale.', ru:'Комитет потребовал более тщательной проверки социальных последствий.', sq:'Komisioni kërkoi një shqyrtim më të kujdesshëm të pasojave sociale.', tr:'Komite, sosyal sonuçların daha dikkatli incelenmesini talep etti.' })
    ]
  },
  {
    word: 'anmaßend', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'an-ma-ßend',
    topics: ['business-communication','social-and-relationships','management-and-leadership'], usageLabels: ['formal','written','sensitive','advanced'], contextLabels: [],
    grammarNotes: ['adjective; describes arrogant or presumptuous behavior'],
    collocations: [{ text: 'anmaßend auftreten', meaning: 'to behave arrogantly or presumptuously' }],
    wordFamilies: [{ lemma: 'die Anmaßung', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings({ ar:'متغطرس؛ متعالٍ؛ وقح في الادعاء', ckb:'خۆبەزلزان؛ بێڕێز بە شێوەی بانگەشە', en:'arrogant; presumptuous', fa:'متکبر؛ گستاخانه مدعی؛ خودبزرگ‌بین', kmr:'xwe-mezinbîn; bêedebane daxwazkar', pl:'zarozumiały; pretensjonalnie roszczeniowy', ro:'arogant; prezumțios', ru:'самонадеянный; высокомерный', sq:'arrogant; pretendues', tr:'kibirli; haddini aşan' }),
    examples: [
      ex('Der Ton der Antwort wirkte anmaßend, obwohl der sachliche Einwand berechtigt war.', { ar:'بدا أسلوب الرد متغطرساً، رغم أن الاعتراض الموضوعي كان مبرراً.', ckb:'تۆنی وەڵامەکە خۆبەزلزان دیار بوو، هەرچەندە ڕەخنەی بابەتی ڕەوا بوو.', en:'The tone of the reply sounded presumptuous, although the factual objection was justified.', fa:'لحن پاسخ متکبرانه به نظر می‌رسید، هرچند ایراد محتوایی موجه بود.', kmr:'Tona bersivê xwe-mezinbîn xuya dikir, herçend nerazîbûna rastîn rewa bû.', pl:'Ton odpowiedzi brzmiał zarozumiale, choć merytoryczny zarzut był uzasadniony.', ro:'Tonul răspunsului a părut arogant, deși obiecția factuală era justificată.', ru:'Тон ответа показался высокомерным, хотя содержательное возражение было обоснованным.', sq:'Toni i përgjigjes dukej arrogant, megjithëse kundërshtimi faktik ishte i justifikuar.', tr:'Yanıtın tonu kibirli geldi, gerçi somut itiraz haklıydı.' }),
      ex('Es wäre anmaßend, über die Bedürfnisse der Betroffenen zu sprechen, ohne sie einzubeziehen.', { ar:'سيكون من المتعالي الحديث عن احتياجات المتأثرين من دون إشراكهم.', ckb:'خۆبەزلزانە دەبێت ئەگەر باس لە پێویستییەکانی ئەوانە بکەین کە کاریگەریان لێدەکەوێت بەبێ بەشداری پێکردنیان.', en:'It would be presumptuous to speak about the needs of those affected without involving them.', fa:'گستاخانه است که درباره نیازهای افراد درگیر صحبت کنیم بدون آنکه آن‌ها را دخیل کنیم.', kmr:'Bê tevlîkirina kesên bandor lêketî, axaftin li ser pêdiviyên wan xwe-mezinbînî ye.', pl:'Byłoby zarozumiałe mówić o potrzebach osób dotkniętych sprawą bez ich udziału.', ro:'Ar fi prezumțios să vorbim despre nevoile celor afectați fără a-i implica.', ru:'Было бы самонадеянно говорить о потребностях затронутых людей, не вовлекая их.', sq:'Do të ishte arrogante të flitej për nevojat e të prekurve pa i përfshirë ata.', tr:'Etkilenen kişileri dahil etmeden onların ihtiyaçları hakkında konuşmak haddini aşmak olurdu.' })
    ]
  }
];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: `German ${level} Generated Batch ${batch}`, source: 'Hybrid', defaultMeaningLanguages: langs, labels, entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
let output = '';
try { output = execFileSync('dotnet', ['run', '--project', projectPath, '--', '--target', 'shared', '--yes', outPath], { cwd: root, encoding: 'utf8', stdio: ['ignore','pipe','pipe'] }); }
catch (e) { output = `${e.stdout || ''}\n${e.stderr || ''}`; console.log(output); throw new Error('Import failed'); }
console.log(output);
const ok = output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) throw new Error('Import did not meet strict success criteria');
const currentTokens = fs.readFileSync(sourcePath, 'utf8').split(',').map(t => t.trim()).filter(Boolean);
const remaining = currentTokens.slice();
for (const w of expected) { const idx = remaining.indexOf(w); if (idx === -1) throw new Error(`Processed token not found for deletion: ${w}`); remaining.splice(idx, 1); }
fs.writeFileSync(sourcePath, remaining.join(', '), 'utf8');
console.log('SOURCE_UPDATED: yes');
console.log('REMAINING_COUNT:', remaining.length);
console.log('FIRST_10:', remaining.slice(0, 10).join(' | '));
console.log('JSON_PATH:', outPath);
