# 🏗️ SYSTEM ARCHITECTURE

## Overview
Vietnam Food Guide follows a **Layered Architecture** pattern separating concerns into distinct layers.

```
┌──────────────────────────────────────┐
│      Presentation Layer (UI)         │
│    Views, Windows, Controls          │
└────────────────┬─────────────────────┘
                 │
┌────────────────▼─────────────────────┐
│    Business Logic Layer (Services)   │
│  FoodService, SpeechService, etc     │
└────────────────┬─────────────────────┘
                 │
┌────────────────▼─────────────────────┐
│      Data Layer (Models, Storage)    │
│    FoodItem, StorageService          │
└──────────────────────────────────────┘
```

## Component Responsibilities

### 1. Presentation Layer
- **Views/** - XAML windows and UI controls
- **Purpose**: Display data, handle user interaction
- **Technology**: WPF, XAML
- **Should NOT contain**: Business logic, data access

### 2. Business Logic Layer
- **Services/** - Core business operations
  - IFoodService, FoodService - Food data operations
  - ISpeechService, SpeechService - Text-to-speech operations
  - StorageService - Local persistence
  - LanguageService - Multi-language support
  - MapService - Maps operations
- **Purpose**: Implement business rules, data manipulation
- **Should NOT contain**: UI code, view models

### 3. Data Layer
- **Models/** - Data structures and entities
  - Entities - Business entities (FoodItem)
  - DTOs - Data transfer objects
- **Purpose**: Represent data, basic validation
- **Should NOT contain**: Business logic, service calls

## Data Flow

```
User Input (MainWindow)
        ↓
View Event Handler
        ↓
Service Method Call (e.g., SearchAsync)
        ↓
Business Logic Processing
        ↓
Model Operations
        ↓
Return Result to View
        ↓
UI Update
```

## Design Patterns Used

### 1. **Service Layer Pattern**
Services encapsulate business logic and are injected into views.

```csharp
public class MainWindow
{
    private readonly IFoodService _foodService;

    public MainWindow(IFoodService foodService)
    {
        _foodService = foodService;
    }

    private void LoadFoods()
    {
        var foods = _foodService.LoadFoods();
        DisplayFoods(foods);
    }
}
```

### 2. **Dependency Injection**
Dependencies are provided (injected) rather than created internally.

### 3. **Repository Pattern**
FoodService acts as repository abstracting data access.

### 4. **Adapter Pattern**
SpeechService adapts Google TTS API to application needs.

### 5. **Facade Pattern**
StorageService provides simple interface to complex storage operations.

## Technology Stack

```
Frontend (Presentation):
├── WPF (Windows Presentation Foundation)
├── XAML (XML-based UI markup)
└── C# (Code-behind)

Business Logic:
├── C# Services
├── LINQ for queries
└── Async/await for I/O

Data & Persistence:
├── JSON (foods.json)
├── Local file storage
└── NAudio for audio

External Services:
├── Google Maps API (via WebView2)
├── Google Translate TTS API
└── .NET Framework 4.8

Tools:
├── Visual Studio 2019+
├── NuGet (package management)
└── Git (version control)
```

## Key Interfaces

All services implement interfaces for better testability and decoupling.

```csharp
// Services expose contracts via interfaces
public interface IFoodService
{
    List<FoodItem> LoadFoods();
    List<FoodItem> SearchByKeyword(string keyword);
    // ...
}

// Views depend on abstractions, not concrete implementations
public class MainWindow
{
    private readonly IFoodService _service;
    // Can swap implementation without changing view
}
```

## Thread Safety

- **Main Thread**: UI updates must happen on main thread
- **Background Threads**: Long operations (network, I/O) on background
- **Async/Await**: Used for non-blocking operations

```csharp
// Safe async pattern
public async void LoadFoodsAsync()
{
    var foods = await Task.Run(() => _service.LoadFoods());
    // Back on main thread - safe to update UI
    FoodList.ItemsSource = foods;
}
```

## Error Handling Strategy

```
Level 1: Validation (Helpers)
    ↓ Prevent invalid data
Level 2: Business Logic (Services)
    ↓ Handle domain errors
Level 3: Presentation (Views)
    ↓ Show user-friendly messages
```

## Performance Considerations

1. **Caching** - Cache food data after loading
2. **Lazy Loading** - Load images on demand
3. **Async Operations** - Don't block UI thread
4. **Collection Efficiency** - Use LINQ carefully
5. **Memory Management** - Dispose resources properly

## Security

- **Input Validation** - All user input validated
- **API Calls** - HTTPS for external services
- **Local Storage** - Store non-sensitive data only
- **Error Messages** - Don't expose system details

## Scalability

### Current: Single-tier (Client-side)
- All data in JSON file
- All logic on client

### Future: Multi-tier
- Add ASP.NET backend
- Database for persistent storage
- Cache layer for performance
- API gateway for security

```
Web Client ↔ API Gateway ↔ ASP.NET Backend ↔ Database
```

## Testing Architecture

```
Views → ViewModels/Services → Models
 ↕         ↕                  ↕
Tests  Unit Tests        Unit Tests
```

- **Unit Tests**: Service and Model logic
- **Integration Tests**: Service + storage
- **UI Tests**: View interactions (future)
