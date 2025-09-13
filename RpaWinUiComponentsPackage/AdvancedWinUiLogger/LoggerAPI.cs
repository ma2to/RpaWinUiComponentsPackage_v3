using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Infrastructure.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger.Internal.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Internal.Domain.ValueObjects.Core;

// CLEAN API DESIGN - Single namespace, single using statement
namespace RpaWinUiComponentsPackage.AdvancedWinUiLogger;

/// <summary>
/// 🚀 PROFESSIONAL ENTERPRISE LOGGER - Clean API Facade pre File-Only Logging
/// 
/// 🎯 HLAVNÉ CHARAKTERISTIKY:
/// ✅ SINGLE USING: using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
/// ✅ FILE-ONLY LOGGING: BEZ UI komponentov - čisto file-based logging systém
/// ✅ JEDINÁ API METÓDA: LoggerAPI.CreateFileLogger() - simplifikovaný entry point
/// ✅ SIZE-BASED ROTATION: Automatická rotácia súborov na základe veľkosti
/// ✅ MICROSOFT.EXTENSIONS.LOGGING.ABSTRACTIONS: Bezproblémová integrácia s existujúcimi systémami
/// ✅ HIGH PERFORMANCE: Async logging s batch zapisovaním pre performance
/// ✅ FAIL-SAFE DESIGN: Funguje aj bez external loggera - žiadne crashes
/// 
/// 📁 ŠTRUKTÚRA SÚBOROVÉHO SYSTÉMU:
/// Vytvárajú sa súbory v nasledujúcom formáte:
/// - MyApp.log          (aktuálny log súbor, rastie dokým nedosiahne limit)
/// - MyApp_1.log        (prvý rotovaný súbor po dosiahnutí limitu)
/// - MyApp_2.log        (druhý rotovaný súbor)
/// - ...
/// 
/// 🔄 ROTAČNÝ MECHANIZMUS:
/// 1. Aktuálny súbor (MyApp.log) rastie počas zapisovania
/// 2. Keď dosiahne maxFileSizeMB limit, súbor sa uzavrie
/// 3. MyApp.log sa premenuje na MyApp_1.log (existujúce číslované súbory sa posunú)
/// 4. Vytvorí sa nový MyApp.log pre pokračovanie zapisovania
/// 5. Ak je maxFileSizeMB = null, súbor rastie neobmedzene (žiadna rotácia)
/// 
/// 💡 USAGE PATTERN - COMPLETE EXAMPLE:
/// 
/// // 📋 ZÁKLADNÉ POUŽITIE S ROTÁCIOU:
/// var fileLogger = LoggerAPI.CreateFileLogger(
///     externalLogger: myAppLogger,    // Nullable - môže byť null, žiadny crash
///     logDirectory: @"C:\MyApp\Logs", // Adresár sa vytvorí ak neexistuje
///     baseFileName: "MyApp",          // Bez prípony - pridá sa automaticky .log
///     maxFileSizeMB: 10);            // Rotácia každých 10MB
/// 
/// // 📋 POUŽITIE BEZ ROTÁCIE (neobmedzená veľkosť):
/// var unlimitedLogger = LoggerAPI.CreateFileLogger(
///     externalLogger: null,           // Žiadny external logger - žiadny problém
///     logDirectory: @"C:\Temp\Logs",
///     baseFileName: "UnlimitedApp",
///     maxFileSizeMB: null);          // null = neobmedzená veľkosť
/// 
/// // 📋 POUŽITIE VYTVORENÝCH LOGGEROV (štandardný ILogger interface):
/// // Podporuje LoggerExtensions z Core.Extensions:
/// fileLogger.Info("🚀 Application started");
/// fileLogger.Warning("⚠️ Configuration issue detected");
/// fileLogger.Error(exception, "🚨 Operation failed: {Operation}", operationName);
/// 
/// // Alebo štandardné Microsoft.Extensions.Logging metódy:
/// fileLogger.LogInformation("Standard info message");
/// fileLogger.LogError(exception, "Standard error message");
/// 
/// 🔒 FAIL-SAFE FEATURES:
/// - ExternalLogger môže byť null - žiadny crash, len sa nezaloguje do externého systému
/// - Ak sa nedá vytvoriť súbor, internal error sa zaloguje do externalLogger (ak existuje)
/// - Ak adresár neexistuje, pokúsi sa ho vytvoriť
/// - Thread-safe operácie pre concurrent aplikácie
/// - Automatic disposal resources pre proper cleanup
/// </summary>
public static class LoggerAPI
{
    /// <summary>
    /// 🎯 JEDINÁ API METÓDA - Create professional file logger with automatic rotation and size management
    /// 
    /// 📋 DETAILNÝ POPIS FUNKCIONALITY:
    /// Táto metóda vytvorí ILogger inštanciu ktorá zapisuje všetky logy do súborov na disku.
    /// Podporuje automatickú rotáciu súborov keď dosiahnu definovanú veľkosť.
    /// Je thread-safe a optimalizovaná pre high-performance scenáre.
    /// 
    /// 📁 FILE MANAGEMENT SYSTEM:
    /// Ak maxFileSizeMB nie je null (rotácia zapnutá):
    /// - MyApp.log          (aktuálny aktívny log súbor)
    /// - MyApp_1.log        (prvý rotovaný súbor po dosiahnutí 10MB limitu)
    /// - MyApp_2.log        (druhý rotovaný súbor)
    /// - MyApp_3.log        (pokračuje podľa potreby...)
    /// 
    /// Ak maxFileSizeMB je null (rotácia vypnutá):
    /// - MyApp.log          (rastie neobmedzene, žiadna rotácia)
    /// 
    /// 🔄 ROTATION PROCESS (ak je zapnutá):
    /// 1. Logger písže do MyApp.log dokým nedosiahne maxFileSizeMB limit
    /// 2. Keď sa limit dosiahne:
    ///    a) Aktuálny MyApp.log sa uzavrie a flush-ne na disk
    ///    b) Existujúce rotované súbory sa prečíslujú: MyApp_1.log → MyApp_2.log, atď.
    ///    c) MyApp.log sa premenuje na MyApp_1.log
    ///    d) Vytvorí sa nový prázdny MyApp.log pre pokračovanie logovania
    /// 3. Proces pokračuje transparentne bez straty logov
    /// 
    /// 💡 VSTUPNÉ PARAMETRE - DETAILNÝ POPIS:
    /// 
    /// externalLogger (ILogger? - nullable):
    /// - Voliteľný externí logger pre internal operations logging
    /// - Ak je null: žiadne internal logy sa nezapíšu, ale FileLogger funguje normálne
    /// - Ak je poskytnutý: internal operácie sa logujú (startup, rotation, errors)
    /// - FAIL-SAFE: ak external logger crashne, FileLogger pokračuje v práci
    /// - Príklady použitia: console logger, existing app logger, testing logger
    /// 
    /// logDirectory (string - povinný):
    /// - Absolútna cesta k adresáru kde sa majú ukladať log súbory
    /// - Príklady: @"C:\MyApp\Logs", @"D:\ApplicationData\Logs\MyModule"
    /// - Ak adresár neexistuje: automaticky sa vytvorí (vrátane parent directories)
    /// - Ak sa nedá vytvoriť: vyhodí sa DirectoryNotFoundException s detailným popisom
    /// - Požadované permissions: Write access pre vytvorenie súborov a podadresárov
    /// 
    /// baseFileName (string - povinný):
    /// - Základné meno log súboru BEZ prípony (automaticky sa pridá .log)
    /// - Príklady: "MyApplication", "DataProcessor", "WebAPI"
    /// - Nepovoľuje: špeciálne znaky (\/:*?"<>|), bude sanitizovaný
    /// - Výsledný súbor: baseFileName + ".log" (napr. "MyApp.log")
    /// 
    /// maxFileSizeMB (int? - nullable):
    /// - Maximum veľkosť súboru v megabajtoch pred spustením rotácie
    /// - null = žiadna rotácia, súbor rastie neobmedzene (USE WITH CAUTION!)
    /// - 1-1000 = praktické hodnoty pre production (1MB-1GB range)
    /// - Typické hodnoty: 5MB (development), 50MB (production), 100MB+ (high-volume)
    /// - Kontrola sa vykonáva po každom log entry batch-u
    /// 
    /// 🎯 VÝSTUPNÉ DÁTA - RETURN VALUE:
    /// 
    /// Vracia: ILogger implementation (FileLoggerService)
    /// - Implementuje plný Microsoft.Extensions.Logging.ILogger interface
    /// - Podporuje LoggerExtensions z Core.Extensions (.Info(), .Warning(), .Error())
    /// - Podporuje structured logging s parametrami
    /// - Thread-safe pre concurrent použitie
    /// - Implementuje IDisposable pre proper cleanup
    /// - Automatically flush-uje dáta na disk pre data integrity
    /// 
    /// 🚀 PERFORMANCE CHARACTERISTICS:
    /// - Async write operations - neblokuje calling thread
    /// - Batch writing - multiple log entries sa zapisujú naraz
    /// - Memory buffering - znižuje disk I/O overhead
    /// - Lock-free reads - log queries počas write operations
    /// - Optimalized for high-volume scenarios (thousands of logs per second)
    /// 
    /// 🔒 ERROR HANDLING A FAIL-SAFE BEHAVIOR:
    /// - Ak logDirectory neexistuje: pokúsi sa vytvoriť, ak sa nepodarí → exception
    /// - Ak sa nedá vytvoriť súbor: loguje do externalLogger (ak existuje) a vyhodí exception  
    /// - Ak sa nedá zapísať do súboru: pokúsi sa retry 3x, potom loguje warning
    /// - Ak externalLogger crashne: FileLogger pokračuje normálne (isolated failure)
    /// - Thread safety: multiple threads môžu simultánne logovať bez corruption
    /// 
    /// 📊 USAGE EXAMPLES - REAL WORLD SCENARIOS:
    /// 
    /// // 🏢 ENTERPRISE APPLICATION - s rotáciou a external logging:
    /// var appLogger = serviceProvider.GetService<ILogger<MyApp>>();
    /// var fileLogger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: appLogger,      // Internal operations sa logujú do app logger
    ///     logDirectory: @"C:\ProgramData\MyCompany\MyApp\Logs",
    ///     baseFileName: "MyApp",          // Vytvorí MyApp.log, MyApp_1.log, atď.
    ///     maxFileSizeMB: 50);            // 50MB rotation limit
    /// 
    /// // 🔬 DEVELOPMENT/TESTING - jednoduchý file logging:
    /// var devLogger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: null,           // Žiadne internal logy
    ///     logDirectory: Path.GetTempPath(), // Temp folder
    ///     baseFileName: "DevTest",        // DevTest.log
    ///     maxFileSizeMB: 5);             // Malé súbory pre testing
    /// 
    /// // 📈 HIGH-VOLUME SYSTEM - veľké súbory bez častej rotácie:
    /// var highVolumeLogger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: systemLogger,
    ///     logDirectory: @"D:\HighVolumeLogs",
    ///     baseFileName: "TransactionLog",
    ///     maxFileSizeMB: 500);           // 500MB pred rotáciou
    /// 
    /// // 📜 AUDIT LOGGING - neobmedzená veľkosť (compliance requirements):
    /// var auditLogger = LoggerAPI.CreateFileLogger(
    ///     externalLogger: complianceLogger,
    ///     logDirectory: @"C:\AuditLogs",
    ///     baseFileName: "ComplianceAudit",
    ///     maxFileSizeMB: null);          // Žiadna rotácia - rastie neobmedzene
    /// 
    /// // 🎮 POUŽITIE VYTVORENÝCH LOGGEROV:
    /// fileLogger.Info("🚀 Application started at {StartTime}", DateTime.Now);
    /// fileLogger.Warning("⚠️ Configuration fallback used for {Setting}", settingName);
    /// fileLogger.Error(exception, "🚨 Critical operation failed: {Operation}", operationName);
    /// </summary>
    /// <param name="externalLogger">
    /// Voliteľný externí logger pre internal operations logging.
    /// Ak je null, žiadne internal logy sa nezapisujú ale FileLogger funguje normálne.
    /// Ak je poskytnutý, internal operácie (startup, rotation, errors) sa logujú.
    /// FAIL-SAFE: ak external logger fail-ne, FileLogger pokračuje v práci.
    /// </param>
    /// <param name="logDirectory">
    /// Absolútna cesta k adresáru pre log súbory. Musí byť valid path.
    /// Ak neexistuje, automaticky sa vytvorí. Vyžaduje write permissions.
    /// Príklady: @"C:\MyApp\Logs", @"D:\ApplicationData\Logs\MyModule"
    /// </param>
    /// <param name="baseFileName">
    /// Základné meno log súboru BEZ prípony (.log sa pridá automaticky).
    /// Špeciálne znaky budú sanitizované. Príklady: "MyApp", "DataProcessor"
    /// </param>
    /// <param name="maxFileSizeMB">
    /// Maximum veľkosť súboru v MB pred rotáciou. null = žiadna rotácia.
    /// Typické hodnoty: 5MB (dev), 50MB (production), 100MB+ (high-volume).
    /// null znamená súbor rastie neobmedzene - USE WITH CAUTION!
    /// </param>
    /// <returns>
    /// ILogger implementation optimalizovaná pre file logging.
    /// Podporuje štandardný Microsoft.Extensions.Logging interface + LoggerExtensions.
    /// Thread-safe, high-performance, s automatic resource cleanup.
    /// </returns>
    /// <exception cref="ArgumentException">Ak logDirectory alebo baseFileName sú invalid</exception>
    /// <exception cref="DirectoryNotFoundException">Ak sa nedá vytvoriť logDirectory</exception>
    /// <exception cref="UnauthorizedAccessException">Ak nie sú write permissions</exception>
    public static ILogger CreateFileLogger(
        ILogger? externalLogger,
        string logDirectory,
        string baseFileName,
        int? maxFileSizeMB)
    {
        // 📊 PHASE 1: INPUT VALIDATION AND PARAMETER ANALYSIS
        // Validujeme všetky vstupné parametre pred vytvorením FileLogger inštancie.
        // Logujeme proces vytvorenia pre audit trail a troubleshooting.
        externalLogger?.Info("📁 LoggerAPI: CREATION START - Creating file logger with parameters: " +
            "Directory='{Directory}', BaseFileName='{BaseFileName}', MaxSizeMB={MaxSize}", 
            logDirectory, baseFileName, maxFileSizeMB?.ToString() ?? "unlimited");
        
        // Validácia logDirectory - musí byť non-null a non-empty
        if (string.IsNullOrWhiteSpace(logDirectory))
        {
            const string error = "LogDirectory cannot be null or empty";
            externalLogger?.Error("❌ LoggerAPI: VALIDATION FAILED - {Error}", error);
            throw new ArgumentException(error, nameof(logDirectory));
        }
        
        // Validácia baseFileName - musí byť valid pre file system
        if (string.IsNullOrWhiteSpace(baseFileName))
        {
            const string error = "BaseFileName cannot be null or empty";
            externalLogger?.Error("❌ LoggerAPI: VALIDATION FAILED - {Error}", error);
            throw new ArgumentException(error, nameof(baseFileName));
        }
        
        // Boundary check pre maxFileSizeMB - ak nie je null, musí byť > 0
        if (maxFileSizeMB.HasValue && maxFileSizeMB.Value <= 0)
        {
            const string error = "MaxFileSizeMB must be greater than 0 if specified";
            externalLogger?.Error("❌ LoggerAPI: VALIDATION FAILED - {Error}, provided value: {Value}", 
                error, maxFileSizeMB.Value);
            throw new ArgumentException(error, nameof(maxFileSizeMB));
        }
        
        try 
        {
            // 📊 PHASE 2: CONFIGURATION OBJECT CREATION
            // Vytvárame LoggerConfiguration objekt s validovanými parametrami.
            // Mapujeme public API parametre na internal configuration štruktúru.
            // Create internal LoggerConfiguration from LoggerOptions
            var config = new Internal.Core.Models.LoggerConfiguration
            {
                LogDirectory = logDirectory.Trim(),                    // Cleanup whitespace
                BaseFileName = baseFileName.Trim(),                   // Cleanup whitespace
                MaxFileSizeMB = maxFileSizeMB,                        // Can be null
                MaxLogFiles = 50,                                     // Reasonable retention limit
                EnableAutoRotation = maxFileSizeMB.HasValue,          // Auto-determine rotation
                EnableRealTimeViewing = false,                        // File-only mode
                MinLogLevel = (Microsoft.Extensions.Logging.LogLevel)Internal.Configuration.LogLevel.Information     // Standard minimum level
            };
            
            externalLogger?.Info("📋 LoggerAPI: CONFIG CREATED - Configuration prepared: " +
                "Directory={Directory}, BaseFileName={BaseFileName}, MaxFileSizeMB={MaxFileSizeMB}, MaxLogFiles={MaxLogFiles}", 
                config.LogDirectory, config.BaseFileName, config.MaxFileSizeMB, config.MaxLogFiles);
            
            // 📊 PHASE 3: FILE LOGGER SERVICE INSTANTIATION
            // Vytvárame actual FileLoggerService inštanciu s prepared configuration.
            // Constructor môže vyhodiť výnimky ak sú problémy s file system operations.
            var fileLogger = new Internal.Infrastructure.Services.FileLoggerService(config, externalLogger);
            
            // 📊 PHASE 4: SUCCESS CONFIRMATION AND RETURN
            // Potvrdenie úspešného vytvorenia pre audit trail.
            // Vraciame ILogger interface pre clean API abstraction.
            externalLogger?.Info("✅ LoggerAPI: CREATION SUCCESS - File logger created successfully, " +
                "ready for logging operations");
            
            return fileLogger;
        }
        catch (DirectoryNotFoundException ex)
        {
            // 🚨 DIRECTORY CREATION FAILURE
            // LogDirectory neexistuje a nepodarilo sa ho vytvoriť
            externalLogger?.Error(ex, "🚨 LoggerAPI: DIRECTORY ERROR - Cannot create or access log directory: " +
                "'{Directory}'. Check permissions and path validity.", logDirectory);
            throw; // Re-throw pre caller handling
        }
        catch (UnauthorizedAccessException ex)
        {
            // 🚨 PERMISSIONS FAILURE
            // Nemáme write permissions pre specified directory
            externalLogger?.Error(ex, "🚨 LoggerAPI: PERMISSIONS ERROR - Insufficient permissions for log directory: " +
                "'{Directory}'. Application needs write access.", logDirectory);
            throw; // Re-throw pre caller handling
        }
        catch (Exception ex)
        {
            // 🚨 UNEXPECTED CRITICAL ERROR
            // Akákoľvek iná neočakávaná chyba počas vytvorenia loggera
            externalLogger?.Error(ex, "🚨 LoggerAPI: CRITICAL CREATION ERROR - Unexpected exception during file logger creation. " +
                "Directory='{Directory}', BaseFileName='{BaseFileName}', MaxSizeMB={MaxSize}", 
                logDirectory, baseFileName, maxFileSizeMB);
            throw; // Re-throw - caller must handle critical failures
        }
    }

    /// <summary>
    /// 🎯 CLEAN API FILE LOGGER CREATION - Modern configuration-based API
    /// 
    /// 🔧 MODERN API PATTERN:
    /// Táto metóda predstavuje modern approach k vytvoreniu file loggera pomocou konfiguračného
    /// objektu LoggerOptions. Poskytuje lepší IntelliSense support, type safety a extensibility
    /// pre budúce features bez breaking changes v API signature.
    /// 
    /// 📋 CONFIGURATION-DRIVEN DESIGN:
    /// - LoggerOptions objekt obsahuje všetky nastavenia
    /// - Predefined options pre common scenarios (Default, Development, HighVolume)
    /// - Selective customization - iba zmeniť potrebné properties
    /// - Future-proof - nové features môžu byť pridané bez API changes
    /// 
    /// 💡 USAGE PATTERNS:
    /// 
    /// // PREDEFINED OPTIONS:
    /// var logger = LoggerAPI.CreateFileLogger(LoggerOptions.Default);
    /// var devLogger = LoggerAPI.CreateFileLogger(LoggerOptions.Development);
    /// 
    /// // CUSTOM OPTIONS:
    /// var options = new LoggerOptions 
    /// { 
    ///     LogDirectory = @"C:\MyApp\Logs",
    ///     BaseFileName = "application",
    ///     MaxFileSizeMB = 25
    /// };
    /// var logger = LoggerAPI.CreateFileLogger(options);
    /// 
    /// // SELECTIVE CUSTOMIZATION:
    /// var options = LoggerOptions.Default;
    /// options.LogDirectory = myLogPath;
    /// options.MaxFileSizeMB = 50;
    /// var logger = LoggerAPI.CreateFileLogger(options);
    /// 
    /// 🔄 DELEGATION TO LEGACY API:
    /// Táto metóda internally deleguje na existujúcu CreateFileLogger() method
    /// pre zachovanie behavior consistency a code reuse. Legacy API zostáva
    /// available pre backward compatibility.
    /// </summary>
    /// <param name="options">
    /// LoggerOptions objekt s complete configuration pre file logger.
    /// Obsahuje LogDirectory (POVINNÝ), BaseFileName, file rotation settings,
    /// naming options a všetky ďalšie logger parameters.
    /// </param>
    /// <returns>
    /// ILogger implementation optimalizovaná pre file logging.
    /// Identický behavior ako legacy API ale s modern configuration approach.
    /// Thread-safe, high-performance, s automatic resource cleanup.
    /// </returns>
    /// <exception cref="ArgumentNullException">Ak options parameter je null</exception>
    /// <exception cref="ArgumentException">Ak LogDirectory v options je null alebo empty</exception>
    /// <exception cref="DirectoryNotFoundException">Ak sa nedá vytvoriť log directory</exception>
    /// <exception cref="UnauthorizedAccessException">Ak nie sú write permissions</exception>
    public static ILogger CreateFileLogger(LoggerOptions options)
    {
        // 📊 PHASE 1: OPTIONS VALIDATION
        // Validujeme configuration object pred delegáciou na legacy API
        if (options == null)
        {
            const string error = "LoggerOptions cannot be null";
            throw new ArgumentNullException(nameof(options), error);
        }

        // 📋 PHASE 2: PARAMETER EXTRACTION AND DELEGATION
        // Extrahujeme parameters z LoggerOptions a delegujeme na proven legacy API
        // Toto zabezpečuje behavior consistency medzi both APIs
        
        // Convert MaxFileSizeBytes to MaxFileSizeMB for legacy API compatibility
        int? maxSizeMB = options.MaxFileSizeBytes > 0 ? (int)(options.MaxFileSizeBytes / (1024 * 1024)) : null;

        // Delegate to existing proven implementation with default values
        // External logger je null lebo táto overload nemá external logger parameter
        // Pre external logger support by aplikácia mala použiť first overload
        return CreateFileLogger(
            externalLogger: null,
            logDirectory: options.LogDirectory,
            baseFileName: options.BaseFileName,
            maxFileSizeMB: maxSizeMB
        );
    }

    /// <summary>
    /// 🎯 CLEAN API FILE LOGGER WITH EXTERNAL LOGGER - Configuration-based s logging support
    /// 
    /// 🔧 ENHANCED CONFIGURATION API:
    /// Combination of modern LoggerOptions configuration s external logger support.
    /// Poskytuje both configuration convenience a external logging capability
    /// pre complex scenarios kde potrebujeme chain loggers.
    /// 
    /// 💡 USAGE SCENARIOS:
    /// 
    /// // S EXTERNAL LOGGER PRE AUDIT TRAIL:
    /// var mainLogger = // získaný z DI container
    /// var options = LoggerOptions.Default;
    /// options.LogDirectory = @"C:\AppLogs";
    /// var fileLogger = LoggerAPI.CreateFileLogger(mainLogger, options);
    /// 
    /// // DEVELOPMENT S DEBUG LOGGING:
    /// var debugLogger = // console alebo debug logger
    /// var devOptions = LoggerOptions.Development;
    /// var fileLogger = LoggerAPI.CreateFileLogger(debugLogger, devOptions);
    /// </summary>
    /// <param name="externalLogger">
    /// Optional external logger pre logging creation process a chaining.
    /// null = žiadne external logging (silent creation).
    /// </param>
    /// <param name="options">
    /// LoggerOptions objekt s complete configuration.
    /// Všetky same properties ako single-parameter overload.
    /// </param>
    /// <returns>
    /// ILogger implementation s same capabilities ako other overloads.
    /// </returns>
    /// <exception cref="ArgumentNullException">Ak options parameter je null</exception>
    /// <exception cref="ArgumentException">Ak LogDirectory v options je null alebo empty</exception>
    /// <exception cref="DirectoryNotFoundException">Ak sa nedá vytvoriť log directory</exception>
    /// <exception cref="UnauthorizedAccessException">Ak nie sú write permissions</exception>
    public static ILogger CreateFileLogger(ILogger? externalLogger, LoggerOptions options)
    {
        // 📊 VALIDATION PHASE
        if (options == null)
        {
            const string error = "LoggerOptions cannot be null";
            externalLogger?.Error("❌ LoggerAPI: VALIDATION FAILED - {Error}", error);
            throw new ArgumentNullException(nameof(options), error);
        }

        // 📋 DELEGATION PHASE
        // Convert options to individual parameters a delegate to proven implementation
        int? maxSizeMB = options.MaxFileSizeBytes > 0 ? (int)(options.MaxFileSizeBytes / (1024 * 1024)) : null;

        return CreateFileLogger(
            externalLogger: externalLogger,
            logDirectory: options.LogDirectory,
            baseFileName: options.BaseFileName,
            maxFileSizeMB: maxSizeMB
        );
    }

    #region Internal Mapping Methods

    /// <summary>
    /// INTERNAL: Maps public LoggerOptions to internal LoggerOptions
    /// CLEAN ARCHITECTURE: Protects internal types from external exposure
    /// </summary>
    private static Internal.Configuration.LoggerOptions MapToInternal(LoggerOptions publicOptions)
    {
        return new Internal.Configuration.LoggerOptions
        {
            LogDirectory = publicOptions.LogDirectory,
            BaseFileName = publicOptions.BaseFileName,
            MaxFileSizeBytes = publicOptions.MaxFileSizeBytes,
            MaxFileSizeMB = publicOptions.MaxFileSizeBytes > 0 ? (int)(publicOptions.MaxFileSizeBytes / (1024 * 1024)) : 100,
            MaxFileCount = publicOptions.MaxFileCount,
            MaxLogFiles = publicOptions.MaxFileCount,
            EnableAutoRotation = publicOptions.EnableAutoRotation,
            EnableBackgroundLogging = publicOptions.EnableBackgroundLogging,
            EnablePerformanceMonitoring = publicOptions.EnablePerformanceMonitoring,
            DateFormat = publicOptions.DateFormat,
            EnableRealTimeViewing = false,
            EnableStructuredLogging = true,
            MinLogLevel = Internal.Configuration.LogLevel.Information,
            BufferSize = 1000,
            FlushInterval = TimeSpan.FromSeconds(5)
        };
    }

    #endregion
}

#region Public Result Types - Clean API

/// <summary>
/// Public API: Generic result wrapper for Logger operations
/// SENIOR DEVELOPER: Clean Result pattern for professional error handling
/// </summary>
public sealed class LoggerResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static LoggerResult Success() => new() { IsSuccess = true };
    public static LoggerResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

/// <summary>
/// Public API: Generic result wrapper with value for Logger operations
/// SENIOR DEVELOPER: Clean Result pattern with typed return values
/// </summary>
public sealed class LoggerResult<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Value { get; init; }
    
    public static LoggerResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static LoggerResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

#endregion