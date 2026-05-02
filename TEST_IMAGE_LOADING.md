# TEST IMAGE LOADING - DIAGNOSTIC GUIDE

## CURRENT IMPLEMENTATION STATUS

### ✅ Code Changes Complete
1. **FoodItem.cs** - Added `ImageSource` property with auto-convert from `Image` string
2. **XAML Bindings** - Changed from `Image` to `ImageSource` in MainWindow.xaml and FavoritesWindow.xaml
3. **csproj** - Images configured as Content with CopyToOutputDirectory=PreserveNewest
4. **Force Update** - Added in StorageService.GetFavorites() and FavoritesApiService.GetUserFavoritesAsync()
5. **Build** - Successfully built, images copied to bin/Debug/net48/Assets/Images/

### 📊 IMAGE PATH FORMAT IN DATABASE
```
/Assets/Images/Q1.jpg
/Assets/Images/Q2.jpg
...
/Assets/Images/Q12.jpg
```

### 🔍 HOW UpdateImageSource() HANDLES PATHS

The `UpdateImageSource()` method in `FoodItem.cs` handles 3 path formats:

1. **HTTP/HTTPS URLs** (e.g., `http://example.com/image.jpg`)
   - Uses: `new Uri(_image, UriKind.Absolute)`

2. **Pack URI** (e.g., `/Assets/Images/Q1.jpg`)
   - Uses: `pack://application:,,,/Assets/Images/Q1.jpg`
   - This is for WPF embedded resources

3. **Relative Path** (e.g., `Q1.jpg`)
   - Looks in: `{AppDir}/Assets/Images/Q1.jpg`
   - Uses: `new Uri(imagePath, UriKind.Absolute)`

### ⚠️ POTENTIAL ISSUE

The database stores paths as `/Assets/Images/Q1.jpg` which triggers the **Pack URI** handler.
However, images are configured as **Content** (not Resource), so Pack URI won't work!

## SOLUTION

We need to update `UpdateImageSource()` to handle Content files correctly:

```csharp
private void UpdateImageSource()
{
    if (string.IsNullOrEmpty(_image))
    {
        ImageSource = null;
        return;
    }

    try
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        
        // Try different path formats
        if (_image.StartsWith("http://") || _image.StartsWith("https://"))
        {
            // URL
            bitmap.UriSource = new Uri(_image, UriKind.Absolute);
        }
        else
        {
            // For Content files, always use file system path
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = System.IO.Path.GetFileName(_image);
            var imagePath = System.IO.Path.Combine(appDir, "Assets", "Images", fileName);
            
            if (System.IO.File.Exists(imagePath))
            {
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                System.Diagnostics.Debug.WriteLine($"[FoodItem] Loading image from: {imagePath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[FoodItem] Image not found: {imagePath}");
                ImageSource = null;
                bitmap.EndInit();
                return;
            }
        }
        
        bitmap.EndInit();
        bitmap.Freeze(); // Improve performance
        ImageSource = bitmap;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[FoodItem] Error loading image '{_image}': {ex.Message}");
        ImageSource = null;
    }
}
```

## TESTING STEPS

### 1. Check if images exist in output folder
```powershell
ls VietnamFoodGuide/bin/Debug/net48/Assets/Images/
```
Expected: Q1.jpg through Q12.jpg and banner.png

### 2. Run the app and check Debug Output
Look for these messages:
- `[FoodItem] Loading image from: C:\...\bin\Debug\net48\Assets\Images\Q1.jpg`
- `[FoodItem] Image not found: ...` (if path is wrong)
- `[FoodItem] Error loading image '...'` (if exception occurs)

### 3. Visual Test
- Open MainWindow - should see restaurant images
- Open FavoritesWindow - should see same images
- Open FoodDetailWindow - should see large image

### 4. If images still don't show
Check these:
- Are images in bin/Debug/net48/Assets/Images/?
- Is UpdateImageSource() being called? (check Debug output)
- Is ImageSource property being set? (add breakpoint)
- Is XAML binding correct? (should be `Source="{Binding ImageSource}"`)

## QUICK FIX COMMAND

If you want to test with simpler paths, update SQLiteFoodService.cs to use just filenames:
```csharp
Image = "Q1.jpg",  // Instead of "/Assets/Images/Q1.jpg"
```

This will trigger the "Relative Path" handler which already works correctly.
