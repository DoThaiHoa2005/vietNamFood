# 📚 API DOCUMENTATION

## Services API Reference

### IFoodService

#### Methods

##### `LoadFoods()`
```csharp
List<FoodItem> LoadFoods()
```
**Description**: Load all foods from data source (foods.json)
**Returns**: List of FoodItem, never null (empty list if no data)
**Exception**: None (returns empty on error)
**Example**:
```csharp
var service = new FoodService();
var foods = service.LoadFoods();
foreach (var food in foods)
{
    Console.WriteLine($"{food.Name} - {food.City}");
}
```

---

##### `SearchByKeyword(string keyword)`
```csharp
List<FoodItem> SearchByKeyword(string keyword)
```
**Description**: Search foods by name or city (case-insensitive)
**Parameters**:
- `keyword` (required): Search term (1-500 characters)

**Returns**: Filtered list ordered by rating
**Exception**: ArgumentNullException if keyword null
**Example**:
```csharp
var results = service.SearchByKeyword("phở");
// Returns: Phở Thìn Lò Đúc, Phở Bát Đá Long Biên, ...
```

---

##### `FilterByCategory(string category)`
```csharp
List<FoodItem> FilterByCategory(string category)
```
**Description**: Filter foods by category
**Parameters**:
- `category`: One of: "Phở", "Bún", "Cơm", "Bánh Mì", "Bánh Khác", "Thức uống"

**Returns**: Filtered list
**Example**:
```csharp
var phos = service.FilterByCategory("Phở");
```

---

### ISpeechService

#### Methods

##### `Speak(string text, string cultureCode)`
```csharp
void Speak(string text, string cultureCode)
```
**Description**: Speak text using text-to-speech (Google TTS)
**Parameters**:
- `text`: Text to speak (max 200 chars, will be truncated)
- `cultureCode`: Language code ("vi-VN", "en-US", "zh-CN")

**Returns**: void (fires OnPlaybackStarted event)
**Exception**: None (errors via OnError event)
**Example**:
```csharp
var speech = new SpeechService();
speech.OnError += (error) => MessageBox.Show(error);
speech.Speak("Chào mừng bạn đến phố ẩm thực Vĩnh Khánh", "vi-VN");
```

---

##### `Stop()`
```csharp
void Stop()
```
**Description**: Stop current speech playback
**Example**:
```csharp
speech.Stop();
```

---

##### Property: `IsPlaying`
```csharp
bool IsPlaying { get; }
```
**Description**: Check if currently playing audio
**Returns**: true if playing, false otherwise

---

#### Events

##### `OnPlaybackStarted`
```csharp
event Action OnPlaybackStarted
```
**Fired**: When audio playback begins

##### `OnPlaybackCompleted`
```csharp
event Action OnPlaybackCompleted
```
**Fired**: When audio playback finishes

##### `OnError`
```csharp
event Action<string> OnError
```
**Fired**: On TTS error (internet issue, API error, etc.)
**Parameter**: Error message

---

### IStorageService

#### Methods

##### `AddFavorite(FoodItem item)`
```csharp
void AddFavorite(FoodItem item)
```
**Description**: Add food to favorites (duplicate check)
**Example**:
```csharp
storage.AddFavorite(foodItem);
```

---

##### `RemoveFavorite(string foodName)`
```csharp
void RemoveFavorite(string foodName)
```
**Description**: Remove food from favorites
**Example**:
```csharp
storage.RemoveFavorite("Phở Thìn Lò Đúc");
```

---

##### `GetFavorites()`
```csharp
List<FoodItem> GetFavorites()
```
**Description**: Load all favorite foods
**Returns**: List of favorite FoodItems, never null

---

##### `IsFavorite(string foodName)`
```csharp
bool IsFavorite(string foodName)
```
**Description**: Check if food is in favorites
**Returns**: true if favorite, false otherwise

---

## Data Models

### FoodItem
```csharp
public class FoodItem
{
    public string Name { get; set; }           // Food name
    public string City { get; set; }           // Location city
    public string Category { get; set; }       // Category
    public string Image { get; set; }          // Image path/URL
    public string DescriptionVI { get; set; } // Vietnamese description
    public string DescriptionEN { get; set; } // English description
    public string DescriptionCN { get; set; } // Chinese description
    public double Latitude { get; set; }      // Map latitude
    public double Longitude { get; set; }     // Map longitude
    public double Rating { get; set; }        // Rating 0-5
}
```

### JSON Format
```json
[
    {
        "Name": "Phở Thìn Lò Đúc",
        "City": "Hà Nội",
        "Category": "Phở",
        "Image": "/Assets/Images/pho_thin.png",
        "DescriptionVI": "...",
        "DescriptionEN": "...",
        "DescriptionCN": "...",
        "Latitude": 21.01809,
        "Longitude": 105.85527,
        "Rating": 4.8
    }
]
```

---

## Usage Examples

### Example 1: Load and Display Foods
```csharp
var foodService = new FoodService();
var foods = foodService.LoadFoods();

FoodListView.ItemsSource = foods;
```

### Example 2: Search with Speech
```csharp
var foodService = new FoodService();
var speechService = new SpeechService();

// Search
var results = foodService.SearchByKeyword("bánh mì");

// Speak description
if (results.Count > 0)
{
    var food = results[0];
    speechService.Speak(food.DescriptionVI, "vi-VN");
}
```

### Example 3: Manage Favorites
```csharp
var storage = new StorageService();

// Add to favorites
storage.AddFavorite(foodItem);

// Check if favorite
if (storage.IsFavorite(foodItem.Name))
{
    FavoriteButton.Content = "⭐ Đã yêu thích";
}

// Get all favorites
var favorites = storage.GetFavorites();
```

### Example 4: Error Handling
```csharp
var speechService = new SpeechService();

speechService.OnError += (error) =>
{
    System.Diagnostics.Debug.WriteLine($"Speech error: {error}");
    MessageBox.Show("Could not speak: " + error);
};

speechService.Speak("Hello", "en-US");
```

---

## Error Codes & Messages

| Error | Cause | Solution |
|-------|-------|----------|
| "Lỗi thuyết minh: Device not found" | NAudio issue | Restart app or reinstall NAudio |
| "Lỗi thuyết minh: No internet" | Network issue | Check internet connection |
| "File not found: Data/foods.json" | Missing data file | Place foods.json in Data folder |
| "Could not parse JSON" | Invalid JSON syntax | Check JSON file format |

---

## Best Practices

### ✅ Do
```csharp
// Use async for long operations
var foods = await Task.Run(() => _foodService.LoadFoods());

// Always check for null
if (foods != null && foods.Count > 0)
{
    // Process foods
}

// Handle events
_speechService.OnError += HandleError;
_speechService.OnPlaybackCompleted += () => Console.WriteLine("Done!");

// Dispose resources
using (var service = new SpeechService())
{
    service.Speak("Test", "vi-VN");
}
```

### ❌ Don't
```csharp
// Don't block UI thread
var foods = _foodService.LoadFoods(); // blocks!

// Don't assume data exists
FoodList[0].Name;  // crash if empty!

// Don't forget error handling
_speechService.Speak("Text", "vi-VN");
// no event handler - errors silently fail

// Don't leak resources
var service = new SpeechService();
service.Speak("Text", "vi-VN");
// forgot to Dispose()
```

---

## Performance Tips

1. **Cache frequently used data**
   ```csharp
   private List<FoodItem> _cachedFoods;
   public List<FoodItem> GetFoods()
   {
       return _cachedFoods ?? (_cachedFoods = LoadFoods());
   }
   ```

2. **Lazy load images**
   ```csharp
   <Image Source="{Binding Image}" />  <!-- WPF lazy loads by default -->
   ```

3. **Batch UI updates**
   ```csharp
   ListBox.BeginUpdate();
   foreach (var item in items) AddItem(item);
   ListBox.EndUpdate();
   ```

---

## Versioning

Current API Version: **v1.0.0**

Future: Planned for backend API integration
