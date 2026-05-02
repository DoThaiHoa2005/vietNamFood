# 🏗️ ARCHITECTURE: Image Display Fix

## 📊 System Architecture

### Before Fix (❌ BROKEN)

```
┌─────────────────────────────────────────────────────────────┐
│                      FavoritesWindow                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  <Image Source="{Binding Image}" />                        │
│           ↓                                                 │
│     Binding: "Assets/Images/Q1.jpg" (string)               │
│           ↓                                                 │
│     ❌ WPF can't convert string → ImageSource              │
│           ↓                                                 │
│     Result: Gray background (no image)                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                SQLiteFavoritesService                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  GetUserFavorites(userId):                                 │
│    SELECT f.* FROM Favorites fav                           │
│    JOIN Foods f ON fav.FoodId = f.Id  ← ❌ CROSS-DB JOIN  │
│                                                             │
│  Problem: favorites.db ≠ foods.db                          │
│  SQLite doesn't support cross-database JOIN                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### After Fix (✅ WORKING)

```
┌─────────────────────────────────────────────────────────────┐
│                      FavoritesWindow                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  <Image Source="{Binding Image,                            │
│                  Converter={StaticResource ImageConverter}}"/>│
│           ↓                                                 │
│     Binding: "Assets/Images/Q1.jpg" (string)               │
│           ↓                                                 │
│     StringToImageSourceConverter.Convert()                 │
│           ↓                                                 │
│     1. Remove leading "/" if present                       │
│     2. Create URI: pack://application:,,,/Assets/...       │
│     3. Create BitmapImage                                  │
│     4. Freeze() for thread-safety                          │
│           ↓                                                 │
│     Return: BitmapImage (ImageSource)                      │
│           ↓                                                 │
│     ✅ WPF displays image correctly                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                SQLiteFavoritesService                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  GetUserFavorites(userId):                                 │
│                                                             │
│    Step 1: Query favorites.db                              │
│    ┌─────────────────────────────────┐                     │
│    │ SELECT FoodId FROM Favorites    │                     │
│    │ WHERE UserId = @UserId          │                     │
│    └─────────────────────────────────┘                     │
│           ↓                                                 │
│    Result: [1, 3, 5] (FoodIds)                             │
│                                                             │
│    Step 2: Load from foods.db                              │
│    ┌─────────────────────────────────┐                     │
│    │ var foodService =               │                     │
│    │   new SQLiteFoodService();      │                     │
│    │ var allFoods =                  │                     │
│    │   foodService.LoadFoods();      │                     │
│    └─────────────────────────────────┘                     │
│           ↓                                                 │
│    Result: All 12 foods with images                        │
│                                                             │
│    Step 3: Filter in memory (LINQ)                         │
│    ┌─────────────────────────────────┐                     │
│    │ foreach (var foodId in foodIds) │                     │
│    │ {                               │                     │
│    │   var food = allFoods.Find(     │                     │
│    │     f => f.Id == foodId);       │                     │
│    │   if (food != null)             │                     │
│    │     favorites.Add(food);        │                     │
│    │ }                               │                     │
│    └─────────────────────────────────┘                     │
│           ↓                                                 │
│    ✅ Return: List<FoodItem> with images                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## 🔄 Data Flow

### Complete Image Display Flow

```
┌──────────────┐
│   User       │
│   Opens      │
│   Favorites  │
└──────┬───────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  FavoritesWindow.xaml.cs                                 │
│  ─────────────────────────────────────────────────────   │
│  LoadFavorites():                                        │
│    var favorites = _favoritesService.GetUserFavorites()  │
│    FavoritesList.ItemsSource = favorites                 │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  SQLiteFavoritesService.cs                               │
│  ─────────────────────────────────────────────────────   │
│  GetUserFavorites(userId):                               │
│    1. Query favorites.db → Get FoodIds                   │
│    2. Load foods.db → Get all foods                      │
│    3. Filter → Return matching foods                     │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  FoodItem Model                                          │
│  ─────────────────────────────────────────────────────   │
│  {                                                       │
│    Id: 1,                                                │
│    Name: "Alo Quán - Seafood",                          │
│    Image: "Assets/Images/Q1.jpg",  ← STRING             │
│    City: "TP.HCM - Vĩnh Khánh",                         │
│    Rating: 4.5                                           │
│  }                                                       │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  FavoritesWindow.xaml (WPF Binding)                      │
│  ─────────────────────────────────────────────────────   │
│  <Image Source="{Binding Image,                          │
│                  Converter={StaticResource               │
│                             ImageConverter}}" />         │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  StringToImageSourceConverter.cs                         │
│  ─────────────────────────────────────────────────────   │
│  Convert(value, ...):                                    │
│    Input:  "Assets/Images/Q1.jpg" (string)              │
│    Process:                                              │
│      1. Remove "/" if starts with "/"                    │
│      2. Create URI:                                      │
│         "pack://application:,,,/Assets/Images/Q1.jpg"   │
│      3. Create BitmapImage from URI                      │
│      4. Freeze() for thread-safety                       │
│    Output: BitmapImage (ImageSource)                     │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  WPF Image Control                                       │
│  ─────────────────────────────────────────────────────   │
│  Receives: BitmapImage object                            │
│  Loads:    Assets/Images/Q1.jpg                          │
│  Displays: ✅ Restaurant image                           │
└──────────────────────────────────────────────────────────┘
       │
       ↓
┌──────────────┐
│   User       │
│   Sees       │
│   Image! ✅  │
└──────────────┘
```

## 🗂️ Database Architecture

### Two Separate Databases

```
┌─────────────────────────────────────────────────────────┐
│  foods.db                                               │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  Table: Foods                                           │
│  ┌────┬──────────────────┬──────────────────────────┐  │
│  │ Id │ Name             │ Image                    │  │
│  ├────┼──────────────────┼──────────────────────────┤  │
│  │ 1  │ Alo Quán         │ Assets/Images/Q1.jpg     │  │
│  │ 2  │ Quán Ốc Đào 2    │ Assets/Images/Q2.jpg     │  │
│  │ 3  │ Bún Cá Châu Đốc  │ Assets/Images/Q3.jpg     │  │
│  │ ...│ ...              │ ...                      │  │
│  │ 12 │ Ốc Oanh          │ Assets/Images/Q12.jpg    │  │
│  └────┴──────────────────┴──────────────────────────┘  │
│                                                         │
│  Managed by: SQLiteFoodService                          │
│  Location: bin/Debug/net48/foods.db                     │
│                                                         │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  favorites.db                                           │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  Table: Favorites                                       │
│  ┌────┬────────┬────────┬─────────────────────────┐    │
│  │ Id │ UserId │ FoodId │ CreatedDate             │    │
│  ├────┼────────┼────────┼─────────────────────────┤    │
│  │ 1  │ 1      │ 1      │ 2026-05-02 10:30:00     │    │
│  │ 2  │ 1      │ 3      │ 2026-05-02 10:31:00     │    │
│  │ 3  │ 1      │ 5      │ 2026-05-02 10:32:00     │    │
│  └────┴────────┴────────┴─────────────────────────┘    │
│                                                         │
│  Managed by: SQLiteFavoritesService                     │
│  Location: bin/Debug/net48/favorites.db                 │
│                                                         │
└─────────────────────────────────────────────────────────┘

         ↓ GetUserFavorites() ↓

┌─────────────────────────────────────────────────────────┐
│  Combined Result (in memory)                            │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  List<FoodItem>:                                        │
│  ┌────┬──────────────────┬──────────────────────────┐  │
│  │ Id │ Name             │ Image                    │  │
│  ├────┼──────────────────┼──────────────────────────┤  │
│  │ 1  │ Alo Quán         │ Assets/Images/Q1.jpg     │  │
│  │ 3  │ Bún Cá Châu Đốc  │ Assets/Images/Q3.jpg     │  │
│  │ 5  │ Ốc Vũ            │ Assets/Images/Q5.jpg     │  │
│  └────┴──────────────────┴──────────────────────────┘  │
│                                                         │
│  ✅ Full food info with images                         │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## 🎨 UI Component Architecture

### Window Hierarchy

```
┌─────────────────────────────────────────────────────────┐
│  MainWindow                                             │
│  ─────────────────────────────────────────────────────  │
│  ┌─────────────────────────────────────────────────┐   │
│  │ FoodList (ItemsControl)                         │   │
│  │ ┌─────────────────────────────────────────────┐ │   │
│  │ │ DataTemplate                                │ │   │
│  │ │ ┌─────────┬─────────────────────────────┐   │ │   │
│  │ │ │ [IMAGE] │ Name: Alo Quán              │   │ │   │
│  │ │ │ Q1.jpg  │ City: TP.HCM - Vĩnh Khánh   │   │ │   │
│  │ │ │         │ Rating: ⭐ 4.5              │   │ │   │
│  │ │ │         │ [Xem chi tiết]              │   │ │   │
│  │ │ └─────────┴─────────────────────────────┘   │ │   │
│  │ │                                             │ │   │
│  │ │ <Image Source="{Binding Image,              │ │   │
│  │ │         Converter={StaticResource           │ │   │
│  │ │                    ImageConverter}}" />     │ │   │
│  │ │                    ↑                        │ │   │
│  │ │                    └─ Uses Converter ✅     │ │   │
│  │ └─────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  FavoritesWindow                                        │
│  ─────────────────────────────────────────────────────  │
│  ┌─────────────────────────────────────────────────┐   │
│  │ FavoritesList (ItemsControl)                    │   │
│  │ ┌─────────────────────────────────────────────┐ │   │
│  │ │ DataTemplate (same as MainWindow)           │ │   │
│  │ │ ┌─────────┬─────────────────────────────┐   │ │   │
│  │ │ │ [IMAGE] │ Name: Alo Quán              │   │ │   │
│  │ │ │ Q1.jpg  │ City: TP.HCM - Vĩnh Khánh   │   │ │   │
│  │ │ │         │ Rating: ⭐ 4.5              │   │ │   │
│  │ │ │         │ [Xem chi tiết]              │   │ │   │
│  │ │ └─────────┴─────────────────────────────┘   │ │   │
│  │ │                                             │ │   │
│  │ │ <Image Source="{Binding Image,              │ │   │
│  │ │         Converter={StaticResource           │ │   │
│  │ │                    ImageConverter}}" />     │ │   │
│  │ │                    ↑                        │ │   │
│  │ │                    └─ Uses Converter ✅     │ │   │
│  │ └─────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  FoodDetailWindow                                       │
│  ─────────────────────────────────────────────────────  │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Hero Image Section                              │   │
│  │ ┌─────────────────────────────────────────────┐ │   │
│  │ │                                             │ │   │
│  │ │         [LARGE HERO IMAGE]                  │ │   │
│  │ │         Q1.jpg (full width)                 │ │   │
│  │ │                                             │ │   │
│  │ │         ⭐ 4.5                              │ │   │
│  │ └─────────────────────────────────────────────┘ │   │
│  │                                                 │   │
│  │ Code-behind:                                    │   │
│  │   var converter =                               │   │
│  │     new StringToImageSourceConverter();        │   │
│  │   FoodImage.Source = converter.Convert(...)    │   │
│  │                      ↑                          │   │
│  │                      └─ Uses Converter ✅       │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

## 🔧 Converter Architecture

### StringToImageSourceConverter Flow

```
Input: "Assets/Images/Q1.jpg" (string)
  │
  ↓
┌─────────────────────────────────────────┐
│ Step 1: Validate Input                  │
│ ─────────────────────────────────────   │
│ if (value == null || empty)             │
│   return null                           │
└─────────────────────────────────────────┘
  │
  ↓
┌─────────────────────────────────────────┐
│ Step 2: Clean Path                      │
│ ─────────────────────────────────────   │
│ if (imagePath.StartsWith("/"))          │
│   imagePath = imagePath.Substring(1)    │
│                                         │
│ Result: "Assets/Images/Q1.jpg"          │
└─────────────────────────────────────────┘
  │
  ↓
┌─────────────────────────────────────────┐
│ Step 3: Create URI                      │
│ ─────────────────────────────────────   │
│ var uri = new Uri(                      │
│   "pack://application:,,,/" +           │
│   imagePath,                            │
│   UriKind.Absolute)                     │
│                                         │
│ Result: pack://application:,,,/         │
│         Assets/Images/Q1.jpg            │
└─────────────────────────────────────────┘
  │
  ↓
┌─────────────────────────────────────────┐
│ Step 4: Create BitmapImage              │
│ ─────────────────────────────────────   │
│ var bitmap = new BitmapImage()          │
│ bitmap.BeginInit()                      │
│ bitmap.UriSource = uri                  │
│ bitmap.CacheOption =                    │
│   BitmapCacheOption.OnLoad              │
│ bitmap.EndInit()                        │
└─────────────────────────────────────────┘
  │
  ↓
┌─────────────────────────────────────────┐
│ Step 5: Freeze for Thread-Safety        │
│ ─────────────────────────────────────   │
│ bitmap.Freeze()                         │
│                                         │
│ Makes bitmap immutable and              │
│ safe for cross-thread access            │
└─────────────────────────────────────────┘
  │
  ↓
Output: BitmapImage (ImageSource) ✅
```

## 📊 Summary Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                    IMAGE DISPLAY FIX                         │
│                    ─────────────────                         │
│                                                              │
│  Problem:                                                    │
│    ❌ FavoritesWindow: No images                            │
│    ❌ FoodDetailWindow: No hero image                       │
│                                                              │
│  Root Causes:                                                │
│    1. ❌ Cross-database JOIN (not supported)                │
│    2. ❌ String → ImageSource binding (not supported)       │
│                                                              │
│  Solutions:                                                  │
│    1. ✅ Created StringToImageSourceConverter               │
│    2. ✅ Fixed SQLiteFavoritesService.GetUserFavorites()    │
│    3. ✅ Updated all XAML files to use converter            │
│    4. ✅ Updated FoodDetailWindow code-behind               │
│                                                              │
│  Result:                                                     │
│    ✅ MainWindow: Images showing                            │
│    ✅ FavoritesWindow: Images showing (FIXED)               │
│    ✅ FoodDetailWindow: Hero image showing (FIXED)          │
│                                                              │
│  Status: ✅ COMPLETE                                        │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

**Date**: 2026-05-02  
**Status**: ✅ COMPLETE  
**Architecture**: Verified and documented
