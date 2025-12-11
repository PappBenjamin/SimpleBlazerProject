# JSON Data Manager - Complete Code Explanation

## 📋 Overview
This is a Blazor Server application that allows you to:
1. Upload JSON files
2. View data in a table (hiding image columns)
3. Search across all fields
4. View detailed item information (with image previews)
5. Edit items and save changes
6. Delete items with confirmation

---

## 🏗️ Architecture & How Data Flows

```
┌─────────────────────────────────────────────────────────┐
│          USER UPLOADS JSON FILE                         │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────────┐
        │  FileReaderService         │
        │  - Reads file stream       │
        │  - Uses IFileReader        │
        │  - Delegates to            │
        │    JsonFileReader          │
        └────────────┬───────────────┘
                     │ Returns: List<string[]>
                     ▼
        ┌────────────────────────────┐
        │  JsonDataList.razor        │
        │  - Converts to Dictionary  │
        │  - Passes to Service       │
        └────────────┬───────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │ DataManagementService          │
        │ (SINGLETON - persists data)    │
        │ - Stores data in memory        │
        │ - Handles CRUD operations      │
        │ - Manages edits/deletes        │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────┐
        │  JsonDataList.razor        │
        │  - Displays table          │
        │  - Shows filtered data     │
        │  - Calls ImageHandling     │
        └────────────────────────────┘
```

---

## 📁 Service Layer

### 1. **DataManagementService.cs** (SINGLETON)
**Purpose**: Central data storage and management

**Key Data Structure**:
```csharp
private List<Dictionary<string, object>> _currentData;
// Each item is a Dictionary where:
// - Key = column name (e.g., "title", "image")
// - Value = cell value (e.g., "Chevrolet", "https://...")
```

**Key Methods**:
- `InitializeData()` - Loads data from uploaded file
- `GetAllData()` - Returns copy of all data
- `GetItemByIndex()` - Gets specific item
- `UpdateItem()` - Modifies an item (used after edit)
- `DeleteItem()` - Removes an item
- `SearchMultipleColumns()` - Searches all fields
- `ExportToJson()` / `ExportToCsv()` - Export data

**Why Singleton?**
- Data persists across page navigation
- When you edit an item and return to the list, data is still there
- If it was Scoped, new instance = lost data

---

[//]: # (### 2. **ImageHandlingService.cs**)

[//]: # (**Purpose**: Detect and validate image URLs)

[//]: # ()
[//]: # (**Key Methods**:)

[//]: # (- `IsImageUrl&#40;url&#41;` - Checks if URL has image extension &#40;.jpg, .png, etc.&#41;)

[//]: # (- `LooksLikeImageUrl&#40;value&#41;` - Detects if a string looks like an image URL)

[//]: # (- `IsImageColumn&#40;columnName&#41;` - Checks if column name suggests it contains images)

[//]: # (  - Looks for: "image", "img", "photo", "avatar", "icon", "logo", etc.)

[//]: # ()
[//]: # (**Used For**:)

[//]: # (- Hiding image columns from table display)

[//]: # (- Showing images in detail modal instead of URLs)

[//]: # (- Showing image preview in edit form)

---

### 2. **FileReaderService.cs**
**Purpose**: Factory pattern for reading different file types

**Flow**:
```
ReadFile(stream, "file.json")
    ↓
Check extension → ".json"
    ↓
Get JsonFileReader from dictionary
    ↓
Call JsonFileReader.ReadFile(stream)
    ↓
Returns: List<string[]>
  [0] = ["image", "title", "start_production", "class"]  // headers
  [1] = ["https://...", "Chevrolet", "1989", "Minivan"]   // row 1
  [2] = ["https://...", "Pontiac", "1997", "Minivan"]     // row 2
```

---

### 3. **JsonFileReader.cs**
**Purpose**: Parses JSON files

**Process**:
1. Read entire JSON file as string
2. Deserialize to `List<Dictionary<string, object>>`
3. **Handles missing fields** - Collects ALL unique keys from all objects
4. Builds rows with empty strings for missing values
5. Returns as List<string[]> (table format)

**Why handle missing fields?**
```json
[
  { "name": "Car1", "image": "url", "class": "Minivan" },
  { "name": "Car2", "class": "Luxury" }  // Missing "image"
]
```
Old code would crash. New code fills with empty string.

---

## 🎨 UI Layer (Blazor Components)

### **JsonDataList.razor** - Main Page (`/json-data`)

#### **Lifecycle**:
```
1. Page loads
   ↓
2. OnInitialized() runs
   - if (allData == null)  // First time only
   - Load from service
   ↓
3. Display upload form
4. User uploads JSON
   ↓
5. HandleFileSelected() runs
   - Read file via FileReaderService
   - Convert to List<Dictionary>
   - Pass to DataManagementService.InitializeData()
   - Update local allData variable
   ↓
6. Display data table with filtered rows
7. User clicks View/Edit/Delete
```

#### **Key Variables**:
```csharp
private List<Dictionary<string, object>> allData;      // Full dataset
private List<Dictionary<string, object>> filteredData;  // Filtered by search
private List<string> columnNames;                        // Column headers
private Dictionary<string, object> selectedItem;        // For modals
private string searchTerm;                              // Search input
```


#### **Data Flow in Table**:
```
allData has 100 items

↓ User searches "Minivan"
↓ FilterData() returns 20 items matching "Minivan"
↓ filteredData = 20 items

↓ Table loops through filteredData
for (item, idx) in filteredData  // idx = 0, 1, 2... 19
{
    actualIndex = allData.IndexOf(item)  // actualIndex = 0, 3, 7... 99
    <button @onclick="() => EditItem(actualIndex)">
}
```

#### **OnParametersSet()**:
```csharp
protected override void OnParametersSet()
{
    filteredData = FilterData();  // Re-filter when search term changes
}
```
This runs whenever `searchTerm` changes, automatically filtering results.

[//]: # (#### **GetDisplayColumns&#40;&#41;**:)

[//]: # (```csharp)

[//]: # (return columnNames.Where&#40;col => !ImageHandlingService.IsImageColumn&#40;col&#41;&#41;;)

[//]: # (```)

[//]: # (Filters out image columns from table display)

[//]: # (- Input: ["image", "title", "start_production", "class"])

[//]: # (- Output: ["title", "start_production", "class"])

[//]: # ()
[//]: # (---)

### **JsonEditItem.razor** - Edit Page (`/json-edit-item/{Index:int}`)

#### **Route Parameter**:
```csharp
[Parameter]
public int Index { get; set; }  // Index passed from navigation
```

#### **Lifecycle**:
```
1. Page loads with Index parameter
   ↓
2. OnInitialized() runs
   - Get item from DataManagementService.GetItemByIndex(Index)
   - Create a copy (don't modify original until save)
   ↓
3. Display form with all fields
   - Image fields show preview
   - Other fields show as text input
   ↓
4. User edits fields
   - @onchange event captures changes
   - UpdateItemValue() updates dictionary
   ↓
5. User clicks "Save Changes"
   - SaveChanges() calls:
     DataManagementService.UpdateItem(Index, item)
   - Navigate back to /json-data
   ↓
6. JsonDataList loads
   - OnInitialized() checks if allData == null
   - It's NOT null (service still has data)
   - Displays updated data
```

#### **UpdateItemValue() Method**:
```csharp
private void UpdateItemValue(string key, string value)
{
    if (item.ContainsKey(key))
    {
        item[key] = value;  // Update in memory
    }
}
```
Called every time user changes a field
Doesn't call service yet - just updates local copy

#### **Form Binding**:
```razor
<input type="text" 
       value="@stringValue"  // Display current value
       @onchange="@((ChangeEventArgs e) => UpdateItemValue(key, e.Value?.ToString() ?? ""))" />
```
Why this instead of `@bind`?
- `@bind` would try to bind to System.Object (crashes)
- `value` + `@onchange` manually handles string conversion
- Works with Dictionary<string, object>

---

## 🔄 Data Persistence Flow

### **Upload → Edit → Save → Display**:

```
1. UPLOAD PHASE
   ┌─────────────────┐
   │ User uploads    │
   │ cars.json       │
   └────────┬────────┘
            │
            ▼
   ┌─────────────────────────────────────┐
   │ FileReaderService reads file        │
   │ Returns: List<string[]>             │
   │ [["image","title",...], ["url","Car1",...]...]
   └────────┬────────────────────────────┘
            │
            ▼
   ┌──────────────────────────────────────────┐
   │ JsonDataList converts to Dictionary      │
   │ [{ "image": "url", "title": "Car1"... }] │
   └────────┬─────────────────────────────────┘
            │
            ▼
   ┌──────────────────────────────────────────┐
   │ DataManagementService.InitializeData()   │
   │ _currentData = [{ "image": "url"... }, ...]
   │ (SINGLETON - stays in memory)            │
   └──────────────────────────────────────────┘

2. EDIT PHASE
   ┌─────────────────────────────────────┐
   │ User clicks Edit on item index 5    │
   │ Navigates to /json-edit-item/5      │
   └────────┬────────────────────────────┘
            │
            ▼
   ┌──────────────────────────────────────────┐
   │ JsonEditItem.OnInitialized()             │
   │ item = Service.GetItemByIndex(5)         │
   │ item = { "image": "url", "title": ... } │
   │ (Creates COPY of item)                   │
   └──────────────────────────────────────────┘

3. EDIT INPUT PHASE
   ┌──────────────────────────────────────┐
   │ User changes: "Car1" → "Car1 Modified"│
   └────────┬─────────────────────────────┘
            │
            ▼
   ┌──────────────────────────────────────────┐
   │ @onchange event fires                    │
   │ UpdateItemValue("title", "Car1 Modified")│
   │ item["title"] = "Car1 Modified"          │
   │ (Updates local copy ONLY)                │
   └──────────────────────────────────────────┘

4. SAVE PHASE
   ┌──────────────────────────────┐
   │ User clicks Save Changes     │
   │ SaveChanges() runs           │
   └────────┬─────────────────────┘
            │
            ▼
   ┌──────────────────────────────────────────┐
   │ DataManagementService.UpdateItem(5, item)│
   │ _currentData[5] = item (MODIFIED!)       │
   │ (Singleton still has all data)           │
   └────────┬─────────────────────────────────┘
            │
            ▼
   ┌──────────────────────────────┐
   │ Navigate to /json-data       │
   └────────┬─────────────────────┘
            │
            ▼
   ┌──────────────────────────────────────────┐
   │ JsonDataList.OnInitialized()             │
   │ if (allData == null) { ... }  // FALSE!  │
   │ allData is ALREADY SET                   │
   │ Shows MODIFIED data in table             │
   │ "Car1 Modified" appears!                 │
   └──────────────────────────────────────────┘
```

---

## 🎯 Key Design Decisions

### **1. Why Singleton for DataManagementService?**
- **Scoped** = New instance per page = Data lost on navigation ❌
- **Singleton** = One instance = Data persists = Edits saved ✅

### **2. Why Copy Item Before Editing?**
```csharp
item = new Dictionary<string, object>(loadedItem);  // Copy!
```
If you don't copy and user cancels:
- Original data modified without saving
- Changes lost but still in service

### **4. Why Use IndexOf for Mapping?**
When user searches for "Minivan" and sees 3 results:
- Display index: 0, 1, 2 (in filtered list)
- Actual index: could be 0, 10, 50 (in full data)
- `allData.IndexOf(item)` finds the real position

---

## 🔍 Search & Filter Flow

```
User types: "Chevrolet" in search box

1. Search input value changes
   ↓
2. @bind-Value="searchTerm" updates searchTerm variable
   ↓
3. OnParametersSet() runs (whenever parameters change)
   ↓
4. filteredData = FilterData()
   ↓
5. FilterData() calls:
   DataManagementService.SearchMultipleColumns(searchTerm)
   ↓
6. Service returns items where ANY field contains "Chevrolet"
   ↓
7. Table re-renders with only filtered rows
```

---

## 📊 Modal System

### **View Details Modal**:
```csharp
@if (selectedItem != null && showViewModal)
{
    // Show all fields
    // Images display as <img> tags
    // Other data as text
}
```

When you click "View":
1. `ViewItem(index)` runs
2. Loads item from service
3. Sets `selectedItem = item`
4. Sets `showViewModal = true`
5. Modal appears on screen
6. Close button sets both to false/null

### **Delete Confirmation Modal**:
```csharp
@if (showDeleteConfirm && deleteIndex >= 0)
{
    // Show confirmation
    // "Cancel" hides it
    // "Delete" calls ConfirmDeleteAction()
}
```

---


### PDF data conversion to json

I used a python script to convert the PDF data into JSON format. The script extracts the relevant fields from the PDF and structures them into a JSON array of objects, each representing a car with its attributes.

