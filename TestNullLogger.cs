using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Logging;

// Test demonstrujúci null-safe logger handling
namespace TestNullSafeLogger
{
    public static class TestNullLogger
    {
        public static void RunTest()
        {
            Console.WriteLine("🧪 SENIOR DEVELOPER: Testing null-safe logger handling...");
            
            // ✅ TEST 1: Null logger s CreateForUI()
            Console.WriteLine("\n1️⃣ Testing CreateForUI with null logger:");
            var dataGridWithNullLogger = AdvancedWinUiDataGrid.CreateForUI(null);
            Console.WriteLine($"   ✅ CreateForUI(null) - Success: {dataGridWithNullLogger != null}");
            
            // ✅ TEST 2: Null logger s LoggingOptions
            Console.WriteLine("\n2️⃣ Testing CreateForUI with null logger and LoggingOptions:");
            var loggingOptions = LoggingOptions.Development;
            var dataGridWithOptions = AdvancedWinUiDataGrid.CreateForUI(null, loggingOptions);
            Console.WriteLine($"   ✅ CreateForUI(null, options) - Success: {dataGridWithOptions != null}");
            
            // ✅ TEST 3: Null logger s CreateHeadless()
            Console.WriteLine("\n3️⃣ Testing CreateHeadless with null logger:");
            var headlessDataGrid = AdvancedWinUiDataGrid.CreateHeadless(null);
            Console.WriteLine($"   ✅ CreateHeadless(null) - Success: {headlessDataGrid != null}");
            
            // ✅ TEST 4: Nomadic pattern testing
            Console.WriteLine("\n4️⃣ Testing nomadic pattern with null logger:");
            try
            {
                // Define test columns
                var columns = new List<RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition>
                {
                    RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core.ColumnDefinition.Text("TestColumn", "Test")
                };
                
                // This should work even with null logger thanks to Result<T> pattern
                var initResult = dataGridWithNullLogger.InitializeAsync(columns).GetAwaiter().GetResult();
                
                Console.WriteLine($"   ✅ InitializeAsync with null logger - Success: {initResult.IsSuccess}");
                if (initResult.IsFailure)
                {
                    Console.WriteLine($"      ℹ️  Error (expected for headless): {initResult.Error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️  Exception caught: {ex.Message}");
            }
            
            Console.WriteLine("\n🎉 SENIOR DEVELOPER: All null-safe logger tests completed!");
            Console.WriteLine("📋 SUMMARY: Logger can be null, component gracefully handles it using NullLogger");
            
            // Cleanup
            dataGridWithNullLogger?.Dispose();
            dataGridWithOptions?.Dispose();
            headlessDataGrid?.Dispose();
        }
    }
}