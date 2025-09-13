using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiLogger;
using LoggingOptions = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Logging.LoggingOptions;
using LoggingStrategy = RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Logging.LoggingStrategy;

/// <summary>
/// 🎯 SENIOR DEVELOPER: Professional logging setup example
/// Demonstruje správne použitie Microsoft.Extensions.Logging + AdvancedWinUiLogger + AdvancedWinUiDataGrid
/// </summary>
namespace Professional_Logging_Example
{
    public static class ProfessionalLoggingSetup
    {
        public static void DemonstrateSetup()
        {
            Console.WriteLine("🔧 SENIOR DEVELOPER: Professional Logging Setup Example");
            Console.WriteLine("=".PadRight(80, '='));
            
            // 📋 STEP 1: Create base Microsoft.Extensions.Logging logger
            Console.WriteLine("\n1️⃣ STEP 1: Setup Microsoft.Extensions.Logging");
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole()
                       .AddDebug()
                       .SetMinimumLevel(LogLevel.Debug);
            });
            var baseLogger = loggerFactory.CreateLogger<ProfessionalLoggingSetup>();
            Console.WriteLine("   ✅ Base logger created with Console and Debug providers");
            
            // 📋 STEP 2: Create AdvancedWinUiLogger with file rotation
            Console.WriteLine("\n2️⃣ STEP 2: Create AdvancedWinUiLogger with 10MB rotation");
            var advancedLogger = LoggerAPI.CreateFileLogger(
                externalLogger: baseLogger,                     // Pass base logger for dual logging
                logDirectory: @"C:\Temp\DataGridLogs",          // Log directory (will be created if needed)
                baseFileName: "DataGridDemo",                   // Base filename (creates DataGridDemo.log)
                maxFileSizeMB: 10                               // 10MB rotation as requested
            );
            Console.WriteLine("   ✅ AdvancedWinUiLogger created with 10MB file rotation");
            Console.WriteLine($"   📂 Log directory: C:\\Temp\\DataGridLogs");
            Console.WriteLine($"   📄 Log files: DataGridDemo.log (current), DataGridDemo_1.log, DataGridDemo_2.log...");
            
            // 📋 STEP 3: Configure LoggingOptions for DataGrid component
            Console.WriteLine("\n3️⃣ STEP 3: Configure LoggingOptions for DataGrid");
            
            // Development scenario - detailed logging
            var developmentOptions = LoggingOptions.Development with
            {
                CategoryPrefix = "DataGrid-Dev",
                LogMethodParameters = true,
                LogPerformanceMetrics = true,
                LogConfigurationDetails = true,
                LogUnhandledErrors = true,
                LogResultPatternStates = true,
                Strategy = LoggingStrategy.Immediate,
                MinimumLogLevel = LogLevel.Debug
            };
            Console.WriteLine("   ✅ Development LoggingOptions configured (detailed logging)");
            
            // Production scenario - optimized logging
            var productionOptions = LoggingOptions.Production with
            {
                CategoryPrefix = "DataGrid-Prod",
                LogMethodParameters = false,
                LogPerformanceMetrics = true,
                LogConfigurationDetails = false,
                LogUnhandledErrors = true,
                LogResultPatternStates = false,
                Strategy = LoggingStrategy.Batch,
                MinimumLogLevel = LogLevel.Information
            };
            Console.WriteLine("   ✅ Production LoggingOptions configured (optimized performance)");
            
            // High-performance scenario - minimal logging
            var highPerfOptions = LoggingOptions.HighPerformance with
            {
                CategoryPrefix = "DataGrid-HighPerf",
                LogMethodParameters = false,
                LogPerformanceMetrics = false,
                LogConfigurationDetails = false,
                LogUnhandledErrors = true,
                LogResultPatternStates = false,
                Strategy = LoggingStrategy.InMemory,
                MinimumLogLevel = LogLevel.Warning
            };
            Console.WriteLine("   ✅ HighPerformance LoggingOptions configured (minimal overhead)");
            
            // 📋 STEP 4: Create AdvancedWinUiDataGrid with professional logging
            Console.WriteLine("\n4️⃣ STEP 4: Create AdvancedWinUiDataGrid with professional logging");
            
            // UI mode with development logging
            var uiDataGrid = AdvancedWinUiDataGrid.CreateForUI(advancedLogger, developmentOptions);
            Console.WriteLine("   ✅ UI DataGrid created with AdvancedWinUiLogger and Development options");
            
            // Headless mode with production logging  
            var headlessDataGrid = AdvancedWinUiDataGrid.CreateHeadless(advancedLogger, productionOptions);
            Console.WriteLine("   ✅ Headless DataGrid created with AdvancedWinUiLogger and Production options");
            
            // 📋 STEP 5: Demonstrate logging output
            Console.WriteLine("\n5️⃣ STEP 5: Test logging output");
            advancedLogger.LogInformation("🎉 Professional logging setup completed successfully!");
            advancedLogger.LogDebug("Debug message: This demonstrates detailed logging capability");
            advancedLogger.LogWarning("Warning message: This demonstrates warning level logging");
            Console.WriteLine("   ✅ Log messages sent to both console and file system");
            
            // 📋 SUMMARY
            Console.WriteLine("\n🎯 SETUP SUMMARY:");
            Console.WriteLine("   ✅ Microsoft.Extensions.Logging base logger ➔ Console + Debug output");
            Console.WriteLine("   ✅ AdvancedWinUiLogger ➔ File rotation every 10MB");
            Console.WriteLine("   ✅ AdvancedWinUiDataGrid ➔ Professional logging with custom LoggingOptions");
            Console.WriteLine("   ✅ Multiple LoggingOptions presets ➔ Development, Production, HighPerformance");
            
            Console.WriteLine("\n📁 FILE STRUCTURE:");
            Console.WriteLine("   C:\\Temp\\DataGridLogs\\");
            Console.WriteLine("   ├── DataGridDemo.log          (current active log file)");
            Console.WriteLine("   ├── DataGridDemo_1.log        (first rotated file when >10MB)");
            Console.WriteLine("   ├── DataGridDemo_2.log        (second rotated file)");
            Console.WriteLine("   └── ...");
            
            Console.WriteLine("\n🚀 NEXT STEPS:");
            Console.WriteLine("   1. Use the created DataGrid instances for your operations");
            Console.WriteLine("   2. All operations will be automatically logged with comprehensive detail");
            Console.WriteLine("   3. Check log files for detailed execution traces and error information");
            Console.WriteLine("   4. Adjust LoggingOptions based on your performance requirements");
            
            // Cleanup
            uiDataGrid?.Dispose();
            headlessDataGrid?.Dispose();
            advancedLogger = null;
            loggerFactory?.Dispose();
            
            Console.WriteLine("\n✨ Professional logging setup demonstration completed!");
        }
    }
}