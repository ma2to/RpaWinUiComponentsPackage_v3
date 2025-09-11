# AdvancedWinUiDataGrid - Praktické Príklady a Use Cases

## Komplexné Praktické Príklady pre Enterprise Aplikácie

### Úvod do Praktických Príkladov

Táto sekcia poskytuje **rozsiahle praktické príklady** ako implementovať AdvancedWinUiDataGrid v reálnych enterprise scenároch. Každý príklad obsahuje kompletný kód, vysvetlenie architektúrnych rozhodnutí, a best practices.

---

## 1. ENTERPRISE EMPLOYEE MANAGEMENT SYSTEM

### 1.1 Kompletný Employee Management Príklad

#### **Scenár:** Personálne oddelenie potrebuje spravovať zamestnancov s pokročilými funkciami ako vyhľadávanie, filtrovanie, validáciu, a bulk operácie.

#### **Employee Domain Model:**
```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public DateTime? LastReview { get; set; }
    public int VacationDaysRemaining { get; set; }
}
```

#### **Kompletná Implementácia:**
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Application.Services;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Core;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Configuration;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.DataOperations;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.SearchAndFilter;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.Validation;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Domain.ValueObjects.UI;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.SharedKernel.Results;

namespace EmployeeManagement.Services
{
    /// <summary>
    /// ENTERPRISE: Complete Employee Management System using AdvancedWinUiDataGrid
    /// REAL_WORLD: Production-ready implementation with all features
    /// </summary>
    public class EmployeeManagementService : IDisposable
    {
        #region Private Fields
        
        private readonly IDataGridService _dataGridService;
        private readonly ILogger<EmployeeManagementService> _logger;
        private readonly List<Employee> _employees;
        private bool _disposed = false;
        
        #endregion

        #region Constructor and Initialization

        public EmployeeManagementService(ILogger<EmployeeManagementService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _employees = new List<Employee>();
            
            // Create DataGrid service with UI support
            _dataGridService = DataGridServiceFactory.CreateWithUI(logger);
            
            _logger.LogInformation("EmployeeManagementService initialized");
        }

        /// <summary>
        /// INITIALIZATION: Setup complete employee management grid
        /// </summary>
        public async Task<Result<bool>> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing Employee Management Grid");
                
                // Define comprehensive column structure
                var columns = CreateEmployeeColumns();
                
                // Configure for employee management
                var configuration = CreateEmployeeConfiguration();
                
                // Initialize the DataGrid
                var result = await _dataGridService.InitializeAsync(columns, configuration);
                
                if (result.IsSuccess)
                {
                    // Load sample employee data
                    await LoadSampleEmployeesAsync();
                    _logger.LogInformation("Employee Management Grid initialized successfully");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Employee Management Grid");
                return Result<bool>.Failure("Failed to initialize Employee Management Grid", ex);
            }
        }

        #endregion

        #region Column Configuration

        /// <summary>
        /// ENTERPRISE: Comprehensive column definition for employee management
        /// </summary>
        private IReadOnlyList<ColumnDefinition> CreateEmployeeColumns()
        {
            return new List<ColumnDefinition>
            {
                // ID Column - Auto width, read-only
                ColumnDefinition.Numeric<int>("Id", "Employee ID")
                    .With(column => column with 
                    { 
                        IsReadOnly = true,
                        Width = ColumnWidth.Pixels(80),
                        Alignment = ColumnAlignment.Center
                    }),

                // First Name - Required field with validation
                ColumnDefinition.Required("FirstName", typeof(string), "First Name")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(150, minWidth: 100, maxWidth: 200),
                        ValidationRules = new[]
                        {
                            ColumnValidationRule.Required("First name is required"),
                            ColumnValidationRule.MaxLength(50, "First name cannot exceed 50 characters")
                        }
                    }),

                // Last Name - Required field with validation  
                ColumnDefinition.Required("LastName", typeof(string), "Last Name")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(150, minWidth: 100, maxWidth: 200),
                        ValidationRules = new[]
                        {
                            ColumnValidationRule.Required("Last name is required"),
                            ColumnValidationRule.MaxLength(50, "Last name cannot exceed 50 characters")
                        }
                    }),

                // Email - With email validation
                ColumnDefinition.WithValidation("Email", typeof(string),
                    ColumnValidationRule.Required("Email is required"),
                    ColumnValidationRule.Custom("EmailFormat", 
                        value => value is string email && IsValidEmail(email),
                        "Please enter a valid email address"))
                    .With(column => column with 
                    { 
                        DisplayName = "Email Address",
                        Width = ColumnWidth.Pixels(200, minWidth: 150, maxWidth: 300)
                    }),

                // Department - Dropdown style (implemented as text for now)
                ColumnDefinition.Text("Department")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(120, minWidth: 100, maxWidth: 150),
                        ValidationRules = new[]
                        {
                            ColumnValidationRule.Custom("ValidDepartment",
                                value => IsValidDepartment(value?.ToString()),
                                "Please select a valid department")
                        }
                    }),

                // Position
                ColumnDefinition.Text("Position", "Job Position")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(150, minWidth: 100, maxWidth: 200)
                    }),

                // Manager
                ColumnDefinition.Text("Manager", "Direct Manager")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(150, minWidth: 100, maxWidth: 200)
                    }),

                // Hire Date - Date column with formatting
                ColumnDefinition.DateTime("HireDate", "Hire Date", "yyyy-MM-dd")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(120, minWidth: 100),
                        ValidationRules = new[]
                        {
                            ColumnValidationRule.Custom("ValidHireDate",
                                value => value is DateTime date && date <= DateTime.Today,
                                "Hire date cannot be in the future")
                        }
                    }),

                // Last Review Date - Optional date
                ColumnDefinition.DateTime("LastReview", "Last Review", "yyyy-MM-dd")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(120, minWidth: 100)
                    }),

                // Salary - Numeric with currency formatting
                ColumnDefinition.Numeric<decimal>("Salary", "Annual Salary", "C0")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(120, minWidth: 100, maxWidth: 150),
                        ValidationRules = new[]
                        {
                            ColumnValidationRule.Custom("ValidSalary",
                                value => value is decimal salary && salary >= 0,
                                "Salary must be a positive number")
                        }
                    }),

                // Vacation Days - Numeric
                ColumnDefinition.Numeric<int>("VacationDaysRemaining", "Vacation Days", "N0")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(100, minWidth: 80, maxWidth: 120),
                        Alignment = ColumnAlignment.Center
                    }),

                // Active Status - Checkbox
                ColumnDefinition.CheckBox("IsActive", "Active")
                    .With(column => column with 
                    { 
                        Width = ColumnWidth.Pixels(80, minWidth: 60, maxWidth: 100)
                    }),

                // Validation Alerts - Shows validation errors
                ColumnDefinition.ValidAlerts("Validation Errors", minimumWidth: 200),

                // Actions - Delete row functionality
                ColumnDefinition.DeleteRow("Actions", requireConfirmation: true, 
                    confirmationMessage: "Are you sure you want to delete this employee?")
            };
        }

        #endregion

        #region Configuration Setup

        /// <summary>
        /// CONFIGURATION: Enterprise-grade configuration for employee management
        /// </summary>
        private DataGridConfiguration CreateEmployeeConfiguration()
        {
            // UI Configuration
            var uiConfig = new UIConfiguration
            {
                Colors = new ColorConfiguration
                {
                    HeaderBackgroundColor = Windows.UI.Color.FromArgb(255, 240, 248, 255), // Light blue
                    RowAlternatingBackgroundColor = Windows.UI.Color.FromArgb(255, 248, 250, 252), // Very light gray
                    ValidationErrorBackgroundColor = Windows.UI.Color.FromArgb(255, 255, 240, 240), // Light red
                    SelectionBackgroundColor = Windows.UI.Color.FromArgb(255, 220, 240, 255) // Selection blue
                },
                RowHeight = 32,
                HeaderHeight = 40,
                ShowGridLines = true,
                ShowRowNumbers = true,
                AllowColumnReordering = true,
                AllowColumnResizing = true,
                AllowRowSelection = true,
                SelectionMode = SelectionMode.Multiple
            };

            // Performance Configuration
            var performanceConfig = new PerformanceConfiguration
            {
                VirtualizationThreshold = 100,
                EnableLazyLoading = true,
                CacheSize = 1000,
                EnablePerformanceMonitoring = true
            };

            // Validation Configuration
            var validationConfig = new ValidationConfiguration
            {
                ValidateOnEdit = true,
                ValidateOnImport = true,
                StrictMode = false,
                MaxValidationErrors = 50,
                ShowValidationSummary = true
            };

            return new DataGridConfiguration
            {
                Name = "EmployeeManagementGrid",
                IsReadOnly = false,
                Performance = performanceConfig,
                Validation = validationConfig,
                UI = uiConfig,
                EnableAuditLog = true,
                EnablePerformanceMonitoring = true
            };
        }

        #endregion

        #region Employee CRUD Operations

        /// <summary>
        /// ADD EMPLOYEE: Add new employee with validation
        /// </summary>
        public async Task<Result<bool>> AddEmployeeAsync(Employee employee)
        {
            try
            {
                _logger.LogInformation("Adding new employee: {FirstName} {LastName}", 
                    employee.FirstName, employee.LastName);
                
                // Convert employee to dictionary
                var rowData = EmployeeToDictionary(employee);
                
                // Add to DataGrid
                var result = await _dataGridService.AddRowAsync(rowData);
                
                if (result.IsSuccess)
                {
                    // Add to local collection
                    _employees.Add(employee);
                    _logger.LogInformation("Employee added successfully: {Id}", employee.Id);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add employee: {FirstName} {LastName}", 
                    employee.FirstName, employee.LastName);
                return Result<bool>.Failure("Failed to add employee", ex);
            }
        }

        /// <summary>
        /// UPDATE EMPLOYEE: Update existing employee
        /// </summary>
        public async Task<Result<bool>> UpdateEmployeeAsync(int employeeId, Employee updatedEmployee)
        {
            try
            {
                _logger.LogInformation("Updating employee: {Id}", employeeId);
                
                // Find employee index
                var index = _employees.FindIndex(e => e.Id == employeeId);
                if (index == -1)
                {
                    return Result<bool>.Failure($"Employee with ID {employeeId} not found");
                }
                
                // Convert to dictionary
                var rowData = EmployeeToDictionary(updatedEmployee);
                
                // Update in DataGrid
                var result = await _dataGridService.UpdateRowAsync(index, rowData);
                
                if (result.IsSuccess)
                {
                    // Update local collection
                    _employees[index] = updatedEmployee;
                    _logger.LogInformation("Employee updated successfully: {Id}", employeeId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update employee: {Id}", employeeId);
                return Result<bool>.Failure("Failed to update employee", ex);
            }
        }

        /// <summary>
        /// DELETE EMPLOYEE: Delete employee with confirmation
        /// </summary>
        public async Task<Result<bool>> DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                _logger.LogInformation("Deleting employee: {Id}", employeeId);
                
                // Find employee index
                var index = _employees.FindIndex(e => e.Id == employeeId);
                if (index == -1)
                {
                    return Result<bool>.Failure($"Employee with ID {employeeId} not found");
                }
                
                // Delete from DataGrid
                var result = await _dataGridService.DeleteRowAsync(index);
                
                if (result.IsSuccess)
                {
                    // Remove from local collection
                    _employees.RemoveAt(index);
                    _logger.LogInformation("Employee deleted successfully: {Id}", employeeId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete employee: {Id}", employeeId);
                return Result<bool>.Failure("Failed to delete employee", ex);
            }
        }

        #endregion

        #region Search and Filter Operations

        /// <summary>
        /// SEARCH EMPLOYEES: Advanced search with multiple criteria
        /// </summary>
        public async Task<Result<SearchResult>> SearchEmployeesAsync(
            string searchTerm, 
            string? department = null, 
            bool activeOnly = false)
        {
            try
            {
                _logger.LogInformation("Searching employees: {SearchTerm}, Department: {Department}, ActiveOnly: {ActiveOnly}", 
                    searchTerm, department, activeOnly);
                
                // Create search options
                var searchOptions = new SearchOptions
                {
                    CaseSensitive = false,
                    WholeWordOnly = false,
                    SearchType = SearchType.Contains
                };
                
                // Perform search
                var result = await _dataGridService.SearchAsync(searchTerm, searchOptions);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Search completed: {TotalMatches} matches found", 
                        result.Value.TotalMatches);
                    
                    // Apply additional filters if specified
                    if (!string.IsNullOrEmpty(department))
                    {
                        await _dataGridService.ApplyFilterAsync("Department", department);
                    }
                    
                    if (activeOnly)
                    {
                        await _dataGridService.ApplyFilterAsync("IsActive", true);
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search employees");
                return Result<SearchResult>.Failure("Failed to search employees", ex);
            }
        }

        /// <summary>
        /// FILTER BY DEPARTMENT: Filter employees by department
        /// </summary>
        public async Task<Result<bool>> FilterByDepartmentAsync(string department)
        {
            try
            {
                _logger.LogInformation("Filtering employees by department: {Department}", department);
                
                var result = await _dataGridService.ApplyFilterAsync("Department", department);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Department filter applied successfully");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to filter by department: {Department}", department);
                return Result<bool>.Failure("Failed to filter by department", ex);
            }
        }

        /// <summary>
        /// CLEAR ALL FILTERS: Remove all applied filters
        /// </summary>
        public async Task<Result<bool>> ClearAllFiltersAsync()
        {
            try
            {
                _logger.LogInformation("Clearing all filters");
                
                var result = await _dataGridService.ClearFiltersAsync();
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("All filters cleared successfully");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear filters");
                return Result<bool>.Failure("Failed to clear filters", ex);
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// BULK IMPORT: Import employees from external source
        /// </summary>
        public async Task<Result<ImportResult>> BulkImportEmployeesAsync(List<Employee> employees)
        {
            try
            {
                _logger.LogInformation("Bulk importing {Count} employees", employees.Count);
                
                // Convert employees to dictionaries
                var importData = employees.Select(EmployeeToDictionary).ToList();
                
                // Import data
                var result = await _dataGridService.ImportFromDictionaryAsync(importData, ImportMode.Append);
                
                if (result.IsSuccess)
                {
                    // Update local collection
                    _employees.AddRange(employees);
                    _logger.LogInformation("Bulk import completed: {ImportedRows}/{TotalRows} rows imported", 
                        result.Value.ImportedRows, result.Value.TotalRows);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk import employees");
                return Result<ImportResult>.Failure("Failed to bulk import employees", ex);
            }
        }

        /// <summary>
        /// BULK EXPORT: Export employees to external format
        /// </summary>
        public async Task<Result<List<Dictionary<string, object?>>>> BulkExportEmployeesAsync(bool includeValidationErrors = false)
        {
            try
            {
                _logger.LogInformation("Bulk exporting employees, IncludeValidationErrors: {IncludeValidationErrors}", 
                    includeValidationErrors);
                
                var result = await _dataGridService.ExportToDataTableAsync(includeValidationErrors);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Bulk export completed: {Count} employees exported", 
                        result.Value.Count);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk export employees");
                return Result<List<Dictionary<string, object?>>>.Failure("Failed to bulk export employees", ex);
            }
        }

        #endregion

        #region Clipboard Operations

        /// <summary>
        /// COPY SELECTED: Copy selected employees to clipboard
        /// </summary>
        public async Task<Result<bool>> CopySelectedEmployeesToClipboardAsync(IReadOnlyList<int> selectedRowIndices)
        {
            try
            {
                _logger.LogInformation("Copying {Count} selected employees to clipboard", selectedRowIndices.Count);
                
                var result = await _dataGridService.CopySelectedRowsAsync(selectedRowIndices);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Selected employees copied to clipboard successfully");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy selected employees to clipboard");
                return Result<bool>.Failure("Failed to copy selected employees to clipboard", ex);
            }
        }

        /// <summary>
        /// PASTE FROM CLIPBOARD: Paste employee data from clipboard
        /// </summary>
        public async Task<Result<PasteResult>> PasteEmployeesFromClipboardAsync(int startRowIndex = 0)
        {
            try
            {
                _logger.LogInformation("Pasting employees from clipboard starting at row {StartRowIndex}", startRowIndex);
                
                var result = await _dataGridService.PasteFromClipboardAsync(startRowIndex, 0, false);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Clipboard paste completed: {RowsAdded} rows added, {RowsUpdated} rows updated", 
                        result.Value.RowsAdded, result.Value.RowsUpdated);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to paste employees from clipboard");
                return Result<PasteResult>.Failure("Failed to paste employees from clipboard", ex);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// VALIDATE ALL: Validate all employee data
        /// </summary>
        public async Task<Result<ValidationSummary>> ValidateAllEmployeesAsync()
        {
            try
            {
                _logger.LogInformation("Validating all employee data");
                
                // Get current data for validation
                var exportResult = await _dataGridService.ExportToDataTableAsync(false);
                if (!exportResult.IsSuccess)
                {
                    return Result<ValidationSummary>.Failure("Failed to export data for validation");
                }
                
                var validationErrors = new List<ValidationError>();
                var validRowCount = 0;
                
                // Validate each employee
                for (int i = 0; i < exportResult.Value.Count; i++)
                {
                    var rowData = exportResult.Value[i];
                    var rowErrors = ValidateEmployeeData(rowData, i);
                    
                    if (rowErrors.Any())
                    {
                        validationErrors.AddRange(rowErrors);
                    }
                    else
                    {
                        validRowCount++;
                    }
                }
                
                var summary = new ValidationSummary
                {
                    TotalRows = exportResult.Value.Count,
                    ValidRows = validRowCount,
                    InvalidRows = exportResult.Value.Count - validRowCount,
                    ValidationErrors = validationErrors,
                    IsValid = validationErrors.Count == 0
                };
                
                _logger.LogInformation("Validation completed: {ValidRows}/{TotalRows} valid, {ErrorCount} errors", 
                    summary.ValidRows, summary.TotalRows, summary.ValidationErrors.Count);
                
                return Result<ValidationSummary>.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate all employees");
                return Result<ValidationSummary>.Failure("Failed to validate all employees", ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Convert Employee to Dictionary for DataGrid
        /// </summary>
        private Dictionary<string, object?> EmployeeToDictionary(Employee employee)
        {
            return new Dictionary<string, object?>
            {
                ["Id"] = employee.Id,
                ["FirstName"] = employee.FirstName,
                ["LastName"] = employee.LastName,
                ["Email"] = employee.Email,
                ["Department"] = employee.Department,
                ["Position"] = employee.Position,
                ["Manager"] = employee.Manager,
                ["HireDate"] = employee.HireDate,
                ["LastReview"] = employee.LastReview,
                ["Salary"] = employee.Salary,
                ["VacationDaysRemaining"] = employee.VacationDaysRemaining,
                ["IsActive"] = employee.IsActive
            };
        }

        /// <summary>
        /// Convert Dictionary to Employee
        /// </summary>
        private Employee DictionaryToEmployee(Dictionary<string, object?> dict)
        {
            return new Employee
            {
                Id = Convert.ToInt32(dict.GetValueOrDefault("Id", 0)),
                FirstName = dict.GetValueOrDefault("FirstName")?.ToString() ?? string.Empty,
                LastName = dict.GetValueOrDefault("LastName")?.ToString() ?? string.Empty,
                Email = dict.GetValueOrDefault("Email")?.ToString() ?? string.Empty,
                Department = dict.GetValueOrDefault("Department")?.ToString() ?? string.Empty,
                Position = dict.GetValueOrDefault("Position")?.ToString() ?? string.Empty,
                Manager = dict.GetValueOrDefault("Manager")?.ToString() ?? string.Empty,
                HireDate = Convert.ToDateTime(dict.GetValueOrDefault("HireDate", DateTime.Today)),
                LastReview = dict.GetValueOrDefault("LastReview") as DateTime?,
                Salary = Convert.ToDecimal(dict.GetValueOrDefault("Salary", 0m)),
                VacationDaysRemaining = Convert.ToInt32(dict.GetValueOrDefault("VacationDaysRemaining", 0)),
                IsActive = Convert.ToBoolean(dict.GetValueOrDefault("IsActive", true))
            };
        }

        /// <summary>
        /// Load sample employee data for demonstration
        /// </summary>
        private async Task LoadSampleEmployeesAsync()
        {
            var sampleEmployees = new List<Employee>
            {
                new Employee
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@company.com",
                    Department = "Engineering",
                    Position = "Senior Developer",
                    Manager = "Jane Wilson",
                    HireDate = new DateTime(2020, 3, 15),
                    LastReview = new DateTime(2023, 3, 15),
                    Salary = 85000m,
                    VacationDaysRemaining = 15,
                    IsActive = true
                },
                new Employee
                {
                    Id = 2,
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@company.com",
                    Department = "Marketing",
                    Position = "Marketing Manager",
                    Manager = "Mike Brown",
                    HireDate = new DateTime(2019, 7, 1),
                    LastReview = new DateTime(2023, 7, 1),
                    Salary = 75000m,
                    VacationDaysRemaining = 22,
                    IsActive = true
                },
                new Employee
                {
                    Id = 3,
                    FirstName = "Robert",
                    LastName = "Davis",
                    Email = "robert.davis@company.com",
                    Department = "Sales",
                    Position = "Sales Representative",
                    Manager = "Lisa Anderson",
                    HireDate = new DateTime(2021, 1, 10),
                    LastReview = new DateTime(2023, 1, 10),
                    Salary = 55000m,
                    VacationDaysRemaining = 18,
                    IsActive = true
                },
                new Employee
                {
                    Id = 4,
                    FirstName = "Emily",
                    LastName = "Wilson",
                    Email = "emily.wilson@company.com",
                    Department = "HR",
                    Position = "HR Specialist",
                    Manager = "David Miller",
                    HireDate = new DateTime(2022, 5, 20),
                    Salary = 60000m,
                    VacationDaysRemaining = 25,
                    IsActive = true
                },
                new Employee
                {
                    Id = 5,
                    FirstName = "Michael",
                    LastName = "Brown",
                    Email = "michael.brown@company.com",
                    Department = "Finance",
                    Position = "Financial Analyst",
                    Manager = "Susan Taylor",
                    HireDate = new DateTime(2018, 11, 5),
                    LastReview = new DateTime(2023, 11, 5),
                    Salary = 70000m,
                    VacationDaysRemaining = 20,
                    IsActive = false // Inactive employee
                }
            };
            
            await BulkImportEmployeesAsync(sampleEmployees);
        }

        /// <summary>
        /// Validate employee data
        /// </summary>
        private List<ValidationError> ValidateEmployeeData(Dictionary<string, object?> rowData, int rowIndex)
        {
            var errors = new List<ValidationError>();
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(rowData.GetValueOrDefault("FirstName")?.ToString()))
            {
                errors.Add(ValidationError.Create("FirstName", "First name is required", rowData.GetValueOrDefault("FirstName"), rowIndex));
            }
            
            if (string.IsNullOrWhiteSpace(rowData.GetValueOrDefault("LastName")?.ToString()))
            {
                errors.Add(ValidationError.Create("LastName", "Last name is required", rowData.GetValueOrDefault("LastName"), rowIndex));
            }
            
            // Validate email
            var email = rowData.GetValueOrDefault("Email")?.ToString();
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                errors.Add(ValidationError.Create("Email", "Valid email address is required", email, rowIndex));
            }
            
            // Validate department
            var department = rowData.GetValueOrDefault("Department")?.ToString();
            if (!IsValidDepartment(department))
            {
                errors.Add(ValidationError.Create("Department", "Please select a valid department", department, rowIndex));
            }
            
            // Validate salary
            if (rowData.TryGetValue("Salary", out var salaryObj) && salaryObj != null)
            {
                if (decimal.TryParse(salaryObj.ToString(), out var salary) && salary < 0)
                {
                    errors.Add(ValidationError.Create("Salary", "Salary must be a positive number", salary, rowIndex));
                }
            }
            
            return errors;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate department
        /// </summary>
        private bool IsValidDepartment(string? department)
        {
            var validDepartments = new[] { "Engineering", "Marketing", "Sales", "HR", "Finance", "Operations", "IT" };
            return !string.IsNullOrWhiteSpace(department) && validDepartments.Contains(department);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            if (!_disposed)
            {
                _dataGridService?.Dispose();
                _disposed = true;
                _logger.LogInformation("EmployeeManagementService disposed");
            }
        }

        #endregion
    }

    /// <summary>
    /// Validation Summary Result
    /// </summary>
    public class ValidationSummary
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new();
        public bool IsValid { get; set; }
    }
}
```

---

## 2. FINANCIAL TRADING SYSTEM EXAMPLE

### 2.1 Real-Time Trading Dashboard

#### **Scenár:** Trading desk potrebuje real-time dashboard pre monitoring obchodovania s pokročilými funkciami ako price alerts, performance tracking, a risk management.

#### **Trading Position Model:**
```csharp
public class TradingPosition
{
    public string Symbol { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal BuyPrice { get; set; }
    public int Quantity { get; set; }
    public decimal MarketValue => CurrentPrice * Quantity;
    public decimal TotalCost => BuyPrice * Quantity;
    public decimal UnrealizedPnL => MarketValue - TotalCost;
    public decimal UnrealizedPnLPercent => TotalCost != 0 ? (UnrealizedPnL / TotalCost) * 100 : 0;
    public DateTime PurchaseDate { get; set; }
    public string Sector { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public bool HasAlert { get; set; }
    public decimal? AlertPrice { get; set; }
}

public enum RiskLevel
{
    Low,
    Medium, 
    High,
    Critical
}
```

#### **Trading Dashboard Service:**
```csharp
/// <summary>
/// ENTERPRISE: Real-time trading dashboard using AdvancedWinUiDataGrid
/// REAL_TIME: Live market data integration with performance monitoring
/// </summary>
public class TradingDashboardService : IDisposable
{
    private readonly IDataGridService _dataGridService;
    private readonly ILogger<TradingDashboardService> _logger;
    private readonly Timer _marketDataTimer;
    private readonly List<TradingPosition> _positions;
    private readonly Random _marketSimulator;
    private bool _disposed = false;

    public TradingDashboardService(ILogger<TradingDashboardService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _positions = new List<TradingPosition>();
        _marketSimulator = new Random();
        
        // Create high-performance DataGrid service
        _dataGridService = DataGridServiceFactory.CreateWithUI(logger);
        
        // Setup market data timer (simulate real-time updates)
        _marketDataTimer = new Timer(UpdateMarketData, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        
        _logger.LogInformation("TradingDashboardService initialized");
    }

    /// <summary>
    /// Initialize trading dashboard with real-time columns
    /// </summary>
    public async Task<Result<bool>> InitializeAsync()
    {
        try
        {
            var columns = CreateTradingColumns();
            var configuration = CreateTradingConfiguration();
            
            var result = await _dataGridService.InitializeAsync(columns, configuration);
            
            if (result.IsSuccess)
            {
                await LoadSamplePositionsAsync();
                _logger.LogInformation("Trading Dashboard initialized successfully");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Trading Dashboard");
            return Result<bool>.Failure("Failed to initialize Trading Dashboard", ex);
        }
    }

    /// <summary>
    /// Create advanced trading columns with real-time formatting
    /// </summary>
    private IReadOnlyList<ColumnDefinition> CreateTradingColumns()
    {
        return new List<ColumnDefinition>
        {
            // Symbol - Stock ticker
            ColumnDefinition.Text("Symbol", "Ticker")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(80, minWidth: 60, maxWidth: 100),
                    IsReadOnly = true,
                    Alignment = ColumnAlignment.Center
                }),

            // Company name
            ColumnDefinition.Text("Company", "Company Name")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(200, minWidth: 150, maxWidth: 300)
                }),

            // Current Price - Real-time updating
            ColumnDefinition.Numeric<decimal>("CurrentPrice", "Current Price", "C2")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(100, minWidth: 80, maxWidth: 120),
                    IsReadOnly = true // Updated automatically
                }),

            // Buy Price - Historical
            ColumnDefinition.Numeric<decimal>("BuyPrice", "Buy Price", "C2")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(100, minWidth: 80, maxWidth: 120)
                }),

            // Quantity
            ColumnDefinition.Numeric<int>("Quantity", "Qty", "N0")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(80, minWidth: 60, maxWidth: 100),
                    Alignment = ColumnAlignment.Center
                }),

            // Market Value - Calculated field
            ColumnDefinition.Numeric<decimal>("MarketValue", "Market Value", "C0")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(120, minWidth: 100, maxWidth: 150),
                    IsReadOnly = true
                }),

            // Unrealized P&L - Critical metric
            ColumnDefinition.Numeric<decimal>("UnrealizedPnL", "Unrealized P&L", "C0")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(120, minWidth: 100, maxWidth: 150),
                    IsReadOnly = true
                }),

            // Unrealized P&L Percentage
            ColumnDefinition.Numeric<decimal>("UnrealizedPnLPercent", "P&L %", "P2")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(80, minWidth: 60, maxWidth: 100),
                    IsReadOnly = true,
                    Alignment = ColumnAlignment.Center
                }),

            // Purchase Date
            ColumnDefinition.DateTime("PurchaseDate", "Purchase Date", "MM/dd/yyyy")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(100, minWidth: 80, maxWidth: 120)
                }),

            // Sector
            ColumnDefinition.Text("Sector")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(100, minWidth: 80, maxWidth: 150)
                }),

            // Risk Level - Enum dropdown
            ColumnDefinition.Text("RiskLevel", "Risk")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(80, minWidth: 60, maxWidth: 100),
                    ValidationRules = new[]
                    {
                        ColumnValidationRule.Custom("ValidRiskLevel",
                            value => Enum.TryParse<RiskLevel>(value?.ToString(), out _),
                            "Please select a valid risk level")
                    }
                }),

            // Alert Status
            ColumnDefinition.CheckBox("HasAlert", "Alert")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(60, minWidth: 50, maxWidth: 80)
                }),

            // Alert Price
            ColumnDefinition.Numeric<decimal?>("AlertPrice", "Alert Price", "C2")
                .With(column => column with 
                { 
                    Width = ColumnWidth.Pixels(100, minWidth: 80, maxWidth: 120)
                }),

            // Actions
            ColumnDefinition.DeleteRow("Actions", requireConfirmation: true,
                confirmationMessage: "Are you sure you want to close this position?")
        };
    }

    /// <summary>
    /// High-performance configuration for trading
    /// </summary>
    private DataGridConfiguration CreateTradingConfiguration()
    {
        var uiConfig = new UIConfiguration
        {
            Colors = new ColorConfiguration
            {
                HeaderBackgroundColor = Windows.UI.Color.FromArgb(255, 25, 25, 112), // Navy blue
                RowAlternatingBackgroundColor = Windows.UI.Color.FromArgb(255, 248, 248, 255), // Ghost white
                ValidationErrorBackgroundColor = Windows.UI.Color.FromArgb(255, 255, 182, 193), // Light pink
                SelectionBackgroundColor = Windows.UI.Color.FromArgb(255, 173, 216, 230) // Light blue
            },
            RowHeight = 28, // Compact rows for more data
            HeaderHeight = 35,
            ShowGridLines = true,
            ShowRowNumbers = false, // Not needed for trading
            AllowColumnReordering = true,
            AllowColumnResizing = true,
            AllowRowSelection = true,
            SelectionMode = SelectionMode.Multiple
        };

        var performanceConfig = new PerformanceConfiguration
        {
            VirtualizationThreshold = 50, // Enable virtualization sooner
            EnableLazyLoading = true,
            CacheSize = 2000, // Large cache for frequent updates
            EnablePerformanceMonitoring = true
        };

        return new DataGridConfiguration
        {
            Name = "TradingDashboard",
            IsReadOnly = false,
            Performance = performanceConfig,
            UI = uiConfig,
            EnableAuditLog = true, // Important for trading compliance
            EnablePerformanceMonitoring = true
        };
    }

    /// <summary>
    /// Real-time market data updates
    /// </summary>
    private async void UpdateMarketData(object? state)
    {
        try
        {
            if (_disposed || !_positions.Any()) return;

            _logger.LogDebug("Updating market data for {Count} positions", _positions.Count);

            // Simulate market price changes
            for (int i = 0; i < _positions.Count; i++)
            {
                var position = _positions[i];
                
                // Simulate price movement (+/- 5%)
                var priceChange = (decimal)(_marketSimulator.NextDouble() * 0.1 - 0.05);
                position.CurrentPrice *= (1 + priceChange);
                position.CurrentPrice = Math.Max(0.01m, Math.Round(position.CurrentPrice, 2));
                
                // Update calculated fields
                // MarketValue, UnrealizedPnL, UnrealizedPnLPercent are calculated properties
                
                // Check for price alerts
                if (position.HasAlert && position.AlertPrice.HasValue)
                {
                    if (position.CurrentPrice >= position.AlertPrice.Value)
                    {
                        _logger.LogWarning("Price alert triggered for {Symbol}: Current {CurrentPrice}, Alert {AlertPrice}",
                            position.Symbol, position.CurrentPrice, position.AlertPrice.Value);
                        // In real application, would send notification
                    }
                }

                // Update the row in DataGrid
                var rowData = PositionToDictionary(position);
                await _dataGridService.UpdateRowAsync(i, rowData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating market data");
        }
    }

    /// <summary>
    /// Load sample trading positions
    /// </summary>
    private async Task LoadSamplePositionsAsync()
    {
        var samplePositions = new List<TradingPosition>
        {
            new TradingPosition
            {
                Symbol = "AAPL",
                Company = "Apple Inc.",
                CurrentPrice = 150.25m,
                BuyPrice = 145.00m,
                Quantity = 100,
                PurchaseDate = new DateTime(2023, 1, 15),
                Sector = "Technology",
                RiskLevel = RiskLevel.Low,
                HasAlert = true,
                AlertPrice = 160.00m
            },
            new TradingPosition
            {
                Symbol = "MSFT",
                Company = "Microsoft Corporation",
                CurrentPrice = 280.50m,
                BuyPrice = 275.25m,
                Quantity = 50,
                PurchaseDate = new DateTime(2023, 2, 10),
                Sector = "Technology",
                RiskLevel = RiskLevel.Low,
                HasAlert = false
            },
            new TradingPosition
            {
                Symbol = "TSLA",
                Company = "Tesla Inc.",
                CurrentPrice = 220.75m,
                BuyPrice = 250.00m,
                Quantity = 25,
                PurchaseDate = new DateTime(2023, 3, 5),
                Sector = "Automotive",
                RiskLevel = RiskLevel.High,
                HasAlert = true,
                AlertPrice = 200.00m
            },
            new TradingPosition
            {
                Symbol = "AMZN",
                Company = "Amazon.com Inc.",
                CurrentPrice = 125.30m,
                BuyPrice = 120.00m,
                Quantity = 40,
                PurchaseDate = new DateTime(2023, 1, 25),
                Sector = "E-commerce",
                RiskLevel = RiskLevel.Medium,
                HasAlert = false
            },
            new TradingPosition
            {
                Symbol = "GOOGL",
                Company = "Alphabet Inc.",
                CurrentPrice = 105.80m,
                BuyPrice = 110.50m,
                Quantity = 30,
                PurchaseDate = new DateTime(2023, 2, 20),
                Sector = "Technology",
                RiskLevel = RiskLevel.Medium,
                HasAlert = true,
                AlertPrice = 115.00m
            }
        };

        _positions.AddRange(samplePositions);
        var importData = samplePositions.Select(PositionToDictionary).ToList();
        await _dataGridService.ImportFromDictionaryAsync(importData, ImportMode.Replace);

        _logger.LogInformation("Loaded {Count} sample trading positions", samplePositions.Count);
    }

    /// <summary>
    /// Convert trading position to dictionary
    /// </summary>
    private Dictionary<string, object?> PositionToDictionary(TradingPosition position)
    {
        return new Dictionary<string, object?>
        {
            ["Symbol"] = position.Symbol,
            ["Company"] = position.Company,
            ["CurrentPrice"] = position.CurrentPrice,
            ["BuyPrice"] = position.BuyPrice,
            ["Quantity"] = position.Quantity,
            ["MarketValue"] = position.MarketValue,
            ["UnrealizedPnL"] = position.UnrealizedPnL,
            ["UnrealizedPnLPercent"] = position.UnrealizedPnLPercent,
            ["PurchaseDate"] = position.PurchaseDate,
            ["Sector"] = position.Sector,
            ["RiskLevel"] = position.RiskLevel.ToString(),
            ["HasAlert"] = position.HasAlert,
            ["AlertPrice"] = position.AlertPrice
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _marketDataTimer?.Dispose();
            _dataGridService?.Dispose();
            _disposed = true;
            _logger.LogInformation("TradingDashboardService disposed");
        }
    }
}
```

---

Táto dokumentácia pokračuje s ďalšími príkladmi... Mám pokračovať s vytvorením ešte viacerých praktických príkladov ako Inventory Management, Customer CRM, Project Management, atď?