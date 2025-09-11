# PART VIII: COMPLETE PRACTICAL IMPLEMENTATION GUIDE

## 36. **Complete Usage Examples - Real World Scenarios**

### 36.1 **Enterprise Employee Management System**

This comprehensive example demonstrates how to build a complete employee management system using the AdvancedWinUiDataGrid component with all enterprise features enabled.

```csharp
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;

/// <summary>
/// ENTERPRISE EXAMPLE: Complete Employee Management System
/// FEATURES: All advanced features including validation, search, import/export
/// REAL_WORLD: Production-ready implementation with error handling
/// </summary>
public class EmployeeManagementSystem
{
    #region Private Fields
    
    private AdvancedWinUiDataGrid? _dataGrid;
    private ILogger<EmployeeManagementSystem>? _logger;
    
    #endregion
    
    #region Initialization - Step 1: Setup DataGrid with Complete Configuration
    
    /// <summary>
    /// STEP 1: Initialize the DataGrid with comprehensive employee data structure
    /// ENTERPRISE: Full configuration with validation, colors, and performance tuning
    /// </summary>
    public async Task<bool> InitializeEmployeeGridAsync()
    {
        try
        {
            // 1.1 CREATE DATAGRID INSTANCE
            _dataGrid = AdvancedWinUiDataGrid.CreateForUI(_logger);
            
            // 1.2 DEFINE COLUMN STRUCTURE
            var columns = await CreateEmployeeColumnsAsync();
            
            // 1.3 CONFIGURE VALIDATION RULES
            var validationConfig = CreateEmployeeValidationConfiguration();
            
            // 1.4 CONFIGURE UI COLORS AND THEMES
            var colorConfig = CreateEnterpriseColorConfiguration();
            
            // 1.5 CONFIGURE PERFORMANCE FOR LARGE DATASETS
            var performanceConfig = CreatePerformanceConfiguration();
            
            // 1.6 INITIALIZE WITH ALL CONFIGURATIONS
            var result = await _dataGrid.InitializeAsync(
                columns: columns,
                colorConfiguration: colorConfig,
                validationConfiguration: validationConfig,
                performanceConfiguration: performanceConfig,
                minimumRows: 5); // Always have 5 empty rows available
            
            if (!result.IsSuccess)
            {
                _logger?.LogError("Failed to initialize employee grid: {Error}", result.Error);
                return false;
            }
            
            _logger?.LogInformation("✅ Employee management system initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Failed to initialize employee management system");
            return false;
        }
    }
    
    /// <summary>
    /// COLUMN DEFINITION: Complete employee data structure with all field types
    /// ENTERPRISE: Comprehensive business entity with validation and formatting
    /// </summary>
    private async Task<IReadOnlyList<ColumnDefinition>> CreateEmployeeColumnsAsync()
    {
        return new List<ColumnDefinition>
        {
            // SELECTION COLUMN: Checkbox for multi-select operations
            ColumnDefinition.CheckBox("Selected", "☑️"),
            
            // BASIC INFORMATION
            ColumnDefinition.Required("EmployeeId", typeof(int), "Employee ID", "Employee ID is required"),
            ColumnDefinition.Required("FirstName", typeof(string), "First Name", "First name is required"),
            ColumnDefinition.Required("LastName", typeof(string), "Last Name", "Last name is required"),
            ColumnDefinition.Text("MiddleName", "Middle Name"),
            
            // CONTACT INFORMATION WITH VALIDATION
            ColumnDefinition.WithValidation("Email", typeof(string), 
                ColumnValidationRule.Required("Email is required"),
                ColumnValidationRule.Email("Invalid email format")),
            
            ColumnDefinition.WithValidation("Phone", typeof(string),
                ColumnValidationRule.Phone("Invalid phone number format")),
            
            // EMPLOYMENT DETAILS
            ColumnDefinition.DateTime("HireDate", "Hire Date", "MM/dd/yyyy"),
            ColumnDefinition.DateTime("LastReviewDate", "Last Review", "MM/dd/yyyy"),
            
            // FINANCIAL INFORMATION
            ColumnDefinition.Numeric<decimal>("Salary", "Annual Salary", "C2"), // Currency format
            ColumnDefinition.Numeric<decimal>("HourlyRate", "Hourly Rate", "C2"),
            ColumnDefinition.Numeric<int>("VacationDays", "Vacation Days"),
            ColumnDefinition.Numeric<int>("SickDays", "Sick Days"),
            
            // DEPARTMENT AND POSITION
            ColumnDefinition.Text("Department", "Department"),
            ColumnDefinition.Text("Position", "Position"),
            ColumnDefinition.Text("Manager", "Reports To"),
            
            // STATUS AND FLAGS
            ColumnDefinition.Boolean("IsActive", "Active"),
            ColumnDefinition.Boolean("IsFullTime", "Full Time"),
            ColumnDefinition.Boolean("HasBenefits", "Benefits"),
            
            // ADDRESSES
            ColumnDefinition.Text("Address", "Address"),
            ColumnDefinition.Text("City", "City"),
            ColumnDefinition.Text("State", "State"),
            ColumnDefinition.Text("ZipCode", "ZIP Code"),
            
            // NOTES AND COMMENTS
            ColumnDefinition.Text("Notes", "Notes"),
            
            // SPECIAL COLUMNS
            ColumnDefinition.ValidAlerts("Validation", "⚠️"), // Shows validation errors
            ColumnDefinition.DeleteRow("🗑️", requireConfirmation: true, 
                confirmationMessage: "Are you sure you want to delete this employee?")
        };
    }
    
    /// <summary>
    /// VALIDATION CONFIGURATION: Enterprise-grade validation rules
    /// BUSINESS_RULES: Complex validation logic for employee data integrity
    /// </summary>
    private ValidationConfiguration CreateEmployeeValidationConfiguration()
    {
        return new ValidationConfiguration
        {
            // REAL-TIME VALIDATION: Validate as user types
            EnableRealTimeValidation = true,
            ValidateOnImport = true,
            ValidateOnSave = true,
            
            // ERROR DISPLAY CONFIGURATION
            MaxErrorsPerRow = 5,
            ValidationTimeout = TimeSpan.FromSeconds(10),
            
            // DISPLAY OPTIONS
            DisplayOptions = new ValidationDisplayOptions
            {
                ShowErrorIcons = true,
                ShowErrorTooltips = true,
                HighlightInvalidRows = true,
                ShowValidationSummary = true,
                GroupErrorsByType = true
            },
            
            // PERFORMANCE OPTIMIZATION
            PerformanceOptions = new ValidationPerformanceOptions
            {
                EnableCaching = true,
                CacheExpiry = TimeSpan.FromMinutes(5),
                BatchSize = 100,
                UseParallelProcessing = true,
                MaxConcurrentTasks = Environment.ProcessorCount
            },
            
            // CUSTOM BUSINESS RULES
            CustomRules = new List<IValidationRule>
            {
                new EmployeeSalaryValidationRule(),
                new EmployeeDateValidationRule(),
                new EmployeeDepartmentValidationRule()
            }
        };
    }
    
    /// <summary>
    /// COLOR CONFIGURATION: Professional enterprise theme
    /// UI_DESIGN: Corporate color scheme with accessibility compliance
    /// </summary>
    private ColorConfiguration CreateEnterpriseColorConfiguration()
    {
        return new ColorConfiguration
        {
            // HEADER COLORS
            HeaderBackground = Color.FromArgb(255, 41, 98, 144),    // Corporate blue
            HeaderForeground = Color.FromArgb(255, 255, 255, 255),  // White text
            
            // ALTERNATING ROW COLORS
            AlternateRowBackground = Color.FromArgb(255, 248, 249, 250), // Light gray
            NormalRowBackground = Color.FromArgb(255, 255, 255, 255),    // White
            
            // SELECTION COLORS
            SelectedRowBackground = Color.FromArgb(255, 0, 120, 215),    // Windows blue
            SelectedRowForeground = Color.FromArgb(255, 255, 255, 255),  // White text
            
            // VALIDATION COLORS
            ValidationErrorBackground = Color.FromArgb(255, 254, 242, 242), // Light red
            ValidationErrorForeground = Color.FromArgb(255, 185, 28, 28),   // Dark red
            ValidationWarningBackground = Color.FromArgb(255, 255, 251, 235), // Light yellow
            ValidationWarningForeground = Color.FromArgb(255, 180, 83, 9),    // Dark orange
            
            // BORDER COLORS
            GridBorderColor = Color.FromArgb(255, 229, 231, 235),    // Light border
            CellBorderColor = Color.FromArgb(255, 243, 244, 246),    // Very light border
            
            // HOVER EFFECTS
            HoverRowBackground = Color.FromArgb(255, 243, 244, 246),  // Light gray hover
            
            // THEME SETTINGS
            Theme = ColorTheme.Professional,
            UseSystemColors = false,
            EnableAnimations = true
        };
    }
    
    /// <summary>
    /// PERFORMANCE CONFIGURATION: Optimized for large employee datasets
    /// SCALABILITY: Handles 100K+ employee records efficiently
    /// </summary>
    private PerformanceConfiguration CreatePerformanceConfiguration()
    {
        return new PerformanceConfiguration
        {
            // VIRTUALIZATION SETTINGS
            EnableVirtualization = true,
            VirtualizationMode = VirtualizationMode.Recycling,
            ItemContainerRecycling = true,
            
            // BATCH PROCESSING
            BatchSize = 1000,
            MaxBatchProcessingTime = TimeSpan.FromSeconds(30),
            
            // MEMORY MANAGEMENT
            EnableDataCaching = true,
            CacheSize = 50000, // Cache 50K rows in memory
            EnableLazyLoading = true,
            
            // UI PERFORMANCE
            EnableProgressReporting = true,
            ProgressReportingInterval = TimeSpan.FromMilliseconds(500),
            MaxUIUpdateFrequency = 60, // 60 FPS max
            
            // THREADING
            MaxConcurrentOperations = Environment.ProcessorCount * 2,
            UseBackgroundProcessing = true,
            
            // OPTIMIZATION FLAGS
            OptimizeForLargeDatasets = true,
            EnableSmartIndexing = true,
            EnableCompressionForStorage = true
        };
    }
    
    #endregion
    
    #region Data Loading - Step 2: Load Employee Data
    
    /// <summary>
    /// STEP 2: Load employee data from various sources
    /// ENTERPRISE: Multiple data source support with error handling
    /// </summary>
    public async Task<bool> LoadEmployeeDataAsync()
    {
        try
        {
            // 2.1 LOAD FROM DATABASE
            var employeesFromDb = await LoadEmployeesFromDatabaseAsync();
            if (employeesFromDb.Any())
            {
                var importResult = await ImportEmployeeDataAsync(employeesFromDb);
                if (!importResult.IsSuccess)
                {
                    _logger?.LogError("Failed to import employees from database: {Error}", importResult.Error);
                    return false;
                }
            }
            
            // 2.2 LOAD FROM EXCEL FILE (if exists)
            var excelFile = @"C:\Data\Employees.xlsx";
            if (File.Exists(excelFile))
            {
                var excelData = await LoadEmployeesFromExcelAsync(excelFile);
                if (excelData.Any())
                {
                    await ImportEmployeeDataAsync(excelData, ImportMode.Append);
                }
            }
            
            // 2.3 VALIDATE ALL DATA AFTER LOADING
            await ValidateAllEmployeeDataAsync();
            
            _logger?.LogInformation("✅ Employee data loaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Failed to load employee data");
            return false;
        }
    }
    
    /// <summary>
    /// DATABASE INTEGRATION: Load employees from SQL Server
    /// ENTERPRISE: Production database integration with connection pooling
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> LoadEmployeesFromDatabaseAsync()
    {
        var employees = new List<Dictionary<string, object?>>();
        
        const string sql = @"
            SELECT 
                EmployeeId, FirstName, LastName, MiddleName, Email, Phone,
                HireDate, LastReviewDate, Salary, HourlyRate, 
                VacationDays, SickDays, Department, Position, Manager,
                IsActive, IsFullTime, HasBenefits,
                Address, City, State, ZipCode, Notes
            FROM Employees 
            WHERE IsDeleted = 0
            ORDER BY LastName, FirstName";
        
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var employee = new Dictionary<string, object?>
                {
                    ["EmployeeId"] = reader["EmployeeId"],
                    ["FirstName"] = reader["FirstName"]?.ToString(),
                    ["LastName"] = reader["LastName"]?.ToString(),
                    ["MiddleName"] = reader.IsDBNull("MiddleName") ? null : reader["MiddleName"]?.ToString(),
                    ["Email"] = reader["Email"]?.ToString(),
                    ["Phone"] = reader["Phone"]?.ToString(),
                    ["HireDate"] = reader["HireDate"] as DateTime?,
                    ["LastReviewDate"] = reader.IsDBNull("LastReviewDate") ? null : reader["LastReviewDate"] as DateTime?,
                    ["Salary"] = reader.IsDBNull("Salary") ? null : reader["Salary"] as decimal?,
                    ["HourlyRate"] = reader.IsDBNull("HourlyRate") ? null : reader["HourlyRate"] as decimal?,
                    ["VacationDays"] = reader["VacationDays"] as int? ?? 0,
                    ["SickDays"] = reader["SickDays"] as int? ?? 0,
                    ["Department"] = reader["Department"]?.ToString(),
                    ["Position"] = reader["Position"]?.ToString(),
                    ["Manager"] = reader["Manager"]?.ToString(),
                    ["IsActive"] = (bool)reader["IsActive"],
                    ["IsFullTime"] = (bool)reader["IsFullTime"],
                    ["HasBenefits"] = (bool)reader["HasBenefits"],
                    ["Address"] = reader["Address"]?.ToString(),
                    ["City"] = reader["City"]?.ToString(),
                    ["State"] = reader["State"]?.ToString(),
                    ["ZipCode"] = reader["ZipCode"]?.ToString(),
                    ["Notes"] = reader.IsDBNull("Notes") ? null : reader["Notes"]?.ToString(),
                    ["Selected"] = false // Default selection state
                };
                
                employees.Add(employee);
            }
            
            _logger?.LogInformation("📊 Loaded {Count} employees from database", employees.Count);
            return employees;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Failed to load employees from database");
            return new List<Dictionary<string, object?>>();
        }
    }
    
    /// <summary>
    /// EXCEL INTEGRATION: Load employees from Excel file using EPPlus
    /// ENTERPRISE: Professional Excel integration with error handling
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> LoadEmployeesFromExcelAsync(string filePath)
    {
        var employees = new List<Dictionary<string, object?>>();
        
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or Commercial
            
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0]; // First worksheet
            
            if (worksheet.Dimension == null)
            {
                _logger?.LogWarning("Excel file is empty: {FilePath}", filePath);
                return employees;
            }
            
            var startRow = 2; // Assuming row 1 contains headers
            var endRow = worksheet.Dimension.End.Row;
            
            for (int row = startRow; row <= endRow; row++)
            {
                var employee = new Dictionary<string, object?>
                {
                    ["EmployeeId"] = worksheet.Cells[row, 1].GetValue<int>(),
                    ["FirstName"] = worksheet.Cells[row, 2].GetValue<string>(),
                    ["LastName"] = worksheet.Cells[row, 3].GetValue<string>(),
                    ["MiddleName"] = worksheet.Cells[row, 4].GetValue<string>(),
                    ["Email"] = worksheet.Cells[row, 5].GetValue<string>(),
                    ["Phone"] = worksheet.Cells[row, 6].GetValue<string>(),
                    ["HireDate"] = worksheet.Cells[row, 7].GetValue<DateTime?>(),
                    ["LastReviewDate"] = worksheet.Cells[row, 8].GetValue<DateTime?>(),
                    ["Salary"] = worksheet.Cells[row, 9].GetValue<decimal?>(),
                    ["HourlyRate"] = worksheet.Cells[row, 10].GetValue<decimal?>(),
                    ["VacationDays"] = worksheet.Cells[row, 11].GetValue<int?>() ?? 0,
                    ["SickDays"] = worksheet.Cells[row, 12].GetValue<int?>() ?? 0,
                    ["Department"] = worksheet.Cells[row, 13].GetValue<string>(),
                    ["Position"] = worksheet.Cells[row, 14].GetValue<string>(),
                    ["Manager"] = worksheet.Cells[row, 15].GetValue<string>(),
                    ["IsActive"] = worksheet.Cells[row, 16].GetValue<bool>(),
                    ["IsFullTime"] = worksheet.Cells[row, 17].GetValue<bool>(),
                    ["HasBenefits"] = worksheet.Cells[row, 18].GetValue<bool>(),
                    ["Address"] = worksheet.Cells[row, 19].GetValue<string>(),
                    ["City"] = worksheet.Cells[row, 20].GetValue<string>(),
                    ["State"] = worksheet.Cells[row, 21].GetValue<string>(),
                    ["ZipCode"] = worksheet.Cells[row, 22].GetValue<string>(),
                    ["Notes"] = worksheet.Cells[row, 23].GetValue<string>(),
                    ["Selected"] = false
                };
                
                employees.Add(employee);
            }
            
            _logger?.LogInformation("📊 Loaded {Count} employees from Excel file", employees.Count);
            return employees;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Failed to load employees from Excel file: {FilePath}", filePath);
            return new List<Dictionary<string, object?>>();
        }
    }
    
    /// <summary>
    /// DATA IMPORT: Import employee data with progress reporting
    /// ENTERPRISE: Bulk import with validation and progress feedback
    /// </summary>
    private async Task<Result<ImportResult>> ImportEmployeeDataAsync(
        List<Dictionary<string, object?>> employees, 
        ImportMode mode = ImportMode.Replace)
    {
        if (_dataGrid == null)
            return Result<ImportResult>.Failure("DataGrid not initialized");
        
        // CREATE PROGRESS REPORTER
        var progressReporter = new Progress<ValidationProgress>(progress =>
        {
            var percentage = Math.Round(progress.PercentageComplete, 1);
            _logger?.LogInformation("📥 Import Progress: {Percentage}% - {Operation} ({ProcessedRows}/{TotalRows})",
                percentage, progress.CurrentOperation, progress.ProcessedRows, progress.TotalRows);
        });
        
        // IMPORT WITH PROGRESS REPORTING
        var result = await _dataGrid.ImportFromDictionaryAsync(
            data: employees,
            checkboxStates: null,
            startRow: 1,
            mode: mode,
            timeout: TimeSpan.FromMinutes(10), // Allow up to 10 minutes for large imports
            validationProgress: progressReporter);
        
        if (result.IsSuccess)
        {
            _logger?.LogInformation("✅ Successfully imported {ImportedRows} employees", result.Value.ImportedRows);
        }
        else
        {
            _logger?.LogError("❌ Import failed: {Error}", result.Error);
        }
        
        return result;
    }
    
    #endregion
    
    #region Search and Filter Operations - Step 3: Advanced Data Operations
    
    /// <summary>
    /// STEP 3A: Advanced search functionality
    /// ENTERPRISE: Multiple search strategies with performance optimization
    /// </summary>
    public async Task<bool> SearchEmployeesAsync(string searchTerm, EmployeeSearchOptions? options = null)
    {
        if (_dataGrid == null || string.IsNullOrWhiteSpace(searchTerm))
            return false;
        
        try
        {
            var searchOptions = new SearchOptions
            {
                ColumnNames = options?.SearchColumns,
                CaseSensitive = options?.CaseSensitive ?? false,
                UseRegex = options?.UseRegex ?? false,
                WholeWordOnly = options?.WholeWordOnly ?? false,
                MaxResults = options?.MaxResults ?? 10000,
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            _logger?.LogInformation("🔍 Searching employees for: '{SearchTerm}'", searchTerm);
            
            var result = await _dataGrid.SearchAsync(searchTerm, searchOptions);
            
            if (result.IsSuccess)
            {
                _logger?.LogInformation("✅ Search completed. Found {MatchCount} matches", 
                    result.Value?.Matches?.Count ?? 0);
                return true;
            }
            else
            {
                _logger?.LogError("❌ Search failed: {Error}", result.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Search operation failed");
            return false;
        }
    }
    
    /// <summary>
    /// STEP 3B: Complex filtering system
    /// ENTERPRISE: Business rule filters with logical operators
    /// </summary>
    public async Task<bool> ApplyEmployeeFiltersAsync(EmployeeFilterCriteria criteria)
    {
        if (_dataGrid == null) return false;
        
        try
        {
            var filters = new List<FilterExpression>();
            
            // DEPARTMENT FILTER
            if (!string.IsNullOrEmpty(criteria.Department))
            {
                filters.Add(new FilterExpression
                {
                    ColumnName = "Department",
                    Operator = FilterOperator.Equals,
                    Value = criteria.Department,
                    LogicOperator = FilterLogicOperator.And
                });
            }
            
            // ACTIVE STATUS FILTER
            if (criteria.ActiveOnly)
            {
                filters.Add(new FilterExpression
                {
                    ColumnName = "IsActive",
                    Operator = FilterOperator.Equals,
                    Value = true,
                    LogicOperator = FilterLogicOperator.And
                });
            }
            
            // SALARY RANGE FILTER
            if (criteria.MinSalary.HasValue)
            {
                filters.Add(new FilterExpression
                {
                    ColumnName = "Salary",
                    Operator = FilterOperator.GreaterThanOrEqual,
                    Value = criteria.MinSalary.Value,
                    LogicOperator = FilterLogicOperator.And
                });
            }
            
            if (criteria.MaxSalary.HasValue)
            {
                filters.Add(new FilterExpression
                {
                    ColumnName = "Salary",
                    Operator = FilterOperator.LessThanOrEqual,
                    Value = criteria.MaxSalary.Value,
                    LogicOperator = FilterLogicOperator.And
                });
            }
            
            // HIRE DATE RANGE FILTER
            if (criteria.HiredAfter.HasValue)
            {
                filters.Add(new FilterExpression
                {
                    ColumnName = "HireDate",
                    Operator = FilterOperator.GreaterThanOrEqual,
                    Value = criteria.HiredAfter.Value,
                    LogicOperator = FilterLogicOperator.And
                });
            }
            
            // FULL-TIME STATUS FILTER
            if (criteria.FullTimeOnly)
            {
                filters.Add(new FilterExpression
                {
                    ColumnName = "IsFullTime",
                    Operator = FilterOperator.Equals,
                    Value = true,
                    LogicOperator = FilterLogicOperator.And
                });
            }
            
            _logger?.LogInformation("🔧 Applying {FilterCount} filters to employee data", filters.Count);
            
            var result = await _dataGrid.ApplyFiltersAsync(filters);
            
            if (result.IsSuccess)
            {
                _logger?.LogInformation("✅ Filters applied successfully");
                return true;
            }
            else
            {
                _logger?.LogError("❌ Filter application failed: {Error}", result.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Filter operation failed");
            return false;
        }
    }
    
    #endregion
    
    #region Validation and Data Quality - Step 4: Ensure Data Integrity
    
    /// <summary>
    /// STEP 4: Comprehensive data validation
    /// ENTERPRISE: Multi-level validation with detailed reporting
    /// </summary>
    public async Task<ValidationReport> ValidateAllEmployeeDataAsync()
    {
        if (_dataGrid == null)
            return new ValidationReport { Success = false, ErrorMessage = "DataGrid not initialized" };
        
        try
        {
            var progressReporter = new Progress<ValidationProgress>(progress =>
            {
                var percentage = Math.Round(progress.PercentageComplete, 1);
                _logger?.LogInformation("✅ Validation Progress: {Percentage}% - {ProcessedRows}/{TotalRows} rows processed",
                    percentage, progress.ProcessedRows, progress.TotalRows);
            });
            
            _logger?.LogInformation("🔍 Starting comprehensive employee data validation");
            
            var result = await _dataGrid.ValidateAllAsync(progressReporter);
            
            if (result.IsSuccess)
            {
                var errors = result.Value ?? Array.Empty<ValidationError>();
                var report = new ValidationReport
                {
                    Success = true,
                    TotalErrors = errors.Length,
                    ErrorsByType = errors.GroupBy(e => e.Level)
                                        .ToDictionary(g => g.Key, g => g.Count()),
                    ErrorDetails = errors.Take(100).ToList(), // Limit details for performance
                    ValidationTime = DateTime.UtcNow
                };
                
                _logger?.LogInformation("✅ Validation completed. Found {ErrorCount} validation issues", errors.Length);
                
                // LOG ERROR SUMMARY
                if (errors.Length > 0)
                {
                    var errorSummary = string.Join(", ", 
                        report.ErrorsByType.Select(kvp => $"{kvp.Value} {kvp.Key}(s)"));
                    _logger?.LogWarning("⚠️ Validation Issues Found: {ErrorSummary}", errorSummary);
                }
                
                return report;
            }
            else
            {
                _logger?.LogError("❌ Validation failed: {Error}", result.Error);
                return new ValidationReport 
                { 
                    Success = false, 
                    ErrorMessage = result.Error,
                    ValidationTime = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Validation operation failed");
            return new ValidationReport 
            { 
                Success = false, 
                ErrorMessage = ex.Message,
                ValidationTime = DateTime.UtcNow
            };
        }
    }
    
    /// <summary>
    /// ADVANCED VALIDATION: Clean invalid employee records
    /// ENTERPRISE: Validation-based data cleanup with safety checks
    /// </summary>
    public async Task<CleanupReport> CleanupInvalidEmployeeDataAsync(ValidationCleanupOptions options)
    {
        if (_dataGrid == null)
            return new CleanupReport { Success = false, ErrorMessage = "DataGrid not initialized" };
        
        try
        {
            var criteria = new ValidationDeletionCriteria
            {
                Mode = options.DeleteMode,
                Severity = options.TargetSeverity,
                SpecificRuleNames = options.SpecificRules
            };
            
            var deletionOptions = new ValidationDeletionOptions
            {
                RequireConfirmation = options.RequireConfirmation,
                CreateBackup = options.CreateBackup,
                Progress = new Progress<ValidationDeletionProgress>(progress =>
                {
                    _logger?.LogInformation("🗑️ Cleanup Progress: Evaluated {EvaluatedRows}/{TotalRows}, " +
                        "Deleted {DeletedRows} rows", 
                        progress.EvaluatedRows, progress.TotalRows, progress.DeletedRows);
                })
            };
            
            _logger?.LogInformation("🗑️ Starting validation-based cleanup with mode: {Mode}", options.DeleteMode);
            
            var result = await _dataGrid.DeleteRowsWithValidationAsync(criteria, deletionOptions);
            
            if (result.IsSuccess)
            {
                var deletionResult = result.Value;
                var report = new CleanupReport
                {
                    Success = true,
                    TotalRowsEvaluated = deletionResult.TotalRowsEvaluated,
                    RowsDeleted = deletionResult.RowsDeleted,
                    RemainingRows = deletionResult.RemainingRows,
                    OperationDuration = deletionResult.OperationDuration,
                    CleanupTime = DateTime.UtcNow
                };
                
                _logger?.LogInformation("✅ Cleanup completed in {Duration}ms. " +
                    "Deleted {DeletedRows} invalid records, {RemainingRows} records remain",
                    deletionResult.OperationDuration.TotalMilliseconds,
                    deletionResult.RowsDeleted,
                    deletionResult.RemainingRows);
                
                return report;
            }
            else
            {
                _logger?.LogError("❌ Cleanup failed: {Error}", result.Error);
                return new CleanupReport 
                { 
                    Success = false, 
                    ErrorMessage = result.Error,
                    CleanupTime = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Cleanup operation failed");
            return new CleanupReport 
            { 
                Success = false, 
                ErrorMessage = ex.Message,
                CleanupTime = DateTime.UtcNow
            };
        }
    }
    
    #endregion
    
    #region Export and Reporting - Step 5: Data Export
    
    /// <summary>
    /// STEP 5: Export employee data to various formats
    /// ENTERPRISE: Multiple export formats with filtering and customization
    /// </summary>
    public async Task<bool> ExportEmployeeDataAsync(EmployeeExportOptions options)
    {
        if (_dataGrid == null) return false;
        
        try
        {
            var progressReporter = new Progress<ExportProgress>(progress =>
            {
                var percentage = Math.Round(progress.PercentageComplete, 1);
                _logger?.LogInformation("📤 Export Progress: {Percentage}% - {ProcessedRows}/{TotalRows} rows processed",
                    percentage, progress.ProcessedRows, progress.TotalRows);
            });
            
            _logger?.LogInformation("📤 Starting employee data export to {Format}", options.Format);
            
            switch (options.Format)
            {
                case ExportFormat.Excel:
                    return await ExportToExcelAsync(options, progressReporter);
                    
                case ExportFormat.Csv:
                    return await ExportToCsvAsync(options, progressReporter);
                    
                case ExportFormat.Json:
                    return await ExportToJsonAsync(options, progressReporter);
                    
                case ExportFormat.Pdf:
                    return await ExportToPdfReportAsync(options, progressReporter);
                    
                default:
                    _logger?.LogError("❌ Unsupported export format: {Format}", options.Format);
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Export operation failed");
            return false;
        }
    }
    
    /// <summary>
    /// EXCEL EXPORT: Professional Excel export with formatting
    /// ENTERPRISE: Rich Excel output with charts and formatting
    /// </summary>
    private async Task<bool> ExportToExcelAsync(EmployeeExportOptions options, IProgress<ExportProgress> progress)
    {
        try
        {
            // EXPORT DATA FROM DATAGRID
            var result = await _dataGrid!.ExportToDictionaryAsync(
                includeValidAlerts: options.IncludeValidationErrors,
                exportOnlyChecked: options.ExportOnlySelected,
                exportOnlyFiltered: options.ExportOnlyFiltered,
                removeAfter: false,
                timeout: TimeSpan.FromMinutes(10),
                exportProgress: progress);
            
            if (!result.IsSuccess || result.Value == null)
            {
                _logger?.LogError("❌ Failed to export data from DataGrid: {Error}", result.Error);
                return false;
            }
            
            var employees = result.Value;
            
            // CREATE EXCEL FILE WITH FORMATTING
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Employees");
            
            // ADD HEADERS WITH FORMATTING
            var headers = new[] 
            {
                "Employee ID", "First Name", "Last Name", "Middle Name", "Email", "Phone",
                "Hire Date", "Last Review", "Salary", "Hourly Rate", "Vacation Days", "Sick Days",
                "Department", "Position", "Manager", "Active", "Full Time", "Benefits",
                "Address", "City", "State", "ZIP Code", "Notes"
            };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
            }
            
            // ADD DATA ROWS
            for (int row = 0; row < employees.Count; row++)
            {
                var employee = employees[row];
                var excelRow = row + 2; // Start from row 2 (after headers)
                
                worksheet.Cells[excelRow, 1].Value = employee.GetValueOrDefault("EmployeeId");
                worksheet.Cells[excelRow, 2].Value = employee.GetValueOrDefault("FirstName");
                worksheet.Cells[excelRow, 3].Value = employee.GetValueOrDefault("LastName");
                worksheet.Cells[excelRow, 4].Value = employee.GetValueOrDefault("MiddleName");
                worksheet.Cells[excelRow, 5].Value = employee.GetValueOrDefault("Email");
                worksheet.Cells[excelRow, 6].Value = employee.GetValueOrDefault("Phone");
                worksheet.Cells[excelRow, 7].Value = employee.GetValueOrDefault("HireDate");
                worksheet.Cells[excelRow, 8].Value = employee.GetValueOrDefault("LastReviewDate");
                worksheet.Cells[excelRow, 9].Value = employee.GetValueOrDefault("Salary");
                worksheet.Cells[excelRow, 10].Value = employee.GetValueOrDefault("HourlyRate");
                worksheet.Cells[excelRow, 11].Value = employee.GetValueOrDefault("VacationDays");
                worksheet.Cells[excelRow, 12].Value = employee.GetValueOrDefault("SickDays");
                worksheet.Cells[excelRow, 13].Value = employee.GetValueOrDefault("Department");
                worksheet.Cells[excelRow, 14].Value = employee.GetValueOrDefault("Position");
                worksheet.Cells[excelRow, 15].Value = employee.GetValueOrDefault("Manager");
                worksheet.Cells[excelRow, 16].Value = employee.GetValueOrDefault("IsActive");
                worksheet.Cells[excelRow, 17].Value = employee.GetValueOrDefault("IsFullTime");
                worksheet.Cells[excelRow, 18].Value = employee.GetValueOrDefault("HasBenefits");
                worksheet.Cells[excelRow, 19].Value = employee.GetValueOrDefault("Address");
                worksheet.Cells[excelRow, 20].Value = employee.GetValueOrDefault("City");
                worksheet.Cells[excelRow, 21].Value = employee.GetValueOrDefault("State");
                worksheet.Cells[excelRow, 22].Value = employee.GetValueOrDefault("ZipCode");
                worksheet.Cells[excelRow, 23].Value = employee.GetValueOrDefault("Notes");
            }
            
            // FORMAT COLUMNS
            worksheet.Cells[2, 7, employees.Count + 1, 8].Style.Numberformat.Format = "mm/dd/yyyy"; // Dates
            worksheet.Cells[2, 9, employees.Count + 1, 10].Style.Numberformat.Format = "$#,##0.00"; // Currency
            
            // AUTO-FIT COLUMNS
            worksheet.Cells.AutoFitColumns();
            
            // ADD SUMMARY SHEET
            if (options.IncludeSummary)
            {
                await AddEmployeeSummarySheetAsync(package, employees);
            }
            
            // SAVE FILE
            var fileInfo = new FileInfo(options.FilePath ?? $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            await package.SaveAsAsync(fileInfo);
            
            _logger?.LogInformation("✅ Excel export completed: {FilePath} ({RowCount} employees)", 
                fileInfo.FullName, employees.Count);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "💥 Excel export failed");
            return false;
        }
    }
    
    #endregion
}

#region Supporting Classes and Enums

/// <summary>
/// Employee search configuration options
/// </summary>
public class EmployeeSearchOptions
{
    public List<string>? SearchColumns { get; set; }
    public bool CaseSensitive { get; set; } = false;
    public bool UseRegex { get; set; } = false;
    public bool WholeWordOnly { get; set; } = false;
    public int MaxResults { get; set; } = 10000;
}

/// <summary>
/// Employee filter criteria for advanced filtering
/// </summary>
public class EmployeeFilterCriteria
{
    public string? Department { get; set; }
    public bool ActiveOnly { get; set; } = true;
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public DateTime? HiredAfter { get; set; }
    public bool FullTimeOnly { get; set; } = false;
}

/// <summary>
/// Validation report with detailed statistics
/// </summary>
public class ValidationReport
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalErrors { get; set; }
    public Dictionary<ValidationLevel, int> ErrorsByType { get; set; } = new();
    public List<ValidationError> ErrorDetails { get; set; } = new();
    public DateTime ValidationTime { get; set; }
}

/// <summary>
/// Cleanup options for validation-based data cleanup
/// </summary>
public class ValidationCleanupOptions
{
    public ValidationDeletionMode DeleteMode { get; set; } = ValidationDeletionMode.DeleteInvalidRows;
    public List<ValidationSeverity>? TargetSeverity { get; set; }
    public List<string>? SpecificRules { get; set; }
    public bool RequireConfirmation { get; set; } = true;
    public bool CreateBackup { get; set; } = true;
}

/// <summary>
/// Cleanup operation report
/// </summary>
public class CleanupReport
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalRowsEvaluated { get; set; }
    public int RowsDeleted { get; set; }
    public int RemainingRows { get; set; }
    public TimeSpan OperationDuration { get; set; }
    public DateTime CleanupTime { get; set; }
}

/// <summary>
/// Export configuration options
/// </summary>
public class EmployeeExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.Excel;
    public string? FilePath { get; set; }
    public bool ExportOnlySelected { get; set; } = false;
    public bool ExportOnlyFiltered { get; set; } = false;
    public bool IncludeValidationErrors { get; set; } = false;
    public bool IncludeSummary { get; set; } = true;
}

public enum ExportFormat
{
    Excel,
    Csv,
    Json,
    Pdf
}

/// <summary>
/// Custom validation rules for employee data
/// </summary>
public class EmployeeSalaryValidationRule : IValidationRule
{
    public string RuleName => "EmployeeSalaryValidation";
    public string TargetColumn => "Salary";
    public string ErrorMessage => "Salary must be between $20,000 and $500,000";
    public ValidationLevel Severity => ValidationLevel.Warning;
    
    public async Task<bool> ValidateAsync(Dictionary<string, object?> rowData, int rowIndex, IReadOnlyList<ColumnDefinition> columns)
    {
        if (rowData.TryGetValue("Salary", out var salaryValue) && salaryValue is decimal salary)
        {
            return salary >= 20000 && salary <= 500000;
        }
        return true; // Skip validation if salary is not provided
    }
}

public class EmployeeDateValidationRule : IValidationRule
{
    public string RuleName => "EmployeeDateValidation";
    public string TargetColumn => "HireDate";
    public string ErrorMessage => "Hire date cannot be in the future and last review must be after hire date";
    public ValidationLevel Severity => ValidationLevel.Error;
    
    public async Task<bool> ValidateAsync(Dictionary<string, object?> rowData, int rowIndex, IReadOnlyList<ColumnDefinition> columns)
    {
        var today = DateTime.Today;
        
        // Hire date validation
        if (rowData.TryGetValue("HireDate", out var hireDateValue) && hireDateValue is DateTime hireDate)
        {
            if (hireDate > today) return false; // Future hire date
            
            // Last review date validation
            if (rowData.TryGetValue("LastReviewDate", out var reviewDateValue) && reviewDateValue is DateTime reviewDate)
            {
                if (reviewDate < hireDate) return false; // Review before hire
            }
        }
        
        return true;
    }
}

public class EmployeeDepartmentValidationRule : IValidationRule
{
    public string RuleName => "EmployeeDepartmentValidation";
    public string TargetColumn => "Department";
    public string ErrorMessage => "Department must be valid";
    public ValidationLevel Severity => ValidationLevel.Warning;
    
    private readonly HashSet<string> _validDepartments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Engineering", "Sales", "Marketing", "HR", "Finance", "Operations", "IT", "Customer Service"
    };
    
    public async Task<bool> ValidateAsync(Dictionary<string, object?> rowData, int rowIndex, IReadOnlyList<ColumnDefinition> columns)
    {
        if (rowData.TryGetValue("Department", out var deptValue) && deptValue is string department)
        {
            return string.IsNullOrWhiteSpace(department) || _validDepartments.Contains(department);
        }
        return true;
    }
}

#endregion
```

### 36.2 **Financial Trading System Example**

```csharp
/// <summary>
/// FINANCIAL EXAMPLE: Real-time trading data grid with advanced features
/// PERFORMANCE: Optimized for high-frequency updates and large datasets
/// </summary>
public class TradingDataGrid
{
    private AdvancedWinUiDataGrid? _dataGrid;
    private readonly Timer _priceUpdateTimer;
    private readonly Random _random = new Random();
    
    /// <summary>
    /// Initialize trading grid with real-time price updates
    /// </summary>
    public async Task<bool> InitializeTradingGridAsync()
    {
        _dataGrid = AdvancedWinUiDataGrid.CreateForUI();
        
        var columns = new List<ColumnDefinition>
        {
            ColumnDefinition.Text("Symbol", "Symbol"),
            ColumnDefinition.Text("Company", "Company Name"),
            ColumnDefinition.Numeric<decimal>("Price", "Current Price", "C2"),
            ColumnDefinition.Numeric<decimal>("Change", "Change", "+0.00;-0.00;0.00"),
            ColumnDefinition.Numeric<decimal>("ChangePct", "Change %", "+0.00%;-0.00%;0.00%"),
            ColumnDefinition.Numeric<long>("Volume", "Volume", "#,##0"),
            ColumnDefinition.Numeric<decimal>("High", "Day High", "C2"),
            ColumnDefinition.Numeric<decimal>("Low", "Day Low", "C2"),
            ColumnDefinition.Numeric<decimal>("Open", "Open", "C2"),
            ColumnDefinition.DateTime("LastUpdate", "Last Update", "HH:mm:ss")
        };
        
        var colorConfig = new ColorConfiguration
        {
            // Financial color scheme - green for gains, red for losses
            PositiveChangeBackground = Color.FromArgb(255, 220, 252, 220),
            NegativeChangeBackground = Color.FromArgb(255, 255, 230, 230),
            Theme = ColorTheme.Financial
        };
        
        var performanceConfig = new PerformanceConfiguration
        {
            EnableVirtualization = true,
            OptimizeForRealTimeUpdates = true,
            MaxUIUpdateFrequency = 10 // 10 updates per second max
        };
        
        var result = await _dataGrid.InitializeAsync(columns, colorConfig, performanceConfig);
        
        if (result.IsSuccess)
        {
            // Start real-time price updates
            _priceUpdateTimer = new Timer(UpdatePricesCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Simulate real-time price updates
    /// </summary>
    private async void UpdatePricesCallback(object? state)
    {
        if (_dataGrid == null) return;
        
        // Get current data
        var result = await _dataGrid.ExportToDictionaryAsync();
        if (!result.IsSuccess || result.Value == null) return;
        
        var stocks = result.Value;
        var updates = new List<Dictionary<string, object?>>();
        
        // Update prices for random selection of stocks
        var stocksToUpdate = stocks.OrderBy(x => _random.Next()).Take(Math.Min(10, stocks.Count));
        
        foreach (var stock in stocksToUpdate)
        {
            var currentPrice = (decimal)(stock["Price"] ?? 0m);
            var priceChange = (decimal)(_random.NextDouble() * 2 - 1); // -1 to +1
            var newPrice = Math.Max(0.01m, currentPrice + priceChange);
            
            var updatedStock = new Dictionary<string, object?>(stock)
            {
                ["Price"] = newPrice,
                ["Change"] = newPrice - currentPrice,
                ["ChangePct"] = currentPrice > 0 ? ((newPrice - currentPrice) / currentPrice) * 100 : 0,
                ["LastUpdate"] = DateTime.Now
            };
            
            updates.Add(updatedStock);
        }
        
        // Import updated data
        if (updates.Any())
        {
            await _dataGrid.ImportFromDictionaryAsync(updates, mode: ImportMode.Merge);
        }
    }
}
```