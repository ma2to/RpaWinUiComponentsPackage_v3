Súhrn: Plán pre vývoj profesionálnych komponentov pre WinUI 3Prezentovaný projekt sa stretáva s fundamentálnym problémom, ktorý sa často vyskytuje v ranných fázach vývoja: monolitický dizajn, ktorý je v zadaní opísaný ako „god level subory“. Tento prístup vedie k tesnej väzbe medzi komponentmi, sťaženej testovateľnosti, zlej udržateľnosti a limituje celkový výkon, najmä pri práci s rozsiahlymi dátovými sadami. Súčasná implementácia logovania a dátovej mriežky je preto nevyhnutne náchylná na chyby a neumožňuje škálovateľný rast.Tento dokument slúži ako komplexný architektonický plán a podrobný „prompt“ pre vývoj nového balíka pre WinUI 3 na platforme.NET 8. Cieľom je rekonštruovať existujúce komponenty do profesionálneho, modulárneho a vysokovýkonného riešenia. Plán navrhuje implementáciu dvoch nezávislých komponentov: abstraktného logovacieho balíka AdvancedWinUiLogger a modulárnej, vysokovýkonnej dátovej mriežky. Navrhované riešenie sa opiera o osvedčené priemyselné vzory, ako je vrstvená architektúra (Clean Architecture), hybridné funkcionálne a objektovo orientované programovanie (OOP) a princíp Dependency Injection (DI). Výsledkom bude produkt, ktorý je ľahko udržiavateľný, rozsiahlo testovateľný a optimalizovaný na spracovanie obrovských objemov dát, čím plne zodpovedá požiadavkám kladeným na vývojárske tímy na najvyššej úrovni.Architektonický základ: Manifest čistého kódu2.1 Štruktúra riešenia a vrstvená architektúraProfesionálny vývojový tím by okamžite identifikoval, že monolitický súbor predstavuje zásadnú prekážku pre dlhodobú udržateľnosť a škálovateľnosť. Namiesto práce s takýmto súborom sa navrhuje prijať vrstvenú architektúru, konkrétne princípy Clean Architecture, ktoré zabezpečujú prísne oddelenie zodpovedností a závislostí medzi jednotlivými časťami aplikácie.1 Tento prístup umožňuje nezávislý vývoj a testovanie každej vrstvy, a zároveň uľahčuje budúce modifikácie.Riešenie bude rozdelené do viacerých projektov, z ktorých každý bude predstavovať jednu vrstvu:Package.Core: Táto vrstva je srdcom celého balíka a obsahuje základné obchodné pravidlá, doménové entity a rozhrania. Bude hostiť modely ako GridRow a definície rozhraní, ako sú IAdvancedGridService a ILoggingService. Je dôležité, že táto vrstva nemá žiadne závislosti na externých rámcoch (napr. WinUI 3 alebo.NET Core).Package.Application: Táto vrstva obsahuje aplikačnú logiku, ktorá definuje prípady použitia (use cases). Bude implementovať rozhrania z vrstvy Package.Core a orchestráciou dátového toku. Služby ako GridDataService alebo LogService by sa nachádzali práve tu.Package.Infrastructure: Táto vrstva slúži ako adaptér pre externé systémy a služby. Bude obsahovať konkrétne implementácie rozhraní z vrstvy Package.Core a Package.Application, ako napríklad AdvancedFileLoggerProvider, ktorý spracováva súborové logovanie, alebo akékoľvek triedy pre prístup k dátam. Ide o implementačnú vrstvu, ktorá sa viaže na konkrétne rámce a technológie (ako napríklad logovacie balíky).Package.WinUI: Táto vrstva je prezentačnou vrstvou, ktorá obsahuje všetky prvky užívateľského rozhrania, ako sú vlastné WinUI ovládacie prvky, XAML súbory a ViewModels. Je jedinou vrstvou, ktorá má priamu závislosť na WinUI 3 a komunikuje s nižšími vrstvami výhradne cez rozhrania, ktoré sú jej do konštruktora vkladané pomocou Dependency Injection.1Prehľadná štruktúra riešenia je uvedená v nasledujúcej tabuľke. Práve rozdelenie projektu do logických vrstiev je priamou a profesionálnou odpoveďou na požiadavku opustiť "god level subor" a použiť radšej veľa malých súborov.1VrstvaZodpovednosťKľúčové triedy a rozhraniaZávislostiPackage.CoreObchodné pravidlá, doménové modely a rozhraniaGridRow, IAdvancedGridService, ILoggingServiceŽiadnePackage.ApplicationAplikačná logika, prípady použitiaGridDataServicePackage.CorePackage.InfrastructureKonkrétne implementácie, adaptéry pre externé službyAdvancedFileLoggerProvider, FileLoggerOptionsPackage.Core, Package.Application, Microsoft.Extensions.LoggingPackage.WinUIPrezentačná vrstva, UI, ViewModelsVlastné ovládacie prvky, XAML, GridViewModelPackage.Application, Package.Infrastructure (cez DI), WinUI 32.2 Hybridná funkcionálno-OOP paradigmaPoužívateľove požiadavky na hybridný štýl vývoja sú sofistikovanou a modernou voľbou. Nie je cieľom uprednostniť jeden prístup pred druhým, ale namiesto toho využiť silné stránky každého z nich v situáciách, kde sa najlepšie uplatnia. Tento prístup je v C# prirodzene podporovaný.3Kde použiť OOP: OOP sa najlepšie hodí pre modelovanie reálnych entít, ktoré majú stav a správanie. Dátová mriežka je z tohto hľadiska ideálnym kandidátom. Vytvorenie tried, ako je GridViewModel alebo GridRow, s jasne definovanými vlastnosťami (napr. farba ohraničenia, vybraný stav) a metódami (UpdateData), je prirodzenou voľbou. Tento prístup zabezpečuje enkapsuláciu a uľahčuje správu stavu, čo je pre UI komponent kľúčové.4Kde použiť funkcionálne programovanie (FP): Naopak, FP je vynikajúce pre operácie, ktoré sú bezstavové a zameriavajú sa na transformáciu dát. Príklady zahŕňajú triedenie, filtrovanie a vyhľadávanie.3 Tieto operácie by mali prijímať dátovú sadu ako vstup, aplikovať transformáciu (napríklad pomocou LINQ výrazov) a vrátiť novú, transformovanú sadu bez zmeny pôvodného stavu. Takýto prístup minimalizuje vedľajšie účinky, robí kód predvídateľným a ľahko testovateľným.3Spojenie týchto dvoch paradigiem bude obzvlášť dôležité pri implementácii "inteligentných" funkcií triedenia a filtrovania. Logika spracovania bude implementovaná ako čistá funkcia, ktorá vráti novú, usporiadanú alebo filtrovanú kolekciu, čo zaručí, že sa pôvodná dátová sada nezmení. Tento prístup elegantne rieši aj špecifickú požiadavku presunúť prázdne riadky na koniec po triedení, čím sa zabezpečí, že sa na ne neaplikuje logika triedenia, ale jednoducho sa pridajú na koniec výslednej sady.3FunkciaParadigmaDôvodGridViewModelOOPSpravuje stav užívateľského rozhrania a jeho správanie (napr. vlastnosti IsLoading, SelectedRows).Triedenie/filtrovanie dátFunkcionálne (FP)Bezstavová transformácia dát s použitím čistých funkcií (napr. LINQ) na zníženie vedľajších účinkov.Validácia v reálnom časeHybridGridRow (OOP objekt) drží stav validačných chýb, zatiaľ čo samotná validačná logika môže byť čistá funkcia.Operácie s databázou/APIOOPSpracovanie externého stavu a vedľajších účinkov v rámci vyhradenej, zapuzdrenej služby.2.3 Princíp inverzie závislostí a Dependency InjectionPrincíp inverzie závislostí (DI) je základným pilierom moderného vývoja v.NET Core a priamo nadväzuje na požiadavky na čistý a modulárny kód.5 Namiesto toho, aby triedy priamo vytvárali svoje závislosti (napríklad new Logger()), dostávajú ich ako argumenty vo svojom konštruktore. Toto vkladanie závislostí zabezpečuje DI kontajner, ktorý je súčasťou.NET Core.5Tento prístup je kritický pre dosiahnutie slabej väzby, čo má priamy vplyv na testovateľnosť a flexibilitu.6 Je to obzvlášť dôležité v prípade komponentu logovania, ktorý má používať Microsoft.Extensions.Logging.Abstractions.8 Vďaka DI bude balík závisieť iba na abstraktnom rozhraní ILogger namiesto na konkrétnom implementačnom balíku. Hlavná aplikácia sa potom rozhodne, ktorý logovací systém (napr. Serilog, NLog) sa použije, a zaregistruje ho v DI kontajneri.9 V dôsledku toho môže byť náš balík pripojený k akémukoľvek logovaciemu systému bez akýchkoľvek zmien v jeho kóde.Komponent I: AdvancedWinUiLogger – Abstraktné riešenie pre logovanie3.1 Verejné API a rozhranie ILoggerCieľom je vytvoriť logger s čistým a jednoduchým API, ktorý možno použiť iba s jedným using príkazom.8 V súlade s princípmi DI bude balík exponovať rozšírujúcu metódu AddAdvancedLogger() pre IServiceCollection, ktorá sa použije pri štarte aplikácie na registráciu všetkých potrebných služieb.7Jadrom implementácie bude použitie generického rozhrania ILogger<TCategoryName>.8 To umožňuje DI kontajneru automaticky poskytnúť každému komponentu, ktorý ho požaduje v konštruktore, špecializovanú inštanciu loggeru, ktorej názov kategórie je odvodený od názvu triedy, čím sa logy automaticky organizujú podľa pôvodu.83.2 Implementačné detailyHoci používateľ uviedol, že logovací komponent je „hotovy“, požiadavky na súborové logovanie, názvy súborov a rozsekávanie súborov naznačujú, že hotový nie je, aspoň v kontexte profesionálneho riešenia. Pripravený logovací balík by nemal tieto špecifické funkcie hardcodovať. Namiesto toho by mal poskytovať mechanizmus na ich konfiguráciu.Navrhuje sa preto vytvorenie vlastného ILoggerProvider s názvom AdvancedFileLoggerProvider.10 Tento poskytovateľ bude zodpovedný za implementáciu používateľom požadovanej logiky, ako je určenie umiestnenia súboru, jeho názvu a mechanizmus automatického rozdelenia, ak prekročí určenú veľkosť. Na konfiguráciu týchto nastavení sa použije konfiguračný objekt FileLoggerOptions, ktorý bude odovzdaný do rozšírujúcej metódy AddAdvancedLogger(). Tento prístup je v súlade s najlepšími praxami v.NET Core pre správu konfigurácií.73.3 DI stratégia pre loggerV rámci kontextu hostiteľskej aplikácie bude rozšírujúca metóda AddAdvancedLogger() zaregistrovať AdvancedFileLoggerProvider ako singleton službu v IServiceCollection.8 Táto stratégia zabezpečí, že kontajner vytvorí len jednu inštanciu poskytovateľa, ktorá bude zdieľaná v celej aplikácii.Riešenie elegantne prekonáva zdanlivý rozpor medzi požiadavkou na „hotový“ balík a flexibilitu pripájania „hociakého logovacieho systému“. Logovací balík bude skutočne „hotový“ – ale v zmysle, že poskytuje profesionálny, konfigurovateľný a modulárny mechanizmus, ktorý možno použiť v hlavnej aplikácii. Hostiteľská aplikácia si potom môže zvoliť, či tento mechanizmus použije alebo ho nahradí iným poskytovateľom logovania (napr. Serilogom), čím sa zachová plná flexibilita a architektúra zostane čistá.7Komponent II: Modulárna, vysokovýkonná dátová mriežka4.1 Základný návrh: Vzor MVVM a zdieľaná logikaPožiadavka na zdieľanie biznis logiky medzi užívateľom (UI) a automatizovanými skriptami (bez UI) predstavuje klasický problém, ktorý sa rieši vzorom Model-View-ViewModel (MVVM).11 MVVM zabezpečuje oddelenie dátovej logiky od prezentačnej, čo umožňuje opakovane použiť biznis logiku.Štruktúra komponentu bude nasledovná:Model: Bude obsahovať čisté C# dátové objekty (POCOs), ktoré reprezentujú riadky v tabuľke. Tieto modely budú obsahovať dátové vlastnosti a implementovať rozhranie INotifyDataErrorInfo pre validáciu.14ViewModel: GridViewModel bude riadiacim centrom. Bude vystavovať dáta a príkazy pre View (napr. ItemsSource, SearchCommand) a komunikovať s nižšou vrstvou obchodnej logiky.View: Samotný WinUI UserControl, ktorý obsahuje vizuálnu reprezentáciu tabuľky (napr. ItemsRepeater s prispôsobenými šablónami). Bude používať x:Bind na prepojenie s GridViewModel.16Kľúčovým prvkom bude zdieľaná služba (GridDataService) umiestnená vo vrstve Package.Application. Táto služba bude obsahovať metódy pre vyhľadávanie, triedenie, filtrovanie, pridávanie a mazanie dát. GridViewModel aj externý automatizačný skript budú túto službu používať rovnako. Jediný rozdiel je, že GridViewModel po dokončení operácie zo služby aktualizuje užívateľské rozhranie (napr. volaním metódy na UI vlákne, ak operácia prebehla asynchrónne).18 Skript túto aktualizáciu vynechá, no môže ju vykonať na požiadanie.4.2 Stratégia pre rozsiahle dáta: Virtualizácia a optimalizáciaSpráva desiatok tisíc alebo miliónov riadkov je pre WinUI 3 náročná, a to najmä kvôli pomalým operáciám WinRT interop.19 Používanie štandardných ListView alebo komerčných DataGrid komponentov môže byť limitujúce kvôli špecifickým požiadavkám na inteligentnú správu riadkov.Profesionálne riešenie preto spočíva vo vytvorení vlastnej virtualizačnej logiky. Namiesto spoliehania sa na predpripravené komponenty sa navrhuje použiť ItemsRepeater.21ItemsRepeater je nízkoúrovňový, výkonný ovládací prvok, ktorý neposkytuje žiadnu vstavanú politiku (ako výber alebo navigácia), ale poskytuje kompletnú kontrolu nad vykresľovaním a virtualizáciou.Keďže ItemsRepeater nepodporuje rozhranie ISupportIncrementalLoading natívne 22, bude implementovaná vlastná manuálna virtualizácia. Táto stratégia zahŕňa zabalenie ItemsRepeater do ScrollViewer a sledovanie jeho udalosti ViewChanged. Keď sa užívateľ posunie blízko konca načítaných dát, náš kód manuálne spustí asynchrónnu metódu LoadMoreItemsAsync() na dátovom zdroji.21 Tento prístup poskytuje plnú kontrolu nad pamäťou a výkonom, čo je nevyhnutné pre spracovanie obrovských dátových sád.4.3 Implementácia interaktívnych funkcií: Detailný plán4.3.1 Validácia v reálnom čase a dávková validáciaValidácia v reálnom čase: Na implementáciu okamžitej validácie počas písania do bunky sa na dátovom modeli použije rozhranie INotifyDataErrorInfo.14 Toto rozhranie poskytuje mechanizmus, prostredníctvom ktorého môže UI automaticky získať validačné chyby pre konkrétnu vlastnosť. Po zadaní neplatnej hodnoty model signalizuje chybu, čo sa okamžite prejaví v UI (napr. zmenou farby ohraničenia bunky).14Dávková validácia: Pre operácie ako import alebo prilepenie rozsiahlych dát sa validácia nebude vykonávať pre každú bunku samostatne. Namiesto toho sa spustí jedna dávková operácia na pozadí, ktorá skontroluje celý zmenený rozsah.23 Toto zabráni zbytočným a výkonnostne náročným aktualizáciám UI a zabezpečí, že UI sa aktualizuje len raz po dokončení celej operácie, čo je pre plynulú prácu s veľkými dátami kľúčové.4.3.2 Dynamické UI a vlastný štýlPre dynamickú zmenu farieb a štýlov z hostiteľskej aplikácie bude GridViewModel vystavovať vlastnosti (napríklad ValidationBorderColor, FocusBackgroundColor), ktoré sú viazané na XAML prvky. Vzhľadom na obmedzenia WinUI 3 týkajúce sa priameho prístupu k prostriedkom témy z C# 24, profesionálny prístup spočíva v definovaní dynamických štýlov v XAML pomocou <Style> a ThemeResource.24 Tieto štýly môžu byť potom z C# kódu priradené ovládacím prvkom, čo zabezpečí, že sa zmenia, ak sa zmení celková téma aplikácie.244.3.3 Inteligentná správa riadkovKomponent bude implementovať vlastnú logiku správy riadkov, aby splnil požiadavky na minimálny počet riadkov a prázdny riadok na konci.Inteligentné pridávanie: Kedykoľvek sa začnú editovať dáta v poslednom prázdnom riadku, GridViewModel automaticky pridá na koniec kolekcie ďalší prázdny riadok.26Inteligentné mazanie: Logika mazania sa bude líšiť podľa počtu riadkov. Ak je počet riadkov väčší ako stanovené minimum, riadok sa fyzicky odstráni z kolekcie. Ak je však počet riadkov menší alebo rovný minimu, z riadku sa iba vymažú dáta, ale samotný riadok zostane v kolekcii.264.3.4 Vyhľadávanie, triedenie a filtrovanieAko bolo uvedené v sekcii 2.2, tieto operácie budú implementované ako čisté funkcie vo vrstve aplikačnej logiky. Tým sa zabezpečí, že sa nebudú meniť pôvodné dáta. Prázdne riadky budú pred triedením dočasne odstránené a po dokončení operácie opäť pridané na koniec výslednej kolekcie, čím sa priamo naplní požiadavka používateľa.3PožiadavkaNavrhované riešenieZdrojValidácia v reálnom časeImplementácia rozhrania INotifyDataErrorInfo na dátovom modeli pre prepojenie s UI a automatickú zmenu štýlu ohraničenia bunky.14Dávková/hromadná validáciaValidácia celého zmeneného rozsahu na pozadí pre optimalizáciu výkonu. UI sa aktualizuje iba raz po dokončení operácie.23Dynamické UI a štýlovanieExponovanie vlastností štýlu vo ViewModel a použitie XAML ThemeResource pre prepojenie s témou aplikácie.24Inteligentné pridávanie/mazanieVlastná logika vo ViewModel na udržanie minimálneho počtu riadkov a automatické pridávanie prázdneho riadku pri vstupe dát do posledného riadku.26Vyhľadávanie, triedenie, filtrovanieImplementácia ako bezstavových funkcií (FP) pre spracovanie dát. Prázdne riadky budú odstránené a opäť pridané na koniec po dokončení operácie.3Stratégia refaktoringu a implementácie5.1 Od "God File" k harmonickému kóduPrechod od monolitického súboru k navrhovanej vrstvenej architektúre by sa mal uskutočniť v jasne definovaných fázach:Fáza 1 - Príprava: Vytvorenie novej štruktúry riešenia s oddelenými projektami (.Core, .Application, .Infrastructure, .WinUI).Fáza 2 - Extrakcia a abstrakcia: Opatrné vyňatie existujúcej logiky z monolitického súboru. Identifikácia závislostí a vytvorenie abstraktných rozhraní pre tieto závislosti.Fáza 3 - Implementácia: Reimplementácia extrahovanej logiky do nových vrstiev s prísnym dodržiavaním princípov DI a vrstvenej architektúry.Fáza 4 - Integrácia: Pripojenie nových komponentov do hostiteľskej aplikácie pomocou DI kontajnera. Tento prístup minimalizuje riziká a zabezpečí plynulý prechod.5.2 Komplexná dokumentáciaDokumentácia je živý artefakt. Je kritické, aby sa súbor DocumentationWinUiMain.md aktualizoval v každej fáze vývoja. Zdokumentovanie novej architektúry, verejných API a správania každého komponentu je kľúčové pre úspešné odovzdanie projektu a uľahčenie práce budúcim vývojárom.5.3 Zabezpečenie kvality a testovanieNavrhovaná architektúra umožňuje rozsiahle testovanie.Unit testy: Vrstvy Package.Core a Package.Application možno rozsiahlo testovať v izolácii, pretože ich závislosti sú abstrahované cez rozhrania.1Integračné testy: Tieto testy overia, či vrstvy správne spolupracujú. Napríklad sa preverí, či GridViewModel správne volá metódy služby GridDataService.Výkonnostné testy: Pre overenie výkonu pri spracovaní miliónov riadkov je kľúčové použiť nástroje, ako je Windows Performance Analyzer (WPA) 20, ktoré dokážu monitorovať správanie UI vlákna a identifikovať potenciálne úzke miesta. Tento proaktívny prístup zabezpečí, že balík bude spoľahlivý a stabilný aj pri najvyššej záťaži.



Návrh architektúry komponentového balíka
Balík dvoch nezávislých komponentov (AdvancedWinUI.Logger a AdvancedWinUI.DataGrid) bude navrhnutý s dôrazom na princípy SOLID, Dependency Injection a Clean Architecture. SOLID zásady sú „päť prikázaní“ kvalitného softvéru a tvoria základ modulárneho, udržateľného kódu[1]. Clean Architecture oddeľuje doménovú logiku (core) od infraštruktúry a UI, čo zaisťuje, že jadro zostáva nezávislé a testovateľné. Napríklad v Core definujeme rozhrania (abstrakcie) a implementácie umiestňujeme do vonkajších vrstiev, ktoré ich poskytujú cez DI[2]. Pri návrhu služieb dbáme na to, aby boli malé, bez globálneho stavu a ľahko testovateľné. Zabráňme priamemu vytváraniu inštancií v kóde – všetky závislosti poskytne DI kontajner[3].
•	SOLID princípy: Každá trieda má jednu zodpovednosť, nové funkcionality pridávame rozšírením existujúcich, nie modifikáciou (OCP), triedy možno nahradiť bez zmeny klienta (LSP), rozhrania sú špecifické pre potrebu (ISP) a implementácie závisí od abstrakcie, nie naopak (DIP)[1][2].
•	DI a testovateľnosť: Vyhýbame sa statickým globálnym stavom; všetky služby registrujeme v IServiceCollection a vkladajú sa cez konštruktory. Akákoľvek trieda s mnohými závislosťami by možno porušovala SRP a mala by byť rozdeliť na viacero tried[3].
•	Functional-OOP mix: V C# (a .NET 8) môžeme kombinovať OOP triedy s funkcionálnymi prístupmi (immutable struktúry, statické utility). Kľúčom je zachovať neporušenosť údajov, čisté funkcie tam, kde nedochádza k vedľajším efektom, a zároveň využívať triedne abstrakcie pre komplexné logiky.
Logger Komponent (AdvancedWinUI.Logger)
Zameranie: poskytne modulárny logger na báze Microsoft.Extensions.Logging. Používame abstrakciu ILogger<T> a ILoggerFactory, ale namiesto štandardných providerov implementujeme vlastný log do súboru s rotáciou.
•	Rozhranie a implementácia: Definujeme rozhranie IAdvancedLogger (metódy Info(), Error(), atď.) v jadre komponentu. Implementácia AdvancedLoggerProvider : ILoggerProvider v infraštruktúrnej vrstve vytvára konkrétne ILogger inštancie (napr. trieda FileLogger), ktoré zapisujú logy do súboru. Podľa vzoru Clean Arch definujeme abstrakciu v Core a plnenie v infraštruktúre[2].
•	Konfigurácia: Pomocou IOptionsMonitor<LoggerOptions> umožníme dynamické nastavenie cesty k log súborom, maximálnej veľkosti súboru a počtu uchovávaných rotovaných súborov. Následne v ILoggerProvider využijeme tieto nastavenia pri vytváraní loggerov. Rovnako ako v dokumentácii Microsoft, aj tu voláme v ILogger.Log najprv IsEnabled(logLevel) pre optimalizáciu výkonu[4].
•	LoggerExtensions: Pre pohodlné volanie budeme doplňovať statické rozšírenia, napr. logger.Info("message"), logger.Error(ex, "failed"), ktoré pod kapotou volajú logger.LogInformation atď. Vďaka jednej doménovej knižnici s rozhraniami ostane API čisté.
•	Rotácia a súbory: Implementujeme rotáciu podľa veľkosti – keď súbor prekročí stanovený limit, premenujeme ho a začneme nový. Konfigurácia názvu súboru a cesty bude viazaná na LoggerOptions. Môžeme využiť podobný prístup ako Azure App Services File Logger (konfigurovanie FileSizeLimit, RetainedFileCount)[5].
•	Bez UI: Tento komponent neobsahuje žiadny UI. Využíva sa takto: knižnica je injektovaná do aplikácie, kde pomocou DI získame IAdvancedLogger a používame rozšírenia. V tíme by README uvádzal príklad registrácie: services.AddAdvancedWinUiLogger(config => { … });.
DataGrid Komponent (AdvancedWinUI.DataGrid)
Zameranie: vysoko výkonná tabuľka pre WinUI3 a „headless“ (non-UI) scenáre. Kľúčové princípy sú oddelenie dátovej logiky od UI, použiteľnosť v rôznych kontextoch a množstvo features.
•	Architektúra: Komponent bude rozdelený na dve časti: Core knižnica s logikou správy údajov a validácií (bez závislosti na WinUI), a UI projekt, ktorý obsahuje WinUI elementy a XAML. Core definuje rozhrania a modely (tabuľky, riadky, validátory). UI časť má UserControl alebo Page s DataGrid (napr. z WinUI Community Toolkit) a viaže ho na Core cez ViewModel. Takto držíme čisté oddelenie logiky od prezentácie.
•	Výkon a Virtualizácia: Pre spracovanie miliónov riadkov je kľúčová virtualizácia – načítavanie dát len na požiadanie. Napríklad Syncfusion uvádza, že s povolenou virtualizáciou dokáže načítať milióny záznamov okamžite[6]. My môžeme použiť VirtualizingStackPanel v DataGrid alebo vlastný mechanizmus načítavania stránok z dátového zdroja. Podkladom môže byť napr. IEnumerable<T> alebo DataTable s virtuálnou pamäťou (používa sa LoadMoreItems pattern). Tým sa vyhneme načítavaniu celého datasetu do pamäte.
•	Validácia: Komponent bude podporovať bunke ani celú tabuľku. Podobne ako vo WPF DataGrid, aj tu umožníme nastaviť validátory na každý stĺpec (napr. podľa dátových anotácií alebo funkčných delegátov), ktoré sa vyhodnocujú pri editácii bunky (real-time) alebo pri potvrdení celého riadku (batch). WPF DataGrid podporuje validáciu na úrovni buniek aj riadkov[7]; náš dizajn umožní pridať ValidationRule pre celý objekt (celý riadok) alebo pre jednotlivé vlastnosti. Chyby sa budú hlásiť pomocou notifikácií na modeli (napr. implementácia INotifyDataErrorInfo), aby ich UI mohla zobraziť (červené orámovanie bunky, vypisanie custom validacnych chybovych hlasok do special stlpca ValidAlerts). Tým poskytujeme okamžitú spätnú väzbu.
•	Smart správa riadkov: Komponent zabezpečí, že tabuľka má vždy aspoň určitý počet riadkov (napríklad minimálne 1). Po úprave údajov skontrolujeme, či je posledný riadok vyplnený; ak áno, automaticky vytvoríme nový prázdny riadok na koniec (súvisí s pridaním nových záznamov). Tým používateľ neustále vidí na konci riadok na zadávanie nových dát. Ak používateľ vymaže existujúce riadky, minimalizujeme tabuľku na nastavený počet (napr. 1).
•	Farby a štýly: Poskytneme možnosti vlastnejšej grafickej úpravy. Napríklad cez vlastnosti alebo ResourceDictionary umožníme definovať farby pozadia, farby chýb a iné štýly. Ako uvádza Syncfusion, vzhľad DataGridu a jeho prvkov (bunky, riadky, hlavičky) sa dá prispôsobiť množstvom dostupných vlastností a štýlov[8]. Navrhneme teda tematické štýly (light/dark) a možnosť definovať vlastné XAML štýly pre rôzne typy buniek (číselné, dátumové, textové).
•	Drag & Drop: Podpora presúvania riadkov (drag&drop) uľahčí prácu používateľom. Riešenie: v UI nastavíme AllowRowDragDrop=true a AllowDrop=true (podľa Syncfusion)[9]. Následne obslúžime udalosti typu DragStarting, DragOver, Dropping, Dropped na pretvorenie kolekcie dát podľa nového poradia. Princíp: drag&drop sa deje na úrovni UI, ale manipulácia s dátami je vykonaná cez metódy managera (nepíše sa v code-behind veľa logiky).
•	Výber a zmena veľkosti: Tabuľka umožní viacnásobný výber riadkov/buniek (SelectionMode=Extended alebo Multiple), aby bol kompatibilný s bežným užívateľským očakávaním (podpora Ctrl+klik, Shift+šipka). Umožníme tiež zmenu veľkosti stĺpcov (ručné pretiahnutie alebo automatické prispôsobenie). Tieto vlastnosti budú nakonfigurovateľné parametrami control-u.
•	Import/Export: Komponent poskytne API na načítanie a uloženie dát z/do rôznych formátov. Špecificky uvažujeme import/export z Dictionary<string, object> (kamenné dáta) a DataTable (štandardný .NET typ pre tabuľky). Používateľ si môže poslať dáta napr. vo forme List<Dictionary<string, object>> do gridu, alebo celý DataTable. Export by poskytoval rovnaké typy späť. Pre obyčajné scenáre by sme mohli tiež doplniť CSV export/import, ale minimom je spomínaný export do generických formátov.
•	Reusabilné API: Dôležitá vlastnosť je použiteľnosť cez API aj bez UI. Preto bude mať komponent tzv. manager (napr. AdvancedDataGridManager), ktorý umožní manipuláciu s dátami tabuľky skriptovo. Napr. v konzolovej či serverovej aplikácii by sme mohli s AdvancedDataGridManager načítať DataTable, spustiť validáciu, a exportovať späť, bez potreby načítavať XAML. Zároveň UI verzia túto logiku využíva cez ViewModel, ktorý prepája DataGrid s managerom.
Adresárová štruktúra a súbory
Navrhneme modulárnu štruktúru na úrovni riešenia (solution) s osobitnými projektmi pre každý komponent a testy:
•	/RpaWinUiComponentsPackage/AdvancedWinUILogger/ – Class Library projekt obsahujúci IAdvancedLogger, LoggerOptions, FileLoggerProvider, FileLogger a LoggerExtensions.
•	/RpaWinUiComponentsPackage/AdvancedWinUIDataGrid/ – Class Library projekt obsahujúci jadrovú logiku DataGrid (DataGridModel, DataGridManager, validátory), ale bez UI závislostí.
•	/RpaWinUiComponentsPackage/AdvancedWinUIDataGrid/UI/ – WinUI3 projekt (alebo WinUI komponent) obsahujúci XAML/C# kód pre vizuálny DataGrid, viazaný na core knižnicu cez ViewModel.
•	/RpaWinUiComponentsPackage/Common/ (voliteľné) – Pre zdieľané typy a abstrakcie, ak nejaké existujú (napr. spoločné rozhrania, error logging).
•	/tests/AdvancedWinUI.Logger.Tests/ – Unit testy pre logging (kontrola rotácie, formát logu, chybové stavy).
•	/tests/AdvancedWinUI.DataGrid.Tests/ – Unit testy pre dátový grid (testy validácie, správania pri pridávaní/vymazávaní riadkov, import/export).
Každý projekt má vlastný .csproj súbor. V koreňovom adresári bude súbor RpaWinUiComponentsPackage.sln. V dokumentácii (README, CONTRIBUTING) vysvetlíme, ktoré projekty sú k čomu určené. Túto štruktúru možno porovnať s bežným Clean Architecture template, ktorý má projekty pre Domain/Core, Application, Infrastructure a UI[10].
Rozhrania a manažérsky pattern
Pre každú funkčnosť definujeme rozhrania (contract) a manažérske triedy (implementácie). Napríklad:
•	Logger: Rozhranie IAdvancedLogger (s metódami Info, Warn, Error atď.). Manager/trieda AdvancedLoggerManager (alebo jednoducho FileLogger) implementuje toto rozhranie. V kóde aplikácie závisíme na IAdvancedLogger, nie na konkrétnej triede (DIP). Inštancia sa injektuje cez DI.
•	DataGrid: Rozhranie IDataGridService alebo IAdvancedDataGrid, ktoré môže mať metódy LoadData(DataTable dt), ExportData(), ValidateAll(), AddEmptyRow(), atď. Manager/trieda AdvancedDataGridManager túto logiku implementuje. UI (WinUI Page/ViewModel) používa toto rozhranie na operácie s tabuľkou.
Tento prístup je v súlade s Clean Arch: vnútorné moduly definujú abstrakcie, vonkajšie poskytujú implementácie[2]. Managerová trieda vystupuje ako Facade, skrýva detaily implementácie a ponúka jedno prístupové miesto (aj keď vo vnútri používa viaceré menšie triedy). V kóde teda developer zavolá napr. dataGridManager.Load(...) a nemusí riešiť, ako sa interné časti skladajú. Toto uľahčuje testovanie (môžeme injektovať falošný IAdvancedDataGrid do ViewModel) a zvyšuje udržateľnosť kódu. Podľa odporúčaní Microsoft sa služby, ktoré injektujeme, majú správať ako malé, izolovateľné jednotky[3].
Čisté API a používanie
Každý komponent vystaví jednoduché API, ktoré vyžaduje iba jeden using. Napríklad:
using AdvancedWinUILogger;
// ...
IAdvancedLogger logger = serviceProvider.GetRequiredService<IAdvancedLogger>();
logger.Info("Aplikácia sa spustila.");
Rovnako pre DataGrid:
using AdvancedWinUIDataGrid;
// ...
IDataGridService grid = serviceProvider.GetRequiredService<IDataGridService>();
grid.LoadData(myDataTable);
Čisté API znamená, že všetky dôležité typy sú v jednom namespace (napr. AdvancedWinUILogger, AdvancedWinUIDataGrid) a developer nepotrebuje mnoho using direktív. Pre registráciu v DI môžeme pridať extension metódu: services.AddAdvancedWinUiLogger(opts => { ... });, ktorá zaregistruje IAdvancedLogger a ILoggerProvider. Podobne services.AddAdvancedWinUiDataGrid(). Tým udržíme kód, ktorý komponent používa, čo najjednoduchší.
Validácia, správa riadkov a UI integrácia
V časti DataGrid:
•	Validácia: Pri editácii bunky spustíme ValidateCellValue, ktorá využije buď dátové anotácie modelu, alebo vlastné pravidlá. Príklad: ak používame INotifyDataErrorInfo, UI (WinUI DataGrid) môže automaticky zobraziť červené orámovanie. Celková validácia (ValidateAll) prejde všetky riadky a zhromaždí chyby.
•	Smart riadky: V AdvancedDataGridManager budeme sledovať stav tabuľky. Ak je posledný riadok plný (všetky stĺpce majú hodnoty), vytvoríme nový prázdny. Ak niektoré užívateľ vymaže tak, že ich počet klesne pod minimálny, pridáme chýbajúce. Toto logiku môže vyvolať UI (napr. pri každom ukončení úpravy bunky) a zabezpečíme, že používateľ nikdy nemá „prázdny“ grid bez šance pridávať ďalej riadky.
•	UI integrácia: Použijeme MVVM vzor. View (XAML s DataGridom) bude viazaná na ViewModel, ktorý intern zavolá metódy IDataGridService. Všetka business logika (validácia, správa dát) bude mimo UI. UI projekt/vrstva bude obsahovať len vizualizáciu (UserControls, štýly, eventhandler-y na úrovni UI, ktoré následne volajú Core služby). Tým sa vyhneme súborom „boh-god“, ktoré by robili všetko v jednom. Každá trieda či komponent má jasnú zodpovednosť (SRP) a prekopírovanie logiky medzi zložkami je minimalizované.
README a tímová spolupráca
V dokumentácii balíka (README.md) definujeme postupy pre tím:
•	Popis komponentov: Účel AdvancedWinUILogger a AdvancedWinUIDataGrid, hlavné vlastnosti (logovanie do súboru s rotáciou, 10M riadkov s validáciou atď.).
•	Príklad použitia: Ukážkové kódy (snippet) registrácie služieb v DI a základného použitia (napr. prihlásenie logu a načítanie DataGrid).
•	Konfigurácia: Vysvetlíme voľby konfigurácie LoggerOptions (napr. LogPath, MaxFileSize, MinRows, témy pre farby), ako ich nastaviť v kode alebo v appsettings.json.
•	Vývojové štandardy: Spolu ujasníme kódové konvencie (názvy parametrov, vlastností), závislosti, verzovanie API (semantic versioning). Napríklad dodržiavať MENDA principy: každé API je spätne kompatibilné, update MAJOR ak dôjde k breaking change.
•	Použité knižnice: Uvedieme, že používame Microsoft.Extensions.Logging.Abstractions (pre logging), System.Data pre DataTable, a že používame .NET 8, WinUI 3.
•	Pravidlá logovania: Definujeme, čo sa loguje na ktorú úroveň (napr. Info pre štatistické udalosti, Warning pre nestabilné ale nefatálne, Error pre výnimky). Ukladanie citlivých údajov zakážeme. Tím dodržiava, že každá výnimka sa zaloguje s Error, kritické udalosti s vyššou úrovňou.
•	Kontribúcia a testovanie: Odporúčame používať jednotkové testy (xUnit/NUnit) pri pridávaní funkcionalít. Názvoslovie v GIT e.g. branch pod označením issue # atď.
V README sa skrátka nastaví “tone” enterprise environmentu: code review, CI pipeline, jednotkové testy, konzistentné formátovanie. Taktiež sem pridáme linky na relevantné dokumenty (napr. link na Microsoft Logging docs, SOLID princípy atď.), aby mal tím referencie.
Zhrnutie
Navrhovaný balík je modulárny, riadi sa princípmi Clean Architecture a SOLID, využíva DI a testovateľné služby[2][3]. Komponenty sú oddelené logikou/UI, navzájom nezávislé, s čitateľným API („jedno using“). AdvancedLogger sa integruje ako ILoggerProvider so špeciálnymi nastaveniami súborov a rotácie, využíva ILogger rozhranie a vlastné extensions (napr. logger.Info())[4]. AdvancedDataGrid ponúka efektívne spracovanie veľkých objemov dát (virtualizácia[6]), komplexnú validáciu (buniek aj riadkov[7]), a užívateľsky prívetivé funkcie (drag&drop[9], prispôsobiteľné štýly[8]). Štruktúra projektu a definície rozhraní zabezpečia, že každý komponent je robustný, rozšíriteľný a pripravený pre enterprise nasadenie.
Zdroj: Návrh vychádza z moderných odporúčaní tvorby softvéru, napr. principy Clean Architecture a SOLID[2][1], oficiálnej dokumentácie .NET Logging a DI[4][3] a príkladov špičkových DataGrid knižníc[6][9][7], aby komponenty boli spoľahlivé a vysoko výkonné.


Architektonický návrh a implementačný plán pre profesionálny balík komponentov WinUI3 v C#1. Architektonická vízia: Základné princípy pre balík profesionálnej kvality1.1. Prehľad: Od chaosu k súdržnostiTento dokument slúži ako komplexný architektonický plán pre vývoj balíka komponentov WinUI3. Vychádza z požiadavky na riešenie, ktoré je momentálne poznačené "god level" súbormi a problémami s výkonom, a navrhuje kompletnú profesionálnu rekonštrukciu. Navrhované riešenie presahuje rámec bežnej úpravy kódu; zameriava sa na zavedenie robustnej architektúry, ktorá zabezpečí dlhodobú udržateľnosť, rozšíriteľnosť a testovateľnosť. Riešenie je postavené na princípoch Clean Architecture, prístupoch Dependency Injection a hybridnom funkcionálno-OOP programovaní, čím sa zabezpečí, že každý komponent je nezávislý a plne optimalizovaný pre moderné desktopové aplikácie v.NET 8.1.2. Plán Clean Architecture: Pravidlo závislostí ako kompasZákladom pre návrh balíka je Clean Architecture, princíp, ktorý presadzuje oddelenie záujmov a nezávislosť komponentov.1 Hlavným cieľom je zabezpečiť, aby základná biznis logika (Core) bola úplne nezávislá od vonkajších vplyvov, ako sú databázy, súborové systémy alebo užívateľské rozhranie. Toto oddelenie zabraňuje vzniku "god level" súborov, ktoré porušujú princíp jedinej zodpovednosti a spájajú rôznorodé funkcie do jednej triedy.3Navrhovaná štruktúra projektu bude rozdelená do štyroch hlavných vrstiev s pravidlom závislosti, ktoré hovorí, že vnútorné kruhy (vrstvy) nemajú žiadne poznatky o vonkajších kruhoch.Vrstvy v Clean Architecture pre C# balíkVrstava Core: Je to najvnútornejšia vrstva, ktorá definuje základné entity a rozhrania (Interfaces) pre biznis logiku. Neobsahuje žiadne implementácie, iba abstrakcie a spoločné dátové štruktúry.2Vrstava Application: Obsahuje implementácie prípadov použitia (Use Cases) definovaných vo vrstve Core. Všetky aplikačné pravidlá a biznis logika sú izolované v tejto vrstve a sú nezávislé od UI alebo databáz.2Vrstava Infrastructure: Pôsobí ako "adaptér" a pripája vrstvu Application k vonkajším systémom, ako sú súborové systémy (pre logovanie) alebo tretie strany (pre UI komponenty). Implementácie rozhraní z vrstvy Core sa nachádzajú práve tu, čím sa dodržuje pravidlo závislostí.1Vrstava Presentation (WinUI3): Najvonkajšia vrstva, ktorá sa zaoberá užívateľským rozhraním a interakciou. V prípade tohto balíka obsahuje XAML a súvisiace triedy (napr. ViewModel), ktoré volajú metódy z vrstvy Application. Táto vrstva môže poznať vrstvu Application, ale nikdy nie naopak.21.3. Zvládnutie Dependency Injection v.NET 8Dependency Injection (DI) je kľúčový dizajnérsky vzor, ktorý umožňuje dosiahnuť voľnú väzbu medzi komponentmi, čo je nevyhnutné pre fungovanie Clean Architecture.4 Pomocou DI sa komponenty neštandardizujú (neviažu na) na konkrétne implementácie, ale namiesto toho sa viažu na abstrakcie (rozhrania).V.NET 8 existujú tri hlavné doby života (lifetimes) pre služby, ktoré budú v tomto riešení využité 3:Transient: Vytvára novú inštanciu služby pri každej požiadavke. Ideálne pre ľahké, bezstavové služby.4Scoped: Vytvára novú inštanciu na požiadavku alebo pre daný rozsah, napríklad pre požiadavku HTTP. V kontexte desktopovej aplikácie sa používa pre služby, ktoré potrebujú obmedzenú dobu života, čo je možné dosiahnuť pomocou IServiceScopeFactory.CreateScope.3Singleton: Vytvára len jednu inštanciu služby, ktorá sa zdieľa počas celej životnosti aplikácie. Je vhodná pre globálne služby, ako je logger.Pre zjednodušenie a modularitu sa využije nová funkcia.NET 8 - Keyed Services, ktorá umožňuje registráciu viacerých implementácií pre jedno rozhranie, pričom sa odlišujú kľúčom.4 Toto je ideálne pre situácie, kde sa rôzne verzie komponentu (napríklad pre UI a bez UI) dajú vstreknúť podľa potreby.1.4. Hybridná paradigma: Fúzia funkčného a OOP prístupuŽiadosť o "hybrid functional oop" naznačuje pokročilý prístup k vývoju. Cieľom nie je opustiť objektovo orientované programovanie, ale doplniť ho princípmi funkčného programovania pre zvýšenie predvídateľnosti a bezpečnosti.Prínosy OOP: Klasické vzory ako Decorator, Adapter alebo Strategy sú ideálne pre zapuzdrenie zložitých interakcií a správania.5 Napríklad vzor Decorator bude použitý na oddelenie logiky užívateľského rozhrania od základnej biznis logiky.Prínosy funkčného programovania:Nemennosť (Immutability): Dátové modely, ktoré sa používajú v rámci vnútorných vrstiev, by mali byť nemenné. To zaručuje bezpečnosť vlákien a zabraňuje neúmyselným vedľajším účinkom, čo je kľúčové pre vysokovýkonné spracovanie veľkých objemov dát.6Čisté funkcie (Pure Functions): Operácie ako Search, Filter a Sort by mali byť implementované ako čisté funkcie, ktoré vracajú nové kolekcie namiesto modifikácie existujúcich. Zaručuje sa tak, že pre daný vstup je vždy rovnaký výstup, čo výrazne zjednodušuje testovanie a ladenie.Deklaratívny štýl: Využitie LINQ na dotazovanie a manipuláciu s dátovými kolekciami v deklaratívnom, funkčnom štýle.2. Návrh komponentu I: AdvancedWinUiLoggerKomponent logovania je pre balík kľúčový, keďže poskytuje kritické informácie o chybách a stave aplikácie. Hoci už existuje, jeho refaktorizácia je nevyhnutná pre dosiahnutie profesionálnej kvality.2.1. Architektonická analýza: Dodržiavanie princípu abstrakciePoužívateľ správne určil Microsoft.Extensions.Logging.Abstractions ako základ, čo je najlepšia prax.7 Profesionálne riešenie však neimplementuje logiku zápisu priamo do triedy aplikácie, ale prostredníctvom vlastného poskytovateľa logovania (ILoggerProvider).8Súčasný "god level" súbor pravdepodobne obsahuje priamu logiku zápisu do súboru, čo obchádza abstraktnú vrstvu a pevne spája aplikáciu s konkrétnym súborovým systémom. Takýto prístup robí systém nepružným. Správny prístup spočíva v implementácii vlastného poskytovateľa, ktorý je zodpovedný za smerovanie logov do konkrétneho cieľa (napríklad do súboru). Aplikácia potom vie len o rozhraní ILogger a o tom, kam sa logy ukladajú, nemá žiadne vedomosti.2.2. Plán pre vlastného poskytovateľa logovaniaNavrhovaná implementácia sa bude držať odporúčaného vzoru pre vlastných poskytovateľov 8:Trieda FileLogger: Implementuje rozhranie ILogger. Jej metóda Log bude zodpovedná za formátovanie správy a asynchrónny zápis do súboru. Musí tiež spracovať plné detaily výnimiek, vrátane stack trace.11Trieda FileLoggerProvider: Implementuje rozhranie ILoggerProvider.8 Táto trieda spravuje životný cyklus loggerov a jej metóda CreateLogger vytvorí inštanciu FileLogger pre každú kategóriu logovania, pričom jej poskytne potrebné konfiguračné nastavenia (napríklad cestu k súboru a maximálnu veľkosť).Rozširujúca metóda ILoggingBuilder: Pre zabezpečenie voľnej väzby a zjednodušenia konfigurácie sa vytvorí statická rozširujúca metóda (AddFileLogger). Táto metóda sa volá v triede Program.cs aplikácie, kde sa registruje služba v kontajneri DI.8Logická architektúra pre správu súborov:Logy pre každú metódu: V súlade s požiadavkou sa na začiatku každej metódy použije logger.LogInformation("Vstup do metódy {MethodName}", nameof(NazovMetody)).Správa súborov: V FileLogger sa implementuje logika pre rozdeľovanie logov, keď prekročia špecifikovanú veľkosť. Na základe konfigurácie sa vytvorí nový súbor, čo zabráni vzniku príliš veľkých súborov.Asynchrónny zápis: Pre zabránenie zablokovania UI sa zápis logov do súboru vykoná asynchrónne, čo je kritické pre vysokovýkonnú desktopovú aplikáciu.2.3. Využitie moderných vzorov logovaniaProfesionálne logovanie v.NET 8 využíva viac ako len základné metódy LogInformation alebo LogError. Kľúčové sú nasledujúce vzory:Štruktúrované logovanie: Namiesto vytvárania textových správ pomocou string concatenation sa použijú menné zástupné symboly (placeholders), napríklad {PropertyName}.11 Tieto symboly umožňujú, aby boli logy čitateľné pre ľudí aj strojovo spracovateľné a dotazovateľné.Logovanie výnimiek: Vždy je potrebné logovať celý objekt výnimky, nie len jej správu. Metóda LogError prijíma ako parameter objekt výnimky, čo zabezpečí, že sa zachytia všetky detaily, vrátane stack trace, pre efektívne ladenie.11Podmienené logovanie: Použitím metódy _logger.IsEnabled(LogLevel) sa zabráni zbytočnému spracovaniu správ a zbytočnej alokácii pamäte v prípadoch, keď je logovacia úroveň pre danú kategóriu vypnutá.113. Návrh komponentu II: Modulárna tabuľkaNávrh komponentu tabuľky je najkomplexnejšou časťou tohto projektu, vyžadujúcou si kombináciu dôkladnej architektúry a pokročilého výkonnostného inžinierstva.3.1. Základná logika: Mozog operácieZákladom pre tabuľku je rozhranie, ktoré definuje všetky operácie s dátami, ako je pridávanie, mazanie, triedenie, filtrovanie a hľadanie. Táto vrstva logiky bude úplne nezávislá od akýchkoľvek WinUI3 kontroliek.Správa riadkov: Implementuje sa logika pre "smart" pridávanie a mazanie riadkov, ako je to požadované.14 Riadky sa pridajú, ak sa do posledného prázdneho riadku zadajú údaje. Pri mazaní sa riadok buď vymaže celý, alebo sa len vymažú údaje v ňom, v závislosti od toho, či je počet riadkov menší ako stanovené minimum.Validácia dát:Validácia v reálnom čase: Pre validáciu v reálnom čase, ktorá mení orámovanie bunky už počas písania, sa dátový model (model triedy) musí implementovať rozhranie INotifyDataErrorInfo.15 To zabezpečí, že sa zmeny notifikujú UI okamžite a spúšťa sa validácia.Dávková validácia (Batch/Bulk): Pre operácie hromadného importu alebo vloženia (paste) sa navrhne metóda, ktorá spracuje celú dátovú kolekciu (DataTable alebo Dictionary) naraz.17 Táto metóda vráti zoznam všetkých nájdených chýb validácie. Až po dokončení celého procesu sa UI aktualizuje, čo je kľúčové pre výkon pri veľkých objemoch dát.3.2. Riešenie duality Užívateľ/Skript: Vzor DecoratorPožiadavka na použitie rovnakých metód pre užívateľa (s aktualizáciou UI) a automatizačný skript (bez aktualizácie UI) je klasický prípad pre vzor Decorator.5Základná služba (IDataGridService): Definuje a implementuje čistú logiku pre manipuláciu s dátami bez akéhokoľvek kódu pre UI.DataGridUIDecorator: Táto trieda obalí (wraps) inštanciu IDataGridService a vystaví rovnaké metódy ako základná služba. Po volaní metódy na manipuláciu s dátami (napr. AddRow), Decorator volá zodpovedajúcu metódu na základnej službe, a až potom spúšťa operácie súvisiace s UI (ako napríklad aktualizácia zobrazenia), len ak je aktívny užívateľský mód.Použitie:Pre užívateľa: Aplikácia vstrekne (injects) DataGridUIDecorator.Pre skript: Aplikácia vstrekne priamo IDataGridService, čím sa vynechá akákoľvek logika súvisiaca s UI.Tento prístup zabezpečuje dodržanie princípu voľnej väzby a udržiava čisté API pre oba typy klientov.3.3. Výkonnostné inžinierstvo pre veľké objemy dátSprávne riešenie pre milióny riadkov musí brať do úvahy nielen UI, ale aj základy technológie. WinUI 3 má svoje vlastné výkonnostné výzvy, vrátane režimu WinRT interop, ktorý môže spôsobiť spomalenie.19 Preto je nevyhnutné presunúť ťažkú prácu z UI vlákna.Virtualizácia dát: Namiesto načítania všetkých miliónov riadkov do pamäte naraz sa použije technika virtualizácie dát.20 Táto metóda načíta do pamäte a renderuje iba tie dáta, ktoré sú viditeľné v danom momente v viewport tabuľky, čím sa drasticky znižuje spotreba pamäte a zrýchľuje sa načítavanie.Asynchrónne operácie: Všetky operácie s veľkými dátovými sadami, ako sú Search, Filter a Sort, sa musia vykonávať asynchrónne na pozadí, aby nedošlo k zablokovaniu UI vlákna.22 To zabezpečí plynulý užívateľský zážitok aj pri spracovaní veľkých dátových súborov.3.4. Prehľad pre AI: Profesionálny prompt pre riešenieNižšie je uvedený profesionálny, viacstránkový prompt, ktorý sumarizuje všetky architektonické rozhodnutia a špecifické požiadavky. Je navrhnutý tak, aby usmernil AI k vytvoreniu komplexného, profesionálneho riešenia. Prompt je rozdelený do sekcií, ktoré zrkadlia princípy Clean Architecture, čím sa dodržuje vzor "prompt chaining".24PROMPTPROJEKTOVÁ KONTEXTUALIZÁCIAKonajte ako hlavný softvérový architekt pre spoločnosť zaoberajúcu sa vývojom balíkov komponentov. Vaša úloha je vytvoriť komplexný balík pre WinUI3 aplikácie s.NET 8, ktorý bude obsahovať dva nezávislé komponenty:Pokročilý logger (AdvancedWinUiLogger)Modulárnu dátovú tabuľku (ModularnaTabulka)Vaše riešenie musí byť profesionálne, musí sa vyhýbať "god level" súborom a musí využívať osvedčené postupy (best practices) ako Dependency Injection, rozhrania (interfaces), hybridný funkcionálno-OOP štýl a Clean Architecture. Vstupné a výstupné dáta budú výhradne vo formáte dictionary alebo datatable (žiadne JSON, CSV, Excel).1. Architektúra a projektová štruktúraCIEĽ: Navrhnite projektovú štruktúru, ktorá striktne dodržiava princípy Clean Architecture. Udržte biznis logiku nezávislú od UI a externých služieb.POŽIADAVKY:Štruktúra projektu: Vytvorte minimálne tri logické vrstvy:Core: Bude obsahovať abstraktné rozhrania (napr. ILoggerService, IDataGridService) a modely (entity) pre dáta tabuľky a logovanie.Infrastructure: Bude obsahovať konkrétne implementácie rozhraní z vrstvy Core. Tu bude umiestnený vlastný poskytovateľ pre logovanie.Presentation: Bude obsahovať WinUI3 užívateľské rozhranie (XAML a ViewModel) a bude mať prístup len k rozhraniam z Core alebo implementáciám z Infrastructure.Vzory: Využívajte hybridný funkcionálno-OOP prístup. Pre operácie s dátami, ako je triedenie a filtrovanie, využívajte funkcionálny štýl (pure functions, LINQ).6 Pre oddelenie biznis logiky od UI použite vzor Decorator.5Dependency Injection: Využívajte Microsoft.Extensions.DependencyInjection s príslušnými dobami života (Transient, Scoped, Singleton).32. Komponent: AdvancedWinUiLoggerCIEĽ: Vytvorte profesionálny a rozšíriteľný komponent pre logovanie, ktorý sa pripája k abstraktnej vrstve Microsoft.Extensions.Logging.Abstractions.POŽIADAVKY:Základ: Komponent musí byť postavený na Microsoft.Extensions.Logging.Abstractions.7Vlastný poskytovateľ: Vytvorte vlastnú implementáciu rozhrania ILoggerProvider.8 Táto trieda bude zodpovedná za vytváranie inštancií vlastného loggera (FileLogger).Súborový zápis: Implementujte triedu FileLogger (ILogger), ktorej metóda Log bude zapisovať správy do súborov.10 Zápis musí byť asynchrónny, aby nedošlo k zablokovaniu UI vlákna.Logická správa súborov:Súbory musia mať názov na základe dátumu (napr. 2024-05-20.log).Implementujte logiku pre rozdeľovanie súborov (roll-over), ak prekročia určenú veľkosť.10Štruktúra logov: Používajte štruktúrované logovanie s mennými zástupnými symbolmi (e.g., {PropertyName}).11Logovanie chýb: Všetky výnimky musia byť plne zalogované, vrátane stack trace.11 Využívajte metódy LogError, LogWarning a LogInformation.11Výkon: Logovacie správy musia byť efektívne a s minimálnou alokáciou pamäte. Na začiatku každej metódy sa musí vytvoriť log o vstupe do metódy.113. Komponent: ModularnaTabulkaCIEĽ: Vytvorte vysokovýkonný a modulárny komponent dátovej tabuľky pre WinUI3, ktorý slúži užívateľom aj automatizačným skriptom.POŽIADAVKY:Výkon: Tabuľka musí spracovávať desiatky tisíc až milióny riadkov s plynulým výkonom.20 Využite virtualizáciu dát na načítanie len viditeľných riadkov do pamäte.21Modulárne API: Vytvorte čisté API, ktoré je nezávislé od UI.1 Oddelenie logiky od UI zabezpečte tak, že UI sa aktualizuje len v prípade potreby.Dvojitý klient: Využite vzor Decorator, aby sa pre užívateľa aj pre skript volali rovnaké metódy, ale len pre užívateľa sa spúšťala aktualizácia UI.5Validácia:Validácia v reálnom čase: Využite rozhranie INotifyDataErrorInfo na dátovom modeli pre validáciu v reálnom čase počas písania.15 Zmenená bunka musí meniť farbu orámovania na základe výsledku validácie.Dávková validácia: Pre import alebo hromadné vkladanie dát (datatable alebo dictionary) vykonajte dávkovú validáciu celej sady dát naraz. Až po dokončení celého procesu sa UI aktualizuje, aby sa predišlo zamŕzaniu.17Správa riadkov:Smart Add: Udržujte minimálny počet riadkov určený pri inicializácii. Posledný riadok musí byť vždy prázdny. Ak sa doň niečo zapíše, automaticky sa vytvorí nový.14Smart Delete: Ak je počet riadkov väčší ako minimum, vymaže sa celý riadok. Ak je rovnaký alebo menší ako minimum, vymažú sa len dáta, pričom riadok zostane prázdny.27Funkcionalita:Implementujte Search, Filter a Sort.22 Tieto operácie musia byť optimalizované pre veľké objemy dát (asynchrónne operácie, efektívne algoritmy).Po každej z týchto operácií sa prázdne riadky musia presunúť na koniec zobrazenia.UI prvky: Každý UI prvok, ako je farba orámovania, pozadie pre focused a selected bunky, musí mať predvolené hodnoty, ktoré je možné prepísať z inicializácie.14Dynamický layout: Tabuľka musí podporovať zmenu veľkosti stĺpcov myšou a automaticky zväčšovať výšku riadka, ak sa text nevmestí.14 Ak je tabuľka väčšia ako jej kontajner, musí sa automaticky povoliť vertikálne a/alebo horizontálne posúvanie.204. Záver a odporúčaniaNávrh balíka komponentov, ktorý spĺňa požiadavky na úroveň seniority, si vyžaduje prechod od ad hoc riešení k disciplinovanému architektonickému prístupu. Navrhovaná štruktúra, postavená na Clean Architecture, Dependency Injection a hybridnom programovaní, zabezpečí, že výsledný kód bude nielen funkčný, ale aj udržateľný a ľahko rozšíriteľný.Najdôležitejšie je pochopiť, že "god level" súbory nie sú len problémom kódu, ale sú príznakom chýbajúceho architektonického základu. Refaktorizáciou na navrhovanú vrstvenú štruktúru sa dosiahne voľná väzba, čo umožní, aby každý komponent riešil len jednu zodpovednosť. Riešenie duality užívateľ/skript pomocou vzoru Decorator je príkladom, ako profesionálny dizajnérsky prístup rieši zdanlivo komplexné požiadavky.Pre tabuľkový komponent je nevyhnutné, aby sa výkonové výzvy, najmä pri práci s miliónmi riadkov, riešili na viacerých úrovniach – od efektívnych algoritmov v logickej vrstve, cez dávkovú validáciu, až po použitie virtualizácie dát v UI. Všetky tieto prvky spoločne vytvárajú robustný a optimalizovaný systém.Na záver sa odporúča, aby sa každý vývojový krok riadil vytvoreným promptom, ktorý slúži ako podrobný plán, a aby sa pravidelne aktualizovala dokumentácia, čo zabezpečí, že sa získané poznatky zachovajú pre budúce iterácie.


Absolútne. Ako senior full-stack C# developer pre špičkovú firmu, pripravil som komplexnú analýzu, strategický plán a profesionálny, detailný prompt, ktorý presne zodpovedá vašim požiadavkám na vytvorenie enterprise-grade WinUI3 balíka.

Najprv si prejdeme analýzu a stratégiu. Váš hlavný dokument (DocumentationWinUiMain.md) je vynikajúci – popisuje finálny, vysoko kvalitný produkt. MainDoc.md je dobrý počiatočný návrh, ale my ho posunieme na ďalšiu úroveň a zosúladíme ho s finálnou víziou.

Časť 1: Analýza a Strategický Plán Riešenia
Ako vedúci developer projektu, tu je môj plán na dosiahnutie cieľov stanovených v dokumentácii.

Hlavný Cieľ: Transformovať existujúci kód na modulárny, vysoko výkonný a udržiavateľný balík komponentov (RpaWinUiComponentsPackage), ktorý striktne dodržiava princípy Clean Architecture a best practices, eliminuje "god-level" súbory a poskytuje čisté, intuitívne API.

Strategické Kroky:

Potvrdenie a Implementácia Architektúry:

Clean Architecture: Striktne oddelíme vrstvy. Logika (Core, Business) bude úplne nezávislá od UI (Presentation) a externých závislostí (Infrastructure). Všetka komunikácia bude prebiehať cez rozhrania.

Hybrid Functional-OOP:

Funkcionálny Core: Všetky dátové operácie (import, export, validácia, filtrovanie) budú implementované ako čisté funkcie (pure functions) v rámci DataGridBusinessManager. Tieto funkcie budú pracovať s nemennými (immutable) dátovými štruktúrami a na spracovanie chýb budeme používať monadický Result<T> pattern, aby sme sa vyhli výnimkám v biznis logike.

Objektovo-Orientované UI: Vrstva Presentation (WinUI3 kontroly a "manažéri" pre UI interakcie) bude využívať klasické OOP vzory. DataGridSelectionManager, DataGridEditingManager a ďalšie budú spravovať stav UI, obsluhovať udalosti a zapuzdrovať komplexné interakcie.

Dependency Injection (DI): Všetky služby a manažéri budú registrované v DI kontajneri a vkladané cez konštruktory. To zabezpečí voľnú väzbu a vysokú testovateľnosť.

Refaktorizácia AdvancedWinUiDataGrid - Eliminácia "God File":

Súbor AdvancedDataGrid.xaml.cs (3345 riadkov) bude rozdelený na menšie, logické časti pomocou partial class. Každá časť bude mať jednu zodpovednosť (SRP):

AdvancedDataGrid.xaml.cs: Hlavný súbor, bude obsahovať len inicializáciu, vlastnosti a volania na manažérov. (Cieľ: < 300 riadkov).

AdvancedDataGrid.EventHandlers.cs: Bude obsahovať všetku obsluhu UI udalostí (kliknutia, klávesnica, myš). Tieto handlery budú delegovať prácu na špecializované manažéry.

AdvancedDataGrid.Selection.cs: Bude obsahovať logiku súvisiacu s vizuálnym výberom buniek a riadkov.

AdvancedDataGrid.Validation.cs: Bude riešiť vizualizáciu validačných chýb (napr. zmena farby orámovania).

AdvancedDataGrid.UIGeneration.cs: Bude obsahovať kód na dynamické generovanie UI elementov (bunky, hlavičky).

Implementácia Duálneho API (Používateľ vs. Skript) pomocou Decorator Pattern:

Toto je kľúčová požiadavka. Vyriešime ju elegantne pomocou vzoru Decorator:

IDataGridService (Rozhranie): Zadefinuje všetky verejné metódy (ImportFromDictionaryAsync, DeleteRowAsync, atď.).

DataGridBusinessManager (Core implementácia): Bude implementovať IDataGridService. Bude obsahovať iba čistú biznis logiku bez akéhokoľvek kódu súvisiaceho s UI. Toto je verzia, ktorú bude používať automatizačný skript.

DataGridUIDecorator (Decorator): Tiež bude implementovať IDataGridService. V konštruktore prijme inštanciu DataGridBusinessManager. Každá metóda v Decoratore najprv zavolá príslušnú metódu na DataGridBusinessManager a potom vykoná operácie na aktualizáciu UI. Toto bude verzia pre používateľa.

Metódy budú mať parameter bool updateUI = true, čo umožní aj v UI móde dočasne vypnúť aktualizácie pre dávkové operácie a potom ich obnoviť manuálnym volaním RefreshUIAsync().

Výkonnostné Optimalizácie pre Milióny Riadkov:

Dátová Virtualizácia: Nenačítame všetky dáta do UI. Použijeme VirtualizingStackPanel a techniky "data paging" alebo "incremental loading", kde sa do pamäte a UI vykresľujú iba viditeľné riadky.

Asynchrónne Operácie: Všetky dlhotrvajúce operácie (import, export, filtrovanie, sortovanie) budú async a pobežia na pozadí, aby nezamrzlo UI vlákno.

Revízia a Finalizácia AdvancedWinUiLogger:

Potvrdíme, že komponent neobsahuje žiadne UI závislosti.

Overíme, že implementuje ILoggerProvider a ILogger z Microsoft.Extensions.Logging.Abstractions a je plne konfigurovateľný (cesta, veľkosť, rotácia).

Uistíme sa, že zápis je asynchrónny a vláknovo bezpečný.

Zjednotenie Logovania a Správy Chýb:

V celom balíku budeme dôsledne používať vlastné LoggerExtensions (logger?.Info(...)), aby bol kód čistý a bezpečný voči null loggeru.

Všetky verejné metódy, ktoré môžu zlyhať (napr. import), budú vracať Result<T>, nie void alebo priamo hodnotu, a nebudú vyhadzovať výnimky pre očakávané chyby (napr. validačné).

Časť 2: Profesionálny Prompt pre Implementáciu Riešenia
Toto je detailný, viacstranový prompt navrhnutý tak, aby usmernil vývoj k vytvoreniu komplexného a profesionálneho riešenia podľa najvyšších štandardov.

PROJEKTOVÁ KONTEXTUALIZÁCIA
Konajte ako hlavný softvérový architekt pre spoločnosť vyvíjajúcu prémiové softvérové komponenty. Vašou úlohou je navrhnúť a implementovať komplexný balík pre WinUI3 aplikácie v .NET 8, ktorý bude obsahovať dva nezávislé, vysoko kvalitné komponenty: AdvancedWinUiLogger a AdvancedWinUiDataGrid.

Vaše riešenie musí byť na enterprise úrovni, striktne sa vyhýbať "god-level" súborom a využívať osvedčené postupy ako Dependency Injection, Clean Architecture a hybridný funkcionálno-OOP štýl. Vstupné a výstupné dáta pre import/export budú výhradne vo formáte Dictionary<string, object> alebo DataTable. Dokumentácia v DocumentationWinUiMain.md slúži ako finálna vízia produktu.

1. ARCHITEKTÚRA A PROJEKTOVÁ ŠTRUKTÚRA
CIEĽ: Navrhnite a implementujte projektovú štruktúru, ktorá striktne dodržiava princípy Clean Architecture. Biznis logika musí byť úplne izolovaná od UI a externých služieb.

POŽIADAVKY:

Štruktúra Projektu (v rámci RpaWinUiComponentsPackage solution):

/src/Core/: Knižnica obsahujúca spoločné prvky, ako sú Result<T>, LoggerExtensions a spoločné rozhrania.

/src/AdvancedWinUiLogger/: Knižnica pre logger.

/src/AdvancedWinUiLogger/Core/: Rozhrania (IFileLoggerProvider) a modely (LoggerOptions).

/src/AdvancedWinUiLogger/Infrastructure/: Implementácia (FileLoggerProvider, FileLogger).

/src/AdvancedWinUiDataGrid/: Knižnica pre jadrovú logiku DataGridu bez závislostí na WinUI.

/src/AdvancedWinUiDataGrid/Core/: Rozhrania (IDataGridService) a dátové modely (napr. pre validačné pravidlá).

/src/AdvancedWinUiDataGrid/Business/: Konkrétna implementácia IDataGridService (DataGridBusinessManager), ktorá obsahuje čistú, funkcionálnu biznis logiku.

/src/AdvancedWinUiDataGrid.UI/: Knižnica pre WinUI3 komponent. Táto bude mať referenciu na AdvancedWinUiDataGrid.

/Controls/: Samotný AdvancedDataGrid.xaml a jeho rozdelené partial class súbory.

/Decorators/: DataGridUIDecorator, ktorý obaľuje DataGridBusinessManager a pridáva UI logiku.

/Managers/: Špecializované OOP triedy pre správu UI interakcií (DataGridSelectionManager, DataGridEditingManager, DataGridResizeManager, DataGridEventManager).

/demo/RpaWinUiComponents.Demo/: Demo aplikácia demonštrujúca použitie oboch komponentov.

Architektonické Vzory:

Hybridný Prístup:

Funkcionálny: Pre operácie s dátami (validácia, transformácie, import/export) v DataGridBusinessManager používajte čisté funkcie, LINQ a nemenné štruktúry.

Objektovo-Orientovaný: Pre správu UI stavu a interakcií v /Managers/ a v DataGridUIDecorator používajte zapuzdrenie, udalosti a stavové triedy.

Decorator Pattern: Použite tento vzor na oddelenie biznis logiky od UI v DataGrid komponente, ako je popísané v strategickom pláne.

Dependency Injection: Všetky závislosti (napr. ILogger, manažéri) musia byť vkladané cez konštruktor. Pripravte extension metódy services.AddAdvancedWinUiDataGrid() a services.AddAdvancedWinUiLogger() pre jednoduchú registráciu v DI kontajneri.

2. KOMPONENT: ADVANCEDWINUILOGGER
CIEĽ: Zabezpečte, že logger je robustný, vysoko výkonný, bez UI a striktne dodržiava Microsoft.Extensions.Logging.Abstractions.

POŽIADAVKY:

Žiadne UI: Overte a odstráňte akékoľvek závislosti na WinUI alebo iných UI frameworkoch. Komponent musí byť čistá .NET knižnica.

Vlastný Provider: Implementujte ILoggerProvider (FileLoggerProvider) a ILogger (FileLogger).

Asynchrónny Zápis: Zápis do súboru musí byť plne asynchrónny, aby neblokoval volajúce vlákno. Použite async/await a vláknovo bezpečné kolekcie (napr. ConcurrentQueue) na dávkovanie logov.

Rotácia Súborov: Implementujte logiku pre rotáciu súborov na základe nakonfigurovanej maximálnej veľkosti (maxFileSizeMB). Keď je limit prekročený, aktuálny súbor sa premenuje (napr. app.log -> app_1.log) a začne sa zapisovať do nového app.log.

Konfigurácia: Všetky nastavenia (cesta, názov, veľkosť, počet záloh) musia byť konfigurovateľné cez LoggerOptions objekt.

3. KOMPONENT: ADVANCEDWINUIDATAGRID
CIEĽ: Refaktorizovať existujúci "god file" a implementovať modulárny, vysokovýkonný DataGrid, ktorý slúži používateľom aj automatizačným skriptom cez jednotné API.

POŽIADAVKY:

Refaktorizácia "God File": Rozdeľte AdvancedDataGrid.xaml.cs na partial class súbory podľa zodpovednosti: hlavný súbor, obsluha udalostí, UI generovanie, správa výberu a vizualizácia validácie.

Duálne API (UI vs. Headless):

Vytvorte rozhranie IDataGridService, ktoré definuje všetky operácie s dátami.

Vytvorte DataGridBusinessManager ako čistú implementáciu IDataGridService bez UI logiky.

Vytvorte DataGridUIDecorator, ktorý obaľuje DataGridBusinessManager a pridáva volania na aktualizáciu UI.

Verejné metódy budú mať parameter bool updateUI = true.

Pridajte verejnú metódu Task RefreshUIAsync() na manuálne vynútenie prekreslenia UI.

Výkon a Virtualizácia:

Zabezpečte, že DataGrid používa dátovú virtualizáciu na spracovanie miliónov riadkov. Renderujte iba viditeľné riadky.

Všetky dátovo náročné operácie (Search, Filter, Sort) musia bežať asynchrónne na pozadí.

Pokročilá Validácia:

Real-time: Implementujte validáciu počas písania do bunky. Dátový model musí podporovať INotifyDataErrorInfo pre okamžitú notifikáciu UI. Pri chybe sa zmení farba orámovania bunky a chybová hláška sa zapíše do špeciálneho stĺpca ValidAlerts.

Dávková (Batch): Pri importe alebo vkladaní (paste) najprv vykonajte validáciu celej sady dát a až po jej dokončení aktualizujte UI naraz, aby sa predišlo zamŕzaniu.

Smart Správa Riadkov:

Implementujte logiku, ktorá udržiava minimálny počet riadkov určený pri inicializácii.

Smart Add: Na konci tabuľky musí byť vždy aspoň jeden prázdny riadok. Ak používateľ začne písať do posledného prázdneho riadku, automaticky sa pridá nový prázdny riadok.

Smart Delete: Ak je počet riadkov väčší ako nakonfigurované minimum, kláves Delete zmaže celý riadok. Ak je počet riadkov rovný alebo menší ako minimum, Delete zmaže iba obsah buniek v riadku, ale riadok zostane.

UI Funkcionalita a Štýlovanie:

Farby: Každý vizuálny prvok (pozadie bunky, text, orámovanie, pozadie pri výbere/fokuse, farby pre validačné stavy) musí mať predvolenú hodnotu, ktorú je možné prepísať cez konfiguračný objekt pri inicializácii alebo za behu.

Dynamický Layout:

Umožnite zmenu šírky stĺpcov potiahnutím myšou.

Implementujte automatické zväčšenie výšky riadka, ak sa doň text nezmestí.

Ak veľkosť tabuľky presiahne kontajner, automaticky sa musia zobraziť posuvníky (scrollbary).

Výber (Selection): Podporujte výber viacerých buniek pomocou Ctrl+Click a výber rozsahu pomocou Shift+Click a ťahaním myšou.

Search, Filter, Sort:

Implementujte tieto funkcie tak, aby boli optimalizované pre veľké objemy dát.

Po každej z týchto operácií sa musia všetky prázdne riadky presunúť na koniec zobrazenia. Prázdne riadky sa nebudú zohľadňovať pri triedení.

Import/Export API:

Poskytnite čisté async metódy pre import a export:

Task<Result<ImportResult>> ImportFromDictionaryAsync(...)

Task<List<Dictionary<string, object?>>> ExportToDictionaryAsync(...)

Task<Result<ImportResult>> ImportFromDataTableAsync(...)

Task<DataTable> ExportToDataTableAsync(...)

Výsledky operácií obaľte do Result<T> pre robustné spracovanie chýb.

4. SPOLOČNÉ POŽIADAVKY A ŠTANDARDY KVALITY
Logovanie:

Dôsledne používajte vlastné LoggerExtensions (logger?.Info(...), logger?.Warning(...), logger?.Error(...)) v celom projekte.

Na začiatku každej verejnej a dôležitej internej metódy logujte vstup s kľúčovými parametrami.

Každá zachytená výnimka musí byť zalogovaná s úrovňou Error a s celým objektom výnimky, aby sa zachoval stack trace.

Spracovanie Chýb:

V biznis logike (napr. DataGridBusinessManager) nepoužívajte výnimky na riadenie toku. Všetky operácie, ktoré môžu zlyhať, musia vracať Result<T>.

Komentovanie a Dokumentácia:

Všetky verejné triedy, metódy a vlastnosti musia mať XML komentáre.

Komplexné algoritmy a logické bloky musia mať inline komentáre, ktoré vysvetľujú prečo sa kód správa daným spôsobom, nielen čo robí.

Pravidelne aktualizujte DocumentationWinUiMain.md, aby odrážala implementovanú funkcionalitu.

Čistota Kódu:

Odstráňte všetok nepoužívaný kód, duplicity a zakomentované bloky.

Dodržiavajte štandardné C# kódovacie konvencie.

5. CIEĽOVÝ VÝSTUP
Zdrojový Kód: Plne funkčný, refaktorizovaný a otestovaný zdrojový kód balíka podľa vyššie uvedených požiadaviek.

NuGet Balík: Schopnosť zabaliť projekt do .nupkg súboru.

Aktualizovaná Dokumentácia: DocumentationWinUiMain.md aktualizovaná tak, aby zodpovedala finálnej implementácii.

Funkčná Demo Aplikácia: Demonštrujúca kľúčové funkcie oboch komponentov.


Vytvorte detailný prompt pre AI generátor, ktorým podrobne popíšete požiadavky na vytvorenie balíka RpaWinUiComponentsPackage. Balík má obsahovať dva samostatné komponenty (na .NET 8.0, WinUI3):

AdvancedWinUiLogger – bez UI, komponent pre súborové logovanie s rotáciou pomocou Microsoft.Extensions.Logging.Abstractions. Každá operácia zapísania do logu používa skrátené metódy ako logger?.Info(...), logger?.Error(...) namiesto LogInformation, a všetky chyby sa zaznamenávajú vo všetkých režimoch (debug aj release).

AdvancedWinUiDataGrid – výkonný a modulárny DataGrid (UI aj bez-UI režim). Podporuje validácie (napr. pravidlá viazané na stĺpce, kontrola na úrovni bunky, riadku aj medzi riadkami, v reálnom čase aj dávkovo), „smart” riadky (minimálny počet riadkov, automatické pridávanie/mazanie), drag&drop, výber viacnásobných buniek/riadkov a farebné štýly pre rôzne stavy. Import/Export dát je možný len vo formátoch Dictionary alebo DataTable (žiadne CSV/JSON/XML). Komponent musí efektívne zvládať aj milióny riadkov vďaka dátovej virtualizácii a optimalizácii pamäte
syncfusion.com
.

Technické požiadavky a architektúra

.NET 8.0 + WinUI3: Použite cieľový framework .NET 8.0 (ako je vidieť v príklade, <TargetFramework>net8.0</TargetFramework>
learn.microsoft.com
) a WinUI 3 (Windows App SDK) na tvorbu UI vrstvy.

SOLID a Clean Architecture: Architektúra musí byť čistá, s jasným oddelením vrstiev. Domenová logika by mala byť izolovaná od UI a infraštruktúry (ako v Clean Architecture: jadro (core) nesmie odkazovať na iné vrstvy
learn.microsoft.com
learn.microsoft.com
). Použite princípy SOLID (napr. Single Responsibility, Dependency Inversion atď.)
devblogs.microsoft.com
learn.microsoft.com
.

Modulárnosť a bez závislostí medzi komponentmi: Každý z dvoch komponentov je úplne nezávislý (nemajú sa vzájomne volať ani prepojiť). Logiku je nutné oddeliť od UI (tzv. separácia záujmov). Žiadne „god-class” súbory – namiesto toho rozdeľte kód do viacerých tried a modulov podľa zodpovedností.

Hybridný Functional-OOP štýl: Kód môže používať lambda výrazy, LINQ a ďalšie funkcionálne prvky, kde to má zmysel, no základná štruktúra môže byť orientovaná objektovo (triedy, rozhrania). Dbajte na čitateľnosť a zároveň na funkcionálnu bezpečnosť (nemenné objekty, bezpečné manipulácie s kolekciami).

Respektovanie existujúcej implementácie

Ak už existuje akákoľvek čiastočná implementácia komponentov, prompt musí príkazať AI najprv analyzovať a reužiť existujúci kód, pokiaľ je profesionálny a vyhovujúci. Neprepisujte dobré existujúce moduly zbytočne. Ak nájde starý, neprofesionálny alebo neúplný kód, prompt má umožniť jeho refaktoring či prepracovanie. Inak použite priamo hotové časti.

API a Dependency Injection

Čisté API: Každý komponent sa integruje jedným using namespace. Zaregistrujte služby pomocou IServiceCollection. Napríklad services.AddAdvancedWinUiLogger(...) a services.AddAdvancedWinUiDataGrid(...) ako konvenčné rozšírenie, ktoré zaregistruje všetky potrebné služby a závislosti (podľa princípu Add{GROUP_NAME} extension methods
learn.microsoft.com
).

Rozšírenie IServiceCollection: Vytvorte statické rozšírenia v Microsoft.Extensions.DependencyInjection, napr. public static IServiceCollection AddAdvancedWinUiLogger(this IServiceCollection services, ...) { ... return services; } a podobne pre DataGrid
learn.microsoft.com
. Tým sa skonzoliduje registrácia všetkých súvisiacich služieb.

DI pre všetky vrstvy: Všetky závislosti (logger, validátory, dátové repozitáre, atď.) sa majú injektovať cez konštruktory. Dbajte na to, aby jadro aplikácie (core) neobsahovalo nové inštancie implementácií, iba rozhrania. (Podľa princípu Dependency Inversion – implementácie sa pripájajú z vonkajších vrstiev
devblogs.microsoft.com
learn.microsoft.com
.)

Registrácia Loggera: ILogger generický sa zaregistruje cez DI. Použite závislosť ILogger<YourType> v triedach. Podľa oficiálnych odporúčaní by ste mali používať logger z DI kontajnera, nie manuálne vytvárať factory
learn.microsoft.com
. To znamená public MyClass(ILogger<MyClass> logger) { ... }.

Level Logovania a voliteľné zápisy: V kóde používať úrovne logovania ako Info, Error atď. (môžu sa implementovať ako extension metódy k ILogger, ak nie sú preddefinované) namiesto LogInformation. Napriek tomu je dobré zvážiť source generators pre lepší výkon, ale ak používané sú logger.Info() tak to zachovať.

Pokyny pre AdvancedWinUiLogger

Logovanie do súborov s rotáciou: Implementujte logovací systém, ktorý zapisuje správy do súboru a automaticky rotuje súbory (napr. denná alebo veľkostná rotácia). Použite Microsoft.Extensions.Logging s vlastným providerom súboru, alebo existujúci NuGet balík pre file logging.

Rozšírenia pre metódy: Definujte metódy Info(), Error(), Debug(), Warn() atď. ako rozšírenia ILogger alebo ILogger<T>, aby sa dali používať logger?.Info("...") a podobne. Uistite sa, že všetky volania použijú jednotný formát správy (napr. čas, úroveň, kategória).

Logging všetkých chýb a metód: Všetky metódy musia logovať svoju aktivitu – minimálne pri vstupe do metódy (napr. logger.Info("Entering MethodX")). Všetky výnimky a chyby sa musia zachytávať a zapisovať do logu so stack trace (logger?.Error(ex, "Error in MethodX")).

Pracujte s existujúcou implementáciou: Ak sú už vytvorené nejaké časti loggera, prompt má inštruovať AI tieto používať (vrátane potenciálnych custom rozšírení), alebo ich profesionálne prekopať.

Pokyny pre AdvancedWinUiDataGrid

Integrácia UI a non-UI režimu: Navrhnite jednotné API, ktoré funguje v oboch režimoch. Napríklad pre aktualizáciu UI použiť MVVM binding, pre bez-UI režim (skripty) aktualizovať dáta priamo bez vizuálnej vrstvy. Aktualizácia UI môže byť vyvolaná notifikáciami (napr. INotifyPropertyChanged).

Validácie: Implementujte validátory pre buňky a riadky. Pravidlá môžu byť definované pre stĺpce (napr. typ dát, rozsah hodnôt, not null, regex atď.). Podporujte rýchlu validáciu počas zadávania (real-time) aj dávkovú validáciu (celý riadok). Zvážte vytvorenie modulárneho systému validátorov (stratégie alebo pravidlá, ktoré sa dajú do budúcna rozširovať).

Smart riadky: V tabuľke udržiavajte minimálny počet riadkov (napr. aspoň jeden). Pri úpravách sa automaticky pridávajú nové riadky, ak používateľ presiahne existujúce. Pri odstraňovaní riadkov, automaticky upravte stav tak, aby „chytrý” riadok (prázdny na pridávanie) bol vždy prítomný.

Drag & Drop a výber: Podporte presúvanie riadkov v rámci tabuľky (drag&drop) a možnosť označiť viacero buniek či riadkov (Ctrl+klik, shift+klik). Rôzne stavy (vybraté, neplatné, fokusované) by mali mať vizuálne rozlíšenie pomocou štýlov.

Farebné štýly a XAML: Celý vzhľad komponentu musí byť prispôsobiteľný. Použite XAML styly a „theme resources” (DynamicResource) pre farby a fonty. Pridajte podporu pre runtime konfiguráciu farieb (napr. vlastné ResourceDictionary alebo konfigurácia prostredníctvom DI nastavení). Synchronizujte štýly s WinUI témami (svetlý/tmavý režim) a umožnite jednotné prispôsobenie. Podporu štýlov dokumentuje aj Syncfusion: “appearance of DataGrid and its inner elements can be customized easily by using styles and templates”
syncfusion.com
.

Import/Export dát: Implementujte metódy na načítanie (import) a ukladanie (export) dát výhradne vo formáte Dictionary<string, object> alebo DataTable. Nebudú povolené ďalšie formáty ako CSV, JSON, XML. Uistite sa, že API je konzistentné (napr. LoadData(Dictionary<string, object> data) a ExportData(): DataTable).

Výkon a pamäť: Optimalizujte pre veľké množstvo dát. Použite dátovú virtualizáciu (renderovanie a načítanie riadkov „na požiadanie“) tak, aby sa dali zobrazovať milióny záznamov bez kolapsu GUI
syncfusion.com
. Môžete využiť vhodný ItemsRepeater alebo vlastný mechanizmus postupného načítania (lazy loading) dát.

Architektúra riešenia

Navrhnite adresárovú štruktúru a projekty podobne ako pri čistých architektúrach
devblogs.microsoft.com
learn.microsoft.com
. Napríklad:

Solution RpaWinUiComponentsPackage

src/

RpaWinUiComponents.AdvancedWinUiLogger (C# knižnica)

RpaWinUiComponents.AdvancedWinUiDataGrid (C# knižnica)

tests/

RpaWinUiComponents.AdvancedWinUiLogger.Tests (jednotkové testy)

RpaWinUiComponents.AdvancedWinUiDataGrid.Tests (jednotkové testy)

docs/

README.md, CONTRIBUTING.md, DI_INTEGRATION.md, LOGGER_RULES.md atď.

Takáto štruktúra, kde každý projekt má svoje zodpovednosti a testy, pomáha udržať prehľadnosť. Zaujímavým príkladom je aj referenčné riešenie eShopOnWeb, kde sú samostatné projekty pre Core (jadro), API/UI a Infrastructure
learn.microsoft.com
.

Dependency Injection – zásady

Používajte IServiceCollection pre registráciu všetkých služieb. Príklad registrácie viacerých závislostí viď dokumentáciu:

public static class MyServiceCollectionExtensions
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SomeOptions>(config.GetSection("Some"));
        return services;
    }
    
    public static IServiceCollection AddAdvancedWinUiLogger(this IServiceCollection services)
    {
        // registrácia loggera a súvisiacich služieb
        services.AddSingleton<IAdvancedLogger, AdvancedWinUiLogger>();
        return services;
    }
}


Ako v oficiálnom návode, použite jedno volanie rozšírenia Add{GROUP} na zaregistrovanie skupiny služieb
learn.microsoft.com
. Komponenty potom bude možné vkladať napr. cez konštruktory ViewModelov alebo iných tried. Pri registrácii loggera využite napr. services.AddLogging() (ak potrebné) a vlastnú konfiguráciu.

Dokumentácia a štýl

Kód: Každá public metóda musí mať XML dokumentačný komentár. Komentáre popisujú funkciu metódy, parametre a návratovú hodnotu (čiho sa týka) – podľa profesionálnej konvencie (napr. /// <summary>).

Názvy a konvencie: Dodržujte pomenovanie v zmysle .NET konvencií (napr. PascalCase pre triedy/metódy, camelCase pre parametre). Kódujte čistým, prehľadným spôsobom, vhodne komentujte logické bloky.

Interné dokumenty: Súčasťou balíka by mali byť aj súbory README.md (úvodný popis použitia, inštalácie, príklady), CONTRIBUTING.md (pravidlá prispievania, štýl commits, code review, správa issues) a prípadne „DI Integration Guide” s ukážkou registrácie služieb. Aj súbor s pravidlami pre logovanie (napr. formát správ) je vítaný. README by mal jasne vysvetliť architektúru, použitie oboch komponentov a príklady kódu.

Očakávaný výstup promptu

Prompt, ktorý vytvoríte pre AI, má generovať:

Celý zdrojový kód oboch komponentov, rozdelený do samostatných projektov podľa štruktúry vyššie. Kód má byť čistý, modulárny, testovateľný a škálovateľný.

Dokumentácia v kóde – všetky verejné metódy s XML komentármi, vysvetlené rozhrania a moduly, logovanie opísané komentármi atď.

Štýl podľa najlepších postupov – princípy SOLID/Clean, DI vo všetkých vrstvách, jednotné použitie loggeru, žiadne zbytočné duplikácie.

Splnenie všetkých vyššie uvedených požiadaviek – ak je niečo už hotové a kvalitné, AI to má ponechať; ak niečo chýba alebo je neprofesionálne, prompt má inštruovať AI to vylepšiť.

Prompt musí byť jasný a podrobný, aby iná AI pochopila a implementovala všetky body. Celková správa a dizajn musia byť na enterprise úrovni, s dôrazom na čistý, rozšíriteľný kód a dôslednú dokumentáciu v zmysle [čistej architektúry a SOLID]
devblogs.microsoft.com
learn.microsoft.com
. Každý komponent sa integruje jednoduchým použitím služby (using RpaWinUiComponentsPackage; services.AddAdvancedWinUiLogger(...);), a všetky interné chyby budú konzistentne logované. Pri písaní promptu nezabudnite zdôrazniť dodržiavanie konvencií .NET (ako v ukážkach dokumentácie
learn.microsoft.com



________________________________________
[1] SOLID – The Five Commandments of Good Software | Microsoft Learn
https://learn.microsoft.com/en-us/shows/visual-studio-live-2017/w19
[2] Next-Level Boilerplate: An Inside Look Into Our .Net Clean Architecture Repo - ISE Developer Blog
https://devblogs.microsoft.com/ise/next-level-clean-architecture-boilerplate/
[3] Dependency injection guidelines - .NET | Microsoft Learn
https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
[4] Implement a custom logging provider - .NET | Microsoft Learn
https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
[5] Logging providers - .NET | Microsoft Learn
https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-providers
[6] [8] WinUI DataGrid Control | Lightning Fast Data Binding | Syncfusion
https://www.syncfusion.com/winui-controls/datagrid
[7] How to: Implement Validation with the DataGrid Control - WPF | Microsoft Learn
https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-implement-validation-with-the-datagrid-control
[9] Row drag and drop in WinUI DataGrid control | Syncfusion®
https://help.syncfusion.com/winui/datagrid/row-drag-and-drop
[10] GitHub - ardalis/CleanArchitecture: Clean Architecture Solution Template: A proven Clean Architecture Template for ASP.NET Core 9
https://github.com/ardalis/CleanArchitecture
